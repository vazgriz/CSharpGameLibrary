using System;
using System.Collections.Generic;

namespace CSGL.Vulkan {
    public class VkGraphicsPipelineCreateInfo {
        public VkPipelineCreateFlags flags;
        public IList<VkPipelineShaderStageCreateInfo> stages;
        public VkPipelineVertexInputStateCreateInfo vertexInputState;
        public VkPipelineInputAssemblyStateCreateInfo inputAssemblyState;
        public VkPipelineTessellationStateCreateInfo tessellationState;
        public VkPipelineViewportStateCreateInfo viewportState;
        public VkPipelineRasterizationStateCreateInfo rasterizationState;
        public VkPipelineMultisampleStateCreateInfo multisampleState;
        public VkPipelineDepthStencilStateCreateInfo depthStencilState;
        public VkPipelineColorBlendStateCreateInfo colorBlendState;
        public VkPipelineDynamicStateCreateInfo dynamicState;
        public VkPipelineLayout layout;
        public VkRenderPass renderPass;
        public int subpass;
        public VkGraphicsPipeline basePipelineHandle;
        public int basePipelineIndex;
    }

    public class VkGraphicsPipeline : VkPipeline {
        public IList<VkPipelineShaderStageCreateInfo> Stages { get; private set; }
        public VkPipelineVertexInputStateCreateInfo VertexInputState { get; private set; }
        public VkPipelineInputAssemblyStateCreateInfo InputAssemblyState { get; private set; }
        public VkPipelineTessellationStateCreateInfo TessellationState { get; private set; }
        public VkPipelineViewportStateCreateInfo ViewportState { get; private set; }
        public VkPipelineRasterizationStateCreateInfo RasterizationState { get; private set; }
        public VkPipelineMultisampleStateCreateInfo MultisampleState { get; private set; }
        public VkPipelineDepthStencilStateCreateInfo DepthStencilState { get; private set; }
        public VkPipelineColorBlendStateCreateInfo ColorBlendState { get; private set; }
        public VkPipelineDynamicStateCreateInfo DynamicState { get; private set; }
        public VkRenderPass RenderPass { get; private set; }
        public int Subpass { get; private set; }

        internal VkGraphicsPipeline(VkDevice device, Unmanaged.VkPipeline pipeline, VkGraphicsPipelineCreateInfo info) {
            Device = device;
            this.pipeline = pipeline;
            SetProperties(info);
        }

        public VkGraphicsPipeline(VkDevice device, VkGraphicsPipelineCreateInfo info, VkPipelineCache cache) {
            if (device == null) throw new ArgumentNullException(nameof(device));
            if (info == null) throw new ArgumentNullException(nameof(info));

            Device = device;

            var nativeCache = Unmanaged.VkPipelineCache.Null;
            if (cache != null) {
                nativeCache = cache.Native;
            }

            pipeline = CreatePipelinesInternal(device, new VkGraphicsPipelineCreateInfo[] { info }, nativeCache)[0];

            SetProperties(info);
        }

        void SetProperties(VkGraphicsPipelineCreateInfo info) {
            Flags = info.flags;
            Layout = info.layout;
            if (info.stages != null) {
                var stages = new List<VkPipelineShaderStageCreateInfo>(info.stages.Count);
                foreach (var stage in info.stages) {
                    stages.Add(new VkPipelineShaderStageCreateInfo(stage));
                }
                Stages = stages.AsReadOnly();
            }
            if (info.vertexInputState != null) VertexInputState = new VkPipelineVertexInputStateCreateInfo(info.vertexInputState);
            if (info.inputAssemblyState != null) InputAssemblyState = new VkPipelineInputAssemblyStateCreateInfo(info.inputAssemblyState);
            if (info.tessellationState != null) TessellationState = new VkPipelineTessellationStateCreateInfo(info.tessellationState);
            if (info.viewportState != null) ViewportState = new VkPipelineViewportStateCreateInfo(info.viewportState);
            if (info.rasterizationState != null) RasterizationState = new VkPipelineRasterizationStateCreateInfo(info.rasterizationState);
            if (info.multisampleState != null) MultisampleState = new VkPipelineMultisampleStateCreateInfo(info.multisampleState);
            if (info.depthStencilState != null) DepthStencilState = new VkPipelineDepthStencilStateCreateInfo(info.depthStencilState);
            if (info.colorBlendState != null) ColorBlendState = new VkPipelineColorBlendStateCreateInfo(info.colorBlendState);
            if (info.dynamicState != null) DynamicState = new VkPipelineDynamicStateCreateInfo(info.dynamicState);
            RenderPass = info.renderPass;
            Subpass = info.subpass;
        }

