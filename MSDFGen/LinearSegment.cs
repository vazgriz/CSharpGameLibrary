using System;
using System.Numerics;
using System.Collections.Generic;

namespace MSDFGen {
    public class LinearSegment : EdgeSegment {
        public LinearSegment(Vector2 p0, Vector2 p1, EdgeColor color) : base(color) {
            Points.Add(p0);
            Points.Add(p1);
        }

        public override EdgeSegment Clone() {
            return new LinearSegment(Points[0], Points[1], Color);
        }

        public override Vector2 GetPoint(double t) {
            return Vector2.Lerp(Points[0], Points[1], (float)t);
        }

        public override Vector2 GetDirection(double t) {
            return Points[1] - Points[0];
        }

        public override SignedDistance GetSignedDistance(Vector2 origin, out double t) {
            Vector2 aq = origin - Points[0];
            Vector2 ab = Points[1] - Points[0];
            t = Vector2.Dot(aq, ab) / Vector2.Dot(ab, ab);
            Vector2 eq = Points[t > 0.5d ? 1 : 0] - origin;
            double endPointDistance = eq.Length();

            if (t > 0 && t < 1) {
                double orthoDistance = Vector2.Dot(GetOrthonormal(ab, false, false), aq);
                if (Math.Abs(orthoDistance) < endPointDistance) return new SignedDistance(orthoDistance, 0);
            }
            return new SignedDistance(
                Math.Sign(Vector3.Cross(new Vector3(aq, 0), new Vector3(ab, 0)).Length()) * endPointDistance,
                Math.Abs(Vector2.Dot(Vector2.Normalize(ab), Vector2.Normalize(eq)))
            );
        }

        public override void Bounds(ref double left, ref double bottom, ref double right, ref double top) {
            PointBounds(Points[0], ref left, ref bottom, ref right, ref top);
            PointBounds(Points[1], ref left, ref bottom, ref right, ref top);
        }

        public override void MoveStartPoint(Vector2 to) {
            Points[0] = to;
        }

        public override void MoveEndPoint(Vector2 to) {
            Points[1] = to;
        }

        public override void SplitInThirds(out EdgeSegment part1, out EdgeSegment part2, out EdgeSegment part3) {
            part1 = new LinearSegment(Points[0], GetPoint(1 / 3d), Color);
            part2 = new LinearSegment(GetPoint(1 / 3d), GetPoint(2 / 3d), Color);
            part3 = new LinearSegment(GetPoint(2 / 3d), Points[1], Color);
        }
    }
}
