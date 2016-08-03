using System;
using System.Runtime.InteropServices;

using CSGL.Graphics;

namespace CSGL.NanoVG {
    internal static class NanoVG_native {
        const string lib = "natives";
        [DllImport(lib)]
        internal static extern void nvgInit();
        [DllImport(lib)]
        internal static extern IntPtr nvgCreateContext(int flags);
        [DllImport(lib)]
        internal static extern void nvgDeleteContext(IntPtr ctx);

        [DllImport(lib)]
        internal static extern void nvgBeginFrame(IntPtr ctx, int width, int height, float ratio);
        [DllImport(lib)]
        internal static extern void nvgCancelFrame(IntPtr ctx);
        [DllImport(lib)]
        internal static extern void nvgEndFrame(IntPtr ctx);

        [DllImport(lib)]
        internal static extern void nvgSave(IntPtr ctx);
        [DllImport(lib)]
        internal static extern void nvgRestore(IntPtr ctx);
        [DllImport(lib)]
        internal static extern void nvgReset(IntPtr ctx);

        [DllImport(lib)]
        internal static extern void nvgStrokeColor(IntPtr ctx, Color color);
        [DllImport(lib)]
        internal static extern void nvgStrokePaint(IntPtr ctx, PaintNative paint);
        [DllImport(lib)]
        internal static extern void nvgFillColor(IntPtr ctx, Color color);
        [DllImport(lib)]
        internal static extern void nvgFillPaint(IntPtr ctx, PaintNative paint);
        [DllImport(lib)]
        internal static extern void nvgMiterLimit(IntPtr ctx, float limit);
        [DllImport(lib)]
        internal static extern void nvgStrokeWidth(IntPtr ctx, float width);
        [DllImport(lib)]
        internal static extern void nvgLineCap(IntPtr ctx, int cap);
        [DllImport(lib)]
        internal static extern void nvgLineJoin(IntPtr ctx, int cap);
        [DllImport(lib)]
        internal static extern void nvgGlobalAlpha(IntPtr ctx, float alpha);
        
        [DllImport(lib)]
        internal static extern void nvgResetTransform(IntPtr ctx);
        [DllImport(lib)]
        internal static extern void nvgTransform(IntPtr ctx, float a, float b, float c, float d, float e, float f);
        [DllImport(lib)]
        internal static extern void nvgTranslate(IntPtr ctx, float x, float y);
        [DllImport(lib)]
        internal static extern void nvgRotate(IntPtr ctx, float angle);
        [DllImport(lib)]
        internal static extern void nvgSkewX(IntPtr ctx, float angle);
        [DllImport(lib)]
        internal static extern void nvgSkewY(IntPtr ctx, float angle);
        [DllImport(lib)]
        internal static extern void nvgScale(IntPtr ctx, float x, float y);
        [DllImport(lib)]
        internal unsafe static extern void nvgCurrentTransform(IntPtr ctx, float* xform);
        [DllImport(lib)]
        internal unsafe static extern void nvgTransformPoint(ref float destX, ref float destY, float* xform, ref float srcX, ref float srcY);

        [DllImport(lib)]
        internal static extern int nvgCreateImage(IntPtr ctx, string filename, int imageFlags);
        [DllImport(lib)]
        internal static extern int nvgCreateImageRGBA(IntPtr ctx, int w, int h, int imageFlags, IntPtr data);
        [DllImport(lib)]
        internal static extern void nvgUpdateImage(IntPtr ctx, int image, IntPtr data);
        [DllImport(lib)]
        internal static extern void nvgImageSize(IntPtr ctx, int image, out int w, out int h);
        [DllImport(lib)]
        internal static extern void nvgDeleteImage(IntPtr ctx, int image);

        [DllImport(lib)]
        internal static extern PaintNative nvgLinearGradient(IntPtr ctx, float sx, float sy, float ex, float ey, Color icol, Color ocol);
        [DllImport(lib)]
        internal static extern PaintNative nvgBoxGradient(IntPtr ctx, float x, float y, float w, float h, float r, float f, Color icol, Color ocol);
        [DllImport(lib)]
        internal static extern PaintNative nvgRadialGradient(IntPtr ctx, float cx, float cy, float inr, float outr, Color icol, Color ocol);
        [DllImport(lib)]
        internal static extern PaintNative nvgImagePattern(IntPtr ctx, float ox, float oy, float ex, float ey, float angle, int image, float alpha);

