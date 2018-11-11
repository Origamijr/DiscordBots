using System;
using System.Collections.Generic;
using System.Text;

namespace DelBot.Databases {
    class BinDatabase {


        static byte[] GetBytes(double[] values) {
            var result = new byte[values.Length * sizeof(double)];
            Buffer.BlockCopy(values, 0, result, 0, result.Length);
            return result;
        }

        static byte[] GetBytes(float[] values) {
            var result = new byte[values.Length * sizeof(float)];
            Buffer.BlockCopy(values, 0, result, 0, result.Length);
            return result;
        }

        static byte[] GetBytes(int[] values) {
            var result = new byte[values.Length * sizeof(int)];
            Buffer.BlockCopy(values, 0, result, 0, result.Length);
            return result;
        }


        static double[] GetDoubles(byte[] bytes) {
            var result = new double[bytes.Length / sizeof(double)];
            Buffer.BlockCopy(bytes, 0, result, 0, bytes.Length);
            return result;
        }

        static double[] GetFloats(byte[] bytes) {
            var result = new double[bytes.Length / sizeof(float)];
            Buffer.BlockCopy(bytes, 0, result, 0, bytes.Length);
            return result;
        }

        static double[] GetInts(byte[] bytes) {
            var result = new double[bytes.Length / sizeof(int)];
            Buffer.BlockCopy(bytes, 0, result, 0, bytes.Length);
            return result;
        }
    }
}
