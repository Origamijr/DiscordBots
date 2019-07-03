using System;
using System.Collections.Generic;
using System.Text;

namespace DelBot.Graphics {
    class Mat4 {
        public Vec4 r1, r2, r3, r4;
        public Vec4 c1, c2, c3, c4;

        public Mat4() {
            c1 = new Vec4(1.0f, 0.0f, 0.0f, 0.0f);
            c2 = new Vec4(0.0f, 1.0f, 0.0f, 0.0f);
            c3 = new Vec4(0.0f, 0.0f, 1.0f, 0.0f);
            c4 = new Vec4(0.0f, 0.0f, 0.0f, 1.0f);

            r1 = new Vec4(1.0f, 0.0f, 0.0f, 0.0f);
            r2 = new Vec4(0.0f, 1.0f, 0.0f, 0.0f);
            r3 = new Vec4(0.0f, 0.0f, 1.0f, 0.0f);
            r4 = new Vec4(0.0f, 0.0f, 0.0f, 1.0f);
        }

        public Mat4(float s) {
            c1 = new Vec4(s, 0.0f, 0.0f, 0.0f);
            c2 = new Vec4(0.0f, s, 0.0f, 0.0f);
            c3 = new Vec4(0.0f, 0.0f, s, 0.0f);
            c4 = new Vec4(0.0f, 0.0f, 0.0f, s);

            r1 = new Vec4(s, 0.0f, 0.0f, 0.0f);
            r2 = new Vec4(0.0f, s, 0.0f, 0.0f);
            r3 = new Vec4(0.0f, 0.0f, s, 0.0f);
            r4 = new Vec4(0.0f, 0.0f, 0.0f, s);
        }

        public Mat4(float m11, float m12, float m13, float m14,
            float m21, float m22, float m23, float m24,
            float m31, float m32, float m33, float m34,
            float m41, float m42, float m43, float m44) {

            c1 = new Vec4(m11, m21, m31, m41);
            c2 = new Vec4(m12, m22, m32, m42);
            c3 = new Vec4(m13, m23, m33, m43);
            c4 = new Vec4(m14, m24, m34, m44);

            r1 = new Vec4(m11, m12, m13, m14);
            r2 = new Vec4(m21, m22, m23, m24);
            r3 = new Vec4(m31, m32, m33, m34);
            r4 = new Vec4(m41, m42, m43, m44);
        }

        public Mat4(Vec4 v1, Vec4 v2, Vec4 v3, Vec4 v4) {
            c1 = new Vec4(v1);
            c2 = new Vec4(v2);
            c3 = new Vec4(v3);
            c4 = new Vec4(v4);

            r1 = new Vec4(v1.x, v2.x, v3.x, v4.x);
            r2 = new Vec4(v1.x, v2.x, v3.x, v4.x);
            r3 = new Vec4(v1.x, v2.x, v3.x, v4.x);
            r4 = new Vec4(v1.x, v2.x, v3.x, v4.x);
        }

        public static Mat4 operator +(Mat4 M1, Mat4 M2) {
            return new Mat4(M1.c1 + M2.c1, M1.c2 + M2.c2, M1.c3 + M2.c3, M1.c4 + M2.c4);
        }

        public static Mat4 operator -(Mat4 M1, Mat4 M2) {
            return new Mat4(M1.c1 - M2.c1, M1.c2 - M2.c2, M1.c3 - M2.c3, M1.c4 - M2.c4);
        }

        public static Mat4 operator *(float s, Mat4 M) {
            return new Mat4(s * M.c1, s * M.c2, s * M.c3, s * M.c4);
        }

        public static Mat4 operator *(Mat4 M1, Mat4 M2) {
            return new Mat4(MGM.Dot(M1.r1, M2.c1), MGM.Dot(M1.r1, M2.c2), MGM.Dot(M1.r1, M2.c3), MGM.Dot(M1.r1, M2.c4),
                MGM.Dot(M1.r2, M2.c1), MGM.Dot(M1.r2, M2.c2), MGM.Dot(M1.r2, M2.c3), MGM.Dot(M1.r2, M2.c4),
                MGM.Dot(M1.r3, M2.c1), MGM.Dot(M1.r3, M2.c2), MGM.Dot(M1.r3, M2.c3), MGM.Dot(M1.r3, M2.c4),
                MGM.Dot(M1.r4, M2.c1), MGM.Dot(M1.r4, M2.c2), MGM.Dot(M1.r4, M2.c3), MGM.Dot(M1.r4, M2.c4));
        }


        public Mat4 Transpose() {
            return new Mat4(r1, r2, r3, r4);
        }

        public Mat4 Inverse() {
            // http://www.cg.info.hiroshima-cu.ac.jp/~miyazaki/knowledge/teche23.html

            float det = r1.x * r2.y * r3.z * r4.w + r1.x * r2.z * r3.w * r4.y + r1.x * r2.w * r3.y * r4.z
                + r1.y * r2.x * r3.w * r4.z + r1.y * r2.z * r3.x * r4.w + r1.y * r2.w * r3.z * r4.y
                + r1.z * r2.x * r3.y * r4.w + r1.z * r2.y * r3.w * r4.x + r1.z * r2.w * r3.x * r4.y
                + r1.w * r2.x * r3.z * r4.y + r1.w * r2.y * r3.x * r4.z + r1.w * r2.z * r3.y * r4.x
                - r1.x * r2.y * r3.w * r4.z - r1.x * r2.z * r3.y * r4.w - r1.x * r2.w * r3.z * r4.y
                - r1.y * r2.x * r3.z * r4.w - r1.y * r2.z * r3.w * r4.x - r1.y * r2.w * r3.y * r4.z
                - r1.z * r2.x * r3.w * r4.y - r1.z * r2.y * r3.x * r4.w - r1.z * r2.w * r3.y * r4.x
                - r1.w * r2.x * r3.y * r4.z - r1.w * r2.y * r3.z * r4.x - r1.w * r2.z * r3.x * r4.y;

            return null;

        }
    }
}
