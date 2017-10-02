using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSGL.Vulkan {
    public class VkComputePipelineCreateInfo {
        public VkPipelineCreateFlags flags;
        public VkPipelineShaderStageCreateInfo stage;
        public VkPipelineLayout layout;
        public VkComputePipeline basePipelineHandle;
        public int basePipelineIndex;
    }

    public class VkComputePipeline : VkPipeline {
        internal VkComputePipeline(VkDevice device, Unmanaged.VkPipeline pipeline, VkComputePipelineCreateInfo info) {
            Device = device;
            this.pipeline = pipeline;
            SetProperties(info);
        }

        public VkComputePipeline(VkDevice device, VkComputePipelineCreateInfo info, VkPipelineCache cache) {
            if (device == null) throw new ArgumentNullException(nameof(device));
            if (info == null) throw new ArgumentNullException(nameof(info));

            Device = device;

            var nativeCache = Unmanaged.VkPipelineCache.Null;
            if (cache != null) {
                nativeCache = cache.Native;
            }

            pipeline = CreatePipelinesInternal(device, new VkComputePipelineCreateInfo[] { info }, nativeCache)[0];

            SetProperties(info);
        }

        void SetProperties(VkComputePipelineCreateInfo info) {
            Flags = info.flags;
            Layout = info.layout;
        }

        static internal IList<Unmanaged.VkPipeline> CreatePipelinesInternal(VkDevice device, IList<VkComputePipelineCreateInfo> mInfos, Unmanaged.VkPipelineCache cache) {
            int count = mInfos.Count;
            var infosNative = new NativeArray<Unmanaged.VkComputePipelineCreateInfo>(count);
            var pipelineResults = new List<Unmanaged.VkPipeline>(count);
            var nativeArrays = new DisposableList<IDisposable>(count);

            for (int i = 0; i < count; i++) {
                var mInfo = mInfos[i];
                if (mInfo.stage == null) throw new ArgumentNullException(nameof(mInfo.stage));
                if (mInfo.layout == null) throw new ArgumentNullException(nameof(mInfo.layout));

                var info = new Unmanaged.VkComputePipelineCreateInfo();
                info.sType = VkStructureType.ComputePipelineCreateInfo;
                info.flags = mInfo.flags;

                info.stage = mInfo.stage.GetNative(nativeArrays);

                info.layout = mInfo.layout.Native;
                if (mInfo.basePipelineHandle != null) {
                    info.basePipelineHandle = mInfo.basePipelineHandle.Native;
                }
                info.basePipelineIndex = mInfo.basePipelineIndex;

                infosNative[i] = info;
            }

            using (infosNative)
            using (nativeArrays) {
                unsafe {
                    var pipelinesNative = stackalloc Unmanaged.VkPipeline[count];

                    var result = device.Commands.createComputePipelines(
                        device.Native, cache,
                        (uint)count, infosNative.Address,
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
