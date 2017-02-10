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
        public abstract SignedDistance GetSignedDistance(Vector2 origin, double t);
        public abstract void Bounds(out double left, out double bottom, out double right, out double top);

        public abstract void MoveStatePoint(Vector2 to);
        public abstract void MoveEndPoint(Vector2 to);
        public abstract void SplitInThirds(out EdgeSegment part1, out EdgeSegment part2, out EdgeSegment part3);

        public void DistanceToPseudoDistance(ref SignedDistance distance, Vector2 origin, double t) {
            if (t < 0) {
                Vector2 dir = Vector2.Normalize(GetDirection(0));
                Vector2 aq = origin - GetPoint(0);
                double ts = Vector2.Dot(aq, dir);

                if (ts < 0) {
                    double pseudoDistance = Vector3.Cross(new Vector3(aq, 0), new Vector3(dir, 0)).Length();    //c++ version calculates magnitude of 0-extended 3d vectors

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
                    double pseudoDistance = Vector3.Cross(new Vector3(bq, 0), new Vector3(dir, 0)).Length();    //c++ version calculates magnitude of 0-extended 3d vectors

                    if (Math.Abs(pseudoDistance) <= Math.Abs(distance.distance)) {
                        distance.distance = pseudoDistance;
                        distance.dot = 0;
                    }
                }
            }
        }
    }
}
