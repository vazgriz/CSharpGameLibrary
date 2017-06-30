using System;
using System.Runtime.InteropServices;

namespace CSGL.GLFW.Unmanaged {
    public struct CursorPtr : IEquatable<CursorPtr> {
        internal IntPtr ptr;

        internal CursorPtr(IntPtr ptr) {
            this.ptr = ptr;
        }

        public bool Equals(CursorPtr other) {
            return ptr == other.ptr;
        }

        public static CursorPtr Null { get; } = new CursorPtr();

        public static bool operator ==(CursorPtr a, CursorPtr b) {
            return a.Equals(b);
        }

        public static bool operator !=(CursorPtr a, CursorPtr b) {
            return !a.Equals(b);
        }

        public override bool Equals(object o) {
            if (o is CursorPtr) {
                return (CursorPtr)o == this;
            }
            return false;
        }

        public override int GetHashCode() {
            return ptr.GetHashCode();
        }
    }
}
