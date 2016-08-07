using System;
using System.Text;

using CSGL.Input;
using CSGL.Vulkan;
using CSGL.Vulkan.Unmanaged;
using static CSGL.GLFW.Unmanaged.GLFW_native;

namespace CSGL.GLFW {
    public static class GLFW {
        [ThreadStatic]
        static Exception exception;

        static WindowPositionCallback windowPosition;   //need to store these delegates
        static WindowSizeCallback windowSize;           //otherwise the garbage collector will clean them up while the unmanaged code holds a pointer to them
        static WindowCloseCallback windowClose;
        static WindowRefreshCallback windowRefresh;
        static WindowFocusCallback windowFocus;
        static WindowIconifyCallback windowIconify;
        static FramebufferSizeCallback framebufferSize;
        static MouseButtonCallback mouseButton;
        static CursorPosCallback cursorPos;
        static CursorEnterCallback cursorEnter;
        static ScrollCallback scroll;
        static KeyCallback key;
        static CharCallback _char;
        static CharModsCallback charMods;
        static FileDropCallback fileDrop;
        static MonitorConnectionCallback monitorConnection;
        static JoystickConnectionCallback joystickConnection;

        static GLFW() {
            glfwSetErrorCallback(InternalError);    //has to be set here, so that errors made before glfwInit() are caught
        }
        
        static void CheckError() {      //only way to convert GLFW error to managed exception
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
            return result;
        }

