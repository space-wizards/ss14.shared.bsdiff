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

        [DllImport("bsdiffwrap.dll", EntryPoint = "bsdiff_bzip2_diff", CallingConvention = CallingConvention.Cdecl)]
        private static unsafe extern DiffResult Diff(Byte* old, UInt64 oldsize, Byte* newbuf, UInt64 newsize);

        [DllImport("bsdiffwrap.dll", EntryPoint = "bsdiff_bzip2_patch", CallingConvention = CallingConvention.Cdecl)]
        private static unsafe extern DiffResult Patch(Byte* old, UInt64 oldsize, Byte* patch, UInt64 patchsize);

        [DllImport("bsdiffwrap.dll", EntryPoint = "bsdiff_bzip2_cleanup", CallingConvention = CallingConvention.Cdecl)]
        private static unsafe extern void Cleanup(DiffResult toclean);

        /// <summary>
        /// Generates a bzip2 compressed diff between an old and a new file. The size header is included.
        /// </summary>
        /// <param name="oldFile">The "old" file. This is the same "old" file as used by <see cref="ApplyBzip2Patch(byte[], byte[])"/>.</param>
        /// <param name="newFile">The "new" file that a diff will be made for.</param>
        /// <returns>A buffer of bytes containing a bzip2 compressed diff that can be passed to <see cref="ApplyBzip2Patch(byte[], byte[])"/>.</returns>
        public static byte[] GenerateBzip2Diff(byte[] oldFile, byte[] newFile)
        {
            byte[] resultbuffer;

            unsafe
            {
                fixed (byte* oldPtr = oldFile)
                fixed (byte* newPtr = newFile)
                {
                    var result = Diff(oldPtr, (ulong)oldFile.Length, newPtr, (ulong)newFile.Length);
                    resultbuffer = new byte[result.length];

                    Marshal.Copy((IntPtr)result.ptr, resultbuffer, 0, (int)result.length);

                    Cleanup(result);
                }
            }

            return resultbuffer;
        }

        public static byte[] ApplyBzip2Patch(byte[] oldFile, byte[] patchFile)
        {
            byte[] resultbuffer;

            unsafe
            {
                fixed (byte* oldPtr = oldFile)
                fixed (byte* patchPtr = patchFile)
                { 
                    var result = Patch(oldPtr, (ulong)oldFile.Length, patchPtr, (ulong)patchFile.Length);
                    resultbuffer = new byte[result.length];

                    Marshal.Copy((IntPtr)result.ptr, resultbuffer, 0, (int)result.length);

                    Cleanup(result);
                }
            }

            return resultbuffer;
        }
    }
}
