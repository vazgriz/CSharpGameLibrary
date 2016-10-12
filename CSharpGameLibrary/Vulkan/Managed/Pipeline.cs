using System;
using System.Collections.Generic;

using CSGL.Vulkan.Unmanaged;

namespace CSGL.Vulkan.Managed {
    public class GraphicsPipelineCreateInfo {
        public VkPipelineCreateFlags flags;
        public PipelineShaderStageCreateInfo[] stages;
        public PipelineVertexInputStateCreateInfo vertexInputState;
        public PipelineInputAssemblyStateCreateInfo inputAssemblyState;
        public PipelineTessellationStateCreateInfo tessellationState;
        public PipelineViewportStateCreateInfo viewportState;
        public PipelineRasterizationStateCreateInfo rasterizationState;
        public PipelineMultisampleStateCreateInfo multisampleState;
        public PipelineDepthStencilStateCreateInfo depthStencilState;
        public PipelineColorBlendStateCreateInfo colorBlendState;
        public PipelineDynamicStateCreateInfo dynamicState;
        public PipelineLayout layout;
        public RenderPass renderPass;
        public uint subpass;
        public Pipeline basePipeline;
        public int basePipelineIndex;
    }

    public class Pipeline : IDisposable {
        VkPipeline pipeline;
        public Device Device { get; private set; }

        public VkPipeline Native {
            get {
                return pipeline;
            }
        }

        internal Pipeline(Device device, VkPipeline pipeline) {
            Device = device;
            this.pipeline = pipeline;
        }

        public Pipeline(Device device, GraphicsPipelineCreateInfo info, PipelineCache cache) {
            if (device == null) throw new ArgumentNullException(nameof(device));
            if (info == null) throw new ArgumentNullException(nameof(info));

            Device = device;

            VkPipelineCache nativeCache = VkPipelineCache.Null;
            if (cache != null) {
                nativeCache = cache.Native;
            }

            pipeline = CreatePipelinesInternal(device, new GraphicsPipelineCreateInfo[] { info }, nativeCache)[0];
        }

        public static Pipeline[] CreatePipelines(Device device, GraphicsPipelineCreateInfo[] infos, PipelineCache cache) {
            if (device == null) throw new ArgumentNullException(nameof(device));
            if (infos == null) throw new ArgumentNullException(nameof(infos));
            for (int i = 0; i < infos.Length; i++) {
                if (infos[i] == null) throw new ArgumentNullException(string.Format("Element {0} of {1} is null", i, nameof(infos)));
            }

            VkPipelineCache nativeCache = VkPipelineCache.Null;
            if (cache != null) {
                nativeCache = cache.Native;
            }

            var pipelines = new Pipeline[infos.Length];
            var natives = CreatePipelinesInternal(device, infos, nativeCache);

            for (int i = 0; i < infos.Length; i++) {
                pipelines[i] = new Pipeline(device, natives[i]);
            }

            return pipelines;
        }

