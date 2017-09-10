using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSGL.Vulkan {
    public class ComputePipelineCreateInfo {
        public VkPipelineCreateFlags flags;
        public PipelineShaderStageCreateInfo stage;
        public PipelineLayout layout;
        public Pipeline basePipelineHandle;
        public int basePipelineIndex;
    }

    public class ComputePipeline : Pipeline {
        public Pipeline BasePipeline { get; internal set; }

        internal ComputePipeline(Device device, VkPipeline pipeline, ComputePipelineCreateInfo info) {
            Device = device;
            this.pipeline = pipeline;
            SetProperties(info);
        }

        public ComputePipeline(Device device, ComputePipelineCreateInfo info, PipelineCache cache) {
            if (device == null) throw new ArgumentNullException(nameof(device));
            if (info == null) throw new ArgumentNullException(nameof(info));

            Device = device;

            VkPipelineCache nativeCache = VkPipelineCache.Null;
            if (cache != null) {
                nativeCache = cache.Native;
            }

            pipeline = CreatePipelinesInternal(device, new ComputePipelineCreateInfo[] { info }, nativeCache)[0];

            SetProperties(info);

            //not possible to use basePipelineIndex
            if ((info.flags & VkPipelineCreateFlags.DerivativeBit) == VkPipelineCreateFlags.DerivativeBit && info.basePipelineHandle != null) {
                BasePipeline = info.basePipelineHandle;
            }
        }

        void SetProperties(ComputePipelineCreateInfo info) {
            Flags = info.flags;
            Layout = info.layout;
        }

        static internal VkPipeline[] CreatePipelinesInternal(Device device, ComputePipelineCreateInfo[] mInfos, VkPipelineCache cache) {
            int count = mInfos.Length;
            var infosMarshalled = new MarshalledArray<VkComputePipelineCreateInfo>(count);
            var pipelineResults = new VkPipeline[count];
            var marshalledArrays = new DisposableList<IDisposable>(count);

            for (int i = 0; i < count; i++) {
                var mInfo = mInfos[i];
                VkComputePipelineCreateInfo info = new VkComputePipelineCreateInfo();
                info.sType = VkStructureType.ComputePipelineCreateInfo;
                info.flags = mInfo.flags;

                info.stage = mInfo.stage.GetNative(marshalledArrays);

                info.layout = mInfo.layout.Native;
                if (mInfo.basePipelineHandle != null) {
                    info.basePipelineHandle = mInfo.basePipelineHandle.Native;
                }
                info.basePipelineIndex = mInfo.basePipelineIndex;

                infosMarshalled[i] = info;
            }

            using (infosMarshalled)
            using (marshalledArrays)
            using (var pipelinesMarshalled = new PinnedArray<VkPipeline>(pipelineResults)) {
                var result = device.Commands.createComputePipelines(
                    device.Native, cache,
                    (uint)count, infosMarshalled.Address,
                    device.Instance.AllocationCallbacks, pipelinesMarshalled.Address);

                if (result != VkResult.Success) throw new PipelineException(result, string.Format("Error creating pipeline: {0}", result));
                return pipelineResults;
            }
        }
    }
}
