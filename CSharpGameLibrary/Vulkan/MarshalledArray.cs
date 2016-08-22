using System;
using System.Runtime.InteropServices;

namespace CSGL.Vulkan {
    public class MarshalledArray<T> : IDisposable where T : struct {
        int count;
        int elementSize;
        IntPtr ptr;
        bool disposed = false;

        public MarshalledArray(int count) {
            Init(count);
        }

        public MarshalledArray(T[] array) {
            if (array == null) throw new ArgumentNullException(nameof(array));

            Init(array.Length);

            for (int i = 0; i < array.Length; i++) {
                Marshal.StructureToPtr(array[i], GetAddress(i), false);
            }
        }

        void Init(int count) {
            this.count = count;
            elementSize = Marshal.SizeOf<T>();
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

        public void Dispose() {
            if (disposed) return;

            for (int i = 0; i < count; i++) {
                Marshal.DestroyStructure<T>(GetAddress(i));
            }

            Marshal.FreeHGlobal(ptr);
        }
    }
}
