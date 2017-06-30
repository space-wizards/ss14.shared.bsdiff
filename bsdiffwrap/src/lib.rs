extern crate bsdiff;
extern crate libc;
extern crate bzip2;
extern crate byteorder;

use std::slice;
use std::mem;
use std::io;
use byteorder::{WriteBytesExt, ReadBytesExt, NetworkEndian};

#[repr(C)]
#[derive(Copy, Clone)]
pub struct DiffResult {
    pub length: libc::uint64_t,
    pub ptr: *mut libc::uint8_t
}

#[no_mangle]
pub unsafe extern "C" fn bsdiff_bzip2_diff(
    old: *mut libc::uint8_t,
    oldsize: libc::uint64_t,
    new: *mut libc::uint8_t,
    newsize: libc::uint64_t,
) -> DiffResult {
    let old = slice::from_raw_parts(old, oldsize as usize);
    let new = slice::from_raw_parts(new, newsize as usize);

    let mut writer = bzip2::write::BzEncoder::new(Vec::new(), bzip2::Compression::Default);

    // Write size.
    writer.write_u64::<NetworkEndian>(new.len() as u64).unwrap();

    // TODO: Error handling.
    bsdiff::diff::diff(&old, &new, &mut writer).expect("Diff failed.");

    let mut buffer = writer.finish().unwrap();

    buffer.shrink_to_fit();
    assert_eq!(buffer.len(), buffer.capacity());

    let result = DiffResult { length: buffer.len() as u64, ptr: buffer.as_mut_ptr() };

    // Remove vec but keep buffer allocated.
    mem::forget(buffer);

    result
}

#[no_mangle]
pub unsafe extern "C" fn bsdiff_bzip2_patch(
    old: *mut libc::uint8_t,
    oldsize: libc::uint64_t,
    patch: *mut libc::uint8_t,
    patchsize: libc::uint64_t,
) -> DiffResult {
    let old = slice::from_raw_parts(old, oldsize as usize);
    let patch = slice::from_raw_parts(patch, patchsize as usize);

    let mut reader = bzip2::read::BzDecoder::new(io::Cursor::new(&patch));

    let size = reader.read_u64::<NetworkEndian>().unwrap();

    let mut out = vec![0u8; size as usize];

    // TODO: Error handling.
    bsdiff::patch::patch(&old, &mut reader, &mut out).expect("Patch failed.");

    out.shrink_to_fit();
    assert_eq!(out.len(), out.capacity());

    let result = DiffResult { length: out.len() as u64, ptr: out.as_mut_ptr() };

    // Remove vec but keep buffer allocated.
    mem::forget(out);

    result
}

#[no_mangle]
/// Deallocate the buffer returned by the other two functions since C# can't do it directly.
pub unsafe extern "C" fn bsdiff_bzip2_cleanup(toclean: DiffResult)
{
    // Make new vector and allow it to drop so it deallocates the buffer.
    let buffer = Vec::from_raw_parts(toclean.ptr, toclean.length as usize, toclean.length as usize);
    mem::drop(buffer);
}

#[cfg(test)]
mod test {
    use super::{bsdiff_bzip2_diff, bsdiff_bzip2_patch, bsdiff_bzip2_cleanup};
    use std::slice;
    use std::ops::Deref;

    #[test]
    fn test_it() {
        let mut one = vec![0, 1, 2, 3, 4, 5];
        let mut two = vec![0, 1, 2, 3, 4, 10];

        unsafe {
            let result = bsdiff_bzip2_diff(one.as_mut_ptr(), one.len() as u64, two.as_mut_ptr(), two.len() as u64);
            let result2 = bsdiff_bzip2_patch(one.as_mut_ptr(), one.len() as u64, result.ptr, result.length);

            assert_eq!(slice::from_raw_parts(result2.ptr, result2.length as usize), two.deref());

            bsdiff_bzip2_cleanup(result);
            bsdiff_bzip2_cleanup(result2);
        }
    }
}