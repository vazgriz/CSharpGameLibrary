using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace CSGL.Vulkan {
    public class PipelineCacheCreateInfo {
        public IList<byte> initialData;
    }

    public class VkPipelineCache : INative<Unmanaged.VkPipelineCache>, IDisposable {
        bool disposed;
        Unmanaged.VkPipelineCache pipelineCache;
        public VkDevice Device { get; private set; }

        public Unmanaged.VkPipelineCache Native {
            get {
                return pipelineCache;
            }
        }

        public VkPipelineCache(VkDevice device, PipelineCacheCreateInfo info) {
            if (device == null) throw new ArgumentNullException(nameof(device));
            if (info == null) throw new ArgumentNullException(nameof(info));

            Device = device;

            CreateCache(info);
        }

        void CreateCache(PipelineCacheCreateInfo mInfo) {
            if (mInfo.initialData == null) throw new ArgumentNullException(nameof(mInfo.initialData));

            var info = new Unmanaged.VkPipelineCacheCreateInfo();
            info.sType = VkStructureType.PipelineCacheCreateInfo;
            info.initialDataSize = (IntPtr)mInfo.initialData.Count;

            var initialDataMarshalled = new NativeArray<byte>(mInfo.initialData);
            info.pInitialData = initialDataMarshalled.Address;

            using (initialDataMarshalled) {
                var result = Device.Commands.createPipelineCache(Device.Native, ref info, Device.Instance.AllocationCallbacks, out pipelineCache);
                if (result != VkResult.Success) throw new PipelineCacheException(result, string.Format("Error creating pipeline cache: {0}", result));
            }
        }

        public byte[] GetData() {
            ulong length = 0;
            Device.Commands.getPipelineCacheData(Device.Native, pipelineCache, ref length, IntPtr.Zero);
            byte[] result = new byte[length];

            unsafe {
                fixed (byte* ptr = result) {
                    Device.Commands.getPipelineCacheData(Device.Native, pipelineCache, ref length, (IntPtr)ptr);
                }
            }

            return result;
        }

        public void Merge(IList<VkPipelineCache> srcCaches) {
            unsafe {
                Unmanaged.VkPipelineCache* srcNative = stackalloc Unmanaged.VkPipelineCache[srcCaches.Count];
                Interop.Marshal<Unmanaged.VkPipelineCache, VkPipelineCache>(srcCaches, srcNative);

                Device.Commands.mergePipelineCache(Device.Native, pipelineCache, (uint)srcCaches.Count, (IntPtr)srcNative);
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

        ~VkPipelineCache() {
            Dispose(false);
        }
    }

    public class PipelineCacheException : VulkanException {
        public PipelineCacheException(VkResult result, string message) : base(result, message) { }
    }
}
