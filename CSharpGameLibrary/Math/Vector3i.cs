using System;
using System.Collections.Generic;

namespace System.Numerics { //place in non CSGL namespace to make it easier to use
    public struct Vector3i : IEquatable<Vector3i> {
        public int X;
        public int Y;
        public int Z;

        public static Vector3i Zero { get { return new Vector3i(); } }
        public static Vector3i One { get { return new Vector3i(1, 1, 1); } }
        public static Vector3i UnitX { get { return new Vector3i(1, 0, 0); } }
        public static Vector3i UnitY { get { return new Vector3i(0, 1, 0); } }
        public static Vector3i UnitZ { get { return new Vector3i(0, 0, 1); } }

        public Vector3i(int x, int y, int z) {
            X = x;
            Y = y;
            Z = z;
        }

        public float Length() {
            return (float)Math.Sqrt(X * X + Y * Y + Z * Z);
        }

        public int LengthSquared() {
            return X * X + Y * Y + Z * Z;
        }

        public static Vector3 Normalize(Vector3i v) {
            float length = v.Length();
            return new Vector3(v.X / length, v.Y / length, v.Z / length);
        }

        public static Vector3i Cross(Vector3i a, Vector3i b) {
            return new Vector3i(
                a.Y * b.Z - a.Z * b.Y,
                a.Z * b.X - a.X * b.Z,
                a.X * b.Y - a.Y * b.X
            );
        }

        public static int Dot(Vector3i a, Vector3i b) {
            return a.X * b.X + a.Y * b.Y + a.Z * b.Z;
        }

        public static Vector3i operator + (Vector3i a, Vector3i b) {
            return new Vector3i(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        }

        public static Vector3i operator - (Vector3i a, Vector3i b) {
            return new Vector3i(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        }

        public static Vector3i operator -(Vector3i a) {
            return new Vector3i(-a.X, -a.Y, -a.Z);
        }

        public static Vector3i operator * (Vector3i a, Vector3i b) {
            return new Vector3i(a.X * b.X, a.Y * b.Y, a.Z * b.Z);
        }

        public static Vector3i operator * (Vector3i a, int b) {
            return new Vector3i(a.X * b, a.Y * b, a.Z * b);
        }

        public static Vector3i operator * (int a, Vector3i b) {
            return new Vector3i(a * b.X, a * b.Y, a * b.Z);
        }

        public static Vector3i operator / (Vector3i a, Vector3i b) {
            return new Vector3i(a.X / b.X, a.Y / b.Y, a.Z / b.Z);
        }

        public static Vector3i operator / (Vector3i a, int b) {
            return new Vector3i(a.X / b, a.Y / b, a.Z / b);
        }

        public static bool operator == (Vector3i a, Vector3i b) {
            return a.X == b.X && a.Y == b.Y && a.Z == b.Z;
        }

        public static bool operator != (Vector3i a, Vector3i b) {
            return a.X != b.X || a.Y != b.Y || a.Z != b.Z;
        }

        public override int GetHashCode() {
            return X.GetHashCode() ^ Y.GetHashCode() ^ Z.GetHashCode();
        }

        public override bool Equals(object obj) {
            if (obj is Vector3i) {
                return this == (Vector3i)obj;
            }
            return false;
        }

        public override string ToString() {
            return string.Format("<{0}, {1}, {2}>", X, Y, Z);
        }

        public bool Equals(Vector3i other) {
            return this == other;
        }
    }
}
