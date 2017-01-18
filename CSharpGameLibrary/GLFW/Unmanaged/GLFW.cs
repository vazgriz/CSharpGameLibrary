using System;
using System.Collections.Generic;

using CSGL.Input;
using static CSGL.GLFW.Unmanaged.GLFW_native;

namespace CSGL.GLFW.Unmanaged {
    public static class GLFW {
        [ThreadStatic]
        static Exception exception;
        
        static ErrorCallback errorCallback;
        static MonitorConnectionCallback monitorConnection;
        static JoystickConnectionCallback joystickConnection;

        class WindowCallbacks {
            public WindowPositionCallback windowPosition;   //need to store these delegates
            public WindowSizeCallback windowSize;           //otherwise the garbage collector will clean them up while the unmanaged code holds a pointer to them
            public WindowCloseCallback windowClose;
            public WindowRefreshCallback windowRefresh;
            public WindowFocusCallback windowFocus;
            public WindowIconifyCallback windowIconify;
            public FramebufferSizeCallback framebufferSize;
            public MouseButtonCallback mouseButton;
            public CursorPosCallback cursorPos;
            public CursorEnterCallback cursorEnter;
            public ScrollCallback scroll;
            public KeyCallback key;
            public CharCallback _char;
            public CharModsCallback charMods;
            public FileDropCallback fileDrop;
        }

        static Dictionary<WindowPtr, WindowCallbacks> callbackMap;

        static GLFW() {
            callbackMap = new Dictionary<WindowPtr, WindowCallbacks>();
        }

        static WindowCallbacks GetCallbacks(WindowPtr window) {
            if (!callbackMap.ContainsKey(window)) {
                callbackMap.Add(window, new WindowCallbacks());
            }
            return callbackMap[window];
        }

        public static bool Init() {
            return glfwInit();
        }

        public static void Terminate() {
            glfwTerminate();
        }

        public static ErrorCallback SetErrorCallback(ErrorCallback callback) {
            var old = errorCallback;
            errorCallback = callback;
            glfwSetErrorCallback(callback);
            return old;
        }

        public static void GetVersion(out int major, out int minor, out int revision) {
            glfwGetVersion(out major, out minor, out revision);
        }

        public static string GetVersion() {
            unsafe
            {
                return Interop.GetString(glfwGetVersionString());
            }
        }

        public static MonitorPtr[] GetMonitors() {
            unsafe
            {
                int count;
                MonitorPtr* ptr = glfwGetMonitors(out count);
                MonitorPtr[] result = new MonitorPtr[count];

                for (int i = 0; i < count; i++) {
                    result[i] = ptr[i];
                }

                return result;
            }
        }

        public static MonitorPtr GetPrimaryMonitor() {
            return glfwGetPrimaryMonitor();
        }

        public static void GetMonitorPos(MonitorPtr monitor, out int x, out int y) {
            glfwGetMonitorPos(monitor, out x, out y);
        }

        public static void GetMonitorPhysicalSize(MonitorPtr monitor, out int width, out int height) {
            glfwGetMonitorPhysicalSize(monitor, out width, out height);
        }

        public static string GetMonitorName(MonitorPtr monitor) {
            unsafe
            {
                var s = glfwGetMonitorName(monitor);
                return Interop.GetString(s);
            }
        }

        public static MonitorConnectionCallback SetMonitorCallback(MonitorConnectionCallback callback) {
            var old = monitorConnection;
            monitorConnection = callback;
            glfwSetMonitorCallback(callback);
            return old;
        }

        public static VideoMode[] GetVideoModes(MonitorPtr monitor) {
            unsafe
            {
                int count;
                VideoMode* ptr = glfwGetVideoModes(monitor, out count);
                VideoMode[] result = new VideoMode[count];

                for (int i = 0; i < count; i++) {
                    result[i] = ptr[i];
                }

                return result;
            }
        }

        public static VideoMode GetVideoMode(MonitorPtr monitor) {
            unsafe
            {
                return *glfwGetVideoMode(monitor);
            }
        }

        public static void SetGamma(MonitorPtr monitor, float gamma) {
            glfwSetGamma(monitor, gamma);
        }

        public static GammaRamp GetGammaRamp(MonitorPtr monitor) {
            unsafe
            {
                NativeGammaRamp ngr = *glfwGetGammaRamp(monitor);
                return new GammaRamp(ngr);
            }
        }

