using System;
using System.Runtime.InteropServices;

namespace SS14.Shared.Bsdiff
{
    public static class Bsdiff
    {
        [StructLayout(LayoutKind.Sequential)]
        private unsafe struct DiffResult
        {
            public UInt64 length;
            public Byte* ptr;
        }

        [DllImport("bsdiffwrap.dll", EntryPoint = "bsdiff_bzip2_diff")]
        private static unsafe extern DiffResult Diff(Byte* old, UInt64 oldsize, Byte* newbuf, UInt64 newsize);

        [DllImport("bsdiffwrap.dll", EntryPoint = "bsdiff_bzip2_patch")]
        private static unsafe extern Int32 Patch(Byte* old, UInt64 oldsize, Byte* patch, UInt64 patchsize);

        [DllImport("bsdiffwrap.dll", EntryPoint = "bsdiff_bzip2_cleanup")]
        private static unsafe extern void Cleanup(DiffResult toclean);

        /// <summary>
        /// Generates a bzip2 compressed diff between an old and a new file. The size header is included.
        /// </summary>
        /// <param name="oldFile">The "old" file. This is the same "old" file as used by <see cref="ApplyBzip2Patch(byte[], byte[])"/>.</param>
        /// <param name="newFile">The "new" file that a diff will be made for.</param>
        /// <returns>A buffer of bytes containing a bzip2 compressed diff that can be passed to <see cref="ApplyBzip2Patch(byte[], byte[])"/>.</returns>
        public static byte[] GenerateBzip2Diff(byte[] oldFile, byte[] newFile)
        {
            // Copy the buffers into unmanaged memory where Rust can sanely access them.
            var oldPtr = Marshal.AllocHGlobal(oldFile.Length);
            var newPtr = Marshal.AllocHGlobal(newFile.Length);

            Marshal.Copy(oldFile, 0, oldPtr, oldFile.Length);
            Marshal.Copy(newFile, 0, newPtr, newFile.Length);

            byte[] resultbuffer;

            unsafe
            {
                var result = Diff((Byte*)oldPtr, (ulong)oldFile.Length, (Byte*)newPtr, (ulong)newFile.Length);
                resultbuffer = new byte[result.length];

                Marshal.Copy((IntPtr)result.ptr, resultbuffer, 0, (int)result.length);

                Cleanup(result);
            }

            Marshal.FreeHGlobal(oldPtr);
            Marshal.FreeHGlobal(newPtr);

            return resultbuffer;
        }

        public static byte[] ApplyBzip2Patch(byte[] oldFile, byte[] patchFile)
        {
            throw new NotImplementedException();
        }
    }
}
