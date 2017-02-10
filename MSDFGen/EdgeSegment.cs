using System;
using System.Numerics;
using System.Collections.Generic;

namespace MSDFGen {
    public enum EdgeColor {
        Black = 0,
        Red = 1,
        Green = 2,
        Yellow = 3,
        Blue = 4,
        Magenta = 5,
        Cyan = 6,
        White = 7
    }

    public abstract class EdgeSegment {
        public EdgeColor Color { get; set; }

        protected EdgeSegment(EdgeColor color) {
            Color = color;
        }

        public abstract EdgeSegment Clone();
        public abstract Vector2 GetPoint(double t);
        public abstract Vector2 GetDirection(double t);
        public abstract SignedDistance GetSignedDistance(Vector2 origin, out double t);
        public abstract void Bounds(ref double left, ref double bottom, ref double right, ref double top);

        public abstract void MoveStartPoint(Vector2 to);
        public abstract void MoveEndPoint(Vector2 to);
        public abstract void SplitInThirds(out EdgeSegment part1, out EdgeSegment part2, out EdgeSegment part3);

        public void DistanceToPseudoDistance(ref SignedDistance distance, Vector2 origin, double t) {
            if (t < 0) {
                Vector2 dir = Vector2.Normalize(GetDirection(0));
                Vector2 aq = origin - GetPoint(0);
                double ts = Vector2.Dot(aq, dir);

                if (ts < 0) {
                    double pseudoDistance = Cross(aq, dir);    //c++ version calculates magnitude of 0-extended 3d vectors

                    if (Math.Abs(pseudoDistance) <= Math.Abs(distance.distance)) {
                        distance.distance = pseudoDistance;
                        distance.dot = 0;
                    }
                }
            } else if (t > 1) {
                Vector2 dir = Vector2.Normalize(GetDirection(1));
                Vector2 bq = origin - GetPoint(1);
                double ts = Vector2.Dot(bq, dir);

                if (ts > 0) {
                    double pseudoDistance = Cross(bq, dir);    //c++ version calculates magnitude of 0-extended 3d vectors

                    if (Math.Abs(pseudoDistance) <= Math.Abs(distance.distance)) {
                        distance.distance = pseudoDistance;
                        distance.dot = 0;
                    }
                }
            }
        }

        public Vector2 GetOrthonormal(Vector2 v, bool polarity, bool allowZero) {
            float len = v.Length();

            if (len == 0) return polarity ? new Vector2(0, allowZero ? 0 : 1) : new Vector2(0, allowZero ? 0 : -1);
            return polarity ? new Vector2(-v.Y / len, v.X / len) : new Vector2(v.Y / len, v.X / len);
        }

        protected void PointBounds(Vector2 p, ref double left, ref double bottom, ref double right, ref double top) {
            if (p.X < left) left = p.X;
            if (p.Y < bottom) bottom = p.Y;
            if (p.X > right) right = p.X;
            if (p.Y > top) top = p.Y;
        }

        protected double Cross(Vector2 a, Vector2 b) {
            return Vector3.Cross(new Vector3(a, 0), new Vector3(b, 0)).Length();
        }

        protected int NonZeroSign(double d) {
            int result = Math.Sign(d);
            if (result == 0) return 1;
            return result;
        }
    }
}
