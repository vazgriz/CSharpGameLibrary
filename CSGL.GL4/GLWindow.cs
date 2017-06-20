using System;
using System.Collections.Generic;

using CSGL.GLFW;
using CSGL.GLFW.Unmanaged;

namespace CSGL.GL4 {
    public class GLWindow : Window {
        static Dictionary<WindowPtr, GLWindow> windowMap = new Dictionary<WindowPtr, GLWindow>();

        public static GLWindow GetCurrentContext() {
            lock (windowMap) {
                WindowPtr ptr = GLFW.GLFW.GetCurrentContext();

                //current context may not have been created with this class, return null in that case
                if (windowMap.ContainsKey(ptr)) {
                    return windowMap[GLFW.GLFW.GetCurrentContext()];
                } else {
                    return null;
                }
            }
        }

        public GLWindow(int width, int height, string title, Monitor monitor = null, Window share = null) {
            lock (windowMap) {
                CreateWindow(width, height, title, monitor, share);
                windowMap.Add(Native, this);
            }
        }

        public void MakeContextCurrent() {
            GLFW.GLFW.MakeContextCurrent(Native);
        }

        public void SwapBuffers() {
            GLFW.GLFW.SwapBuffers(Native);
        }

        public new void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected override void Dispose(bool disposing) {
            lock (windowMap) {
                windowMap.Remove(Native);
            }

            base.Dispose(disposing);
        }
    }
}