        public static void SetGammaRamp(MonitorPtr monitor, GammaRamp ramp) {
            unsafe
            {
                fixed (ushort* r = &ramp.red[0])
                fixed (ushort* g = &ramp.green[0])
                fixed (ushort* b = &ramp.blue[0]) {
                    NativeGammaRamp ngr = new NativeGammaRamp(r, g, b, ramp.size);
                    NativeGammaRamp* ptr = &ngr;
                    glfwSetGammaRamp(monitor, ptr);
                }
            }
        }

        public static void DefaultWindowHints() {
            glfwDefaultWindowHints();
        }

        public static void WindowHint(WindowHint hint, int value) {
            glfwWindowHint((int)hint, value);
        }

        public static WindowPtr CreateWindow(int width, int height, string title, MonitorPtr monitor, WindowPtr share) {
            var result = glfwCreateWindow(width, height, title, monitor, share);

            GetCallbacks(result);
            return result;
        }

        public static void DestroyWindow(WindowPtr window) {
            glfwDestroyWindow(window);
            callbackMap.Remove(window);
        }

        public static bool WindowShouldClose(WindowPtr window) {
            var result = glfwWindowShouldClose(window);
            return result;
        }

        public static void SetWindowShouldClose(WindowPtr window, bool value) {
            glfwSetWindowShouldClose(window, value);
        }

        public static void SetWindowTitle(WindowPtr window, string title) {
            glfwSetWindowTitle(window, title);
        }

        public static void SetWindowIcon(WindowPtr window, Image[] images) {
            unsafe
            {
                if (images == null || images.Length == 0) {
                    glfwSetWindowIcon(window, 0, null);
                    return;
                }

                NativeImage[] nimgs = new NativeImage[images.Length];
                fixed (NativeImage* ptr = &nimgs[0]) {
                    for (int i = 0; i < images.Length; i++) {
                        fixed (byte* data = &images[i].data[0]) {
                            nimgs[i] = new NativeImage(images[i].width, images[i].height, data);
                        }
                    }
                    glfwSetWindowIcon(window, images.Length, ptr);
                }
            }
        }

        public static void GetWindowPos(WindowPtr window, out int x, out int y) {
            glfwGetWindowPos(window, out x, out y);
        }

        public static void SetWindowPos(WindowPtr window, int x, int y) {
            glfwSetWindowPos(window, x, y);
        }

        public static void GetWindowSize(WindowPtr window, out int width, out int height) {
            glfwGetWindowSize(window, out width, out height);
        }

        public static void SetWindowSizeLimits(WindowPtr window,
            int minWidth, int minHeight,
            int maxWidth, int maxHeight) {
            glfwSetWindowSizeLimits(window, minWidth, minHeight, maxWidth, maxHeight);
        }

        public static void SetWindowAspectRatio(WindowPtr window, int numerator, int denominator) {
            glfwSetWindowAspectRatio(window, numerator, denominator);
        }

        public static void SetWindowSize(WindowPtr window, int width, int height) {
            glfwSetWindowSize(window, width, height);
        }

        public static void GetFramebufferSize(WindowPtr window, out int width, out int height) {
            glfwGetFramebufferSize(window, out width, out height);
        }

        public static void GetWindowFrameSize(WindowPtr window,
            out int left, out int top, out int right, out int bottom) {
            glfwGetWindowFrameSize(window, out left, out top, out right, out bottom);
        }

        public static void IconifyWindow(WindowPtr window) {
            glfwIconifyWindow(window);
        }

        public static void RestoreWindow(WindowPtr window) {
            glfwRestoreWindow(window);
        }

        public static void MaximizeWindow(WindowPtr window) {
            glfwMaximizeWindow(window);
        }

        public static void ShowWindow(WindowPtr window) {
            glfwShowWindow(window);
        }

        public static void HideWindow(WindowPtr window) {
            glfwHideWindow(window);
        }

        public static void FocusWindow(WindowPtr window) {
            glfwFocusWindow(window);
        }

        public static MonitorPtr GetWindowMonitor(WindowPtr window) {
            return glfwGetWindowMonitor(window);
        }

        public static void SetWindowMonitor(WindowPtr window, MonitorPtr monitor,
            int x, int y,
            int width, int height,
            int refreshRate) {
            glfwSetWindowMonitor(window, monitor, x, y, width, height, refreshRate);
        }

