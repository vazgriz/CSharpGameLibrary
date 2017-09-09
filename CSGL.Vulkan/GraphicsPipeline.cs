using System;
using System.Collections.Generic;

namespace CSGL.Vulkan {
    public class GraphicsPipelineCreateInfo {
        public VkPipelineCreateFlags flags;
        public List<PipelineShaderStageCreateInfo> stages;
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
        public Pipeline basePipelineHandle;
        public int basePipelineIndex;
    }

    public class GraphicsPipeline : Pipeline {
        internal GraphicsPipeline(Device device, VkPipeline pipeline) {
            Device = device;
            this.pipeline = pipeline;
        }

        public GraphicsPipeline(Device device, GraphicsPipelineCreateInfo info, PipelineCache cache) {
            if (device == null) throw new ArgumentNullException(nameof(device));
            if (info == null) throw new ArgumentNullException(nameof(info));

            Device = device;

            VkPipelineCache nativeCache = VkPipelineCache.Null;
            if (cache != null) {
                nativeCache = cache.Native;
            }

            pipeline = CreatePipelinesInternal(device, new GraphicsPipelineCreateInfo[] { info }, nativeCache)[0];
        }

        static internal VkPipeline[] CreatePipelinesInternal(Device device, GraphicsPipelineCreateInfo[] mInfos, VkPipelineCache cache) {
            int count = mInfos.Length;
            var infosMarshalled = new MarshalledArray<VkGraphicsPipelineCreateInfo>(count);
            var pipelineResults = new VkPipeline[count];
            var marshalledArrays = new DisposableList<IDisposable>(count);

            for (int i = 0; i < count; i++) {
                VkGraphicsPipelineCreateInfo info = new VkGraphicsPipelineCreateInfo();
                var mInfo = mInfos[i];

                info.sType = VkStructureType.GraphicsPipelineCreateInfo;
                info.flags = mInfo.flags;

                int stagesCount = mInfo.stages.Count;
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
                    var m = new Marshalled<VkPipelineMultisampleStateCreateInfo>(mInfo.multisampleState.GetNative(marshalledArrays));
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
                if (mInfo.basePipelineHandle != null) {
                    info.basePipelineHandle = mInfo.basePipelineHandle.Native;
                }
                info.basePipelineIndex = mInfo.basePipelineIndex;

                infosMarshalled[i] = info;
            }

            using (infosMarshalled)
            using (marshalledArrays)
            using (var pipelinesMarshalled = new PinnedArray<VkPipeline>(pipelineResults)) {
                var result = device.Commands.createGraphicsPiplines(
                    device.Native, cache,
                    (uint)count, infosMarshalled.Address,
                    device.Instance.AllocationCallbacks, pipelinesMarshalled.Address);

                if (result != VkResult.Success) throw new PipelineException(result, string.Format("Error creating pipeline: {0}", result));
                return pipelineResults;
            }
        }
    }
}
