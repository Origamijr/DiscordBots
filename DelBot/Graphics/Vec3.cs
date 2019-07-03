using System;
using System.Collections.Generic;
using System.Text;

namespace DelBot.Graphics {
    class Vec3 {
        public float x, y, z;

        public Vec3() {
            this.x = 0.0f;
            this.y = 0.0f;
            this.z = 0.0f;
        }

        public Vec3(float x, float y, float z) {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public Vec3(float s) {
            this.x = s;
            this.y = s;
            this.z = s;
        }

        public Vec3(Vec3 v) {
            this.x = v.x;
            this.y = v.y;
            this.z = v.z;
        }

        public static Vec3 operator +(Vec3 v1, Vec3 v2) {
            return new Vec3(v1.x + v2.x, v1.y + v2.y, v1.z + v2.z);
        }

        public static Vec3 operator -(Vec3 v1, Vec3 v2) {
            return new Vec3(v1.x - v2.x, v1.y - v2.y, v1.z - v2.z);
        }

        public static Vec3 operator *(float s, Vec3 v) {
            return new Vec3(s * v.x, s * v.y, s * v.z);
        }

        public static Vec3 operator *(Vec3 v, float s) {
            return new Vec3(s * v.x, s * v.y, s * v.z);
        }

        public static Vec3 operator *(Vec3 v1, Vec3 v2) {
            return new Vec3(v1.x * v2.x, v1.y * v2.y, v1.z * v2.z);
        }

        public static Vec3 operator /(Vec3 v, float s) {
            return new Vec3(v.x / s, v.y / s, v.z / s);
        }

        public Vec3 Normalize() {
            return MGM.InvSqrt(x * x + y * y + z * z) * this;
        }

    }
}
