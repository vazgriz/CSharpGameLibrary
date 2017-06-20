using System;
using System.Collections.Generic;

using CSGL.Input;
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

        bool stickyKeys;
        CursorMode cursorMode;

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

        public bool StickyKeys {
            get {
                return stickyKeys;
            }
            set {
                stickyKeys = value;
                GLFW.SetInputMode(window, InputMode.StickyKeys, stickyKeys ? 1 : 0);
            }
        }

        public CursorMode CursorMode {
            get {
                return cursorMode;
            }
            set {
                cursorMode = value;
                GLFW.SetInputMode(window, InputMode.Cursor, (int)value);
            }
        }

        public event Action<int, int> OnPositionChanged = delegate { };
        public event Action<int, int> OnSizeChanged = delegate { };
        public event Action OnClose = delegate { };
        public event Action OnRefresh = delegate { };
        public event Action<bool> OnFocus = delegate { };
        public event Action<bool> OnIconify = delegate { };
        public event Action<int, int> OnFramebufferChanged = delegate { };

        public event Action<KeyCode, int, KeyAction, KeyMod> OnKey = delegate { };
        public event Action<uint> OnText = delegate { };
        public event Action<uint, KeyMod> OnTextMod = delegate { };

        public event Action<double, double> OnCursorPos = delegate { };
        public event Action<MouseButton, KeyAction, KeyMod> OnMouseButton = delegate { };
        public event Action<double, double> OnScroll = delegate { };

        public event Action<string[]> OnPathDrop = delegate { };

        protected Window() {
            //empty constructor so derived classes can manually call CreateWindow
        }

        public Window(int width, int height, string title, Monitor monitor = null, Window share = null) {
            CreateWindow(width, height, title, monitor, share);
        }

        protected void CreateWindow(int width, int height, string title, Monitor monitor, Window share) {
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

            GLFW.SetKeyCallback(window, Key);
            GLFW.SetCharModsCallback(window, Text);
            GLFW.SetCursorPosCallback(window, CursorPos);
            GLFW.SetMouseButtonCallback(window, MouseButton);
            GLFW.SetScrollCallback(window, Scroll);

            stickyKeys = GLFW.GetInputMode(window, InputMode.StickyKeys) != 0;
            cursorMode = (CursorMode)GLFW.GetInputMode(window, InputMode.Cursor);
        }

        public string GetClipboard() {
            return GLFW.GetClipboardString(window);
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
            shouldClose = true;
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

        void Key(WindowPtr window, KeyCode code, int scancode, KeyAction action, KeyMod modifiers) {
            OnKey(code, scancode, action, modifiers);
        }

        void Text(WindowPtr window, uint utf32, KeyMod modifiers) {
            OnText(utf32);
            OnTextMod(utf32, modifiers);
        }

        void CursorPos(WindowPtr window, double xPos, double yPos) {
            OnCursorPos(xPos, yPos);
        }

        void MouseButton(WindowPtr window, MouseButton button, KeyAction action, KeyMod mod) {
            OnMouseButton(button, action, mod);
        }

        void Scroll(WindowPtr window, double x, double y) {
            OnScroll(x, y);
        }

        void PathDrop(WindowPtr window, int count, IntPtr stringArray) {
            string[] result = new string[count];

            unsafe
            {
                byte** paths = (byte**)stringArray;

                for (int i = 0; i < count; i++) {
                    result[i] = Interop.GetString(paths[i]);
                }
            }

            OnPathDrop(result);
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
