using System;
using System.Numerics;
using System.Collections.Generic;

namespace MSDFGen {
    public class Contour {
        public List<EdgeSegment> Edges { get; private set; }

        public Contour() {
            Edges = new List<EdgeSegment>();
        }

        public void GetBounds(ref double left, ref double bottom, ref double right, ref double top) {
            for(int i = 0; i < Edges.Count; i++) {
                Edges[i].GetBounds(ref left, ref bottom, ref right, ref top);
            }
        }

        public int Winding {
            get {
                if (Edges.Count == 0) return 0;

                double total = 0;

                if (Edges.Count == 1) {
                    Vector2 a = Edges[0].GetPoint(0);
                    Vector2 b = Edges[0].GetPoint(1 / 3f);
                    Vector2 c = Edges[0].GetPoint(2 / 3f);

                    total += Shoelace(a, b);
                    total += Shoelace(b, c);
                    total += Shoelace(c, a);
                } else if (Edges.Count == 2) {
                    Vector2 a = Edges[0].GetPoint(0);
                    Vector2 b = Edges[0].GetPoint(0.5f);
                    Vector2 c = Edges[1].GetPoint(0);
                    Vector2 d = Edges[1].GetPoint(0.5f);

                    total += Shoelace(a, b);
                    total += Shoelace(b, c);
                    total += Shoelace(c, d);
                    total += Shoelace(d, a);
                } else {
                    Vector2 prev = Edges[Edges.Count - 1].GetPoint(0);
                    for (int i = 0; i < Edges.Count; i++) {
                        Vector2 cur = Edges[i].GetPoint(0);
                        total += Shoelace(prev, cur);
                        prev = cur;
                    }
                }

                return Math.Sign(total);
            }
        }

        double Shoelace(Vector2 a, Vector2 b) {
            return (b.X - a.X) * (a.Y + b.Y);
        }
    }
}
