using System;
using System.Runtime.InteropServices;

using CSGL.Input;

namespace CSGL.GLFW.Unmanaged {
    public static unsafe class GLFW_native {
        const string lib = "glfw3";

        [DllImport(lib)]
        public static extern bool glfwInit();

        [DllImport(lib)]
        public static extern void glfwTerminate();

        [DllImport(lib)]
        public static extern void glfwGetVersion(out int major, out int minor, out int revision);

        [DllImport(lib)]
        public static extern byte* glfwGetVersionString();

        [DllImport(lib)]
        public static extern ErrorCallback glfwSetErrorCallback(ErrorCallback callback);

        [DllImport(lib)]
        public static extern MonitorPtr* glfwGetMonitors(out int count);

        [DllImport(lib)]
        public static extern MonitorPtr glfwGetPrimaryMonitor();

        [DllImport(lib)]
        public static extern void glfwGetMonitorPos(MonitorPtr monitor, out int x, out int y);

        [DllImport(lib)]
        public static extern void glfwGetMonitorPhysicalSize(MonitorPtr monitor, out int width, out int height);

        [DllImport(lib)]
        public static extern byte* glfwGetMonitorName(MonitorPtr monitor);

        [DllImport(lib)]
        public static extern IntPtr glfwSetMonitorCallback(MonitorConnectionCallback callback);

        [DllImport(lib)]
        public static extern VideoMode* glfwGetVideoModes(MonitorPtr monitor, out int count);

        [DllImport(lib)]
        public static extern VideoMode* glfwGetVideoMode(MonitorPtr monitor);

        [DllImport(lib)]
        public static extern void glfwSetGamma(MonitorPtr monitor, float gamma);

        [DllImport(lib)]
        public static extern NativeGammaRamp* glfwGetGammaRamp(MonitorPtr monitor);

        [DllImport(lib)]
        public static extern void glfwSetGammaRamp(MonitorPtr monitor, NativeGammaRamp* ramp);

        [DllImport(lib)]
        public static extern void glfwDefaultWindowHints();

        [DllImport(lib)]
        public static extern void glfwWindowHint(int hint, int value);

        [DllImport(lib)]
        public static extern WindowPtr glfwCreateWindow(
            int width, int height,
            string title,
            MonitorPtr monitor, WindowPtr window
        );

        [DllImport(lib)]
        public static extern void glfwDestroyWindow(WindowPtr window);

        [DllImport(lib)]
        public static extern bool glfwWindowShouldClose(WindowPtr window);

        [DllImport(lib)]
        public static extern void glfwSetWindowShouldClose(WindowPtr window, bool value);

        [DllImport(lib)]
        public static extern void glfwSetWindowTitle(WindowPtr window, string title);

        [DllImport(lib)]
        public static extern void glfwSetWindowIcon(WindowPtr window, int count, NativeImage* images);

        [DllImport(lib)]
        public static extern void glfwGetWindowPos(WindowPtr window, out int x, out int y);

        [DllImport(lib)]
        public static extern void glfwSetWindowPos(WindowPtr window, int x, int y);

        [DllImport(lib)]
        public static extern void glfwGetWindowSize(WindowPtr window, out int width, out int height);

        [DllImport(lib)]
        public static extern void glfwSetWindowSizeLimits(
            WindowPtr window,
            int minWidth, int minHeight,
            int maxWidth, int maxHeight
        );

        [DllImport(lib)]
        public static extern void glfwSetWindowAspectRatio(WindowPtr window, int numerator, int denominator);

        [DllImport(lib)]
        public static extern void glfwSetWindowSize(WindowPtr window, int width, int height);

        [DllImport(lib)]
        public static extern void glfwGetFramebufferSize(WindowPtr window, out int width, out int height);

        [DllImport(lib)]
        public static extern void glfwGetWindowFrameSize(
            WindowPtr window,
            out int left, out int top, out int right, out int bottom
        );

        [DllImport(lib)]
        public static extern void glfwIconifyWindow(WindowPtr window);

        [DllImport(lib)]
        public static extern void glfwRestoreWindow(WindowPtr window);

        [DllImport(lib)]
        public static extern void glfwMaximizeWindow(WindowPtr window);

        [DllImport(lib)]
        public static extern void glfwShowWindow(WindowPtr window);

        [DllImport(lib)]
        public static extern void glfwHideWindow(WindowPtr window);

        [DllImport(lib)]
        public static extern void glfwFocusWindow(WindowPtr window);

        [DllImport(lib)]
        public static extern MonitorPtr glfwGetWindowMonitor(WindowPtr window);

        [DllImport(lib)]
        public static extern void glfwSetWindowMonitor(
            WindowPtr window, MonitorPtr monitor,
            int x, int y,
            int width, int height,
            int refreshRate
        );

        [DllImport(lib)]
        public static extern int glfwGetWindowAttrib(WindowPtr window, WindowAttribute attrib);

        [DllImport(lib)]
        public static extern void glfwSetWindowUserPointer(WindowPtr window, IntPtr pointer);

        [DllImport(lib)]
        public static extern IntPtr glfwGetWindowUserPointer(WindowPtr window);

        [DllImport(lib)]
        public static extern IntPtr glfwSetWindowPosCallback(WindowPtr window, WindowPositionCallback callback);

        [DllImport(lib)]
        public static extern IntPtr glfwSetWindowSizeCallback(WindowPtr window, WindowSizeCallback callback);

        [DllImport(lib)]
        public static extern IntPtr glfwSetWindowCloseCallback(WindowPtr window, WindowCloseCallback callback);

