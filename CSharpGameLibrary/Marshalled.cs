using System;
using System.Runtime.InteropServices;

namespace CSGL {
    public class Marshalled<T> : IDisposable where T : struct {
        IntPtr ptr;
        bool disposed = false;
        static int elementSize;

        static Marshalled() {
            elementSize = Marshal.SizeOf<T>();
        }

        public Marshalled() {
            Init();
        }

        public Marshalled(T init) {
            Init();
            Marshal.StructureToPtr<T>(init, ptr, false);
        }

        void Init() {
            ptr = Marshal.AllocHGlobal(elementSize);
        }

        public IntPtr Address {
            get {
                return ptr;
            }
        }

        public T Value {
            get {
                return Marshal.PtrToStructure<T>(ptr);
            }
            set {
                Marshal.StructureToPtr(value, ptr, true);
            }
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        void Dispose(bool disposing) {
            if (disposed) return;

            if (ptr != IntPtr.Zero) Marshal.DestroyStructure<T>(ptr);
            Marshal.FreeHGlobal(ptr);

            disposed = true;
        }

        ~Marshalled() {
            Dispose(false);
        }
    }
}
