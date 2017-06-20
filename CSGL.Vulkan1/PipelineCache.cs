using System;
using System.Runtime.InteropServices;

namespace CSGL.Vulkan1 {
    public class PipelineCache : INative<VkPipelineCache>, IDisposable {
        bool disposed;
        VkPipelineCache pipelineCache;
        public Device Device { get; private set; }

        public VkPipelineCache Native {
            get {
                return pipelineCache;
            }
        }

        public PipelineCache(Device device, byte[] initialData) {
            Device = device;

            CreateCache(initialData);
        }

        void CreateCache(byte[] initialData) {
            var info = new VkPipelineCacheCreateInfo();
            info.sType = VkStructureType.PipelineCacheCreateInfo;
            info.initialDataSize = (ulong)initialData.Length;

            var initialDataMarshalled = new PinnedArray<byte>(initialData);
            info.pInitialData = initialDataMarshalled.Address;

            using (initialDataMarshalled) {
                var result = Device.Commands.createPipelineCache(Device.Native, ref info, Device.Instance.AllocationCallbacks, out pipelineCache);
                if (result != VkResult.Success) throw new PipelineCacheException(string.Format("Error creating pipeline cache: {0}", result));
            }
        }

        public byte[] GetData() {
            ulong length = 0;
            Device.Commands.getPipelineCacheData(Device.Native, pipelineCache, ref length, IntPtr.Zero);
            byte[] result = new byte[length];

            GCHandle handle = GCHandle.Alloc(result, GCHandleType.Pinned);
            Device.Commands.getPipelineCacheData(Device.Native, pipelineCache, ref length, handle.AddrOfPinnedObject());
            handle.Free();

            return result;
        }

        public void Merge(PipelineCache[] srcCaches) {
            unsafe
            {
                VkPipelineCache* srcNative = stackalloc VkPipelineCache[srcCaches.Length];
                Interop.Marshal(srcCaches, srcNative);

                Device.Commands.mergePipelineCache(Device.Native, pipelineCache, (uint)srcCaches.Length, (IntPtr)srcNative);
            }
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        void Dispose(bool disposing) {
            if (disposed) return;

            Device.Commands.destroyPipelineCache(Device.Native, pipelineCache, Device.Instance.AllocationCallbacks);

            disposed = true;
        }

        ~PipelineCache() {
            Dispose(false);
        }
    }

    public class PipelineCacheException : Exception {
        public PipelineCacheException(string message) : base(message) { }
    }
}
