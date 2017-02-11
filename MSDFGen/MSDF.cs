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

    public static class MSDF {
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
    }
}
