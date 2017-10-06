using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using CSGL.GLFW.Unmanaged;
using CSGL.Graphics;
using static CSGL.GLFW.Unmanaged.GLFW_native;

namespace CSGL.GLFW {
    public static class GLFW {
        [ThreadStatic]
        static Exception exception;
        static ErrorCallback errorCallback; //store error callback so it doens't get collected

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
            errorCallback = InternalError;
            glfwSetErrorCallback(errorCallback);    //has to be set here, so that errors made before glfwInit() are caught
            callbackMap = new Dictionary<WindowPtr, WindowCallbacks>();
        }

        static WindowCallbacks GetCallbacks(WindowPtr window) {
            if (!callbackMap.ContainsKey(window)) {
                callbackMap.Add(window, new WindowCallbacks());
            }
            return callbackMap[window];
        }

        public static void CheckError() {      //convert GLFW error to managed exception
            if (exception != null) {    //the error callback creates an exception without throwing it
                var ex = exception;     //and stores it in a thread local variable
                exception = null;       //if that variable is not null when this method is called, an exception is thrown
                throw ex;
            }
        }

        static void InternalError(ErrorCode code, string desc) {
            exception = new GLFWException(code, desc);
        }

        public static bool Init() {
            bool result = glfwInit();
            CheckError();
            Monitor.Init();
            return result;
        }

        public static void Terminate() {
            glfwTerminate();
        }

        public static void GetVersion(out int major, out int minor, out int revision) {
            glfwGetVersion(out major, out minor, out revision);
        }

        public static string GetVersion() {
            unsafe {
                return Interop.GetString(glfwGetVersionString());
            }
        }

        public static IList<MonitorPtr> GetMonitors() {
            unsafe {
                int count;
                MonitorPtr* ptr = glfwGetMonitors(out count);
                CheckError();
                var result = new List<MonitorPtr>(count);

                for (int i = 0; i < count; i++) {
                    result.Add(ptr[i]);
                }

                return result;
            }
        }

        public static MonitorPtr GetPrimaryMonitor() {
            MonitorPtr result = glfwGetPrimaryMonitor();
            CheckError();
            return result;
        }

        public static void GetMonitorPos(MonitorPtr monitor, out int x, out int y) {
            glfwGetMonitorPos(monitor, out x, out y);
            CheckError();
        }

        public static void GetMonitorPhysicalSize(MonitorPtr monitor, out int width, out int height) {
            glfwGetMonitorPhysicalSize(monitor, out width, out height);
            CheckError();
        }

        public static string GetMonitorName(MonitorPtr monitor) {
            unsafe {
                var s = glfwGetMonitorName(monitor);
                CheckError();
                string result = Interop.GetString(s);
                return result;
            }
        }

        public static MonitorConnectionCallback SetMonitorCallback(MonitorConnectionCallback callback) {
            var old = monitorConnection;
            monitorConnection = callback;
            glfwSetMonitorCallback(callback);
            CheckError();
            return old;
        }

        public static IList<VideoMode> GetVideoModes(MonitorPtr monitor) {
            unsafe {
                int count;
                VideoMode* ptr = glfwGetVideoModes(monitor, out count);
                CheckError();
                var result = new List<VideoMode>(count);

                for (int i = 0; i < count; i++) {
                    result.Add(ptr[i]);
                }

                return result;
            }
        }

        public static VideoMode GetVideoMode(MonitorPtr monitor) {
            unsafe {
                VideoMode result = *glfwGetVideoMode(monitor);
                CheckError();
                return result;
            }
        }

        public static void SetGamma(MonitorPtr monitor, float gamma) {
            glfwSetGamma(monitor, gamma);
        }

        public static GammaRamp GetGammaRamp(MonitorPtr monitor) {
            unsafe {
                NativeGammaRamp ngr = *glfwGetGammaRamp(monitor);
                CheckError();
                return new GammaRamp(ngr);
            }
        }

