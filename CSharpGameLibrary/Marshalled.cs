using System;
using System.Runtime.InteropServices;

namespace CSGL {
    public class Marshalled<T> : IDisposable where T : struct {
        IntPtr ptr;
        bool disposed = false;
        static int elementSize;
        bool allocated = false;

        static Marshalled() {
            elementSize = Marshal.SizeOf<T>();
        }

        public static int ElementSize { get { return elementSize; } }

        public Marshalled() {
            Allocate();
        }

        public Marshalled(T value) {
            Allocate();
            Value = value;
        }

        public unsafe Marshalled(void* ptr) {   //meant to be used for stackalloc'ed memory
            if (ptr == null) throw new ArgumentNullException(nameof(ptr));

            this.ptr = (IntPtr)ptr;
        }

        public unsafe Marshalled(void* ptr, T value) {
            if (ptr == null) throw new ArgumentNullException(nameof(ptr));

            this.ptr = (IntPtr)ptr;
            Value = value;
        }

        void Allocate() {
            ptr = Marshal.AllocHGlobal(elementSize);
            allocated = true;
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

            if (allocated) {
                Marshal.FreeHGlobal(ptr);
            }

            disposed = true;
        }

        ~Marshalled() {
            Dispose(false);
        }
    }
}
