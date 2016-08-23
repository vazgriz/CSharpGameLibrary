using System;
using System.Runtime.InteropServices;

namespace CSGL {
    public class PinnedArray<T> : IDisposable where T : struct {
        T[] array;
        GCHandle handle;
        bool disposed = false;
        static int elementSize;

        static PinnedArray() {
            elementSize = Marshal.SizeOf<T>();
        }

        public PinnedArray(T[] array) {
            this.array = array;
            Init();
        }

        public PinnedArray(int count) {
            if (count < 0) throw new ArgumentOutOfRangeException(nameof(count), "Count must be positive");
            array = new T[count];
            Init();
        }

        void Init() {
            handle = GCHandle.Alloc(array, GCHandleType.Pinned);
        }

        public IntPtr Address {
            get {
                return handle.AddrOfPinnedObject();
            }
        }

        public int Length {
            get {
                return array.Length;
            }
        }

        public T this[int i] {
            get {
                return array[i];
            }
            set {
                array[i] = value;
            }
        }

        public IntPtr AddressOf(int i) {
            return Address + (i * elementSize);
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        void Dispose(bool diposing) {
            if (disposed) return;

            handle.Free();

            if (diposing) {
                array = null;
            }

            disposed = true;
        }

        ~PinnedArray() {
            Dispose(false);
        }
    }
}
