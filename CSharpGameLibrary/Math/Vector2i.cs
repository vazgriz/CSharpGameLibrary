using System;
using System.Collections.Generic;

namespace System.Numerics { //place in non CSGL namespace to make it easier to use
    public struct Vector2i : IEquatable<Vector2i> {
        public int X;
        public int Y;

        public static Vector2i Zero { get { return new Vector2i(); } }
        public static Vector2i One { get { return new Vector2i(1, 1); } }
        public static Vector2i UnitX { get { return new Vector2i(1, 0); } }
        public static Vector2i UnitY { get { return new Vector2i(0, 1); } }

        public Vector2i(int x, int y) {
            X = x;
            Y = y;
        }

        public float Length() {
            return (float)Math.Sqrt(X * X + Y * Y);
        }

        public int LengthSquared() {
            return X * X + Y * Y;
        }

        public static Vector2 Normalize(Vector2i v) {
            float length = v.Length();
            return new Vector2(v.X / length, v.Y / length);
        }

        public static int Dot(Vector2i a, Vector2i b) {
            return a.X * b.X + a.Y * b.Y;
        }

        public static Vector2i operator +(Vector2i a, Vector2i b) {
            return new Vector2i(a.X + b.X, a.Y + b.Y);
        }

        public static Vector2i operator -(Vector2i a, Vector2i b) {
            return new Vector2i(a.X - b.X, a.Y - b.Y);
        }

        public static Vector2i operator -(Vector2i a) {
            return new Vector2i(-a.X, -a.Y);
        }

        public static Vector2i operator *(Vector2i a, Vector2i b) {
            return new Vector2i(a.X * b.X, a.Y * b.Y);
        }

        public static Vector2i operator *(Vector2i a, int b) {
            return new Vector2i(a.X * b, a.Y * b);
        }

        public static Vector2i operator *(int a, Vector2i b) {
            return new Vector2i(a * b.X, a * b.Y);
        }

        public static Vector2i operator /(Vector2i a, Vector2i b) {
            return new Vector2i(a.X / b.X, a.Y / b.Y);
        }

        public static Vector2i operator /(Vector2i a, int b) {
            return new Vector2i(a.X / b, a.Y / b);
        }

        public static bool operator ==(Vector2i a, Vector2i b) {
            return a.X == b.X && a.Y == b.Y;
        }

        public static bool operator !=(Vector2i a, Vector2i b) {
            return a.X != b.X || a.Y != b.Y;
        }

        public override int GetHashCode() {
            return X.GetHashCode() ^ Y.GetHashCode();
        }

        public override bool Equals(object obj) {
            if (obj is Vector2i) {
                return this == (Vector2i)obj;
            }
            return false;
        }

        public override string ToString() {
            return string.Format("<{0}, {1}>", X, Y);
        }

        public bool Equals(Vector2i other) {
            return this == other;
        }
    }
}
