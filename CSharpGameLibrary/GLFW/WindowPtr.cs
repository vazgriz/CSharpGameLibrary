using System;
using System.Runtime.InteropServices;

namespace CSGL.GLFW {
    [StructLayout(LayoutKind.Sequential)]
    public struct WindowPtr : IEquatable<WindowPtr> {
        internal IntPtr ptr;

        internal WindowPtr(IntPtr ptr) {
            this.ptr = ptr;
        }

        public bool Equals(WindowPtr other) {
            return ptr == other.ptr;
        }

        public bool IsNull { get { return ptr == IntPtr.Zero; } }

        public static readonly WindowPtr Null = new WindowPtr();

        public static bool operator == (WindowPtr a, WindowPtr b) {
            return a.Equals(b);
        }

        public static bool operator != (WindowPtr a, WindowPtr b) {
            return !a.Equals(b);
        }

        public override bool Equals(object o) {
            if (o is WindowPtr) {
                return (WindowPtr)o == this;
            }
            return false;
        }

        public override int GetHashCode() {
            return ptr.GetHashCode();
        }
    }
}