        public static void SetGammaRamp(MonitorPtr monitor, GammaRamp ramp) {
            unsafe {
                fixed (ushort* r = &ramp.red[0])
                fixed (ushort* g = &ramp.green[0])
                fixed (ushort* b = &ramp.blue[0]) {
                    NativeGammaRamp ngr = new NativeGammaRamp(r, g, b, (uint)ramp.red.Length);
                    NativeGammaRamp* ptr = &ngr;
                    glfwSetGammaRamp(monitor, ptr);
                    CheckError();
                }
            }
        }

        public static void DefaultWindowHints() {
            glfwDefaultWindowHints();
            CheckError();
        }

        public static void WindowHint(WindowHint hint, int value) {
            glfwWindowHint((int)hint, value);
            CheckError();
        }

        public static WindowPtr CreateWindow(int width, int height, string title, MonitorPtr monitor, WindowPtr share) {
            var result = glfwCreateWindow(width, height, title, monitor, share);
            CheckError();

            GetCallbacks(result);
            return result;
        }

        public static void DestroyWindow(WindowPtr window) {
            glfwDestroyWindow(window);
            callbackMap.Remove(window);
        }

        public static bool WindowShouldClose(WindowPtr window) {
            var result = glfwWindowShouldClose(window);
            CheckError();
            return result;
        }

        public static void SetWindowShouldClose(WindowPtr window, bool value) {
            glfwSetWindowShouldClose(window, value);
            CheckError();
        }

        public static void SetWindowTitle(WindowPtr window, string title) {
            glfwSetWindowTitle(window, title);
            CheckError();
        }

        public static void SetWindowIcon(WindowPtr window, IList<Bitmap<Color4b>> images) {
            unsafe {
                if (images == null || images.Count == 0) {
                    glfwSetWindowIcon(window, 0, null);
                    return;
                }

                var nimgs = new NativeImage[images.Count];
                var handles = new GCHandle[images.Count];

                fixed (NativeImage* ptr = nimgs) {
                    for (int i = 0; i < images.Count; i++) {
                        if (images[i] == null) throw new ArgumentNullException(string.Format("Index {0} of {1}", i, nameof(images)));

                        handles[i] = GCHandle.Alloc(images[i].Data, GCHandleType.Pinned);
                        nimgs[i] = new NativeImage(images[i].Width, images[i].Height, (byte*)handles[i].AddrOfPinnedObject());
                    }

                    glfwSetWindowIcon(window, images.Count, ptr);
                    CheckError();

                    for (int i = 0; i < images.Count; i++) {
                        handles[i].Free();
                    }
                }
            }
        }

        public static void GetWindowPos(WindowPtr window, out int x, out int y) {
            glfwGetWindowPos(window, out x, out y);
            CheckError();
        }

        public static void SetWindowPos(WindowPtr window, int x, int y) {
            glfwSetWindowPos(window, x, y);
            CheckError();
        }

        public static void GetWindowSize(WindowPtr window, out int width, out int height) {
            glfwGetWindowSize(window, out width, out height);
            CheckError();
        }

        public static void SetWindowSizeLimits(WindowPtr window,
            int minWidth, int minHeight,
            int maxWidth, int maxHeight) {
            glfwSetWindowSizeLimits(window, minWidth, minHeight, maxWidth, maxHeight);
            CheckError();
        }

        public static void SetWindowAspectRatio(WindowPtr window, int numerator, int denominator) {
            glfwSetWindowAspectRatio(window, numerator, denominator);
            CheckError();
        }

        public static void SetWindowSize(WindowPtr window, int width, int height) {
            glfwSetWindowSize(window, width, height);
            CheckError();
        }

        public static void GetFramebufferSize(WindowPtr window, out int width, out int height) {
            glfwGetFramebufferSize(window, out width, out height);
            CheckError();
        }

        public static void GetWindowFrameSize(WindowPtr window,
            out int left, out int top, out int right, out int bottom) {
            glfwGetWindowFrameSize(window, out left, out top, out right, out bottom);
            CheckError();
        }

        public static void IconifyWindow(WindowPtr window) {
            glfwIconifyWindow(window);
            CheckError();
        }

        public static void RestoreWindow(WindowPtr window) {
            glfwRestoreWindow(window);
            CheckError();
        }

        public static void MaximizeWindow(WindowPtr window) {
            glfwMaximizeWindow(window);
            CheckError();
        }

