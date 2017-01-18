using System;
using System.Collections.Generic;

using CSGL.GLFW.Unmanaged;

namespace CSGL.GLFW {
    public class Window : IDisposable, INative<WindowPtr> {
        bool disposed;
        WindowPtr window;

        string title;
        int x;
        int y;
        int width;
        int height;
        int framebufferWidth;
        int framebufferHeight;

        bool focused;
        bool iconified;
        bool maximized;
        bool resizeable;
        bool decorated;
        bool floating;

        bool shouldClose;

        public WindowPtr Native {
            get {
                return window;
            }
        }

        public bool Focused {
            get {
                return focused;
            }
        }

        public bool Iconified {
            get {
                return iconified;
            }
            set {
                if (value) {
                    GLFW.IconifyWindow(window);
                } else {
                    if (iconified) GLFW.RestoreWindow(window);
                }
                iconified = value;
            }
        }

        public bool Maximized {
            get {
                return maximized;
            }
            set {
                if (value) {
                    GLFW.MaximizeWindow(window);
                } else {
                    if (maximized) GLFW.RestoreWindow(window);
                }
                maximized = value;
            }
        }

        public bool UserResizable {
            get {
                return resizeable;
            }
        }

        public bool Decorated {
            get {
                return decorated;
            }
        }

        public bool Floating {
            get {
                return floating;
            }
        }

        public bool ShouldClose {
            get {
                return shouldClose;
            }
            set {
                GLFW.SetWindowShouldClose(window, value);
                shouldClose = value;
            }
        }

        public string Title {
            get {
                return title;
            }
            set {
                GLFW.SetWindowTitle(window, value);
            }
        }

        public int X {
            get {
                return x;
            }
        }

        public int Y {
            get {
                return y;
            }
        }

        public int Width {
            get {
                return width;
            }
        }

        public int Height {
            get {
                return height;
            }
        }

        public int FramebufferWidth {
            get {
                return framebufferWidth;
            }
        }

        public int FramebufferHeight {
            get {
                return framebufferHeight;
            }
        }

        public event Action<int, int> OnPositionChanged = delegate { };
        public event Action<int, int> OnSizeChanged = delegate { };
        public event Action OnClose = delegate { };
        public event Action OnRefresh = delegate { };
        public event Action<bool> OnFocus = delegate { };
        public event Action<bool> OnIconify = delegate { };
        public event Action<int, int> OnFramebufferChanged = delegate { };

        public Window(int width, int height, string title, Monitor monitor = null, Window share = null) {
            var monitorNative = MonitorPtr.Null;
            if (monitor != null) monitorNative = monitor.Native;

            var windowNative = WindowPtr.Null;
            if (share != null) windowNative = share.Native;

            window = GLFW.CreateWindow(width, height, title, monitorNative, windowNative);

            this.title = title;

            GLFW.GetWindowPos(window, out x, out y);
            GLFW.GetWindowSize(window, out width, out height);
            GLFW.GetFramebufferSize(window, out framebufferWidth, out framebufferHeight);

            GLFW.SetWindowPosCallback(window, Pos);
            GLFW.SetWindowSizeCallback(window, Size);
            GLFW.SetWindowCloseCallback(window, Close);
            GLFW.SetWindowRefreshCallback(window, Refresh);
            GLFW.SetWindowFocusCallback(window, Focus);
            GLFW.SetWindowIconifyCallback(window, Iconify);
            GLFW.SetFramebufferSizeCallback(window, Framebuffer);

            focused = GLFW.GetWindowAttribute(window, WindowAttribute.Focused) != 0;
            iconified = GLFW.GetWindowAttribute(window, WindowAttribute.Iconified) != 0;
            maximized = GLFW.GetWindowAttribute(window, WindowAttribute.Maximized) != 0;
            resizeable = GLFW.GetWindowAttribute(window, WindowAttribute.Resizable) != 0;
            decorated = GLFW.GetWindowAttribute(window, WindowAttribute.Decorated) != 0;
            floating = GLFW.GetWindowAttribute(window, WindowAttribute.Floating) != 0;
        }

        public void Focus() {
            GLFW.FocusWindow(window);
        }

        void Pos(WindowPtr window, int x, int y) {
            this.x = x;
            this.y = y;
            OnPositionChanged(x, y);
        }

        void Size(WindowPtr window, int width, int height) {
            this.width = width;
            this.height = height;
            OnSizeChanged(width, height);
        }

        void Close(WindowPtr window) {
            OnClose();
        }

        void Refresh(WindowPtr window) {
            OnRefresh();
        }

        void Focus(WindowPtr window, bool focused) {
            this.focused = focused;
            OnFocus(focused);
        }

        void Iconify(WindowPtr window, bool iconified) {
            this.iconified = iconified;
            OnIconify(iconified);
        }

        void Framebuffer(WindowPtr window, int width, int height) {
            framebufferWidth = width;
            framebufferHeight = height;
            OnFramebufferChanged(width, height);
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing) {
            if (disposed) return;

            GLFW.DestroyWindow(window);

            disposed = true;
        }

        ~Window() {
            Dispose(false);
        }
    }
}
