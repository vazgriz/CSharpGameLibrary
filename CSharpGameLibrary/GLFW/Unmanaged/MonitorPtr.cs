using System;
using System.Runtime.InteropServices;

namespace CSGL.GLFW.Unmanaged {
    [StructLayout(LayoutKind.Sequential)]
    public struct MonitorPtr : IEquatable<MonitorPtr> {
        internal IntPtr ptr;

        internal MonitorPtr(IntPtr ptr) {
            this.ptr = ptr;
        }

        public bool Equals(MonitorPtr other) {
            return ptr == other.ptr;
        }

        public bool IsNull { get { return ptr == IntPtr.Zero; } }

        public static readonly MonitorPtr Null = new MonitorPtr();

        public static bool operator == (MonitorPtr a, MonitorPtr b) {
            return a.Equals(b);
        }

        public static bool operator !=(MonitorPtr a, MonitorPtr b) {
            return !a.Equals(b);
        }

        public override bool Equals(object o) {
            if (o is MonitorPtr) {
                return (MonitorPtr)o == this;
            }
            return false;
        }

        public override int GetHashCode() {
            return ptr.GetHashCode();
        }
    }
}