        [DllImport(lib)]
        public static extern IntPtr glfwSetWindowRefreshCallback(WindowPtr window, WindowRefreshCallback callback);

        [DllImport(lib)]
        public static extern IntPtr glfwSetWindowFocusCallback(WindowPtr window, WindowFocusCallback callback);

        [DllImport(lib)]
        public static extern IntPtr glfwSetWindowIconifyCallback(WindowPtr window, WindowIconifyCallback callback);

        [DllImport(lib)]
        public static extern IntPtr glfwSetFramebufferSizeCallback(WindowPtr window, FramebufferSizeCallback callback);

        [DllImport(lib)]
        public static extern void glfwPollEvents();

        [DllImport(lib)]
        public static extern void glfwWaitEvents();

        [DllImport(lib)]
        public static extern void glfwWaitEventsTimeout(double timeout);

        [DllImport(lib)]
        public static extern void glfwPostEmptyEvent();

        [DllImport(lib)]
        public static extern int glfwGetInputMode(WindowPtr window, int mode);

        [DllImport(lib)]
        public static extern void glfwSetInputMode(WindowPtr window, int mode, int value);

        [DllImport(lib)]
        public static extern IntPtr glfwGetKeyName(KeyCode key, int scan);
        
        [DllImport(lib)]
        public static extern KeyAction glfwGetKey(WindowPtr window, KeyCode key);

        [DllImport(lib)]
        public static extern KeyAction glfwGetMouseButton(WindowPtr window, MouseButton button);

        [DllImport(lib)]
        public static extern void glfwGetCursorPos(WindowPtr window, out double x, out double y);

        [DllImport(lib)]
        public static extern void glfwSetCursorPos(WindowPtr window, double x, double y);

        [DllImport(lib)]
        public static extern Cursor glfwCreateCursor(NativeImage* image, int xHotspot, int yHotspot);

        [DllImport(lib)]
        public static extern Cursor glfwCreateStandardCursor(CursorShape shape);

        [DllImport(lib)]
        public static extern void glfwDestroyCursor(Cursor cursor);

        [DllImport(lib)]
        public static extern void glfwSetCursor(WindowPtr window, Cursor cursor);

        [DllImport(lib)]
        public static extern IntPtr glfwSetKeyCallback(WindowPtr window, KeyCallback callback);

        [DllImport(lib)]
        public static extern IntPtr glfwSetCharCallback(WindowPtr window, CharCallback callback);

        [DllImport(lib)]
        public static extern IntPtr glfwSetCharModsCallback(WindowPtr window, CharModsCallback callback);

        [DllImport(lib)]
        public static extern IntPtr glfwSetMouseButtonCallback(WindowPtr window, MouseButtonCallback callback);

        [DllImport(lib)]
        public static extern IntPtr glfwSetCursorPosCallback(WindowPtr window, CursorPosCallback callback);

        [DllImport(lib)]
        public static extern IntPtr glfwSetCursorEnterCallback(WindowPtr window, CursorEnterCallback callback);

        [DllImport(lib)]
        public static extern IntPtr glfwSetScrollCallback(WindowPtr window, ScrollCallback callback);

        [DllImport(lib)]
        public static extern IntPtr glfwSetDropCallback(WindowPtr window, FileDropCallback callback);

        [DllImport(lib)]
        public static extern bool glfwJoystickPresent(int joystick);

        [DllImport(lib)]
        public static extern float* glfwGetJoystickAxes(int joystick, out int count);

        [DllImport(lib)]
        public static extern bool* glfwGetJoystickButtons(int joystick, out int count);

        [DllImport(lib)]
        public static extern byte* glfwGetJoystickName(int joystick);

        [DllImport(lib)]
        public static extern IntPtr glfwSetJoystickCallback(WindowPtr window, JoystickConnectionCallback callback);

        [DllImport(lib)]
        public static extern void glfwSetClipboardString(WindowPtr window, string s);

        [DllImport(lib)]
        public static extern byte* glfwGetClipboardString(WindowPtr window);

        [DllImport(lib)]
        public static extern double glfwGetTime();

        [DllImport(lib)]
        public static extern void glfwSetTime(double time);

        [DllImport(lib)]
        public static extern ulong glfwGetTimerValue();

        [DllImport(lib)]
        public static extern ulong glfwGetTimerFrequency();

        [DllImport(lib)]
        public static extern void glfwMakeContextCurrent(WindowPtr window);

        [DllImport(lib)]
        public static extern WindowPtr glfwGetCurrentContext();

        [DllImport(lib)]
        public static extern void glfwSwapBuffers(WindowPtr window);

        [DllImport(lib)]
        public static extern void glfwSwapInterval(int interval);

        [DllImport(lib)]
        public static extern bool glfwExtensionSupported(string extension);

        [DllImport(lib)]
        public static extern IntPtr glfwGetProcAddress(string procName);

        [DllImport(lib)]
        public static extern bool glfwVulkanSupported();

        [DllImport(lib)]
        public static extern byte** glfwGetRequiredInstanceExtensions(out uint count);

        [DllImport(lib)]
        public static extern IntPtr glfwGetInstanceProcAddress(IntPtr instance, string procName);

        [DllImport(lib)]
        public static extern bool glfwGetPhysicalDevicePresentationSupport(IntPtr instance, IntPtr device, uint queuefamily);

        [DllImport(lib)]
        public static extern int glfwCreateWindowSurface(IntPtr instance, WindowPtr window, IntPtr allocator, out IntPtr surface);
    }
}
