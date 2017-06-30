using System;

using CSGL.Graphics;
using CSGL.GLFW.Unmanaged;

namespace CSGL.GLFW {
    public class Cursor : IDisposable, INative<CursorPtr> {
        CursorPtr cursor;
        bool disposed;

        public CursorPtr Native {
            get {
                return cursor;
            }
        }

        public Cursor(Bitmap<Color4b> image, int xHotspot, int yHotspot) {
            cursor = GLFW.CreateCursor(image, xHotspot, yHotspot);
        }

        public Cursor(CursorShape shape) {
            cursor = GLFW.CreateStandardCursor(shape);
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        void Dispose(bool disposing) {
            if (disposed) return;

            GLFW.DestroyCursor(cursor);

            disposed = true;
        }

        ~Cursor() {
            Dispose(false);
        }
    }
}
