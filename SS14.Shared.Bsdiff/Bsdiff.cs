using System;
using System.Runtime.InteropServices;

namespace SS14.Shared.Bsdiff
{
    public static class Bsdiff
    {
        [DllImport("bsdiffwrap.dll", EntryPoint = "testing")]
        public static extern Int32 Testing();
    }
}
