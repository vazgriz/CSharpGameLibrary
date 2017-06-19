using System;
using System.Runtime.InteropServices;

namespace CSGL.OpenGL {
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void DebugDelegate(DebugMessageSource source, DebugMessageType type, uint id, DebugMessageSeverity severity, int length, IntPtr message, IntPtr userParam);
}