        public static void ShowWindow(WindowPtr window) {
            glfwShowWindow(window);
            CheckError();
        }

        public static void HideWindow(WindowPtr window) {
            glfwHideWindow(window);
            CheckError();
        }

        public static void FocusWindow(WindowPtr window) {
            glfwFocusWindow(window);
            CheckError();
        }

        public static MonitorPtr GetWindowMonitor(WindowPtr window) {
            var result = glfwGetWindowMonitor(window);
            CheckError();
            return result;
        }

        public static void SetWindowMonitor(WindowPtr window, MonitorPtr monitor,
            int x, int y,
            int width, int height,
            int refreshRate) {
            glfwSetWindowMonitor(window, monitor, x, y, width, height, refreshRate);
            CheckError();
        }

        public static int GetWindowAttribute(WindowPtr window, WindowAttribute attrib) {
            var result = glfwGetWindowAttrib(window, attrib);
            CheckError();
            return result;
        }

        public static WindowPositionCallback SetWindowPosCallback(WindowPtr window, WindowPositionCallback callback) {
            var callbacks = GetCallbacks(window);
            var old = callbacks.windowPosition;
            callbacks.windowPosition = callback;
            glfwSetWindowPosCallback(window, callback);
            CheckError();
            return old;
        }

        public static WindowSizeCallback SetWindowSizeCallback(WindowPtr window, WindowSizeCallback callback) {
            var callbacks = GetCallbacks(window);
            var old = callbacks.windowSize;
            callbacks.windowSize = callback;
            glfwSetWindowSizeCallback(window, callback);
            CheckError();
            return old;
        }

        public static WindowCloseCallback SetWindowCloseCallback(WindowPtr window, WindowCloseCallback callback) {
            var callbacks = GetCallbacks(window);
            var old = callbacks.windowClose;
            callbacks.windowClose = callback;
            glfwSetWindowCloseCallback(window, callback);
            CheckError();
            return old;
        }

        public static WindowRefreshCallback SetWindowRefreshCallback(WindowPtr window, WindowRefreshCallback callback) {
            var callbacks = GetCallbacks(window);
            var old = callbacks.windowRefresh;
            callbacks.windowRefresh = callback;
            glfwSetWindowRefreshCallback(window, callback);
            CheckError();
            return old;
        }

        public static WindowFocusCallback SetWindowFocusCallback(WindowPtr window, WindowFocusCallback callback) {
            var callbacks = GetCallbacks(window);
            var old = callbacks.windowFocus;
            callbacks.windowFocus = callback;
            glfwSetWindowFocusCallback(window, callback);
            CheckError();
            return old;
        }

        public static WindowIconifyCallback SetWindowIconifyCallback(WindowPtr window, WindowIconifyCallback callback) {
            var callbacks = GetCallbacks(window);
            var old = callbacks.windowIconify;
            callbacks.windowIconify = callback;
            glfwSetWindowIconifyCallback(window, callback);
            CheckError();
            return old;
        }

        public static FramebufferSizeCallback SetFramebufferSizeCallback(WindowPtr window, FramebufferSizeCallback callback) {
            var callbacks = GetCallbacks(window);
            var old = callbacks.framebufferSize;
            callbacks.framebufferSize = callback;
            glfwSetFramebufferSizeCallback(window, callback);
            CheckError();
            return old;
        }

        public static void PollEvents() {
            glfwPollEvents();
            CheckError();
        }

        public static void WaitEvents() {
            glfwWaitEvents();
            CheckError();
        }

        public static void WaitEventsTimeout(double timeout) {
            glfwWaitEventsTimeout(timeout);
            CheckError();
        }

        public static int GetInputMode(WindowPtr window, InputMode mode) {
            var result = glfwGetInputMode(window, mode);
            CheckError();
            return result;
        }

        public static void SetInputMode(WindowPtr window, InputMode mode, int value) {
            glfwSetInputMode(window, mode, value);
            CheckError();
        }

        public static KeyAction GetKey(WindowPtr window, KeyCode key) {
            var result = glfwGetKey(window, key);
            CheckError();
            return result;
        }

        public static KeyAction GetMouseButton(WindowPtr window, MouseButton button) {
            var result = glfwGetMouseButton(window, button);
            CheckError();
            return result;
        }

