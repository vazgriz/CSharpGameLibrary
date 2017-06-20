using System;
using System.Numerics;
using System.Collections.Generic;

using CSGL.Math;
using CSGL.Graphics;

namespace MSDFGen {
    struct MultiDistance {
        public double r;
        public double g;
        public double b;
        public double med;
    }

    public static partial class MSDF {
        public static bool PixelClash(Color4 a, Color4 b, double threshold) {
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

        public static bool PixelClash(Color4b a, Color4b b, double threshold) {
            Color4 af = new Color4(a);
            Color4 bf = new Color4(b);
            return PixelClash(af, bf, threshold);
        }

        struct Clash {
            public int x;
            public int y;
        }

        public static void CorrectErrors(Bitmap<Color4> output, Rectanglei region, Vector2 threshold) {
            List<Clash> clashes = new List<Clash>();
            int w = output.Width;
            int h = output.Height;

            int xStart = Math.Min(Math.Max(0, region.Left), output.Width);
            int yStart = Math.Min(Math.Max(0, region.Top), output.Height);
            int xEnd = Math.Min(Math.Max(0, region.Right), output.Width);
            int yEnd = Math.Min(Math.Max(0, region.Bottom), output.Height);

            for (int y = yStart; y < yEnd; y++) {
                for (int x = xStart; x < xEnd; x++) {
                    if ((x > 0 && PixelClash(output[x, y], output[x-1, y], threshold.X)) ||
                        (x < w - 1 && PixelClash(output[x, y], output[x + 1, y], threshold.X)) ||
                        (y > 0 && PixelClash(output[x, y], output[x, y - 1], threshold.Y)) ||
                        (y < h - 1 && PixelClash(output[x, y], output[x, y + 1], threshold.Y))) {

                        clashes.Add(new Clash { x = x, y = y });
                    }
                }
            }

            for (int i = 0; i < clashes.Count; i++) {
                Color4 pixel = output[clashes[i].x, clashes[i].y];
                float med = Median(pixel.r, pixel.g, pixel.b);
                pixel.r = med;
                pixel.g = med;
                pixel.b = med;
                output[clashes[i].x, clashes[i].y] = pixel;
            }
        }

        public static void CorrectErrors(Bitmap<Color4b> output, Rectanglei region, Vector2 threshold) {
            List<Clash> clashes = new List<Clash>();
            int w = output.Width;
            int h = output.Height;

            int xStart = Math.Min(Math.Max(0, region.Left), output.Width);
            int yStart = Math.Min(Math.Max(0, region.Top), output.Height);
            int xEnd = Math.Min(Math.Max(0, region.Right), output.Width);
            int yEnd = Math.Min(Math.Max(0, region.Bottom), output.Height);

            for (int y = yStart; y < yEnd; y++) {
                for (int x = xStart; x < xEnd; x++) {
                    if ((x > 0 && PixelClash(output[x, y], output[x - 1, y], threshold.X)) ||
                        (x < w - 1 && PixelClash(output[x, y], output[x + 1, y], threshold.X)) ||
                        (y > 0 && PixelClash(output[x, y], output[x, y - 1], threshold.Y)) ||
                        (y < h - 1 && PixelClash(output[x, y], output[x, y + 1], threshold.Y))) {

                        clashes.Add(new Clash { x = x, y = y });
                    }
                }
            }

            for (int i = 0; i < clashes.Count; i++) {
                Color4b pixel = output[clashes[i].x, clashes[i].y];
                int med = Median(pixel.r, pixel.g, pixel.b);
                pixel.r = (byte)med;
                pixel.g = (byte)med;
                pixel.b = (byte)med;
                output[clashes[i].x, clashes[i].y] = pixel;
            }
        }

        static float Median(float a, float b, float c) {
            return Math.Max(Math.Min(a, b), Math.Min(Math.Max(a, b), c));
        }

        static double Median(double a, double b, double c) {
            return Math.Max(Math.Min(a, b), Math.Min(Math.Max(a, b), c));
        }

        static int Median(int a, int b, int c) {
            return Math.Max(Math.Min(a, b), Math.Min(Math.Max(a, b), c));
        }

        public static void GenerateSDF(Bitmap<float> output, Shape shape, double range, Vector2 scale, Vector2 translate) {
            GenerateSDF(output, shape, new Rectangle(0, 0, output.Width, output.Height), range, scale, translate);
        }

        public static void GenerateSDF(Bitmap<byte> output, Shape shape, double range, Vector2 scale, Vector2 translate) {
            GenerateSDF(output, shape, new Rectangle(0, 0, output.Width, output.Height), range, scale, translate);
        }

        public static void GenerateSDF(Bitmap<float> output, Shape shape, Rectangle region, double range, Vector2 scale, Vector2 translate) {
            int contourCount = shape.Contours.Count;
            int[] windings = new int[contourCount];

            for (int i = 0; i < shape.Contours.Count; i++) {
                windings[i] = shape.Contours[i].Winding;
            }

            int xStart = Math.Min(Math.Max(0, (int)region.Left), output.Width);
            int yStart = Math.Min(Math.Max(0, (int)region.Top), output.Height);
            int xEnd = Math.Min(Math.Max(0, (int)region.Right), output.Width);
            int yEnd = Math.Min(Math.Max(0, (int)region.Bottom), output.Height);
            
            double[] contourSD = new double[contourCount];

            for (int y = yStart; y < yEnd; y++) {
                int row = shape.InverseYAxis ? yEnd - (y - yStart) - 1 : y;
                for (int x = xStart; x < xEnd; x++) {
                    output[x, row] = EvaluateSDF(shape, windings, contourSD, x, y, range, scale, region.Position + translate);
                }
            }
        }

        public static void GenerateSDF(Bitmap<byte> output, Shape shape, Rectangle region, double range, Vector2 scale, Vector2 translate) {
            int contourCount = shape.Contours.Count;
            int[] windings = new int[contourCount];

            for (int i = 0; i < shape.Contours.Count; i++) {
                windings[i] = shape.Contours[i].Winding;
            }

            int xStart = Math.Min(Math.Max(0, (int)region.Left), output.Width);
            int yStart = Math.Min(Math.Max(0, (int)region.Top), output.Height);
            int xEnd = Math.Min(Math.Max(0, (int)region.Right), output.Width);
            int yEnd = Math.Min(Math.Max(0, (int)region.Bottom), output.Height);

            double[] contourSD = new double[contourCount];

            for (int y = yStart; y < yEnd; y++) {
                int row = shape.InverseYAxis ? yEnd - (y - yStart) - 1 : y;
                for (int x = xStart; x < xEnd; x++) {
                    output[x, row] = (byte)Math.Min(Math.Min((int)(EvaluateSDF(shape, windings, contourSD, x, y, range, scale, region.Position + translate) * 255), 0), 255);
                }
            }
        }

        static float EvaluateSDF(Shape shape, int[] windings, double[] contourSD, int x, int y, double range, Vector2 scale, Vector2 translate) {
            int contourCount = contourSD.Length;

            double dummy;
            Vector2 p = new Vector2(x + 0.5f, y + 0.5f) / scale - translate;
            double negDist = -SignedDistance.Infinite.distance;
            double posDist = SignedDistance.Infinite.distance;
            int winding = 0;

            for (int i = 0; i < contourCount; i++) {
                Contour contour = shape.Contours[i];
                SignedDistance minDistance = new SignedDistance(-1e240, 1);

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

            return (float)(sd / range) + 0.5f;
        }

        struct EdgePoint {
            public SignedDistance minDistance;
            public EdgeSegment nearEdge;
            public double nearParam;
        }

        public static void GenerateMSDF(Bitmap<Color3> output, Shape shape, double range, Vector2 scale, Vector2 translate, double edgeThreshold) {
            GenerateMSDF(output, shape, new Rectangle(0, 0, output.Width, output.Height), range, scale, translate, edgeThreshold);
        }

        public static void GenerateMSDF(Bitmap<Color3b> output, Shape shape, double range, Vector2 scale, Vector2 translate, double edgeThreshold) {
            GenerateMSDF(output, shape, new Rectangle(0, 0, output.Width, output.Height), range, scale, translate, edgeThreshold);
        }

        public static void GenerateMSDF(Bitmap<Color3> output, Shape shape, Rectangle region, double range, Vector2 scale, Vector2 translate, double edgeThreshold) {
            int contourCount = shape.Contours.Count;
            int[] windings = new int[contourCount];

            for (int i = 0; i < shape.Contours.Count; i++) {
                windings[i] = shape.Contours[i].Winding;
            }

            int xStart = Math.Min(Math.Max(0, (int)region.Left), output.Width);
            int yStart = Math.Min(Math.Max(0, (int)region.Top), output.Height);
            int xEnd = Math.Min(Math.Max(0, (int)region.Right), output.Width);
            int yEnd = Math.Min(Math.Max(0, (int)region.Bottom), output.Height);

            MultiDistance[] contourSD = new MultiDistance[contourCount];

            for (int y = yStart; y < yEnd; y++) {
                int row = shape.InverseYAxis ? yEnd - (y - yStart) - 1 : y;
                for (int x = xStart; x < xEnd; x++) {
                    Vector2 p = (new Vector2(x, y) - region.Position - translate) / scale;
                    output[x, row] = EvaluateMSDF(shape, windings, contourSD, p, range);
                }
            }
        }

        public static void GenerateMSDF(Bitmap<Color3b> output, Shape shape, Rectangle region, double range, Vector2 scale, Vector2 translate, double edgeThreshold) {
            int contourCount = shape.Contours.Count;
            int[] windings = new int[contourCount];

            for (int i = 0; i < shape.Contours.Count; i++) {
                windings[i] = shape.Contours[i].Winding;
            }

            int xStart = Math.Min(Math.Max(0, (int)region.Left), output.Width);
            int yStart = Math.Min(Math.Max(0, (int)region.Top), output.Height);
            int xEnd = Math.Min(Math.Max(0, (int)region.Right), output.Width);
            int yEnd = Math.Min(Math.Max(0, (int)region.Bottom), output.Height);

            MultiDistance[] contourSD = new MultiDistance[contourCount];

            for (int y = yStart; y < yEnd; y++) {
                int row = shape.InverseYAxis ? yEnd - (y - yStart) - 1 : y;
                for (int x = xStart; x < xEnd; x++) {
                    Vector2 p = (new Vector2(x, y) - region.Position - translate) / scale;
                    output[x, row] = new Color3b(EvaluateMSDF(shape, windings, contourSD, p, range));
                }
            }
        }

        public static void GenerateMSDF(Bitmap<Color4b> output, Shape shape, Rectangle region, double range, Vector2 scale, Vector2 translate, double edgeThreshold) {
            int contourCount = shape.Contours.Count;
            int[] windings = new int[contourCount];

            for (int i = 0; i < shape.Contours.Count; i++) {
                windings[i] = shape.Contours[i].Winding;
            }

            int xStart = Math.Min(Math.Max(0, (int)region.Left), output.Width);
            int yStart = Math.Min(Math.Max(0, (int)region.Top), output.Height);
            int xEnd = Math.Min(Math.Max(0, (int)region.Right), output.Width);
            int yEnd = Math.Min(Math.Max(0, (int)region.Bottom), output.Height);

            MultiDistance[] contourSD = new MultiDistance[contourCount];

            for (int y = yStart; y < yEnd; y++) {
                int row = shape.InverseYAxis ? yEnd - (y - yStart) - 1 : y;
                for (int x = xStart; x < xEnd; x++) {
                    Vector2 p = (new Vector2(x, y) - region.Position - translate) / scale;
                    output[x, row] = new Color4b(EvaluateMSDF(shape, windings, contourSD, p, range), 255);
                }
            }
        }

        static Color3 EvaluateMSDF(Shape shape, int[] windings, MultiDistance[] contourSD, Vector2 p, double range) {
            int contourCount = contourSD.Length;
            p += new Vector2(0.5f, 0.5f);

            EdgePoint sr = new EdgePoint {
                minDistance = new SignedDistance(-1e240, 1)
            };
            EdgePoint sg = new EdgePoint {
                minDistance = new SignedDistance(-1e240, 1)
            };
            EdgePoint sb = new EdgePoint {
                minDistance = new SignedDistance(-1e240, 1)
            };

            double d = Math.Abs(SignedDistance.Infinite.distance);
            double negDist = -SignedDistance.Infinite.distance;
            double posDist = SignedDistance.Infinite.distance;
            int winding = 0;

            for (int i = 0; i < contourCount; i++) {
                Contour contour = shape.Contours[i];
                EdgePoint r = new EdgePoint {
                    minDistance = new SignedDistance(-1e240, 1)
                };
                EdgePoint g = new EdgePoint {
                    minDistance = new SignedDistance(-1e240, 1)
                };
                EdgePoint b = new EdgePoint {
                    minDistance = new SignedDistance(-1e240, 1)
                };

                for (int j = 0; j < contour.Edges.Count; j++) {
                    EdgeSegment edge = contour.Edges[j];
                    double param;

                    SignedDistance distance = edge.GetSignedDistance(p, out param);
                    if ((edge.Color & EdgeColor.Red) == EdgeColor.Red && distance < r.minDistance) {
                        r.minDistance = distance;
                        r.nearEdge = edge;
                        r.nearParam = param;
                    }
                    if ((edge.Color & EdgeColor.Green) == EdgeColor.Green && distance < g.minDistance) {
                        g.minDistance = distance;
                        g.nearEdge = edge;
                        g.nearParam = param;
                    }
                    if ((edge.Color & EdgeColor.Blue) == EdgeColor.Blue && distance < b.minDistance) {
                        b.minDistance = distance;
                        b.nearEdge = edge;
                        b.nearParam = param;
                    }
                }

                if (r.minDistance < sr.minDistance) sr = r;
                if (g.minDistance < sg.minDistance) sg = g;
                if (b.minDistance < sb.minDistance) sb = b;

                double medMinDistance = Math.Abs(Median(r.minDistance.distance, g.minDistance.distance, b.minDistance.distance));

                if (medMinDistance < d) {
                    d = medMinDistance;
                    winding = -windings[i];
                }

                if (r.nearEdge != null) {
                    r.nearEdge.DistanceToPseudoDistance(ref r.minDistance, p, r.nearParam);
                }
                if (g.nearEdge != null) {
                    g.nearEdge.DistanceToPseudoDistance(ref g.minDistance, p, g.nearParam);
                }
                if (b.nearEdge != null) {
                    b.nearEdge.DistanceToPseudoDistance(ref b.minDistance, p, b.nearParam);
                }

                medMinDistance = Median(r.minDistance.distance, g.minDistance.distance, b.minDistance.distance);

                contourSD[i].r = r.minDistance.distance;
                contourSD[i].g = g.minDistance.distance;
                contourSD[i].b = b.minDistance.distance;
                contourSD[i].med = medMinDistance;

                if (windings[i] > 0 && medMinDistance >= 0 && Math.Abs(medMinDistance) < Math.Abs(posDist)) {
                    posDist = medMinDistance;
                }
                if (windings[i] < 0 && medMinDistance <= 0 && Math.Abs(medMinDistance) < Math.Abs(negDist)) {
                    negDist = medMinDistance;
                }
            }

            if (sr.nearEdge != null) sr.nearEdge.DistanceToPseudoDistance(ref sr.minDistance, p, sr.nearParam);
            if (sg.nearEdge != null) sg.nearEdge.DistanceToPseudoDistance(ref sg.minDistance, p, sg.nearParam);
            if (sb.nearEdge != null) sb.nearEdge.DistanceToPseudoDistance(ref sb.minDistance, p, sb.nearParam);

            MultiDistance msd = new MultiDistance {
                r = SignedDistance.Infinite.distance,
                g = SignedDistance.Infinite.distance,
                b = SignedDistance.Infinite.distance,
                med = SignedDistance.Infinite.distance,
            };

            if (posDist >= 0 && Math.Abs(posDist) <= Math.Abs(negDist)) {
                msd.med = SignedDistance.Infinite.distance;
                winding = 1;
                for (int i = 0; i < contourCount; i++) {
                    if (windings[i] > 0 && contourSD[i].med > msd.med && Math.Abs(contourSD[i].med) < Math.Abs(negDist)) {
                        msd = contourSD[i];
                    }
                }
            } else if (negDist <= 0 && Math.Abs(negDist) <= Math.Abs(posDist)) {
                msd.med = -SignedDistance.Infinite.distance;
                winding = -1;
                for (int i = 0; i < contourCount; i++) {
                    if (windings[i] < 0 && contourSD[i].med < msd.med && Math.Abs(contourSD[i].med) < Math.Abs(posDist)) {
                        msd = contourSD[i];
                    }
                }
            }

            for (int i = 0; i < contourCount; i++) {
                if (windings[i] != winding && Math.Abs(contourSD[i].med) < Math.Abs(msd.med)) {
                    msd = contourSD[i];
                }
            }

            if (Median(sr.minDistance.distance, sg.minDistance.distance, sb.minDistance.distance) == msd.med) {
                msd.r = sr.minDistance.distance;
                msd.g = sg.minDistance.distance;
                msd.b = sb.minDistance.distance;
            }

            return new Color3((float)(msd.r / range) + 0.5f, (float)(msd.g / range) + 0.5f, (float)(msd.b / range) + 0.5f);
        }
    }
}
