using System;
using System.Collections.Generic;
using System.Text;

namespace DelBot.Graphics {
    class Vec4 {
        public float x, y, z, w;

        public Vec4() {
            this.x = 0.0f;
            this.y = 0.0f;
            this.z = 0.0f;
            this.w = 0.0f;
        }

        public Vec4(float x, float y, float z, float w) {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }

        public Vec4(float s) {
            this.x = s;
            this.y = s;
            this.z = s;
            this.w = s;
        }

        public Vec4(Vec3 v, float s) {
            this.x = v.x;
            this.y = v.y;
            this.z = v.z;
            this.w = s;
        }

        public Vec4(Vec4 v) {
            this.x = v.x;
            this.y = v.y;
            this.z = v.z;
            this.w = v.w;
        }

        public static Vec4 operator +(Vec4 v1, Vec4 v2) {
            return new Vec4(v1.x + v2.x, v1.y + v2.y, v1.z + v2.z, v1.w + v2.w);
        }

        public static Vec4 operator -(Vec4 v1, Vec4 v2) {
            return new Vec4(v1.x - v2.x, v1.y - v2.y, v1.z - v2.z, v1.w - v2.w);
        }

        public static Vec4 operator *(float s, Vec4 v) {
            return new Vec4(s * v.x, s * v.y, s * v.z, s * v.w);
        }

        public static Vec4 operator *(Vec4 v, float s) {
            return new Vec4(s * v.x, s * v.y, s * v.z, s * v.w);
        }

        public static Vec4 operator *(Vec4 v1, Vec4 v2) {
            return new Vec4(v1.x * v2.x, v1.y * v2.y, v1.z * v2.z, v1.w * v2.w);
        }

        public static Vec4 operator *(Mat4 M, Vec4 v) {
            return new Vec4(MGM.Dot(M.r1, v), MGM.Dot(M.r2, v), MGM.Dot(M.r3, v), MGM.Dot(M.r4, v));
        }

        public static Vec4 operator /(Vec4 v, float s) {
            return new Vec4(v.x / s, v.y / s, v.z / s, v.w / s);
        }

        public Vec4 Normalize() {
            return MGM.InvSqrt(x * x + y * y + z * z + w * w) * this;
        }

    }
}
