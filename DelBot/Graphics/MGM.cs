using System;
using System.Collections.Generic;
using System.Text;

namespace DelBot.Graphics {
    class MGM {
        public static float InvSqrt(float x) {
            float xhalf = 0.5f * x;
            int i = BitConverter.ToInt32(BitConverter.GetBytes(x), 0);
            i = 0x5f3759df - (i >> 1);
            x = BitConverter.ToSingle(BitConverter.GetBytes(i), 0);
            x = x * (1.5f - xhalf * x * x);
            //x = x * (1.5f - xhalf * x * x);
            return x;
        }


        public static float Dot(Vec4 v1, Vec4 v2) {
            return v1.x * v2.x + v1.y * v2.y + v1.z * v2.z + v1.w * v2.w;
        }

        public static float Dot(Vec3 v1, Vec3 v2) {
            return v1.x * v2.x + v1.y * v2.y + v1.z * v2.z;
        }

        public static float Dot(Vec4 v1, Vec3 v2) {
            return v1.x * v2.x + v1.y * v2.y + v1.z * v2.z + v1.w;
        }
    }
}