        public static void Terminate() {
            glfwTerminate();
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

        //public static ErrorCallback SetErrorCallback(ErrorCallback callback) {
        //    var result = glfwSetErrorCallback(callback);
        //}

        public static MonitorPtr[] GetMonitors() {
            unsafe
            {
                int count;
                MonitorPtr* ptr = glfwGetMonitors(out count);
                CheckError();
                MonitorPtr[] result = new MonitorPtr[count];

                for (int i = 0; i < count; i++) {
                    result[i] = ptr[i];
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
            unsafe
            {
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

        public static VideoMode[] GetVideoModes(MonitorPtr monitor) {
            unsafe
            {
                int count;
                VideoMode* ptr = glfwGetVideoModes(monitor, out count);
                CheckError();
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
                VideoMode result = *glfwGetVideoMode(monitor);
                CheckError();
                return result;
            }
        }

        public static void SetGamma(MonitorPtr monitor, float gamma) {
            glfwSetGamma(monitor, gamma);
        }

        public static GammaRamp GetGammaRamp(MonitorPtr monitor) {
            unsafe
            {
                NativeGammaRamp ngr = *glfwGetGammaRamp(monitor);
                CheckError();
                return new GammaRamp(ngr);
            }
        }

        public static void SetGammaRamp(MonitorPtr monitor, GammaRamp ramp) {
            unsafe
            {
                fixed (short* r = &ramp.red[0])
                fixed (short* g = &ramp.green[0])
                fixed (short* b = &ramp.blue[0]) {
                    NativeGammaRamp ngr = new NativeGammaRamp(r, g, b, ramp.size);
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
            return result;
        }

        public static void DestroyWindow(WindowPtr window) {
            glfwDestroyWindow(window);
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

        public static void SetWindowIcon(WindowPtr window, Image[] images) {
            NativeImage[] nimgs = new NativeImage[images.Length];
            unsafe
            {
                fixed (NativeImage* ptr = &nimgs[0]) {
                    for (int i = 0; i < images.Length; i++) {
                        fixed (byte* data = &images[i].data[0]) {
                            nimgs[i] = new NativeImage(images[i].width, images[i].height, data);
                        }
                    }
                    glfwSetWindowIcon(window, images.Length, ptr);
                    CheckError();
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
            var old = windowPosition;
            windowPosition = callback;
            glfwSetWindowPosCallback(window, callback);
            CheckError();
            return old;
        }

        public static WindowSizeCallback SetWindowSizeCallback(WindowPtr window, WindowSizeCallback callback) {
            var old = windowSize;
            windowSize = callback;
            glfwSetWindowSizeCallback(window, callback);
            CheckError();
            return old;
        }

        public static WindowCloseCallback SetWindowCloseCallback(WindowPtr window, WindowCloseCallback callback) {
            var old = windowClose;
            windowClose = callback;
            glfwSetWindowCloseCallback(window, callback);
            CheckError();
            return old;
        }

        public static WindowRefreshCallback SetWindowRefreshCallback(WindowPtr window, WindowRefreshCallback callback) {
            var old = windowRefresh;
            windowRefresh = callback;
            glfwSetWindowRefreshCallback(window, callback);
            CheckError();
            return old;
        }

        public static WindowFocusCallback SetWindowFocusCallback(WindowPtr window, WindowFocusCallback callback) {
            var old = windowFocus;
            windowFocus = callback;
            glfwSetWindowFocusCallback(window, callback);
            CheckError();
            return old;
        }

        public static WindowIconifyCallback SetWindowIconifyCallback(WindowPtr window, WindowIconifyCallback callback) {
            var old = windowIconify;
            windowIconify = callback;
            glfwSetWindowIconifyCallback(window, callback);
            CheckError();
            return old;
        }

        public static FramebufferSizeCallback SetFramebufferSizeCallback(WindowPtr window, FramebufferSizeCallback callback) {
            var old = framebufferSize;
            framebufferSize = callback;
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

        public static int GetInputMode(WindowPtr window, int mode) {
            var result = glfwGetInputMode(window, mode);
            CheckError();
            return result;
        }

        public static void SetInputMode(WindowPtr window, int mode, int value) {
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

        public static Cursor CreateCursor(Image image, int xHotspot, int yHotspot) {
            unsafe
            {
                fixed (byte* data = image.data) {
                    NativeImage nimg = new NativeImage(image.width, image.height, data);
                    NativeImage* ptr = &nimg;
                    var result = glfwCreateCursor(ptr, xHotspot, yHotspot);
                    CheckError();
                    return result;
                }
            }
        }

        public static Cursor CreateCursor(CursorShape shape) {
            var result = glfwCreateStandardCursor(shape);
            CheckError();
            return result;
        }

        public static void DestroyCursor(Cursor cursor) {
            glfwDestroyCursor(cursor);
            CheckError();
        }

        public static void SetCursor(WindowPtr window, Cursor cursor) {
            glfwSetCursor(window, cursor);
            CheckError();
        }

        public static KeyCallback SetKeyCallback(WindowPtr window, KeyCallback callback) {
            var old = key;
            key = callback;
            glfwSetKeyCallback(window, callback);
            CheckError();
            return old;
        }

        public static CharCallback SetCharCallback(WindowPtr window, CharCallback callback) {
            var old = _char;
            _char = callback;
            glfwSetCharCallback(window, callback);
            CheckError();
            return old;
        }

        public static CharModsCallback SetCharModsCallback(WindowPtr window, CharModsCallback callback) {
            var old = charMods;
            charMods = callback;
            glfwSetCharModsCallback(window, callback);
            CheckError();
            return old;
        }

        public static MouseButtonCallback SetMouseButtonCallback(WindowPtr window, MouseButtonCallback callback) {
            var old = mouseButton;
            mouseButton = callback;
            glfwSetMouseButtonCallback(window, callback);
            CheckError();
            return old;
        }

        public static CursorPosCallback SetCursorPosCallback(WindowPtr window, CursorPosCallback callback) {
            var old = cursorPos;
            cursorPos = callback;
            glfwSetCursorPosCallback(window, callback);
            CheckError();
            return old;
        }

        public static CursorEnterCallback SetCursorEnterCallback(WindowPtr window, CursorEnterCallback callback) {
            var old = cursorEnter;
            cursorEnter = callback;
            glfwSetCursorEnterCallback(window, callback);
            CheckError();
            return old;
        }

        public static ScrollCallback SetScrollCallback(WindowPtr window, ScrollCallback callback) {
            var old = scroll;
            scroll = callback;
            glfwSetScrollCallback(window, callback);
            CheckError();
            return old;
        }

        public static FileDropCallback SetDropCallback(WindowPtr window, FileDropCallback callback) {
            var old = fileDrop;
            fileDrop = callback;
            glfwSetDropCallback(window, callback);
            CheckError();
            return old;
        }

        public static bool JoystickPresent(int joystick) {
            var result = glfwJoystickPresent(joystick);
            CheckError();
            return result;
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
                var result = axes;
                CheckError();
                return result;
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
                var result = buttons;
                CheckError();
                return result;
            }
        }

        public static string GetJoystickName(int joystick) {
            unsafe
            {
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
            unsafe
            {
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

        public static string[] GetRequiredInstanceExceptions() {
            string[] result;
            unsafe
            {
                uint count;
                byte** strings = glfwGetRequiredInstanceExtensions(out count);

                if (strings == null) {
                    result = null;
                } else {
                    result = new string[count];
                    for (int i = 0; i < count; i++) {
                        result[i] = Interop.GetString(strings[i]);
                    }
                }
            }
            CheckError();
            return result;
        }

        public static IntPtr GetInstanceProcAddress(VkInstance instance, string proc) {
            var result = glfwGetInstanceProcAddress(instance, proc);
            CheckError();
            return result;
        }

        public static bool GetPhysicalDevicePresentationSupport(VkInstance instance, VkPhysicalDevice device, uint queueFamily) {
            var result = glfwGetPhysicalDevicePresentationSupport(instance, device, queueFamily);
            CheckError();
            return result;
        }

        public static VkResult CreateWindowSurface(VkInstance instance, WindowPtr ptr, ref VkAllocationCallbacks alloc, ref VkSurfaceKHR surface) {
            var result = glfwCreateWindowSurface(instance, ptr, ref alloc, ref surface);
            CheckError();
            return result;
        }
    }
}
