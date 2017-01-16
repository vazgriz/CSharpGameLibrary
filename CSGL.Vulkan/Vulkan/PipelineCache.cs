using System;

namespace CSGL.Vulkan {
    public class PipelineCacheCreateInfo {
        public byte[] initialData;
    }

    public class PipelineCache : INative<VkPipelineCache> {
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
            info.initialDataSize = (ulong)mInfo.initialData.LongLength;
            var initialDataMarshalled = new PinnedArray<byte>(mInfo.initialData);
            info.pInitialData = initialDataMarshalled.Address;

            using (initialDataMarshalled) {
                var result = Device.Commands.createPipelineCache(Device.Native, ref info, Device.Instance.AllocationCallbacks, out pipelineCache);
                if (result != VkResult.Success) throw new PipelineCacheException(string.Format("Error creating pipeline cache: {0}", result));
            }
        }
    }

    public class PipelineCacheException : Exception {
        public PipelineCacheException(string message) : base(message) { }
    }
}
