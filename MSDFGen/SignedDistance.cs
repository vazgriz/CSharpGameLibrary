using System;
using System.Collections.Generic;

namespace MSDFGen {
    public struct SignedDistance {
        public double distance;
        public double dot;

        public static SignedDistance Infinite { get; } = new SignedDistance(-1e240, 1);

        public SignedDistance(double distance, double dot) {
            this.distance = distance;
            this.dot = dot;
        }

        public static bool operator < (SignedDistance a, SignedDistance b) {
            return Math.Abs(a.distance) < Math.Abs(b.distance) ||
                (Math.Abs(a.distance) == Math.Abs(b.distance) && a.dot < b.dot);
        }

        public static bool operator > (SignedDistance a, SignedDistance b) {
            return Math.Abs(a.distance) > Math.Abs(b.distance) ||
                (Math.Abs(a.distance) == Math.Abs(b.distance) && a.dot > b.dot);
        }

        public static bool operator <= (SignedDistance a, SignedDistance b) {
            return Math.Abs(a.distance) < Math.Abs(b.distance) ||
                (Math.Abs(a.distance) == Math.Abs(b.distance) && a.dot <= b.dot);
        }

        public static bool operator >= (SignedDistance a, SignedDistance b) {
            return Math.Abs(a.distance) > Math.Abs(b.distance) ||
                (Math.Abs(a.distance) == Math.Abs(b.distance) && a.dot >= b.dot);
        }
    }
}
