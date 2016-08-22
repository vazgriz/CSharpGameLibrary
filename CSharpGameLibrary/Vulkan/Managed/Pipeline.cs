using System;
using System.Runtime.InteropServices;

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

            CreatePipelinesInternal(device, new GraphicsPipelineCreateInfo[] { info }, nativeCache);
        }

        public static Pipeline[] CreatePipelines(Device device, GraphicsPipelineCreateInfo[] infos, PipelineCache cache) {
            if (device == null) throw new ArgumentNullException(nameof(device));
            if (infos == null) throw new ArgumentNullException(nameof(infos));
            for (int i = 0; i < infos.Length; i++) {
                var info = infos[i];
                if (info == null) throw new ArgumentNullException(string.Format("Element {0} of {1} is null", i, nameof(infos)));
            }

            VkPipelineCache nativeCache = VkPipelineCache.Null;
            if (cache != null) {
                nativeCache = cache.Native;
            }

            return CreatePipelinesInternal(device, infos, nativeCache);
        }

        static Pipeline[] CreatePipelinesInternal(Device device, GraphicsPipelineCreateInfo[] mInfos, VkPipelineCache cache) {
            int count = mInfos.Length;
            var infos = new VkGraphicsPipelineCreateInfo[count];
            var pipelines = new VkPipeline[count];
            var pipelineResults = new Pipeline[count];

            for (int i = 0; i < mInfos.Length; i++) {
                VkGraphicsPipelineCreateInfo info = new VkGraphicsPipelineCreateInfo();
                var mInfo = mInfos[i];

                info.sType = VkStructureType.StructureTypeGraphicsPipelineCreateInfo;
                info.flags = mInfo.Flags;

                info.stageCount = (uint)mInfo.Stages.Length;
                info.pStages = new VkPipelineShaderStageCreateInfo[mInfo.Stages.Length];

                for (int j = 0; j < info.stageCount; j++) {
                    info.pStages[i] = mInfo.Stages[i].GetNative();
                }

                IntPtr vertexInputPtr = Marshal.AllocHGlobal(Marshal.SizeOf<VkPipelineVertexInputStateCreateInfo>());
                IntPtr inputAssemblyPtr = Marshal.AllocHGlobal(Marshal.SizeOf<VkPipelineInputAssemblyStateCreateInfo>());
                IntPtr tesselationPtr = Marshal.AllocHGlobal(Marshal.SizeOf<VkPipelineTessellationStateCreateInfo>());
                IntPtr viewportPtr = Marshal.AllocHGlobal(Marshal.SizeOf<VkPipelineViewportStateCreateInfo>());
                IntPtr rasterizationPtr = Marshal.AllocHGlobal(Marshal.SizeOf<VkPipelineRasterizationStateCreateInfo>());
                IntPtr multisamplePtr = Marshal.AllocHGlobal(Marshal.SizeOf<VkPipelineMultisampleStateCreateInfo>());
                IntPtr depthPtr = Marshal.AllocHGlobal(Marshal.SizeOf<VkPipelineDepthStencilStateCreateInfo>());
                IntPtr colorPtr = Marshal.AllocHGlobal(Marshal.SizeOf<VkPipelineColorBlendStateCreateInfo>());
                IntPtr dynamicPtr = Marshal.AllocHGlobal(Marshal.SizeOf<VkPipelineDynamicStateCreateInfo>());

                Marshal.StructureToPtr(mInfo.VertexInputState.GetNative(), vertexInputPtr, false);
                info.pVertexInputState = vertexInputPtr;

                Marshal.StructureToPtr(mInfo.InputAssemblyState.GetNative(), inputAssemblyPtr, false);
                info.pInputAssemblyState = inputAssemblyPtr;

                Marshal.StructureToPtr(mInfo.TessellationState.GetNative(), tesselationPtr, false);
                info.pTessellationState = tesselationPtr;

                Marshal.StructureToPtr(mInfo.ViewportState.GetNative(), viewportPtr, false);
                info.pViewportState = viewportPtr;

                Marshal.StructureToPtr(mInfo.RasterizationState.GetNative(), rasterizationPtr, false);
                info.pRasterizationState = rasterizationPtr;

                Marshal.StructureToPtr(mInfo.MultisampleState.GetNative(), multisamplePtr, false);
                info.pMultisampleState = multisamplePtr;

                Marshal.StructureToPtr(mInfo.DepthStencilState.GetNative(), depthPtr, false);
                info.pDepthStencilState = depthPtr;

                Marshal.StructureToPtr(mInfo.ColorBlendState.GetNative(), colorPtr, false);
                info.pColorBlendState = colorPtr;

                Marshal.StructureToPtr(mInfo.DynamicState.GetNative(), dynamicPtr, false);
                info.pDynamicState = dynamicPtr;

                info.layout = mInfo.Layout.Native;

                if (mInfo.RenderPass != null) {
                    info.renderPass = mInfo.RenderPass.Native;
                }

                info.subpass = mInfo.Subpass;
                info.basePipelineHandle = mInfo.BasePipeline.Native;
                info.basePipelineIndex = mInfo.BasePipelineIndex;

                infos[i] = info;

                IntPtr infoPtr = Marshal.AllocHGlobal(Marshal.SizeOf<VkGraphicsPipelineCreateInfo>());
                Marshal.StructureToPtr(info, infoPtr, false);
            }

            GCHandle infosHandle = GCHandle.Alloc(infos, GCHandleType.Pinned);
            GCHandle pipelinesHandle = GCHandle.Alloc(pipelines, GCHandleType.Pinned);

            try {
                var result = device.Commands.createGraphicsPiplines(
                    device.Native, cache, 
                    (uint)mInfos.Length, infosHandle.AddrOfPinnedObject(),
                    device.Instance.AllocationCallbacks, pipelinesHandle.AddrOfPinnedObject());

                if (result != VkResult.Success) throw new PipelineException(string.Format("Error creating pipeline: {0}", result));

                for (int i = 0; i < count; i++) {
                    pipelineResults[i] = new Pipeline(device, pipelines[i]);
                }
            }
            finally {
                for (int i = 0; i < count; i++) {
                    var info = infos[i];
                    Marshal.DestroyStructure<VkPipelineVertexInputStateCreateInfo>(info.pVertexInputState);
                    Marshal.DestroyStructure<VkPipelineInputAssemblyStateCreateInfo>(info.pInputAssemblyState);
                    Marshal.DestroyStructure<VkPipelineTessellationStateCreateInfo>(info.pTessellationState);
                    Marshal.DestroyStructure<VkPipelineViewportStateCreateInfo>(info.pViewportState);
                    Marshal.DestroyStructure<VkPipelineRasterizationStateCreateInfo>(info.pRasterizationState);
                    Marshal.DestroyStructure<VkPipelineMultisampleStateCreateInfo>(info.pMultisampleState);
                    Marshal.DestroyStructure<VkPipelineDepthStencilStateCreateInfo>(info.pDepthStencilState);
                    Marshal.DestroyStructure<VkPipelineColorBlendStateCreateInfo>(info.pColorBlendState);
                    Marshal.DestroyStructure<VkPipelineDynamicStateCreateInfo>(info.pDynamicState);
                    
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

                infosHandle.Free();
                pipelinesHandle.Free();
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