        public static void GetCursorPos(WindowPtr window, out double x, out double y) {
            glfwGetCursorPos(window, out x, out y);
            CheckError();
        }

        public static void SetCursorPos(WindowPtr window, double x, double y) {
            glfwSetCursorPos(window, x, y);
            CheckError();
        }

        public static CursorPtr CreateCursor(Bitmap<Color4b> image, int xHotspot, int yHotspot) {
            if (image == null) throw new ArgumentNullException(nameof(image));

            unsafe {
                fixed (Color4b* data = image.Data) {
                    NativeImage nimg = new NativeImage(image.Width, image.Height, (byte*)data);
                    NativeImage* ptr = &nimg;
                    var result = glfwCreateCursor(ptr, xHotspot, yHotspot);
                    CheckError();
                    return result;
                }
            }
        }

        public static CursorPtr CreateStandardCursor(CursorShape shape) {
            var result = glfwCreateStandardCursor(shape);
            CheckError();
            return result;
        }

        public static void DestroyCursor(CursorPtr cursor) {
            glfwDestroyCursor(cursor);
            CheckError();
        }

        public static void SetCursor(WindowPtr window, CursorPtr cursor) {
            glfwSetCursor(window, cursor);
            CheckError();
        }

        public static KeyCallback SetKeyCallback(WindowPtr window, KeyCallback callback) {
            var callbacks = GetCallbacks(window);
            var old = callbacks.key;
            callbacks.key = callback;
            glfwSetKeyCallback(window, callback);
            CheckError();
            return old;
        }

        public static CharCallback SetCharCallback(WindowPtr window, CharCallback callback) {
            var callbacks = GetCallbacks(window);
            var old = callbacks._char;
            callbacks._char = callback;
            glfwSetCharCallback(window, callback);
            CheckError();
            return old;
        }

        public static CharModsCallback SetCharModsCallback(WindowPtr window, CharModsCallback callback) {
            var callbacks = GetCallbacks(window);
            var old = callbacks.charMods;
            callbacks.charMods = callback;
            glfwSetCharModsCallback(window, callback);
            CheckError();
            return old;
        }

        public static MouseButtonCallback SetMouseButtonCallback(WindowPtr window, MouseButtonCallback callback) {
            var callbacks = GetCallbacks(window);
            var old = callbacks.mouseButton;
            callbacks.mouseButton = callback;
            glfwSetMouseButtonCallback(window, callback);
            CheckError();
            return old;
        }

        public static CursorPosCallback SetCursorPosCallback(WindowPtr window, CursorPosCallback callback) {
            var callbacks = GetCallbacks(window);
            var old = callbacks.cursorPos;
            callbacks.cursorPos = callback;
            glfwSetCursorPosCallback(window, callback);
            CheckError();
            return old;
        }

        public static CursorEnterCallback SetCursorEnterCallback(WindowPtr window, CursorEnterCallback callback) {
            var callbacks = GetCallbacks(window);
            var old = callbacks.cursorEnter;
            callbacks.cursorEnter = callback;
            glfwSetCursorEnterCallback(window, callback);
            CheckError();
            return old;
        }

        public static ScrollCallback SetScrollCallback(WindowPtr window, ScrollCallback callback) {
            var callbacks = GetCallbacks(window);
            var old = callbacks.scroll;
            callbacks.scroll = callback;
            glfwSetScrollCallback(window, callback);
            CheckError();
            return old;
        }

        public static FileDropCallback SetDropCallback(WindowPtr window, FileDropCallback callback) {
            var callbacks = GetCallbacks(window);
            var old = callbacks.fileDrop;
            callbacks.fileDrop = callback;
            glfwSetDropCallback(window, callback);
            CheckError();
            return old;
        }

        public static bool JoystickPresent(int joystick) {
            var result = glfwJoystickPresent(joystick);
            CheckError();
            return result;
        }

        public static void GetJoystickAxes(int joystick, IList<float> axes) {
            if (axes == null) throw new ArgumentNullException(nameof(axes));
            axes.Clear();

            unsafe {
                int count;
                float* ptr = glfwGetJoystickAxes(joystick, out count);
                CheckError();

                for (int i = 0; i < count; i++) {
                    axes.Add(ptr[i]);
                }
            }
        }

