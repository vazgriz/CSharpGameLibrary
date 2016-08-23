using System;
using System.Collections.Generic;

namespace CSGL.Vulkan.Managed {
    public class PipelineShaderStageCreateInfo {
        public VkShaderStageFlags Stage { get; set; }
        public ShaderModule Module { get; set; }
        public string Name { get; set; }
        public IntPtr SpecializationInfo { get; set; }

        internal VkPipelineShaderStageCreateInfo GetNative(List<IDisposable> marshalled) {
            var result = new VkPipelineShaderStageCreateInfo();
            result.sType = VkStructureType.StructureTypePipelineShaderStageCreateInfo;
            result.stage = Stage;
            result.module = Module.Native;
            var nativeStr = Interop.GetUTF8(Name);
            var strMarshalled = new MarshalledArray<byte>(nativeStr);
            result.pName = strMarshalled.Address;
            result.pSpecializationInfo = SpecializationInfo;

            marshalled.Add(strMarshalled);

            return result;
        }
    }

    public class PipelineVertexInputStateCreateInfo {
        public VkVertexInputBindingDescription[] VertexBindingDescriptions { get; set; }
        public VkVertexInputAttributeDescription[] VertexAttributeDescriptions { get; set; }

        internal VkPipelineVertexInputStateCreateInfo GetNative(List<IDisposable> marshalled) {
            var result = new VkPipelineVertexInputStateCreateInfo();

            var attMarshalled = new MarshalledArray<VkVertexInputAttributeDescription>(VertexAttributeDescriptions);
            result.sType = VkStructureType.StructureTypePipelineVertexInputStateCreateInfo;
            result.vertexAttributeDescriptionCount = (uint)attMarshalled.Count;
            result.pVertexAttributeDescriptions = attMarshalled.Address;

            var bindMarshalled = new MarshalledArray<VkVertexInputBindingDescription>(VertexBindingDescriptions);
            result.vertexBindingDescriptionCount = (uint)bindMarshalled.Count;
            result.pVertexBindingDescriptions = bindMarshalled.Address;

            marshalled.Add(attMarshalled);
            marshalled.Add(bindMarshalled);

            return result;
        }
    }

    public class PipelineInputAssemblyStateCreateInfo {
        public VkPrimitiveTopology Topology { get; set; }
        public bool PrimitiveRestartEnable { get; set; }

        internal VkPipelineInputAssemblyStateCreateInfo GetNative() {
            var result = new VkPipelineInputAssemblyStateCreateInfo();
            result.sType = VkStructureType.StructureTypePipelineInputAssemblyStateCreateInfo;
            result.topology = Topology;
            result.primitiveRestartEnable = PrimitiveRestartEnable;

            return result;
        }
    }

    public class PipelineTessellationStateCreateInfo {
        public uint PatchControlPoints { get; set; }

        internal VkPipelineTessellationStateCreateInfo GetNative() {
            var result = new VkPipelineTessellationStateCreateInfo();
            result.sType = VkStructureType.StructureTypePipelineTessellationStateCreateInfo;
            result.patchControlPoints = PatchControlPoints;

            return result;
        }
    }

    public class PipelineViewportStateCreateInfo {
        public VkViewport[] Viewports { get; set; }
        public VkRect2D[] Scissors { get; set; }

        internal VkPipelineViewportStateCreateInfo GetNative(List<IDisposable> marshalled) {
            var result = new VkPipelineViewportStateCreateInfo();
            result.sType = VkStructureType.StructureTypePipelineViewportStateCreateInfo;

            var viewportMarshalled = new MarshalledArray<VkViewport>(Viewports);
            result.viewportCount = (uint)viewportMarshalled.Count;
            result.pViewports = viewportMarshalled.Address;

            var scissorMarshalled = new MarshalledArray<VkRect2D>(Scissors);
            result.scissorCount = (uint)scissorMarshalled.Count;
            result.pScissors = scissorMarshalled.Address;

            marshalled.Add(viewportMarshalled);
            marshalled.Add(scissorMarshalled);

            return result;
        }
    }

    public class PipelineRasterizationStateCreateInfo {
        public bool DepthClampEnable { get; set; }
        public bool RasterizerDiscardEnable { get; set; }
        public VkPolygonMode PolygonMode { get; set; }
        public VkCullModeFlags CullMode { get; set; }
        public VkFrontFace FrontFace { get; set; }
        public bool DepthBiasEnable { get; set; }
        public float DepthBiasConstantFactor { get; set; }
        public float DepthBiasClamp { get; set; }
        public float DepthBiasSlopeFactor { get; set; }
        public float LineWidth { get; set; }

        internal VkPipelineRasterizationStateCreateInfo GetNative() {
            var result = new VkPipelineRasterizationStateCreateInfo();
            result.sType = VkStructureType.StructureTypePipelineRasterizationStateCreateInfo;
            result.depthClampEnable = DepthClampEnable;
            result.rasterizerDiscardEnable = RasterizerDiscardEnable;
            result.polygonMode = PolygonMode;
            result.cullMode = CullMode;
            result.frontFace = FrontFace;
            result.depthBiasEnable = DepthBiasEnable;
            result.depthBiasConstantFactor = DepthBiasConstantFactor;
            result.depthBiasClamp = DepthBiasClamp;
            result.depthBiasSlopeFactor = DepthBiasSlopeFactor;
            result.lineWidth = LineWidth;

            return result;
        }
    }

