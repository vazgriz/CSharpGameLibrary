using System;
using System.Collections.Generic;

using CSGL.Vulkan.Unmanaged;

namespace CSGL.Vulkan.Managed {
    public class GraphicsPipelineCreateInfo {
        public VkPipelineCreateFlags Flags { get; set; }
        public PipelineShaderStageCreateInfo[] Stages { get; set; }
        public PipelineVertexInputStateCreateInfo VertexInputState { get; set; }
        public PipelineInputAssemblyStateCreateInfo InputAssemblyState { get; set; }
        public PipelineTessellationStateCreateInfo TessellationState { get; set; }
        public PipelineViewportStateCreateInfo ViewportState { get; set; }
        public PipelineRasterizationStateCreateInfo RasterizationState { get; set; }
        public PipelineMultisampleStateCreateInfo MultisampleState { get; set; }
        public PipelineDepthStencilStateCreateInfo DepthStencilState { get; set; }
        public PipelineColorBlendStateCreateInfo ColorBlendState { get; set; }
        public PipelineDynamicStateCreateInfo DynamicState { get; set; }
        public PipelineLayout Layout { get; set; }
        public RenderPass RenderPass { get; set; }
        public uint Subpass { get; set; }
        public Pipeline BasePipeline { get; set; }
        public int BasePipelineIndex { get; set; }
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
            var marshalledArrays = new List<IDisposable>(mInfos.Length);

            for (int i = 0; i < mInfos.Length; i++) {
                VkGraphicsPipelineCreateInfo info = new VkGraphicsPipelineCreateInfo();
                var mInfo = mInfos[i];

                info.sType = VkStructureType.StructureTypeGraphicsPipelineCreateInfo;
                info.flags = mInfo.Flags;
                
                var stagesMarshalled = new MarshalledArray<VkPipelineShaderStageCreateInfo>(count);
                for (int j = 0; j < count; j++) {
                    stagesMarshalled[j] = mInfo.Stages[j].GetNative(marshalledArrays);
                }

                info.stageCount = (uint)stagesMarshalled.Count;
                info.pStages = stagesMarshalled.Address;

                marshalledArrays.Add(stagesMarshalled);

                if (mInfo.VertexInputState != null) {
                    var m = new Marshalled<VkPipelineVertexInputStateCreateInfo>(mInfo.VertexInputState.GetNative(marshalledArrays));
                    info.pVertexInputState = m.Address;
                    marshalledArrays.Add(m);
                }

                if (mInfo.InputAssemblyState != null) {
                    var m = new Marshalled<VkPipelineInputAssemblyStateCreateInfo>(mInfo.InputAssemblyState.GetNative());
                    info.pInputAssemblyState = m.Address;
                    marshalledArrays.Add(m);
                }

                if (mInfo.TessellationState != null) {
                    var m = new Marshalled<VkPipelineTessellationStateCreateInfo>(mInfo.TessellationState.GetNative());
                    info.pTessellationState = m.Address;
                    marshalledArrays.Add(m);
                }

                if (mInfo.ViewportState != null) {
                    var m = new Marshalled<VkPipelineViewportStateCreateInfo>(mInfo.ViewportState.GetNative(marshalledArrays));
                    info.pViewportState = m.Address;
                }

                if (mInfo.RasterizationState != null) {
                    var m = new Marshalled<VkPipelineRasterizationStateCreateInfo>(mInfo.RasterizationState.GetNative());
                    info.pRasterizationState = m.Address;
                    marshalledArrays.Add(m);
                }

                if (mInfo.MultisampleState != null) {
                    var m = new Marshalled<VkPipelineMultisampleStateCreateInfo>(mInfo.MultisampleState.GetNative());
                    info.pMultisampleState = m.Address;
                    marshalledArrays.Add(m);
                }

                if (mInfo.DepthStencilState != null) {
                    var m = new Marshalled<VkPipelineDepthStencilStateCreateInfo>(mInfo.DepthStencilState.GetNative());
                    info.pDepthStencilState = m.Address;
                    marshalledArrays.Add(m);
                }

                if (mInfo.ColorBlendState != null) {
                    var m = new Marshalled<VkPipelineColorBlendStateCreateInfo>(mInfo.ColorBlendState.GetNative(marshalledArrays));
                    info.pColorBlendState = m.Address;
                    marshalledArrays.Add(m);
                }

                if (mInfo.DynamicState != null) {
                    var m = new Marshalled<VkPipelineDynamicStateCreateInfo>(mInfo.DynamicState.GetNative(marshalledArrays));
                    info.pDynamicState = m.Address;
                    marshalledArrays.Add(m);
                }

                info.layout = mInfo.Layout.Native;

                if (mInfo.RenderPass != null) {
                    info.renderPass = mInfo.RenderPass.Native;
                }

                info.subpass = mInfo.Subpass;
                if (mInfo.BasePipeline != null) {
                    info.basePipelineHandle = mInfo.BasePipeline.Native;
                }
                info.basePipelineIndex = mInfo.BasePipelineIndex;

                infos[i] = info;
            }

            var infosMarshalled = new MarshalledArray<VkGraphicsPipelineCreateInfo>(infos);
            var pipelinesMarshalled = new MarshalledArray<VkPipeline>(count);

            try {
                var result = device.Commands.createGraphicsPiplines(
                    device.Native, cache, 
                    (uint)count, infosMarshalled.Address,
                    device.Instance.AllocationCallbacks, pipelinesMarshalled.Address);

                if (result != VkResult.Success) throw new PipelineException(string.Format("Error creating pipeline: {0}", result));

                for (int i = 0; i < count; i++) {
                    pipelineResults[i] = pipelinesMarshalled[i];
                }
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
