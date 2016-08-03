using System;
using System.Runtime.InteropServices;

using CSGL.Graphics;
using static CSGL.NanoVG.NanoVG_native;

namespace CSGL.NanoVG {
    public class Paint {
        float[] xform;
        float[] extent;

        public float[] XForm {
            get {
                return xform;
            }
        }

        public float[] Extent {
            get {
                return extent;
            }
        }

        public float Radius { get; set; }
        public float Feather { get; set; }
        public Color InnerColor { get; set; }
        public Color OuterColor { get; set; }
        public int Image { get; set; }

        public Paint() {
            xform = new float[6];
            extent = new float[2];
        }

        internal void Set(PaintNative paint) {
            unsafe
            {
                for (int i = 0; i < 6; i++) {
                    float* ptr = paint.xform;
                    XForm[i] = ptr[i];
                }
                for (int i = 0; i < 2; i++) {
                    float* ptr = paint.extent;
                    Extent[i] = ptr[i];
                }
            }
            Radius = paint.radius;
            Feather = paint.feather;
            InnerColor = paint.innerColor;
            OuterColor = paint.outerColor;
            Image = paint.image;
        }

        public void LinearGradient(Context ctx, float startX, float startY, float endX, float endY, Color startColor, Color endColor) {
            Set(nvgLinearGradient(ctx.ctx, startX, startY, endX, endY, startColor, endColor));
            InnerColor = startColor;
            OuterColor = endColor;
        }

        public void BoxGradient(Context ctx, float x, float y, float width, float height,
            float radius, float feather, Color innerColor, Color outerColor) {

            Set(nvgBoxGradient(ctx.ctx, x, y, width, height, radius, feather, innerColor, outerColor));
            InnerColor = innerColor;
            OuterColor = outerColor;
        }

        public void RadialGradient(Context ctx, float x, float y, float innerRadius, float outerRadius, Color innerColor, Color outerColor) {
            Set(nvgRadialGradient(ctx.ctx, x, y, innerRadius, outerRadius, innerColor, outerColor));
            InnerColor = innerColor;
            OuterColor = outerColor;
        }

        public void ImagePattern(Context ctx, float x, float y, float width, float height,
            float angle, int imageHandle, float alpha) {

            Set(nvgImagePattern(ctx.ctx, x, y, width, height, angle, imageHandle, alpha));
        }

        public void ImagePattern(Context ctx, float x, float y, Image image,
            float angle, float alpha) {

            ImagePattern(ctx, x, y, image.Width, image.Height, angle, image.ID, alpha);
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal unsafe struct PaintNative {
        public fixed float xform[6];
        public fixed float extent[2];
        public float radius;
        public float feather;
        public Color innerColor;
        public Color outerColor;
        public int image;

        public PaintNative(Paint paint) {
            fixed (float* ptr = xform)
            {
                for (int i = 0; i < 6; i++) {
                    ptr[i] = paint.XForm[i];
                }
            }
            fixed (float* ptr = extent)
            {
                for (int i = 0; i < 2; i++) {
                    ptr[i] = paint.Extent[i];
                }
            }
            radius = paint.Radius;
            feather = paint.Feather;
            innerColor = paint.InnerColor;
            outerColor = paint.OuterColor;
            image = paint.Image;
        }
    }
}