        [DllImport(lib)]
        internal static extern void nvgScissor(IntPtr ctx, float x, float y, float w, float h);
        [DllImport(lib)]
        internal static extern void nvgIntersectScissor(IntPtr ctx, float x, float y, float w, float h);
        [DllImport(lib)]
        internal static extern void nvgResetScissor(IntPtr ctx);

        [DllImport(lib)]
        internal static extern void nvgBeginPath(IntPtr ctx);
        [DllImport(lib)]
        internal static extern void nvgMoveTo(IntPtr ctx, float x, float y);
        [DllImport(lib)]
        internal static extern void nvgLineTo(IntPtr ctx, float x, float y);
        [DllImport(lib)]
        internal static extern void nvgBezierTo(IntPtr ctx, float c1x, float c1y, float c2x, float c2y, float x, float y);
        [DllImport(lib)]
        internal static extern void nvgQuadTo(IntPtr ctx, float cx, float cy, float x, float y);
        [DllImport(lib)]
        internal static extern void nvgArcTo(IntPtr ctx, float x1, float y1, float x2, float y2, float radius);
        [DllImport(lib)]
        internal static extern void nvgClosePath(IntPtr ctx);
        [DllImport(lib)]
        internal static extern void nvgPathWinding(IntPtr ctx, int dir);
        [DllImport(lib)]
        internal static extern void nvgArc(IntPtr ctx, float cx, float cy, float r, float a0, float a1, int dir);
        [DllImport(lib)]
        internal static extern void nvgRect(IntPtr ctx, float x, float y, float w, float h);
        [DllImport(lib)]
        internal static extern void nvgRoundedRect(IntPtr ctx, float x, float y, float w, float h, float r);
        [DllImport(lib)]
        internal static extern void nvgEllipse(IntPtr ctx, float cx, float cy, float rx, float ry);
        [DllImport(lib)]
        internal static extern void nvgCircle(IntPtr ctx, float cx, float cy, float r);
        [DllImport(lib)]
        internal static extern void nvgFill(IntPtr ctx);
        [DllImport(lib)]
        internal static extern void nvgStroke(IntPtr ctx);

        [DllImport(lib)]
        internal static extern int nvgCreateFont(IntPtr ctx, string name, string filename);
        [DllImport(lib)]
        internal static extern int nvgCreateFontMem(IntPtr ctx, string name, IntPtr data, int ndata, int freeData);
        [DllImport(lib)]
        internal static extern int nvgFindFont(IntPtr ctx, string name);
        [DllImport(lib)]
        internal static extern void nvgFontSize(IntPtr ctx, float size);
        [DllImport(lib)]
        internal static extern void nvgFontBlur(IntPtr ctx, float blur);
        [DllImport(lib)]
        internal static extern void nvgTextLetterSpacing(IntPtr ctx, float spacing);
        [DllImport(lib)]
        internal static extern void nvgTextLineHeight(IntPtr ctx, float lineHeight);
        [DllImport(lib)]
        internal static extern void nvgTextAlign(IntPtr ctx, int align);
        [DllImport(lib)]
        internal static extern void nvgFontFaceId(IntPtr ctx, int font);
        [DllImport(lib)]
        internal static extern void nvgFontFace(IntPtr ctx, string name);
        [DllImport(lib)]
        internal static extern float nvgText(IntPtr ctx, float x, float y, string text, IntPtr end);
        [DllImport(lib)]
        internal static extern void nvgTextBox(IntPtr ctx, float x, float y, float breakRowWidth, string text, IntPtr end);
        [DllImport(lib)]
        internal static extern float nvgTextBounds(IntPtr ctx, float x, float y, string text, IntPtr end, IntPtr bounds);
        [DllImport(lib)]
        internal static extern void nvgTextBoxBounds(IntPtr ctx, float x, float y, float breakRowWidth, string text, IntPtr end, IntPtr bounds);
        [DllImport(lib)]
        internal static extern int nvgTextGlyphPositions(IntPtr ctx, float x, float y, string text, IntPtr end, IntPtr positions, int maxPositions);
        [DllImport(lib)]
        internal static extern void nvgTextMetrics(IntPtr ctx, IntPtr ascender, IntPtr descender, IntPtr lineh);
        [DllImport(lib)]
        internal static extern int nvgTextBreakLines(IntPtr ctx, string text, IntPtr end, float breakRowWidth, IntPtr rows, int maxRows);
    }

    public static class NVG {
        public static void Init() {
            NanoVG_native.nvgInit();
        }
    }
}