    public class PipelineMultisampleStateCreateInfo {
        public VkSampleCountFlags RasterizationSamples { get; set; }
        public bool SampleShadingEnable { get; set; }
        public float MinSampleShading { get; set; }
        public uint[] SampleMask { get; set; }
        public bool AlphaToCoverageEnable { get; set; }
        public bool AlphaToOneEnable { get; set; }

        internal VkPipelineMultisampleStateCreateInfo GetNative() {
            var result = new VkPipelineMultisampleStateCreateInfo();
            result.sType = VkStructureType.StructureTypePipelineMultisampleStateCreateInfo;
            result.rasterizationSamples = RasterizationSamples;
            result.sampleShadingEnable = SampleShadingEnable;
            result.minSampleShading = MinSampleShading;
            result.pSampleMask = SampleMask;
            result.alphaToCoverageEnable = AlphaToCoverageEnable;
            result.alphaToOneEnable = AlphaToOneEnable;

            return result;
        }
    }

    public class PipelineDepthStencilStateCreateInfo {
        public bool DepthTestEnable { get; set; }
        public bool DepthWriteEnable { get; set; }
        public VkCompareOp DepthCompareOp { get; set; }
        public bool DepthBoundsTestEnable { get; set; }
        public bool StencilTestEnable { get; set; }
        public VkStencilOpState Front { get; set; }
        public VkStencilOpState Back { get; set; }
        public float MinDepthBounds { get; set; }
        public float MaxDepthBounds { get; set; }

        internal VkPipelineDepthStencilStateCreateInfo GetNative() {
            var result = new VkPipelineDepthStencilStateCreateInfo();
            result.sType = VkStructureType.StructureTypePipelineDepthStencilStateCreateInfo;
            result.depthTestEnable = DepthTestEnable;
            result.depthWriteEnable = DepthWriteEnable;
            result.depthCompareOp = DepthCompareOp;
            result.depthBoundsTestEnable = DepthBoundsTestEnable;
            result.stencilTestEnable = StencilTestEnable;
            result.front = Front;
            result.back = Back;
            result.minDepthBounds = MinDepthBounds;
            result.maxDepthBounds = MaxDepthBounds;

            return result;
        }
    }

    public class PipelineColorBlendAttachmentState {
        public bool BlendEnable { get; set; }
        public VkBlendFactor SrcColorBlendFactor { get; set; }
        public VkBlendFactor DstColorBlendFactor { get; set; }
        public VkBlendOp ColorBlendOp { get; set; }
        public VkBlendFactor SrcAlphaBlendFactor { get; set; }
        public VkBlendFactor DstAlphaBlendFactor { get; set; }
        public VkBlendOp AlphaBlendOp { get; set; }
        public VkColorComponentFlags ColorWriteMask { get; set; }

        internal VkPipelineColorBlendAttachmentState GetNative() {
            var result = new VkPipelineColorBlendAttachmentState();
            result.blendEnable = BlendEnable;
            result.srcColorBlendFactor = SrcColorBlendFactor;
            result.dstColorBlendFactor = DstColorBlendFactor;
            result.colorBlendOp = ColorBlendOp;
            result.srcAlphaBlendFactor = SrcAlphaBlendFactor;
            result.dstAlphaBlendFactor = DstColorBlendFactor;
            result.alphaBlendOp = AlphaBlendOp;
            result.colorWriteMask = ColorWriteMask;

            return result;
        }
    }

    public class PipelineColorBlendStateCreateInfo {
        public bool LogicOpEnable { get; set; }
        public VkLogicOp LogicOp { get; set; }
        public PipelineColorBlendAttachmentState[] Attachments { get; set; }
        public float[] BlendConstants { get; set; }

        internal VkPipelineColorBlendStateCreateInfo GetNative(List<IDisposable> marshalled) {
            var result = new VkPipelineColorBlendStateCreateInfo();
            result.sType = VkStructureType.StructureTypePipelineColorBlendStateCreateInfo;
            result.logicOpEnable = LogicOpEnable;
            result.logicOp = LogicOp;
            result.attachmentCount = (uint)Attachments.Length;

            var attachments = new VkPipelineColorBlendAttachmentState[Attachments.Length];
            for (int i = 0; i < attachments.Length; i++) {
                attachments[i] = Attachments[i].GetNative();
            }

            var attachMarshalled = new MarshalledArray<VkPipelineColorBlendAttachmentState>(attachments);
            result.attachmentCount = (uint)attachMarshalled.Count;
            result.pAttachments = attachMarshalled.Address;

            result.blendConstants = BlendConstants;

            marshalled.Add(attachMarshalled);

            return result;
        }
    }

    public class PipelineDynamicStateCreateInfo {
        public VkDynamicState[] DynamicStates { get; set; }

        internal VkPipelineDynamicStateCreateInfo GetNative(List<IDisposable> marshalled) {
            var result = new VkPipelineDynamicStateCreateInfo();
            result.sType = VkStructureType.StructureTypePipelineDynamicStateCreateInfo;

            var dynamicMarshalled = new MarshalledArray<int>(DynamicStates.Length);
            for (int i = 0; i < DynamicStates.Length; i++) {
                dynamicMarshalled[i] = (int)DynamicStates[i];
            }
            result.dynamicStateCount = (uint)dynamicMarshalled.Count;
            result.pDynamicStates = dynamicMarshalled.Address;

            marshalled.Add(dynamicMarshalled);

            return result;
        }
    }
}