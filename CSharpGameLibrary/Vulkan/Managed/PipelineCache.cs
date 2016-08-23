using System;

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
            
            var infoMarshalled = new Marshalled<VkPipelineCacheCreateInfo>(info);
            var pipelineCacheMarshalled = new Marshalled<VkPipelineCache>();

            try {
                var result = Device.Commands.createPipelineCache(Device.Native, infoMarshalled.Address, Device.Instance.AllocationCallbacks, pipelineCacheMarshalled.Address);
                if (result != VkResult.Success) throw new PipelineCacheException(string.Format("Error creating pipeline cache: {0}", result));

                pipelineCache = pipelineCacheMarshalled.Value;
            }
            finally {
                infoMarshalled.Dispose();
                pipelineCacheMarshalled.Dispose();
            }
        }
    }

    public class PipelineCacheException : Exception {
        public PipelineCacheException(string message) : base(message) { }
    }
}
