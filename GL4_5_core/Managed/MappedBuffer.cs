using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace CSGL.OpenGL.GL4_5_core.Managed {
    public class MappedBuffer<T> : IDisposable where T : struct {
        public uint ID { get; private set; }
        public BufferAccessMask Mask { get; private set; }
        public IntPtr Ptr { get; private set; }
        public int Count { get; private set; }

        Action<uint> callback;
        T[] array;

        public static event Action<uint> OnFail;
        static int size;

        static MappedBuffer() {
            size = Marshal.SizeOf<T>();
        }

        public MappedBuffer(uint id, BufferAccessMask mask) {
            ID = id;
            Mask = mask;
            Map();
        }

        public MappedBuffer(uint id, BufferAccessMask mask, Action<uint> callback) {
            ID = id;
            Mask = mask;
            this.callback = callback;
            Map();
        }

        void Map() {
            Ptr = GL.MapNamedBuffer(ID, Mask);
            int bufferSize;
            GL.GetNamedBufferParameteriv(ID, BufferPName.BufferSize, out bufferSize);
            Count = bufferSize / size;
            array = new T[Count];

            GCHandle handle = GCHandle.Alloc(array, GCHandleType.Pinned);
            int bytes = Count * size;
            Copy(Ptr, handle.AddrOfPinnedObject(), bytes);
            handle.Free();
        }

        public T this[int i] {
            get {
                return array[i];
            }
            set {
                array[i] = value;
            }
        }

        public void Dispose() {
            GCHandle handle = GCHandle.Alloc(array, GCHandleType.Pinned);   //no method to copy from IntPtr to IntPtr
            int bytes = Count * size;
            Copy(handle.AddrOfPinnedObject(), Ptr, bytes);
            handle.Free();

            bool result = GL.UnmapNamedBuffer(ID);
            if (result) {
                OnFail?.Invoke(ID);
                callback?.Invoke(ID);
            }
            GC.SuppressFinalize(this);
        }

         void Copy(IntPtr source, IntPtr dest, int sizeInBytes) {
            unsafe
            {
                byte* src = (byte*)source;
                byte* dst = (byte*)dest;
                for (int i = 0; i < sizeInBytes; i++) {
                    *(dst + i) = *(src + i);
                }
            }
        }

        ~MappedBuffer() {
            Debug.Fail("MappedBuffer not disposed of properly");
#if DEBUG
            throw new MappedBufferException("Mapped buffer not disposed of properly");
#endif
        }
    }

    public class MappedBufferException : Exception {
        public MappedBufferException(string message) : base(message) { }
    }
}
