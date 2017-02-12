using System;
using System.Numerics;
using System.Collections.Generic;

using CSGL.Graphics;

namespace MSDFGen {
    struct MultiDistance {
        public double r;
        public double g;
        public double b;
        public double med;
    }

    public static partial class MSDF {
        static bool PixelClash(Color3 a, Color3 b, double threshold) {
            bool aIn = ((a.r > .5f) ? 1 : 0) + ((a.g > .5f) ? 1 : 0) + ((a.b > .5f) ? 1 : 0) >= 2;
            bool bIn = ((b.r > .5f) ? 1 : 0) + ((b.g > .5f) ? 1 : 0) + ((b.b > .5f) ? 1 : 0) >= 2;
            if (aIn != bIn) return false;

            if ((a.r > .5f && a.g > .5f && a.b > .5f) ||
                (a.r < .5f && a.g < .5f && a.b < .5f) ||
                (b.r > .5f && b.g > .5f && b.b > .5f) ||
                (b.r < .5f && b.g < .5f && b.b < .5f))
                return false;

            float aa, ab, ba, bb, ac, bc;

            if ((a.r > .5f) != (b.r > .5f) &&
                (a.r < .5f) != (b.r < .5f)) {
                aa = a.r;
                ba = b.r;
                if ((a.g > .5f) != (b.g > .5f) &&
                    (a.g < .5f) != (b.g < .5f)) {
                    ab = a.g;
                    bb = b.g;
                    ac = a.b;
                    bc = b.b;
                } else if ((a.b > .5f) != (b.b > .5f) &&
                    (a.b < .5f) != (b.b < .5f)) {
                    ab = a.b;
                    bb = b.b;
                    ac = a.g;
                    bc = b.g;
                } else
                    return false;
            } else if ((a.g > .5f) != (b.g > .5f) &&
                (a.g < .5f) != (b.g < .5f)  &&
                (a.b > .5f) != (b.b > .5f) &&
                (a.b < .5f) != (b.b < .5f)) {
                aa = a.g;
                ba = b.g;
                ab = a.b;
                bb = b.b;
                ac = a.r;
                bc = b.r;
            } else
                return false;
            return (Math.Abs(aa - ba) >= threshold) &&
                (Math.Abs(ab - bb) >= threshold) &&
                Math.Abs(ac - .5f) >= Math.Abs(bc - .5f);
        }

        struct Clash {
            public int x;
            public int y;
        }

        static void ErrorCorrection(Bitmap<Color3> output, Vector2 threshold) {
            List<Clash> clashes = new List<Clash>();
            int w = output.Width;
            int h = output.Height;

            for (int y = 0; y < h; y++) {
                for (int x = 0; x < w; x++) {
                    if ((x > 0 && PixelClash(output[x, y], output[x-1, y], threshold.X)) ||
                        (x < w - 1 && PixelClash(output[x, y], output[x + 1, y], threshold.X)) ||
                        (y > 0 && PixelClash(output[x, y], output[x, y - 1], threshold.Y)) ||
                        (y < h - 1 && PixelClash(output[x, y], output[x, y + 1], threshold.Y))) {

                        clashes.Add(new Clash { x = x, y = y });
                    }
                }
            }

            for (int i = 0; i < clashes.Count; i++) {
                Color3 pixel = output[clashes[i].x, clashes[i].y];
                float med = Math.Max(Math.Min(pixel.r, pixel.g), Math.Min(Math.Max(pixel.r, pixel.g), pixel.b));
                pixel.r = med;
                pixel.g = med;
                pixel.b = med;
                output[clashes[i].x, clashes[i].y] = pixel;
            }
        }

        public static void GenerateSDF(Bitmap<float> output, Shape shape, double range, Vector2 scale, Vector2 translate) {
            int contourCount = shape.Contours.Count;
            int w = output.Width;
            int h = output.Height;
            List<int> windings = new List<int>(contourCount);

            for (int i = 0; i < contourCount; i++) {
                windings.Add(shape.Contours[i].Winding);
            }

            {
                List<double> contourSD = new List<double>(contourCount);
                for (int k = 0; k < contourCount; k++) {
                    contourSD.Add(0);
                }

                for (int y = 0; y < h; y++) {
                    int row = shape.InverseYAxis ? h - y - 1 : y;
                    for (int x = 0; x < w; x++) {
                        double dummy;
                        Vector2 p = new Vector2(x + 0.5f, y + 0.5f) / scale - translate;
                        double negDist = -SignedDistance.Infinite.distance;
                        double posDist = SignedDistance.Infinite.distance;
                        int winding = 0;

                        for (int i = 0; i < contourCount; i++) {
                            Contour contour = shape.Contours[i];
                            SignedDistance minDistance = new SignedDistance();

                            for (int j = 0; j < contour.Edges.Count; j++) {
                                EdgeSegment edge = contour.Edges[j];
                                SignedDistance distance = edge.GetSignedDistance(p, out dummy);
                                if (distance < minDistance) {
                                    minDistance = distance;
                                }
                            }

                            contourSD[i] = minDistance.distance;
                            if (windings[i] > 0 && minDistance.distance >= 0 && Math.Abs(minDistance.distance) < Math.Abs(posDist)) {
                                posDist = minDistance.distance;
                            }

                            if (windings[i] < 0 && minDistance.distance <= 0 && Math.Abs(minDistance.distance) < Math.Abs(negDist)) {
                                negDist = minDistance.distance;
                            }
                        }

                        double sd = SignedDistance.Infinite.distance;

                        if (posDist >= 0 && Math.Abs(posDist) <= Math.Abs(negDist)) {
                            sd = posDist;
                            winding = 1;
                            for (int i = 0; i < contourCount; i++) {
                                if (windings[i] > 0 && contourSD[i] > sd && Math.Abs(contourSD[i]) < Math.Abs(negDist)) {
                                    sd = contourSD[i];
                                }
                            }
                        } else if (negDist <= 0 && Math.Abs(negDist) <= Math.Abs(posDist)) {
                            sd = negDist;
                            winding = -1;
                            for (int i = 0; i < contourCount; i++) {
                                if (windings[i] < 0 && contourSD[i] < sd && Math.Abs(contourSD[i]) < Math.Abs(posDist)) {
                                    sd = contourSD[i];
                                }
                            }
                        }

                        for (int i = 0; i < contourCount; i++) {
                            if (windings[i] != winding && Math.Abs(contourSD[i]) < Math.Abs(sd)) {
                                sd = contourSD[i];
                            }
                        }

                        output[x, row] = (float)(sd / range) + 0.5f;
                    }
                }
            }
        }
    }
}
