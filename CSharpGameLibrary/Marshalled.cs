using System;
using System.Runtime.InteropServices;

namespace CSGL {
    public class Marshalled<T> : IDisposable where T : struct {
        IntPtr ptr;
        bool disposed = false;
        static int elementSize;
        bool _unsafe = false;

        static Marshalled() {
            elementSize = Marshal.SizeOf<T>();
        }

        public static int ElementSize { get { return elementSize; } }

        public Marshalled() {
            Init();
        }

        public Marshalled(T init) {
            Init();
            Marshal.StructureToPtr(init, ptr, false);
        }

        public unsafe Marshalled(void* ptr) {   //meant to be used for stackalloc'ed memory
            if (ptr == null) throw new ArgumentNullException(nameof(ptr));

            this.ptr = (IntPtr)ptr;
            _unsafe = true;
        }

        public unsafe Marshalled(void* ptr, T init) {
            if (ptr == null) throw new ArgumentNullException(nameof(ptr));

            this.ptr = (IntPtr)ptr;
            Marshal.StructureToPtr(init, this.ptr, false);
            _unsafe = true;
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

            Marshal.DestroyStructure<T>(ptr);

            if (!_unsafe) {
                Marshal.FreeHGlobal(ptr);
            }

            disposed = true;
        }

        ~Marshalled() {
            Dispose(false);
        }
    }
}
