using System;
using System.Runtime.InteropServices;

namespace CSGL {
    public class InteropString : IDisposable {
        byte[] str;
        bool disposed = false;
        GCHandle handle;

        public InteropString(string s) {
            str = Interop.GetUTF8(s);
            Init();
        }

        public InteropString(byte[] s) {
            str = s;
            Init();
        }

        void Init() {
            handle = GCHandle.Alloc(str, GCHandleType.Pinned);
        }

        public IntPtr Address {
            get {
                return handle.AddrOfPinnedObject();
            }
        }

        public int Length {
            get {
                return str.Length;
            }
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        void Dispose(bool disposing) {
            if (disposed) return;

            handle.Free();

            if (disposing) {
                str = null;
            }

            disposed = true;
        }

        ~InteropString() {
            Dispose(false);
        }
    }
}
