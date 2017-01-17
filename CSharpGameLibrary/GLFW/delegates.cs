using System;
using System.Runtime.InteropServices;

using CSGL.Input;
using CSGL.GLFW.Unmanaged;

namespace CSGL.GLFW {
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void ErrorCallback(ErrorCode code, string desc);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void WindowPositionCallback(WindowPtr window, int x, int y);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void WindowSizeCallback(WindowPtr window, int x, int y);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void WindowCloseCallback(WindowPtr window);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void WindowRefreshCallback(WindowPtr window);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void WindowFocusCallback(WindowPtr window, bool focused);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void WindowIconifyCallback(WindowPtr window, bool iconified);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void FramebufferSizeCallback(WindowPtr window, int width, int height);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void MouseButtonCallback(WindowPtr window, MouseButton button, KeyAction action, KeyMod mod);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void CursorPosCallback(WindowPtr window, double x, double y);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void CursorEnterCallback(WindowPtr window, bool entered);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void ScrollCallback(WindowPtr window, double x, double y);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void KeyCallback(WindowPtr window, KeyCode code, int scan, KeyAction action, KeyMod mod);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void CharCallback(WindowPtr window, uint c);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void CharModsCallback(WindowPtr window, uint c, KeyMod mod);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void FileDropCallback(WindowPtr window, int count, IntPtr stringArray);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void MonitorConnectionCallback(MonitorPtr monitor, ConnectionStatus status);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void JoystickConnectionCallback(int joystick, ConnectionStatus status);
}