        public static IList<float> GetJoystickAxes(int joystick) {
            List<float> axes = new List<float>();
            GetJoystickAxes(joystick, axes);
            return axes;
        }

        public static void GetJoystickButtons(int joystick, IList<bool> buttons) {
            if (buttons == null) throw new ArgumentNullException(nameof(buttons));
            buttons.Clear();

            unsafe {
                int count;
                bool* ptr = glfwGetJoystickButtons(joystick, out count);
                CheckError();

                for (int i = 0; i < count; i++) {
                    buttons.Add(ptr[i]);
                }
            }
        }

        public static IList<bool> GetJoystickButtons(int joystick) {
            List<bool> buttons = new List<bool>();
            GetJoystickButtons(joystick, buttons);
            return buttons;
        }

        public static string GetJoystickName(int joystick) {
            unsafe {
                var s = glfwGetJoystickName(joystick);
                CheckError();
                var result = Interop.GetString(s);
                return result;
            }
        }

        public static JoystickConnectionCallback SetJoystickConnectionCallback(WindowPtr window, JoystickConnectionCallback callback) {
            var old = joystickConnection;
            joystickConnection = callback;
            glfwSetJoystickCallback(window, callback);
            CheckError();
            return old;
        }

        public static void SetClipboardString(WindowPtr window, string s) {
            glfwSetClipboardString(window, s);
            CheckError();
        }

        public static string GetClipboardString(WindowPtr window) {
            unsafe {
                var s = glfwGetClipboardString(window);
                CheckError();
                var result = Interop.GetString(s);
                return result;
            }
        }

        public static double GetTime() {
            var result = glfwGetTime();
            CheckError();
            return result;
        }

        public static void SetTime(double time) {
            glfwSetTime(time);
            CheckError();
        }

        public static ulong GetTimerValue() {
            var result = glfwGetTimerValue();
            CheckError();
            return result;
        }

        public static ulong GetTimerFrequency() {
            var result = glfwGetTimerFrequency();
            CheckError();
            return result;
        }

        public static void MakeContextCurrent(WindowPtr window) {
            glfwMakeContextCurrent(window);
            CheckError();
        }

        public static WindowPtr GetCurrentContext() {
            var result = glfwGetCurrentContext();
            CheckError();
            return result;
        }

        public static void SwapBuffers(WindowPtr window) {
            glfwSwapBuffers(window);
            CheckError();
        }

        public static void SwapInterval(int interval) {
            glfwSwapInterval(interval);
            CheckError();
        }

        public static bool ExtensionSupported(string extension) {
            var result = glfwExtensionSupported(extension);
            CheckError();
            return result;
        }

        public static IntPtr GetProcAddress(string procName) {
            var result = glfwGetProcAddress(procName);
            CheckError();
            return result;
        }

        public static bool VulkanSupported() {
            var result = glfwVulkanSupported();
            CheckError();
            return result;
        }

        public static IList<string> GetRequiredInstanceExceptions() {
            List<string> result;

            unsafe {
                uint count;
                byte** strings = glfwGetRequiredInstanceExtensions(out count);
                CheckError();

                if (strings == null) {
                    result = null;
                } else {
                    result = new List<string>((int)count);
                    for (int i = 0; i < count; i++) {
                        result.Add(Interop.GetString(strings[i]));
                    }
                }
            }

            return result;
        }

        public static IntPtr GetInstanceProcAddress(IntPtr instance, string proc) {
            var result = glfwGetInstanceProcAddress(instance, proc);
            CheckError();
            return result;
        }

        public static bool GetPhysicalDevicePresentationSupport(IntPtr instance, IntPtr device, uint queueFamily) {
            var result = glfwGetPhysicalDevicePresentationSupport(instance, device, queueFamily);
            CheckError();
            return result;
        }

        public static int CreateWindowSurface(IntPtr instance, WindowPtr ptr, IntPtr alloc, out ulong surface) {
            var result = glfwCreateWindowSurface(instance, ptr, alloc, out surface);
            CheckError();
            return result;
        }
    }
}