        public static int GetWindowAttribute(WindowPtr window, WindowAttribute attrib) {
            return glfwGetWindowAttrib(window, attrib);
        }

        public static WindowPositionCallback SetWindowPosCallback(WindowPtr window, WindowPositionCallback callback) {
            var callbacks = GetCallbacks(window);
            var old = callbacks.windowPosition;
            callbacks.windowPosition = callback;
            glfwSetWindowPosCallback(window, callback);
            return old;
        }

        public static WindowSizeCallback SetWindowSizeCallback(WindowPtr window, WindowSizeCallback callback) {
            var callbacks = GetCallbacks(window);
            var old = callbacks.windowSize;
            callbacks.windowSize = callback;
            glfwSetWindowSizeCallback(window, callback);
            return old;
        }

        public static WindowCloseCallback SetWindowCloseCallback(WindowPtr window, WindowCloseCallback callback) {
            var callbacks = GetCallbacks(window);
            var old = callbacks.windowClose;
            callbacks.windowClose = callback;
            glfwSetWindowCloseCallback(window, callback);
            return old;
        }

        public static WindowRefreshCallback SetWindowRefreshCallback(WindowPtr window, WindowRefreshCallback callback) {
            var callbacks = GetCallbacks(window);
            var old = callbacks.windowRefresh;
            callbacks.windowRefresh = callback;
            glfwSetWindowRefreshCallback(window, callback);
            return old;
        }

        public static WindowFocusCallback SetWindowFocusCallback(WindowPtr window, WindowFocusCallback callback) {
            var callbacks = GetCallbacks(window);
            var old = callbacks.windowFocus;
            callbacks.windowFocus = callback;
            glfwSetWindowFocusCallback(window, callback);
            return old;
        }

        public static WindowIconifyCallback SetWindowIconifyCallback(WindowPtr window, WindowIconifyCallback callback) {
            var callbacks = GetCallbacks(window);
            var old = callbacks.windowIconify;
            callbacks.windowIconify = callback;
            glfwSetWindowIconifyCallback(window, callback);
            return old;
        }

        public static FramebufferSizeCallback SetFramebufferSizeCallback(WindowPtr window, FramebufferSizeCallback callback) {
            var callbacks = GetCallbacks(window);
            var old = callbacks.framebufferSize;
            callbacks.framebufferSize = callback;
            glfwSetFramebufferSizeCallback(window, callback);
            return old;
        }

        public static void PollEvents() {
            glfwPollEvents();
        }

        public static void WaitEvents() {
            glfwWaitEvents();
        }

        public static void WaitEventsTimeout(double timeout) {
            glfwWaitEventsTimeout(timeout);
        }

        public static int GetInputMode(WindowPtr window, int mode) {
            return glfwGetInputMode(window, mode);
        }

        public static void SetInputMode(WindowPtr window, int mode, int value) {
            glfwSetInputMode(window, mode, value);
        }

        public static KeyAction GetKey(WindowPtr window, KeyCode key) {
            return glfwGetKey(window, key);
        }

        public static KeyAction GetMouseButton(WindowPtr window, MouseButton button) {
            return glfwGetMouseButton(window, button);
        }

        public static void GetCursorPos(WindowPtr window, out double x, out double y) {
            glfwGetCursorPos(window, out x, out y);
        }

        public static void SetCursorPos(WindowPtr window, double x, double y) {
            glfwSetCursorPos(window, x, y);
        }

        public static Cursor CreateCursor(Image image, int xHotspot, int yHotspot) {
            unsafe
            {
                fixed (byte* data = image.data) {
                    NativeImage nimg = new NativeImage(image.width, image.height, data);
                    NativeImage* ptr = &nimg;
                    return glfwCreateCursor(ptr, xHotspot, yHotspot);
                }
            }
        }

        public static Cursor CreateCursor(CursorShape shape) {
            var result = glfwCreateStandardCursor(shape);
            return result;
        }

        public static void DestroyCursor(Cursor cursor) {
            glfwDestroyCursor(cursor);
        }

        public static void SetCursor(WindowPtr window, Cursor cursor) {
            glfwSetCursor(window, cursor);
        }

        public static KeyCallback SetKeyCallback(WindowPtr window, KeyCallback callback) {
            var callbacks = GetCallbacks(window);
            var old = callbacks.key;
            callbacks.key = callback;
            glfwSetKeyCallback(window, callback);
            return old;
        }

