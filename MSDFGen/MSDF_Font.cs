using System;
using System.Numerics;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

using SharpFont;

namespace MSDFGen {
    public static partial class MSDF {
        struct Context {
            public Vector2 position;
            public Shape shape;
            public Contour contour;
        }

        static Vector2 ToVector2(FTVector vector) {
            return new Vector2(vector.X.Value / 64f, vector.Y.Value / 64f);
        }

        static int MoveTo(ref FTVector to, IntPtr user) {
            unsafe
            {
                Context context = Unsafe.Read<Context>((void*)user);
                Contour contour = new Contour();
                context.shape.Contours.Add(contour);
                context.contour = contour;
                context.position = ToVector2(to);
                Unsafe.Write((void*)user, context);
                return 0;
            }
        }

        static int LineTo(ref FTVector to, IntPtr user) {
            unsafe
            {
                Context context = Unsafe.Read<Context>((void*)user);
                context.contour.Edges.Add(new LinearSegment(context.position, ToVector2(to), EdgeColor.White));
                context.position = ToVector2(to);
                Unsafe.Write((void*)user, context);
                return 0;
            }
        }

        static int ConicTo(ref FTVector control, ref FTVector to, IntPtr user) {
            unsafe
            {
                Context context = Unsafe.Read<Context>((void*)user);
                context.contour.Edges.Add(new QuadraticSegment(context.position, ToVector2(control), ToVector2(to), EdgeColor.White));
                context.position = ToVector2(to);
                Unsafe.Write((void*)user, context);
                return 0;
            }
        }

        static int CubicTo(ref FTVector control1, ref FTVector control2, ref FTVector to, IntPtr user) {
            unsafe
            {
                Context context = Unsafe.Read<Context>((void*)user);
                context.contour.Edges.Add(new CubicSegment(context.position, ToVector2(control1), ToVector2(control2), ToVector2(to), EdgeColor.White));
                context.position = ToVector2(to);
                Unsafe.Write((void*)user, context);
                return 0;
            }
        }

        public static Shape LoadGlyph(Face face, int unicode) {
            if (face == null) throw new ArgumentNullException(nameof(face));

            face.LoadChar((uint)unicode, LoadFlags.NoScale, LoadTarget.Normal);

            Shape output = new Shape();

            Context context = new Context {
                shape = output
            };

            OutlineFuncs funcs = new OutlineFuncs();
            funcs.MoveFunction = MoveTo;
            funcs.LineFunction = LineTo;
            funcs.ConicFunction = ConicTo;
            funcs.CubicFunction = CubicTo;
            unsafe
            {
                face.Glyph.Outline.Decompose(funcs, (IntPtr)Unsafe.AsPointer(ref context));
            }
            return output;
        }
    }
}
