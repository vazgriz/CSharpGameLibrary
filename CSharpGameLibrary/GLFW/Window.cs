using System;
using System.Collections.Generic;

using CSGL.GLFW.Unmanaged;
using UGLFW = CSGL.GLFW.Unmanaged.GLFW;

namespace CSGL.GLFW {
    public class Window : INative<WindowPtr> {
        WindowPtr window;

        public WindowPtr Native {
            get {
                return window;
            }
        }

        public Window(int width, int height, string title, Monitor monitor = null, Window share = null) {
            var monitorNative = MonitorPtr.Null;
            if (monitor != null) monitorNative = monitor.Native;

            var windowNative = WindowPtr.Null;
            if (share != null) windowNative = share.Native;

            window = UGLFW.CreateWindow(width, height, title, monitorNative, windowNative);
        }
    }
}
