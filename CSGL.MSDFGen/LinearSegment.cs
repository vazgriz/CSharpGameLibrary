using System;
using System.Numerics;
using System.Collections.Generic;

namespace MSDFGen {
    public class LinearSegment : EdgeSegment {
        Vector2 p0;
        Vector2 p1;

        public LinearSegment(Vector2 p0, Vector2 p1, EdgeColor color) : base(color) {
            this.p0 = p0;
            this.p1 = p1;
        }

        public override EdgeSegment Clone() {
            return new LinearSegment(p0, p1, Color);
        }

        public override Vector2 GetPoint(double t) {
            return Vector2.Lerp(p0, p1, (float)t);
        }

        public override Vector2 GetDirection(double t) {
            return p1 - p0;
        }

        public override SignedDistance GetSignedDistance(Vector2 origin, out double t) {
            Vector2 aq = origin - p0;
            Vector2 ab = p1 - p0;
            t = Vector2.Dot(aq, ab) / Vector2.Dot(ab, ab);
            Vector2 eq = (t > 0.5d ? p1 : p0) - origin;
            double endPointDistance = eq.Length();

            if (t > 0 && t < 1) {
                double orthoDistance = Vector2.Dot(GetOrthonormal(ab, false, false), aq);
                if (Math.Abs(orthoDistance) < endPointDistance) return new SignedDistance(orthoDistance, 0);
            }
            return new SignedDistance(
                NonZeroSign(Cross(aq, ab)) * endPointDistance,
                Math.Abs(Vector2.Dot(Vector2.Normalize(ab), Vector2.Normalize(eq)))
            );
        }

        public override void GetBounds(ref double left, ref double bottom, ref double right, ref double top) {
            PointBounds(p0, ref left, ref bottom, ref right, ref top);
            PointBounds(p1, ref left, ref bottom, ref right, ref top);
        }

        public override void MoveStartPoint(Vector2 to) {
            p0 = to;
        }

        public override void MoveEndPoint(Vector2 to) {
            p1 = to;
        }

        public override void SplitInThirds(out EdgeSegment part1, out EdgeSegment part2, out EdgeSegment part3) {
            part1 = new LinearSegment(p0, GetPoint(1 / 3d), Color);
            part2 = new LinearSegment(GetPoint(1 / 3d), GetPoint(2 / 3d), Color);
            part3 = new LinearSegment(GetPoint(2 / 3d), p1, Color);
        }
    }
}
