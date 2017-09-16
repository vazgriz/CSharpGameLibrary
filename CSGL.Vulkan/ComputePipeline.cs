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
        public ComputePipeline basePipelineHandle;
        public int basePipelineIndex;
    }

    public class ComputePipeline : Pipeline {
        internal ComputePipeline(Device device, Unmanaged.VkPipeline pipeline, ComputePipelineCreateInfo info) {
            Device = device;
            this.pipeline = pipeline;
            SetProperties(info);
        }

        public ComputePipeline(Device device, ComputePipelineCreateInfo info, PipelineCache cache) {
            if (device == null) throw new ArgumentNullException(nameof(device));
            if (info == null) throw new ArgumentNullException(nameof(info));

            Device = device;

            var nativeCache = Unmanaged.VkPipelineCache.Null;
            if (cache != null) {
                nativeCache = cache.Native;
            }

            pipeline = CreatePipelinesInternal(device, new ComputePipelineCreateInfo[] { info }, nativeCache)[0];

            SetProperties(info);
        }

        void SetProperties(ComputePipelineCreateInfo info) {
            Flags = info.flags;
            Layout = info.layout;
        }

        static internal IList<Unmanaged.VkPipeline> CreatePipelinesInternal(Device device, IList<ComputePipelineCreateInfo> mInfos, Unmanaged.VkPipelineCache cache) {
            int count = mInfos.Count;
            var infosMarshalled = new MarshalledArray<Unmanaged.VkComputePipelineCreateInfo>(count);
            var pipelineResults = new List<Unmanaged.VkPipeline>(count);
            var marshalledArrays = new DisposableList<IDisposable>(count);

            for (int i = 0; i < count; i++) {
                var mInfo = mInfos[i];
                var info = new Unmanaged.VkComputePipelineCreateInfo();
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
            using (marshalledArrays) {
                unsafe {
                    var pipelinesNative = stackalloc Unmanaged.VkPipeline[count];

                    var result = device.Commands.createComputePipelines(
                        device.Native, cache,
                        (uint)count, infosMarshalled.Address,
                        device.Instance.AllocationCallbacks, (IntPtr)pipelinesNative);

                    if (result != VkResult.Success) throw new PipelineException(result, string.Format("Error creating pipeline: {0}", result));

                    for (int i = 0; i < count; i++) {
                        pipelineResults.Add(pipelinesNative[i]);
                    }

                    return pipelineResults;
                }
            }
        }
    }
}
