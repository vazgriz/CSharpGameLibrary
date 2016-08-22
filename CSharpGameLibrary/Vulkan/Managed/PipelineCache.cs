using System;
using System.Runtime.InteropServices;

namespace CSGL.Vulkan.Managed {
    public class PipelineCacheCreateInfo {
        public byte[] InitialData;
    }

    public class PipelineCache {
        VkPipelineCache pipelineCache;
        public Device Device { get; private set; }

        public VkPipelineCache Native {
            get {
                return pipelineCache;
            }
        }

        public PipelineCache(Device device) {
            Device = device;

        }

        void CreateCache(PipelineCacheCreateInfo mInfo) {
            var info = new VkPipelineCacheCreateInfo();
            info.initialDataSize = (ulong)mInfo.InitialData.LongLength;
            info.pInitialData = mInfo.InitialData;

            IntPtr infoPtr = Marshal.AllocHGlobal(Marshal.SizeOf<VkPipelineCacheCreateInfo>());
            Marshal.StructureToPtr(info, infoPtr, false);

            IntPtr pipelineCachePtr = Marshal.AllocHGlobal(Marshal.SizeOf<VkPipelineCache>());

            try {
                var result = Device.Commands.createPipelineCache(Device.Native, infoPtr, Device.Instance.AllocationCallbacks, pipelineCachePtr);
                if (result != VkResult.Success) throw new PipelineCacheException(string.Format("Error creating pipeline cache: {0}", result));

                pipelineCache = Marshal.PtrToStructure<VkPipelineCache>(pipelineCachePtr);
            }
            finally {
                Marshal.DestroyStructure<VkPipelineCacheCreateInfo>(infoPtr);

                Marshal.FreeHGlobal(infoPtr);
                Marshal.FreeHGlobal(pipelineCachePtr);
            }
        }
    }

    public class PipelineCacheException : Exception {
        public PipelineCacheException(string message) : base(message) { }
    }
}
