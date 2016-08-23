using System;
using System.Runtime.InteropServices;
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

                IntPtr vertexInputPtr = Marshal.AllocHGlobal(Marshal.SizeOf<VkPipelineVertexInputStateCreateInfo>());
                IntPtr inputAssemblyPtr = Marshal.AllocHGlobal(Marshal.SizeOf<VkPipelineInputAssemblyStateCreateInfo>());
                IntPtr tesselationPtr = Marshal.AllocHGlobal(Marshal.SizeOf<VkPipelineTessellationStateCreateInfo>());
                IntPtr viewportPtr = Marshal.AllocHGlobal(Marshal.SizeOf<VkPipelineViewportStateCreateInfo>());
                IntPtr rasterizationPtr = Marshal.AllocHGlobal(Marshal.SizeOf<VkPipelineRasterizationStateCreateInfo>());
                IntPtr multisamplePtr = Marshal.AllocHGlobal(Marshal.SizeOf<VkPipelineMultisampleStateCreateInfo>());
                IntPtr depthPtr = Marshal.AllocHGlobal(Marshal.SizeOf<VkPipelineDepthStencilStateCreateInfo>());
                IntPtr colorPtr = Marshal.AllocHGlobal(Marshal.SizeOf<VkPipelineColorBlendStateCreateInfo>());
                IntPtr dynamicPtr = Marshal.AllocHGlobal(Marshal.SizeOf<VkPipelineDynamicStateCreateInfo>());

                if (mInfo.VertexInputState != null) {
                    Marshal.StructureToPtr(mInfo.VertexInputState.GetNative(marshalledArrays), vertexInputPtr, false);
                    info.pVertexInputState = vertexInputPtr;
                }

                if (mInfo.InputAssemblyState != null) {
                    Marshal.StructureToPtr(mInfo.InputAssemblyState.GetNative(), inputAssemblyPtr, false);
                    info.pInputAssemblyState = inputAssemblyPtr;
                }

                if (mInfo.TessellationState != null) {
                    Marshal.StructureToPtr(mInfo.TessellationState.GetNative(), tesselationPtr, false);
                    info.pTessellationState = tesselationPtr;
                }

                if (mInfo.ViewportState != null) {
                    Marshal.StructureToPtr(mInfo.ViewportState.GetNative(marshalledArrays), viewportPtr, false);
                    info.pViewportState = viewportPtr;
                }

                if (mInfo.RasterizationState != null) {
                    Marshal.StructureToPtr(mInfo.RasterizationState.GetNative(), rasterizationPtr, false);
                    info.pRasterizationState = rasterizationPtr;
                }

                if (mInfo.MultisampleState != null) {
                    Marshal.StructureToPtr(mInfo.MultisampleState.GetNative(), multisamplePtr, false);
                    info.pMultisampleState = multisamplePtr;
                }

                if (mInfo.DepthStencilState != null) {
                    Marshal.StructureToPtr(mInfo.DepthStencilState.GetNative(), depthPtr, false);
                    info.pDepthStencilState = depthPtr;
                }

                if (mInfo.ColorBlendState != null) {
                    Marshal.StructureToPtr(mInfo.ColorBlendState.GetNative(marshalledArrays), colorPtr, false);
                    info.pColorBlendState = colorPtr;
                }

                if (mInfo.DynamicState != null) {
                    Marshal.StructureToPtr(mInfo.DynamicState.GetNative(marshalledArrays), dynamicPtr, false);
                    info.pDynamicState = dynamicPtr;
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
                    var pipeline = pipelinesMarshalled[i];
                    pipelineResults[i] = pipeline;
                }
            }
            finally {
                pipelinesMarshalled.Dispose();
                infosMarshalled.Dispose();

                foreach (var m in marshalledArrays) {
                    m.Dispose();
                }

                for (int i = 0; i < count; i++) {
                    var info = infos[i];
                    if (info.pVertexInputState != IntPtr.Zero) Marshal.DestroyStructure<VkPipelineVertexInputStateCreateInfo>(info.pVertexInputState);
                    if (info.pInputAssemblyState != IntPtr.Zero) Marshal.DestroyStructure<VkPipelineInputAssemblyStateCreateInfo>(info.pInputAssemblyState);
                    if (info.pTessellationState != IntPtr.Zero) Marshal.DestroyStructure<VkPipelineTessellationStateCreateInfo>(info.pTessellationState);
                    if (info.pViewportState != IntPtr.Zero) Marshal.DestroyStructure<VkPipelineViewportStateCreateInfo>(info.pViewportState);
                    if (info.pRasterizationState != IntPtr.Zero) Marshal.DestroyStructure<VkPipelineRasterizationStateCreateInfo>(info.pRasterizationState);
                    if (info.pMultisampleState != IntPtr.Zero) Marshal.DestroyStructure<VkPipelineMultisampleStateCreateInfo>(info.pMultisampleState);
                    if (info.pDepthStencilState != IntPtr.Zero) Marshal.DestroyStructure<VkPipelineDepthStencilStateCreateInfo>(info.pDepthStencilState);
                    if (info.pColorBlendState != IntPtr.Zero) Marshal.DestroyStructure<VkPipelineColorBlendStateCreateInfo>(info.pColorBlendState);
                    if (info.pDynamicState != IntPtr.Zero) Marshal.DestroyStructure<VkPipelineDynamicStateCreateInfo>(info.pDynamicState);
                    
                    Marshal.FreeHGlobal(info.pVertexInputState);
                    Marshal.FreeHGlobal(info.pInputAssemblyState);
                    Marshal.FreeHGlobal(info.pTessellationState);
                    Marshal.FreeHGlobal(info.pViewportState);
                    Marshal.FreeHGlobal(info.pRasterizationState);
                    Marshal.FreeHGlobal(info.pMultisampleState);
                    Marshal.FreeHGlobal(info.pDepthStencilState);
                    Marshal.FreeHGlobal(info.pColorBlendState);
                    Marshal.FreeHGlobal(info.pDynamicState);
                }
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
