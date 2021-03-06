﻿using SS14.Shared.Bsdiff;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            var bytes = new byte[] { 0, 1, 2, 3, 5 };
            var bytes2 = new byte[] { 0, 1, 2, 3, 10 };

            var patch = Bsdiff.GenerateBzip2Diff(bytes, bytes2);
            var result = Bsdiff.ApplyBzip2Patch(bytes, patch);

            bool equals = result.OrderBy(a => a).SequenceEqual(bytes2.OrderBy(a => a));
            Console.WriteLine(equals ? "YES" : "NO");
            Console.ReadLine();
        }
    }
}
