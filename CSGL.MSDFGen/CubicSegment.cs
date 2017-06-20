using System;
using System.Numerics;
using System.Collections.Generic;

namespace MSDFGen {
    public class CubicSegment : EdgeSegment {
        Vector2 p0;
        Vector2 p1;
        Vector2 p2;
        Vector2 p3;

        public CubicSegment(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, EdgeColor color) : base(color) {
            this.p0 = p0;
            this.p1 = p1;
            this.p2 = p2;
            this.p3 = p3;
        }

        public override EdgeSegment Clone() {
            return new CubicSegment(p0, p1, p2, p3, Color);
        }

        public override Vector2 GetPoint(double t) {
            Vector2 p12 = Vector2.Lerp(p1, p2, (float)t);
            return Vector2.Lerp(
                Vector2.Lerp(
                    Vector2.Lerp(p0, p1, (float)t),
                    p1,
                    (float)t
                ),
                Vector2.Lerp(
                    p12,
                    Vector2.Lerp(p2, p3, (float)t),
                    (float)t
                ),
                (float)t
            );
        }

        public override Vector2 GetDirection(double t) {
            Vector2 tangent = Vector2.Lerp(
                Vector2.Lerp(p1 - p0, p2 - p1, (float)t),
                Vector2.Lerp(p2 - p1, p3 - p2, (float)t),
                (float)t
            );

            if (tangent == Vector2.Zero) {
                if (t == 0) return p2 - p0;
                if (t == 1) return p3 - p1;
            }

            return tangent;
        }

        public override SignedDistance GetSignedDistance(Vector2 origin, out double t) {
            Vector2 qa = p0 - origin;
            Vector2 ab = p1 - p0;
            Vector2 br = p2 - p1 - ab;
            Vector2 _as = (p3 - p2) - (p2 - p1) - br;

            Vector2 epDir = GetDirection(0);
            double minDistance = NonZeroSign(Cross(epDir, qa)) * qa.Length();
            t = -Vector2.Dot(qa, epDir) / Vector2.Dot(epDir, epDir);

            {
                epDir = GetDirection(1);
                double distance = NonZeroSign(Cross(epDir, p3 - origin)) * (p3 - origin).Length();

                if (Math.Abs(distance) < Math.Abs(minDistance)) {
                    minDistance = distance;
                    t = Vector2.Dot(origin + epDir - p3, epDir) / Vector2.Dot(epDir, epDir);
                }
            }

            for (int i = 0; i < 4; i++) {
                double _t = (double)i / 4;
                int step = 0;
                while (true) {
                    Vector2 qpt = GetPoint(_t) - origin;
                    double distance = NonZeroSign(Cross(GetDirection(_t), qpt)) * qpt.Length();

                    if (Math.Abs(distance) < Math.Abs(minDistance)) {
                        minDistance = distance;
                        t = _t;
                    }

                    if (step == 4) break;

                    Vector2 d1 = (3 * _as * (float)(t * t)) + (6 * br * (float)t) + (3 * ab);
                    Vector2 d2 = 6 * _as * (float)t + 6 * br;
                    _t -= Vector2.Dot(qpt, d1) / (Vector2.Dot(d1, d1) + Vector2.Dot(qpt, d2));
                    if (t < 0 || t > 1) break;
                    step++;
                }
            }

            if (t >= 0 && t <= 1) {
                return new SignedDistance(minDistance, 0);
            }
            if (t < 0.5) {
                return new SignedDistance(
                    minDistance,
                    Math.Abs(Vector2.Dot(Vector2.Normalize(GetDirection(0)), Vector2.Normalize(qa)))
                );
            } else {
                return new SignedDistance(
                    minDistance,
                    Math.Abs(Vector2.Dot(Vector2.Normalize(GetDirection(1)), Vector2.Normalize(p3 - origin)))
                );
            }
        }

        public override void GetBounds(ref double left, ref double bottom, ref double right, ref double top) {
            PointBounds(p0, ref left, ref bottom, ref right, ref top);
            PointBounds(p3, ref left, ref bottom, ref right, ref top);

            Vector2 a0 = p1 - p0;
            Vector2 a1 = 2 * (p2 - p1 - a0);
            Vector2 a2 = p3 - 3 * p2 + 3 * p1 - p0;

            Roots roots = new Roots();
            int solutions = SolveQuadratic(ref roots, a2.X, a1.X, a0.X);
            for (int i = 0; i < solutions; i++) {
                if (roots[i] > 0 && roots[i] < 1) {
                    PointBounds(GetPoint(roots[i]), ref left, ref bottom, ref right, ref top);
                }
            }

            solutions = SolveQuadratic(ref roots, a2.Y, a1.Y, a0.Y);
            for (int i = 0; i < solutions; i++) {
                if (roots[i] >0 && roots[i] < 1) {
                    PointBounds(GetPoint(roots[i]), ref left, ref bottom, ref right, ref top);
                }
            }
        }

        public override void MoveStartPoint(Vector2 to) {
            p1 += to - p0;
            p0 = to;
        }

        public override void MoveEndPoint(Vector2 to) {
            p2 += to - p3;
            p3 = to;
        }

        public override void SplitInThirds(out EdgeSegment part1, out EdgeSegment part2, out EdgeSegment part3) {
            part1 = new CubicSegment(
                p0,
                p0 == p1 ? p0 : Vector2.Lerp(p0, p1, 1 / 3f),
                Vector2.Lerp(
                    Vector2.Lerp(p0, p1, 1 / 3f),
                    Vector2.Lerp(p1, p2, 1 / 3f),
                    1 / 3f
                ),
                GetPoint(1 / 3d),
                Color
            );
            part2 = new CubicSegment(
                GetPoint(1 / 3d),
                Vector2.Lerp(
                    Vector2.Lerp(
                        Vector2.Lerp(p0, p1, 1 / 3f),
                        Vector2.Lerp(p1, p2, 1 / 3f),
                        1 / 3f
                    ),
                    Vector2.Lerp(
                        Vector2.Lerp(p1, p2, 1 / 3f),
                        Vector2.Lerp(p2, p3, 1 / 3f),
                        1 / 3f
                    ),
                    2 / 3f
                ),
                Vector2.Lerp(
                    Vector2.Lerp(
                        Vector2.Lerp(p0, p1, 2 / 3f),
                        Vector2.Lerp(p1, p2, 2 / 3f),
                        2 / 3f
                    ),
                    Vector2.Lerp(
                        Vector2.Lerp(p1, p2, 2 / 3f),
                        Vector2.Lerp(p2, p3, 2 / 3f),
                        2 / 3f
                    ),
                    1 / 3f
                ),
                GetPoint(2 / 3d),
                Color
            );
            part3 = new CubicSegment(
                GetPoint(2 / 3d),
                Vector2.Lerp(
                    Vector2.Lerp(p1, p2, 2 / 3f),
                    Vector2.Lerp(p2, p3, 2 / 3f),
                    2 / 3f
                ),
                p2 == p3 ? p3 : Vector2.Lerp(p2, p3, 2 / 3f),
                p3,
                Color
            );
        }
    }
}