        static VkPipeline[] CreatePipelinesInternal(Device device, GraphicsPipelineCreateInfo[] mInfos, VkPipelineCache cache) {
            int count = mInfos.Length;
            var infos = new VkGraphicsPipelineCreateInfo[count];
            var pipelineResults = new VkPipeline[count];
            var marshalledArrays = new List<IDisposable>(count);

            for (int i = 0; i < count; i++) {
                VkGraphicsPipelineCreateInfo info = new VkGraphicsPipelineCreateInfo();
                var mInfo = mInfos[i];

                info.sType = VkStructureType.StructureTypeGraphicsPipelineCreateInfo;
                info.flags = mInfo.flags;

                int stagesCount = mInfo.stages.Length;
                var stagesMarshalled = new MarshalledArray<VkPipelineShaderStageCreateInfo>(stagesCount);
                for (int j = 0; j < stagesCount; j++) {
                    stagesMarshalled[j] = mInfo.stages[j].GetNative(marshalledArrays);
                }

                info.stageCount = (uint)stagesCount;
                info.pStages = stagesMarshalled.Address;

                marshalledArrays.Add(stagesMarshalled);

                if (mInfo.vertexInputState != null) {
                    var m = new Marshalled<VkPipelineVertexInputStateCreateInfo>(mInfo.vertexInputState.GetNative(marshalledArrays));
                    info.pVertexInputState = m.Address;
                    marshalledArrays.Add(m);
                }

                if (mInfo.inputAssemblyState != null) {
                    var m = new Marshalled<VkPipelineInputAssemblyStateCreateInfo>(mInfo.inputAssemblyState.GetNative());
                    info.pInputAssemblyState = m.Address;
                    marshalledArrays.Add(m);
                }

                if (mInfo.tessellationState != null) {
                    var m = new Marshalled<VkPipelineTessellationStateCreateInfo>(mInfo.tessellationState.GetNative());
                    info.pTessellationState = m.Address;
                    marshalledArrays.Add(m);
                }

                if (mInfo.viewportState != null) {
                    var m = new Marshalled<VkPipelineViewportStateCreateInfo>(mInfo.viewportState.GetNative(marshalledArrays));
                    info.pViewportState = m.Address;
                }

                if (mInfo.rasterizationState != null) {
                    var m = new Marshalled<VkPipelineRasterizationStateCreateInfo>(mInfo.rasterizationState.GetNative());
                    info.pRasterizationState = m.Address;
                    marshalledArrays.Add(m);
                }

                if (mInfo.multisampleState != null) {
                    var m = new Marshalled<VkPipelineMultisampleStateCreateInfo>(mInfo.multisampleState.GetNative());
                    info.pMultisampleState = m.Address;
                    marshalledArrays.Add(m);
                }

                if (mInfo.depthStencilState != null) {
                    var m = new Marshalled<VkPipelineDepthStencilStateCreateInfo>(mInfo.depthStencilState.GetNative());
                    info.pDepthStencilState = m.Address;
                    marshalledArrays.Add(m);
                }

                if (mInfo.colorBlendState != null) {
                    var m = new Marshalled<VkPipelineColorBlendStateCreateInfo>(mInfo.colorBlendState.GetNative(marshalledArrays));
                    info.pColorBlendState = m.Address;
                    marshalledArrays.Add(m);
                }

                if (mInfo.dynamicState != null) {
                    var m = new Marshalled<VkPipelineDynamicStateCreateInfo>(mInfo.dynamicState.GetNative(marshalledArrays));
                    info.pDynamicState = m.Address;
                    marshalledArrays.Add(m);
                }

                info.layout = mInfo.layout.Native;

                if (mInfo.renderPass != null) {
                    info.renderPass = mInfo.renderPass.Native;
                }

                info.subpass = mInfo.subpass;
                if (mInfo.basePipeline != null) {
                    info.basePipelineHandle = mInfo.basePipeline.Native;
                }
                info.basePipelineIndex = mInfo.basePipelineIndex;

                infos[i] = info;
            }

            var infosMarshalled = new MarshalledArray<VkGraphicsPipelineCreateInfo>(infos);
            var pipelinesMarshalled = new PinnedArray<VkPipeline>(pipelineResults);

            try {
                var result = device.Commands.createGraphicsPiplines(
                    device.Native, cache, 
                    (uint)count, infosMarshalled.Address,
                    device.Instance.AllocationCallbacks, pipelinesMarshalled.Address);

                if (result != VkResult.Success) throw new PipelineException(string.Format("Error creating pipeline: {0}", result));
            }
            finally {
                pipelinesMarshalled.Dispose();
                infosMarshalled.Dispose();

                foreach (var m in marshalledArrays) m.Dispose();
            }

            return pipelineResults;
        }

        public void Dispose() {
            Device.Commands.destroyPipeline(Device.Native, pipeline, Device.Instance.AllocationCallbacks);
        }
    }

    public class PipelineException : Exception {
        public PipelineException(string message) : base(message) { }
    }
}