        public static CharCallback SetCharCallback(WindowPtr window, CharCallback callback) {
            var callbacks = GetCallbacks(window);
            var old = callbacks._char;
            callbacks._char = callback;
            glfwSetCharCallback(window, callback);
            return old;
        }

        public static CharModsCallback SetCharModsCallback(WindowPtr window, CharModsCallback callback) {
            var callbacks = GetCallbacks(window);
            var old = callbacks.charMods;
            callbacks.charMods = callback;
            glfwSetCharModsCallback(window, callback);
            return old;
        }

        public static MouseButtonCallback SetMouseButtonCallback(WindowPtr window, MouseButtonCallback callback) {
            var callbacks = GetCallbacks(window);
            var old = callbacks.mouseButton;
            callbacks.mouseButton = callback;
            glfwSetMouseButtonCallback(window, callback);
            return old;
        }

        public static CursorPosCallback SetCursorPosCallback(WindowPtr window, CursorPosCallback callback) {
            var callbacks = GetCallbacks(window);
            var old = callbacks.cursorPos;
            callbacks.cursorPos = callback;
            glfwSetCursorPosCallback(window, callback);
            return old;
        }

        public static CursorEnterCallback SetCursorEnterCallback(WindowPtr window, CursorEnterCallback callback) {
            var callbacks = GetCallbacks(window);
            var old = callbacks.cursorEnter;
            callbacks.cursorEnter = callback;
            glfwSetCursorEnterCallback(window, callback);
            return old;
        }

        public static ScrollCallback SetScrollCallback(WindowPtr window, ScrollCallback callback) {
            var callbacks = GetCallbacks(window);
            var old = callbacks.scroll;
            callbacks.scroll = callback;
            glfwSetScrollCallback(window, callback);
            return old;
        }

        public static FileDropCallback SetDropCallback(WindowPtr window, FileDropCallback callback) {
            var callbacks = GetCallbacks(window);
            var old = callbacks.fileDrop;
            callbacks.fileDrop = callback;
            glfwSetDropCallback(window, callback);
            return old;
        }

        public static bool JoystickPresent(int joystick) {
            return glfwJoystickPresent(joystick);
        }

        public static float[] GetJoystickAxes(int joystick) {
            unsafe
            {
                int count;
                float* ptr = glfwGetJoystickAxes(joystick, out count);
                float[] axes = new float[count];
                for (int i = 0; i < count; i++) {
                    axes[i] = ptr[i];
                }
                return axes;
            }
        }

        public static bool[] GetJoystickButtons(int joystick) {
            unsafe
            {
                int count;
                bool* ptr = glfwGetJoystickButtons(joystick, out count);
                bool[] buttons = new bool[count];
                for (int i = 0; i < count; i++) {
                    buttons[i] = ptr[i];
                }
                return buttons;
            }
        }

        public static string GetJoystickName(int joystick) {
            unsafe
            {
                var s = glfwGetJoystickName(joystick);
                return Interop.GetString(s);
            }
        }

        public static JoystickConnectionCallback SetJoystickConnectionCallback(WindowPtr window, JoystickConnectionCallback callback) {
            var old = joystickConnection;
            joystickConnection = callback;
            glfwSetJoystickCallback(window, callback);
            return old;
        }

        public static void SetClipboardString(WindowPtr window, string s) {
            glfwSetClipboardString(window, s);
        }

        public static string GetClipboardString(WindowPtr window) {
            unsafe
            {
                var s = glfwGetClipboardString(window);
                return Interop.GetString(s);
            }
        }

        public static double GetTime() {
            return glfwGetTime();
        }

        public static void SetTime(double time) {
            glfwSetTime(time);
        }

        public static ulong GetTimerValue() {
            return glfwGetTimerValue();
        }

        public static ulong GetTimerFrequency() {
            return glfwGetTimerFrequency();
        }

        public static void MakeContextCurrent(WindowPtr window) {
            glfwMakeContextCurrent(window);
        }

        public static WindowPtr GetCurrentContext() {
            return glfwGetCurrentContext();
        }

        public static void SwapBuffers(WindowPtr window) {
            glfwSwapBuffers(window);
        }

        public static void SwapInterval(int interval) {
            glfwSwapInterval(interval);
        }

        public static bool ExtensionSupported(string extension) {
            return glfwExtensionSupported(extension);
        }

        public static IntPtr GetProcAddress(string procName) {
            return glfwGetProcAddress(procName);
        }
    }
}
