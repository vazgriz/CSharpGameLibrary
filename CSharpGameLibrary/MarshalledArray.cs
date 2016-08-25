using System;
using System.Runtime.InteropServices;

namespace CSGL.Vulkan {
    public class MarshalledArray<T> : IDisposable where T : struct {
        int count;
        static int elementSize;
        IntPtr ptr;
        bool disposed = false;
        bool _unsafe = false;

        static MarshalledArray() {
            elementSize = Marshal.SizeOf<T>();
        }

        public static int ElementSize { get { return elementSize; } }

        public MarshalledArray(int count) {
            Init(count);
        }

        public MarshalledArray(T[] array) {
            if (array != null) {
                Init(array.Length);
                for (int i = 0; i < array.Length; i++) {
                    Marshal.StructureToPtr(array[i], GetAddress(i), false);
                }
            }
        }

        public unsafe MarshalledArray(void* ptr, T[] array) {   //meant to be used for stackalloc'ed memory
            if (ptr == null) throw new ArgumentNullException(nameof(ptr));

            if (array != null) {
                count = array.Length;
                this.ptr = (IntPtr)ptr;
                for (int i = 0; i < count; i++) {
                    Marshal.StructureToPtr(array[i], GetAddress(i), false);
                }
            }
            _unsafe = true;
        }

        void Init(int count) {
            this.count = count;
            ptr = Marshal.AllocHGlobal(count * elementSize);
        }

        public T this[int i] {
            get {
                if (i < 0 || i >= count) throw new IndexOutOfRangeException(string.Format("Index {0} is out of range [0, {1}]", i, count));
                return Marshal.PtrToStructure<T>(GetAddress(i));
            }
            set {
                if (i < 0 || i >= count) throw new IndexOutOfRangeException(string.Format("Index {0} is out of range [0, {1}]", i, count));
                Marshal.StructureToPtr(value, GetAddress(i), true);
            }
        }

        public IntPtr Address {
            get {
                return ptr;
            }
        }

        public IntPtr GetAddress(int i) {
            return ptr + (i * elementSize);
        }

        public int Count {
            get {
                return count;
            }
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Dispose(bool disposing) {
            if (disposed) return;

            if (ptr != IntPtr.Zero) {
                for (int i = 0; i < count; i++) {
                    Marshal.DestroyStructure<T>(GetAddress(i));
                }
            }


            if (!_unsafe) {
                Marshal.FreeHGlobal(ptr);
            }
        }

        ~MarshalledArray() {
            Dispose();
        }
    }
}
