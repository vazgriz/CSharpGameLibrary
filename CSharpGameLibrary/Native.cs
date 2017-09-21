using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace CSGL {
    public class Native<T> : IDisposable where T : struct {
        unsafe void* ptr;
        bool disposed = false;
        static int elementSize;
        bool allocated = false;

        static Native() {
            elementSize = Unsafe.SizeOf<T>();
        }

        public Native() {
            Allocate();
        }

        public Native(T value) {
            Allocate();
            Value = value;
        }

        public unsafe Native(void* ptr) {
            if (ptr == null) throw new ArgumentNullException(nameof(ptr));

            this.ptr = ptr;
        }

        public unsafe Native(void* ptr, T value) {
            if (ptr == null) throw new ArgumentNullException(nameof(ptr));

            this.ptr = ptr;
            Value = value;
        }

        unsafe void Allocate() {
            ptr = (void*)Marshal.AllocHGlobal(elementSize);
            allocated = true;
        }

        public IntPtr Address {
            get {
                unsafe {
                    return (IntPtr)ptr;
                }
            }
        }

        public ref T Value {
            get {
                unsafe {
                    return ref Unsafe.AsRef<T>(ptr);
                }
            }
        }

        public void Dispose() {
            Dispose(true);
        }

        void Dispose(bool disposing) {
            if (disposed) return;

            if (allocated) {
                unsafe {
                    Marshal.FreeHGlobal((IntPtr)ptr);
                }
            }

            disposed = true;
        }

        ~Native() {
            Dispose(false);
        }
    }
}
