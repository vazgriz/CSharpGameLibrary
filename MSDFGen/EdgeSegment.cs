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
        public abstract void GetBounds(ref double left, ref double bottom, ref double right, ref double top);

        public abstract void MoveStartPoint(Vector2 to);
        public abstract void MoveEndPoint(Vector2 to);
        public abstract void SplitInThirds(out EdgeSegment part1, out EdgeSegment part2, out EdgeSegment part3);

        public void DistanceToPseudoDistance(ref SignedDistance distance, Vector2 origin, double t) {
            if (t < 0) {
                Vector2 dir = Vector2.Normalize(GetDirection(0));
                Vector2 aq = origin - GetPoint(0);
                double ts = Vector2.Dot(aq, dir);

                if (ts < 0) {
                    double pseudoDistance = Cross(aq, dir);
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
                    double pseudoDistance = Cross(bq, dir);
                    if (Math.Abs(pseudoDistance) <= Math.Abs(distance.distance)) {
                        distance.distance = pseudoDistance;
                        distance.dot = 0;
                    }
                }
            }
        }

        public Vector2 GetOrthonormal(Vector2 v, bool polarity, bool allowZero) {
            float len = v.Length();

            if (len == 0) return polarity ? new Vector2(0, !allowZero ? 1 : 0) : new Vector2(0, -(!allowZero ? 1 : 0));
            return polarity ? new Vector2(-v.Y / len, v.X / len) : new Vector2(v.Y / len, -v.X / len);
        }

        protected void PointBounds(Vector2 p, ref double left, ref double bottom, ref double right, ref double top) {
            if (p.X < left) left = p.X;
            if (p.Y < bottom) bottom = p.Y;
            if (p.X > right) right = p.X;
            if (p.Y > top) top = p.Y;
        }

        public static double Cross(Vector2 a, Vector2 b) {
            return a.X * b.Y - a.Y * b.X;
        }

        protected int NonZeroSign(double d) {
            int result = Math.Sign(d);
            if (result == 0) return 1;
            return result;
        }

        protected struct Roots {
            public double x0;
            public double x1;
            public double x2;

            public double this[int i] {
                get {
                    switch (i) {
                        case 0: return x0;
                        case 1: return x1;
                        case 2: return x2;
                        default: throw new ArgumentOutOfRangeException();
                    }
                }
            }
        }

        protected int SolveQuadratic(ref Roots roots, double a, double b, double c) {
            if (Math.Abs(a) < 1e-14) {
                if (Math.Abs(b) < 1e-14) {
                    if (c == 0) return -1;
                    return 0;
                }
                roots.x0 = -c / b;
                return 1;
            }

            double discriminant = b * b - 4 * a * c;

            if (discriminant > 0) {
                discriminant = Math.Sqrt(discriminant);
                roots.x0 = (-b + discriminant) / (2 * a);
                roots.x1 = (-b - discriminant) / (2 * a);
                return 2;
            } else if (discriminant == 0) {
                roots.x0 = -b / (2 * a);
                return 1;
            } else {
                return 0;
            }
        }

        protected int SolveCubicNormed(ref Roots roots, double a, double b, double c) {
            double aSquared = a * a;
            double q = (aSquared - 3 * b) / 9;
            double r = (a * (2 * aSquared - 9 * b) + 27 * c) / 54;
            double rSquared = r * r;
            double qCubed = q * q * q;

            if (rSquared < qCubed) {
                double t = r / Math.Sqrt(qCubed);
                if (t < -1) t = -1;
                if (t > 1) t = 1;
                t = Math.Acos(t);
                a /= 3;
                q = -2 * Math.Sqrt(q);

                roots.x0 = q * Math.Cos(t / 3) - a;
                roots.x1 = q * Math.Cos((t + 2 * Math.PI) / 3) - a;
                roots.x2 = q * Math.Cos((t - 2 * Math.PI) / 3) - a;

                return 3;
            } else {
                double A = -Math.Pow(
                    Math.Abs(r) + Math.Sqrt(rSquared - qCubed),
                    1 / 3d
                );
                if (r < 0) A = -A;
                double B = A == 0 ? 0 : q / A;
                a /= 3;

                roots.x0 = (A + B) - a;
                roots.x1 = -0.5 * (A + B) - a;
                roots.x2 = 0.5 * Math.Sqrt(3) * (A - B);

                if (Math.Abs(roots.x2) < 1e-14) return 2;
                return 1;
            }
        }

        protected int SolveCubic(ref Roots roots, double a, double b, double c, double d) {
            if (Math.Abs(a) < 1e-14) {
                return SolveQuadratic(ref roots, b, c, d);
            }

            return SolveCubicNormed(ref roots, b / a, c / a, d / a);
        }
    }
}
