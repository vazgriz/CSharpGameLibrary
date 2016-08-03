using System;
using System.Runtime.InteropServices;

namespace CSGL.GLFW {
    [StructLayout(LayoutKind.Sequential)]
    public struct Cursor : IEquatable<Cursor> {
        internal IntPtr ptr;

        internal Cursor(IntPtr ptr) {
            this.ptr = ptr;
        }

        public bool Equals(Cursor other) {
            return ptr == other.ptr;
        }

        public bool IsNull { get { return ptr == IntPtr.Zero; } }
    }
}
