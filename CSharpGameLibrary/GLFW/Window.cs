using System;
using System.Collections.Generic;

using CSGL.Graphics;
using CSGL.GLFW.Unmanaged;

namespace CSGL.GLFW {
    public class Window : IDisposable, INative<WindowPtr> {
        bool disposed;
        WindowPtr window;

        Monitor monitor;
        IList<Bitmap<Color4b>> icons;

        string title;

        bool shouldClose;
        bool visible;
        bool stickyKeys;
        CursorMode cursorMode;
        Cursor cursor;

        public WindowPtr Native {
            get {
                return window;
            }
        }

        public bool UserResizable { get; private set; }
        public bool Decorated { get; private set; }
        public bool Floating { get; private set; }

        public int X { get; private set; }
        public int Y { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }
        public int FramebufferWidth { get; private set; }
        public int FramebufferHeight { get; private set; }

        public int MinWidth { get; private set; }
        public int MinHeight { get; private set; }
        public int MaxWidth { get; private set; }
        public int MaxHeight { get; private set; }
        public int AspectNumerator { get; private set; }
        public int AspectDenominator { get; private set; }

        public int FrameLeft { get; private set; }
        public int FrameTop { get; private set; }
        public int FrameRight { get; private set; }
        public int FrameBottom { get; private set; }

        public Monitor Monitor {
            get {
                return Monitor.GetMonitor(GLFW.GetWindowMonitor(window));
            }
        }

        public IList<Bitmap<Color4b>> Icons {
            get {
                return icons;
            }
            set {
                GLFW.SetWindowIcon(window, icons);
                icons = value;
            }
        }

        public bool Focused {
            get {
                return GLFW.GetWindowAttribute(window, WindowAttribute.Focused) != 0;
            }
        }

        public bool Iconified {
            get {
                return GLFW.GetWindowAttribute(window, WindowAttribute.Iconified) != 0;
            }
        }

        public bool Maximized {
            get {
                return GLFW.GetWindowAttribute(window, WindowAttribute.Maximized) != 0;
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
                title = value;
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

        public Cursor Cursor {
            get {
                return cursor;
            }
            set {
                if (value != null) {
                    GLFW.SetCursor(window, value.Native);
                } else {
                    GLFW.SetCursor(window, CursorPtr.Null);
                }
                cursor = value;
            }
        }

        public bool Visible {
            get {
                return visible;
            }
            set {
                if (value) {
                    GLFW.ShowWindow(window);
                } else {
                    GLFW.HideWindow(window);
                }
                visible = value;
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

        public event Action<IList<string>> OnPathDrop = delegate { };

        protected Window() {
            //empty constructor so derived classes can manually call CreateWindow
        }

        public Window(int width, int height, string title, Monitor monitor = null, Window share = null) {
            CreateWindow(width, height, title, monitor, share);
        }

        protected void CreateWindow(int width, int height, string title, Monitor monitor, Window share) {
            var monitorNative = MonitorPtr.Null;
            if (monitor != null) {
                monitorNative = monitor.Native;
                this.monitor = monitor;
            }

            var windowNative = WindowPtr.Null;
            if (share != null) windowNative = share.Native;

            window = GLFW.CreateWindow(width, height, title, monitorNative, windowNative);

            this.title = title;

            int x, y, _width, _height, framebufferWidth, framebufferHeight;
            GLFW.GetWindowPos(window, out x, out y);
            GLFW.GetWindowSize(window, out _width, out _height);
            GLFW.GetFramebufferSize(window, out framebufferWidth, out framebufferHeight);
            X = x;
            Y = y;
            Width = _width;
            Height = _height;
            FramebufferWidth = framebufferWidth;
            FramebufferHeight = framebufferHeight;

            int frameLeft, frameTop, frameRight, frameBottom;
            GLFW.GetWindowFrameSize(window, out frameLeft, out frameTop, out frameRight, out frameBottom);

            GLFW.SetWindowPosCallback(window, Pos);
            GLFW.SetWindowSizeCallback(window, Size);
            GLFW.SetWindowCloseCallback(window, Close);
            GLFW.SetWindowRefreshCallback(window, Refresh);
            GLFW.SetWindowFocusCallback(window, Focus);
            GLFW.SetWindowIconifyCallback(window, Iconify);
            GLFW.SetFramebufferSizeCallback(window, Framebuffer);

            UserResizable = GLFW.GetWindowAttribute(window, WindowAttribute.Resizable) != 0;
            Decorated = GLFW.GetWindowAttribute(window, WindowAttribute.Decorated) != 0;
            Floating = GLFW.GetWindowAttribute(window, WindowAttribute.Floating) != 0;
            visible = GLFW.GetWindowAttribute(window, WindowAttribute.Visible) != 0;

            GLFW.SetKeyCallback(window, Key);
            GLFW.SetCharModsCallback(window, Text);
            GLFW.SetCursorPosCallback(window, CursorPos);
            GLFW.SetMouseButtonCallback(window, MouseButton);
            GLFW.SetScrollCallback(window, Scroll);
            GLFW.SetDropCallback(window, PathDrop);

            stickyKeys = GLFW.GetInputMode(window, InputMode.StickyKeys) != 0;
            cursorMode = (CursorMode)GLFW.GetInputMode(window, InputMode.Cursor);
        }

        public string GetClipboard() {
            return GLFW.GetClipboardString(window);
        }

        public void Focus() {
            GLFW.FocusWindow(window);
        }

        public void Iconify() {
            GLFW.IconifyWindow(window);
        }

        public void Maximize() {
            GLFW.MaximizeWindow(window);
        }

        public void Restore() {
            GLFW.RestoreWindow(window);
        }

        public void SetMonitor(Monitor monitor, int x, int y, int width, int height, int refreshRate) {
            MonitorPtr monitorNative = MonitorPtr.Null;
            if (monitor != null) {
                monitorNative = monitor.Native;
                this.monitor = monitor;
            }

            GLFW.SetWindowMonitor(window, monitorNative, x, y, width, height, refreshRate);
        }

        public void SetSizeLimits(int minWidth, int minHeight, int maxWidth, int maxHeight) {
            MinWidth = minWidth;
            MinHeight = minHeight;
            MaxWidth = maxWidth;
            MaxHeight = maxHeight;
            GLFW.SetWindowSizeLimits(window, minWidth, minHeight, maxWidth, maxHeight);
        }

        public void SetAspectRatio(int numerator, int denominator) {
            AspectNumerator = numerator;
            AspectDenominator = denominator;
            GLFW.SetWindowAspectRatio(window, numerator, denominator);
        }

        public void SetSize(int width, int height) {
            Width = width;
            Height = height;
            GLFW.SetWindowSize(window, width, height);
        }

        public void SetPosition(int x, int y) {
            X = x;
            Y = y;
            GLFW.SetWindowPos(window, x, y);
        }

        void Pos(WindowPtr window, int x, int y) {
            X = x;
            Y = y;
            OnPositionChanged(x, y);
        }

        void Size(WindowPtr window, int width, int height) {
            Width = width;
            Height = height;
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
            OnFocus(focused);
        }

        void Iconify(WindowPtr window, bool iconified) {
            OnIconify(iconified);
        }

        void Framebuffer(WindowPtr window, int width, int height) {
            FramebufferWidth = width;
            FramebufferHeight = height;
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
            IList<string> result = new List<string>(count);

            unsafe
            {
                byte** paths = (byte**)stringArray;

                for (int i = 0; i < count; i++) {
                    result.Add(Interop.GetString(paths[i]));
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