        static internal IList<Unmanaged.VkPipeline> CreatePipelinesInternal(VkDevice device, IList<VkGraphicsPipelineCreateInfo> mInfos, Unmanaged.VkPipelineCache cache) {
            int count = mInfos.Count;
            var infosNative = new NativeArray<Unmanaged.VkGraphicsPipelineCreateInfo>(count);
            var pipelineResults = new List<Unmanaged.VkPipeline>(count);
            var nativeArrays = new DisposableList<IDisposable>(count);

            for (int i = 0; i < count; i++) {
                var info = new Unmanaged.VkGraphicsPipelineCreateInfo();
                var mInfo = mInfos[i];

                info.sType = VkStructureType.GraphicsPipelineCreateInfo;
                info.flags = mInfo.flags;

                int stagesCount = mInfo.stages.Count;
                var stagesNative = new NativeArray<Unmanaged.VkPipelineShaderStageCreateInfo>(stagesCount);
                for (int j = 0; j < stagesCount; j++) {
                    stagesNative[j] = mInfo.stages[j].GetNative(nativeArrays);
                }

                info.stageCount = (uint)stagesCount;
                info.pStages = stagesNative.Address;

                nativeArrays.Add(stagesNative);

                if (mInfo.vertexInputState != null) {
                    var m = new Native<Unmanaged.VkPipelineVertexInputStateCreateInfo>(mInfo.vertexInputState.GetNative(nativeArrays));
                    info.pVertexInputState = m.Address;
                    nativeArrays.Add(m);
                }

                if (mInfo.inputAssemblyState != null) {
                    var m = new Native<Unmanaged.VkPipelineInputAssemblyStateCreateInfo>(mInfo.inputAssemblyState.GetNative());
                    info.pInputAssemblyState = m.Address;
                    nativeArrays.Add(m);
                }

                if (mInfo.tessellationState != null) {
                    var m = new Native<Unmanaged.VkPipelineTessellationStateCreateInfo>(mInfo.tessellationState.GetNative());
                    info.pTessellationState = m.Address;
                    nativeArrays.Add(m);
                }

                if (mInfo.viewportState != null) {
                    var m = new Native<Unmanaged.VkPipelineViewportStateCreateInfo>(mInfo.viewportState.GetNative(nativeArrays));
                    info.pViewportState = m.Address;
                }

                if (mInfo.rasterizationState != null) {
                    var m = new Native<Unmanaged.VkPipelineRasterizationStateCreateInfo>(mInfo.rasterizationState.GetNative());
                    info.pRasterizationState = m.Address;
                    nativeArrays.Add(m);
                }

                if (mInfo.multisampleState != null) {
                    var m = new Native<Unmanaged.VkPipelineMultisampleStateCreateInfo>(mInfo.multisampleState.GetNative(nativeArrays));
                    info.pMultisampleState = m.Address;
                    nativeArrays.Add(m);
                }

                if (mInfo.depthStencilState != null) {
                    var m = new Native<Unmanaged.VkPipelineDepthStencilStateCreateInfo>(mInfo.depthStencilState.GetNative());
                    info.pDepthStencilState = m.Address;
                    nativeArrays.Add(m);
                }

                if (mInfo.colorBlendState != null) {
                    var m = new Native<Unmanaged.VkPipelineColorBlendStateCreateInfo>(mInfo.colorBlendState.GetNative(nativeArrays));
                    info.pColorBlendState = m.Address;
                    nativeArrays.Add(m);
                }

                if (mInfo.dynamicState != null) {
                    var m = new Native<Unmanaged.VkPipelineDynamicStateCreateInfo>(mInfo.dynamicState.GetNative(nativeArrays));
                    info.pDynamicState = m.Address;
                    nativeArrays.Add(m);
                }

                info.layout = mInfo.layout.Native;

                if (mInfo.renderPass != null) {
                    info.renderPass = mInfo.renderPass.Native;
                }

                info.subpass = (uint)mInfo.subpass;
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

                    var result = device.Commands.createGraphicsPiplines(
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
