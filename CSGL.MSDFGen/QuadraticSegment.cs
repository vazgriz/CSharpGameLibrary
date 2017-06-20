using System;
using System.Numerics;
using System.Collections.Generic;

namespace MSDFGen {
    public class QuadraticSegment : EdgeSegment {
        Vector2 p0;
        Vector2 p1;
        Vector2 p2;

        public QuadraticSegment(Vector2 p0, Vector2 p1, Vector2 p2, EdgeColor color) : base(color) {
            this.p0 = p0;
            this.p1 = p1;
            this.p2 = p2;
        }

        public override EdgeSegment Clone() {
            return new QuadraticSegment(p0, p1, p2, Color);
        }

        public override Vector2 GetPoint(double t) {
            return Vector2.Lerp(
                Vector2.Lerp(p0, p1, (float)t),
                Vector2.Lerp(p1, p2, (float)t),
                (float)t
            );
        }

        public override Vector2 GetDirection(double t) {
            return Vector2.Lerp(
                p1 - p0,
                p2 - p1,
                (float)t
            );
        }

        public override SignedDistance GetSignedDistance(Vector2 origin, out double t) {
            Vector2 qa = p0 - origin;
            Vector2 ab = p1 - p0;
            Vector2 br = p0 + p2 - p1 - p1;
            double a = Vector2.Dot(br, br);
            double b = 3 * Vector2.Dot(ab, br);
            double c = 2 * Vector2.Dot(ab, ab) + Vector2.Dot(qa, br);
            double d = Vector2.Dot(qa, ab);

            Roots roots = new Roots();
            int solutions = SolveCubic(ref roots, a, b, c, d);

            double minDistance = NonZeroSign(Cross(ab, qa)) * qa.Length();
            t = -Vector2.Dot(qa, ab) / Vector2.Dot(ab, ab);

            {
                double distance = NonZeroSign(Cross(p2 - p1, p2 - origin)) * (p2 - origin).Length();
                if (Math.Abs(distance) < Math.Abs(minDistance)) {
                    minDistance = distance;
                    t = Vector2.Dot(origin - p1, p2 - p1) / Vector2.Dot(p2 - p1, p2 - p1);
                }
            }

            for (int i = 0; i < solutions; i++) {
                if (roots[i] > 0 && roots[i] < 1) {
                    Vector2 endPoint = p0 + ((float)(2 * roots[i]) * ab) + ((float)(roots[i] * roots[i]) * br);
                    double distance = NonZeroSign(Cross(p2 - p0, endPoint - origin)) * (endPoint - origin).Length();

                    if (Math.Abs(distance) <= Math.Abs(minDistance)) {
                        minDistance = distance;
                        t = roots[i];
                    }
                }
            }

            if (t >= 0 && t <= 1) {
                return new SignedDistance(minDistance, 0);
            }
            if (t < .5) {
                return new SignedDistance(minDistance, Math.Abs(Vector2.Dot(Vector2.Normalize(ab), Vector2.Normalize(qa))));
            } else {
                return new SignedDistance(
                    minDistance,
                    Math.Abs(Vector2.Dot(
                        Vector2.Normalize(p2 - p1),
                        Vector2.Normalize(p2 - origin)
                    ))
                );
            }
        }

        public override void GetBounds(ref double left, ref double bottom, ref double right, ref double top) {
            PointBounds(p0, ref left, ref bottom, ref right, ref top);
            PointBounds(p2, ref left, ref bottom, ref right, ref top);
            Vector2 bot = (p1 - p0) - (p2 - p1);

            if (bot.X != 0) {
                double param = (p1.X - p0.X) / bot.X;
                if (param > 0 && param < 1) PointBounds(GetPoint(param), ref left, ref bottom, ref right, ref top);
            }
            if (bot.Y != 0) {
                double param = (p1.Y - p0.Y) / bot.Y;
                if (param > 0 && param < 1) PointBounds(GetPoint(param), ref left, ref bottom, ref right, ref top);
            }
        }

        public override void MoveStartPoint(Vector2 to) {
            Vector2 origSDir = p0 - p1;
            Vector2 origP1 = p1;

            p1 += (float)(Cross(p0 - p1, to - p0) / Cross(p0 - p1, p2 - p1)) * (p2 - p1);
            p0 = to;
            if (Vector2.Dot(origSDir, p0 - p1) < 0) p1 = origP1;
        }

        public override void MoveEndPoint(Vector2 to) {
            Vector2 origEDir = p2 - p1;
            Vector2 origP1 = p1;

            p1 += (float)(Cross(p2 - p1, to - p2) / Cross(p2 - p1, p0 - p1)) * (p0 - p1);
            p2 = to;
            if (Vector2.Dot(origEDir, p2 - p1) < 0) p1 = origP1;
        }

        public override void SplitInThirds(out EdgeSegment part1, out EdgeSegment part2, out EdgeSegment part3) {
            part1 = new QuadraticSegment(
                p0,
                Vector2.Lerp(p0, p1, 1 / 3f),
                GetPoint(1 / 3d),
                Color
            );
            part2 = new QuadraticSegment(
                GetPoint(1 / 3d),
                Vector2.Lerp(
                    Vector2.Lerp(p0, p1, 5 / 9f),
                    Vector2.Lerp(p1, p2, 4 / 9f),
                    0.5f
                ),
                GetPoint(2 / 3d),
                Color
            );
            part3 = new QuadraticSegment(
                GetPoint(2/3d),
                Vector2.Lerp(p1, p2, 2/3f),
                p2,
                Color
            );
        }
    }
}
