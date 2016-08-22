using System;

namespace CSGL.Vulkan.Managed {
    public class PipelineShaderStageCreateInfo {
        public VkShaderStageFlags Stage { get; set; }
        public ShaderModule Module { get; set; }
        public string Name { get; set; }
        public IntPtr SpecializationInfo { get; set; }

        public VkPipelineShaderStageCreateInfo GetNative() {
            var result = new VkPipelineShaderStageCreateInfo();
            result.sType = VkStructureType.StructureTypePipelineShaderStageCreateInfo;
            result.stage = Stage;
            result.module = Module.Native;
            result.pName = Interop.GetUTF8(Name);
            result.pSpecializationInfo = SpecializationInfo;

            return result;
        }
    }

    public class PipelineVertexInputStateCreateInfo {
        public VkVertexInputBindingDescription[] VertexBindingDescriptions { get; set; }
        public VkVertexInputAttributeDescription[] VertexAttributeDescriptions { get; set; }

        public VkPipelineVertexInputStateCreateInfo GetNative() {
            var result = new VkPipelineVertexInputStateCreateInfo();
            result.sType = VkStructureType.StructureTypePipelineVertexInputStateCreateInfo;
            result.vertexAttributeDescriptionCount = (uint)VertexAttributeDescriptions.Length;
            result.pVertexAttributeDescriptions = VertexAttributeDescriptions;
            result.vertexBindingDescriptionCount = (uint)VertexBindingDescriptions.Length;
            result.pVertexBindingDescriptions = VertexBindingDescriptions;

            return result;
        }
    }

    public class PipelineInputAssemblyStateCreateInfo {
        public VkPrimitiveTopology Topology { get; set; }
        public bool PrimitiveRestartEnable { get; set; }

        public VkPipelineInputAssemblyStateCreateInfo GetNative() {
            var result = new VkPipelineInputAssemblyStateCreateInfo();
            result.sType = VkStructureType.StructureTypePipelineInputAssemblyStateCreateInfo;
            result.topology = Topology;
            result.primitiveRestartEnable = PrimitiveRestartEnable;

            return result;
        }
    }

    public class PipelineTessellationStateCreateInfo {
        public uint PatchControlPoints { get; set; }

        public VkPipelineTessellationStateCreateInfo GetNative() {
            var result = new VkPipelineTessellationStateCreateInfo();
            result.sType = VkStructureType.StructureTypePipelineTessellationStateCreateInfo;
            result.patchControlPoints = PatchControlPoints;

            return result;
        }
    }

    public class PipelineViewportStateCreateInfo {
        public VkViewport[] Viewports { get; set; }
        public VkRect2D[] Scissors { get; set; }

        public VkPipelineViewportStateCreateInfo GetNative() {
            var result = new VkPipelineViewportStateCreateInfo();
            result.sType = VkStructureType.StructureTypePipelineViewportStateCreateInfo;
            result.viewportCount = (uint)Viewports.Length;
            result.pViewports = Viewports;
            result.scissorCount = (uint)Scissors.Length;
            result.pScissors = Scissors;

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

        public VkPipelineRasterizationStateCreateInfo GetNative() {
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

        public VkPipelineMultisampleStateCreateInfo GetNative() {
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

        public VkPipelineDepthStencilStateCreateInfo GetNative() {
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

        public VkPipelineColorBlendAttachmentState GetNative() {
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

        public VkPipelineColorBlendStateCreateInfo GetNative() {
            var result = new VkPipelineColorBlendStateCreateInfo();
            result.sType = VkStructureType.StructureTypePipelineColorBlendStateCreateInfo;
            result.logicOpEnable = LogicOpEnable;
            result.logicOp = LogicOp;
            result.attachmentCount = (uint)Attachments.Length;
            result.pAttachments = new VkPipelineColorBlendAttachmentState[Attachments.Length];
            for (int i = 0; i < Attachments.Length; i++) {
                result.pAttachments[i] = Attachments[i].GetNative();
            }
            result.blendConstants = BlendConstants;

            return result;
        }
    }

    public class PipelineDynamicStateCreateInfo {
        public VkDynamicState[] DynamicStates { get; set; }

        public VkPipelineDynamicStateCreateInfo GetNative() {
            var result = new VkPipelineDynamicStateCreateInfo();
            result.sType = VkStructureType.StructureTypePipelineDynamicStateCreateInfo;
            result.dynamicStateCount = (uint)DynamicStates.Length;
            result.pDynamicStates = DynamicStates;

            return result;
        }
    }
}