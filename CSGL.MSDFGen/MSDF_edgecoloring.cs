using System;
using System.Numerics;
using System.Collections.Generic;

namespace MSDFGen {
    public static partial class MSDF {
        static bool IsCorner(Vector2 aDir, Vector2 bDir, double crossThreshold) {
            return Vector2.Dot(aDir, bDir) <= 0 || Math.Abs(EdgeSegment.Cross(aDir, bDir)) > crossThreshold;
        }

        static EdgeColor[] switchColors = { EdgeColor.Cyan, EdgeColor.Magenta, EdgeColor.Yellow };

        static void SwitchColor(ref EdgeColor color, ref ulong seed, EdgeColor banned) {
            EdgeColor combined = color & banned;

            if (combined == EdgeColor.Red || combined == EdgeColor.Green || combined == EdgeColor.Blue) {
                color = combined ^ EdgeColor.White;
                return;
            }

            if (color == EdgeColor.Black || color == EdgeColor.White) {
                color = switchColors[seed % 3];
                seed /= 3;
                return;
            }

            int shifted = (int)color << (int)(1 + (seed & 1));
            color = (EdgeColor)((shifted | shifted >> 3) & (int)EdgeColor.White);
            seed >>= 1;
        }

        public static void EdgeColoringSimple(Shape shape, double angleThreshold, ulong seed) {
            double crossThreshold = Math.Sin(angleThreshold);
            List<int> corners = new List<int>();
            for (int i = 0; i < shape.Contours.Count; i++) {
                Contour contour = shape.Contours[i];
                corners.Clear();

                if (!(contour.Edges.Count == 0)) {
                    Vector2 prevDirection = contour.Edges[contour.Edges.Count - 1].GetDirection(1);
                    
                    for (int j = 0; j < contour.Edges.Count; j++) {
                        EdgeSegment edge = contour.Edges[j];
                        if (IsCorner(Vector2.Normalize(prevDirection), Vector2.Normalize(edge.GetDirection(0)), crossThreshold)) {
                            corners.Add(j);
                        }
                        prevDirection = edge.GetDirection(1);
                    }
                }

                if (corners.Count == 0) {
                    for (int j = 0; j < contour.Edges.Count; j++) {
                        contour.Edges[j].Color = EdgeColor.White;
                    }
                } else if (corners.Count == 1) {
                    EdgeColor[] colors = { EdgeColor.White, EdgeColor.White, EdgeColor.Black };
                    SwitchColor(ref colors[0], ref seed, EdgeColor.Black);
                    SwitchColor(ref colors[2], ref seed, EdgeColor.Black);

                    int corner = corners[0];

                    if (contour.Edges.Count >= 3) {
                        int m = contour.Edges.Count;
                        for (int j = 0; j < m; j++) {
                            int magic = (int)(3 + 2.875f * j / (m - 1) - 1.4375f + 0.5f) - 3;   //see edge-coloring.cpp in the original msdfgen
                            contour.Edges[(corner + j) % m].Color = colors[1 + magic];
                        }
                    } else if (contour.Edges.Count >= 1) {
                        EdgeSegment[] parts = new EdgeSegment[7];
                        contour.Edges[0].SplitInThirds(out parts[0 + 3 * corner], out parts[1 + 3 * corner], out parts[2 + 3 * corner]);
                        if (contour.Edges.Count >= 2) {
                            contour.Edges[1].SplitInThirds(out parts[3 - 3 * corner], out parts[4 - 3 * corner], out parts[5 - 3 * corner]);
                            parts[0].Color = colors[0];
                            parts[1].Color = colors[0];
                            parts[2].Color = colors[1];
                            parts[3].Color = colors[1];
                            parts[4].Color = colors[2];
                            parts[5].Color = colors[2];
                        } else {
                            parts[0].Color = colors[0];
                            parts[1].Color = colors[1];
                            parts[2].Color = colors[2];
                        }
                        contour.Edges.Clear();
                        for (int j = 0; parts[j] != null; j++) {
                            contour.Edges.Add(parts[j]);
                        }
                    }
                } else {
                    int cornerCount = corners.Count;
                    int spline = 0;
                    int start = corners[0];
                    int m = contour.Edges.Count;
                    EdgeColor color = EdgeColor.White;
                    SwitchColor(ref color, ref seed, EdgeColor.Black);
                    EdgeColor initialColor = color;
                    for (int j = 0; j < m; j++) {
                        int index = (start + j) % m;
                        if (spline + 1 < cornerCount && corners[spline + 1] == index) {
                            spline++;
                            SwitchColor(ref color, ref seed, (EdgeColor)(((spline == cornerCount - 1) ? 1 : 0) * (int)initialColor));
                        }
                        contour.Edges[index].Color = color;
                    }
                }
            }
        }
    }
}
