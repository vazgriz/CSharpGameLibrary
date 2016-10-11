using System;
using System.Collections.Generic;

namespace CSGL.Vulkan.Managed {
    public class PipelineShaderStageCreateInfo {
        public VkShaderStageFlags stage;
        public ShaderModule module;
        public string name;
        public IntPtr specializationInfo;

        internal VkPipelineShaderStageCreateInfo GetNative(List<IDisposable> marshalled) {
            var result = new VkPipelineShaderStageCreateInfo();
            result.sType = VkStructureType.StructureTypePipelineShaderStageCreateInfo;
            result.stage = stage;
            result.module = module.Native;
            var strInterop = new InteropString(name);
            result.pName = strInterop.Address;
            result.pSpecializationInfo = specializationInfo;

            marshalled.Add(strInterop);

            return result;
        }
    }

    public class PipelineVertexInputStateCreateInfo {
        public VkVertexInputBindingDescription[] vertexBindingDescriptions;
        public VkVertexInputAttributeDescription[] vertexAttributeDescriptions;

        internal VkPipelineVertexInputStateCreateInfo GetNative(List<IDisposable> marshalled) {
            var result = new VkPipelineVertexInputStateCreateInfo();

            var attMarshalled = new MarshalledArray<VkVertexInputAttributeDescription>(vertexAttributeDescriptions);
            result.sType = VkStructureType.StructureTypePipelineVertexInputStateCreateInfo;
            result.vertexAttributeDescriptionCount = (uint)attMarshalled.Count;
            result.pVertexAttributeDescriptions = attMarshalled.Address;

            var bindMarshalled = new MarshalledArray<VkVertexInputBindingDescription>(vertexBindingDescriptions);
            result.vertexBindingDescriptionCount = (uint)bindMarshalled.Count;
            result.pVertexBindingDescriptions = bindMarshalled.Address;

            marshalled.Add(attMarshalled);
            marshalled.Add(bindMarshalled);

            return result;
        }
    }

    public class PipelineInputAssemblyStateCreateInfo {
        public VkPrimitiveTopology topology;
        public bool primitiveRestartEnable;

        internal VkPipelineInputAssemblyStateCreateInfo GetNative() {
            var result = new VkPipelineInputAssemblyStateCreateInfo();
            result.sType = VkStructureType.StructureTypePipelineInputAssemblyStateCreateInfo;
            result.topology = topology;
            result.primitiveRestartEnable = primitiveRestartEnable ? 1u : 0u;

            return result;
        }
    }

    public class PipelineTessellationStateCreateInfo {
        public uint patchControlPoints;

        internal VkPipelineTessellationStateCreateInfo GetNative() {
            var result = new VkPipelineTessellationStateCreateInfo();
            result.sType = VkStructureType.StructureTypePipelineTessellationStateCreateInfo;
            result.patchControlPoints = patchControlPoints;

            return result;
        }
    }

    public class PipelineViewportStateCreateInfo {
        public VkViewport[] viewports;
        public VkRect2D[] scissors;

        internal VkPipelineViewportStateCreateInfo GetNative(List<IDisposable> marshalled) {
            var result = new VkPipelineViewportStateCreateInfo();
            result.sType = VkStructureType.StructureTypePipelineViewportStateCreateInfo;

            var viewportMarshalled = new MarshalledArray<VkViewport>(viewports);
            result.viewportCount = (uint)viewportMarshalled.Count;
            result.pViewports = viewportMarshalled.Address;

            var scissorMarshalled = new MarshalledArray<VkRect2D>(scissors);
            result.scissorCount = (uint)scissorMarshalled.Count;
            result.pScissors = scissorMarshalled.Address;

            marshalled.Add(viewportMarshalled);
            marshalled.Add(scissorMarshalled);

            return result;
        }
    }

    public class PipelineRasterizationStateCreateInfo {
        public bool depthClampEnable;
        public bool rasterizerDiscardEnable;
        public VkPolygonMode polygonMode;
        public VkCullModeFlags cullMode;
        public VkFrontFace frontFace;
        public bool depthBiasEnable;
        public float depthBiasConstantFactor;
        public float depthBiasClamp;
        public float depthBiasSlopeFactor;
        public float lineWidth;

        internal VkPipelineRasterizationStateCreateInfo GetNative() {
            var result = new VkPipelineRasterizationStateCreateInfo();
            result.sType = VkStructureType.StructureTypePipelineRasterizationStateCreateInfo;
            result.depthClampEnable = depthClampEnable ? 1u : 0u;
            result.rasterizerDiscardEnable = rasterizerDiscardEnable ? 1u : 0u;
            result.polygonMode = polygonMode;
            result.cullMode = cullMode;
            result.frontFace = frontFace;
            result.depthBiasEnable = depthBiasEnable ? 1u : 0u;
            result.depthBiasConstantFactor = depthBiasConstantFactor;
            result.depthBiasClamp = depthBiasClamp;
            result.depthBiasSlopeFactor = depthBiasSlopeFactor;
            result.lineWidth = lineWidth;

            return result;
        }
    }

    public class PipelineMultisampleStateCreateInfo {
        public VkSampleCountFlags rasterizationSamples;
        public bool sampleShadingEnable;
        public float minSampleShading;
        public uint[] sampleMask;
        public bool alphaToCoverageEnable;
        public bool alphaToOneEnable;

        internal VkPipelineMultisampleStateCreateInfo GetNative() {
            var result = new VkPipelineMultisampleStateCreateInfo();
            result.sType = VkStructureType.StructureTypePipelineMultisampleStateCreateInfo;
            result.rasterizationSamples = rasterizationSamples;
            result.sampleShadingEnable = sampleShadingEnable ? 1u : 0u;
            result.minSampleShading = minSampleShading;
            //result.pSampleMask = SampleMask;
            result.alphaToCoverageEnable = alphaToCoverageEnable ? 1u : 0u;
            result.alphaToOneEnable = alphaToOneEnable ? 1u : 0u;

            return result;
        }
    }

    public class PipelineDepthStencilStateCreateInfo {
        public bool depthTestEnable;
        public bool depthWriteEnable;
        public VkCompareOp depthCompareOp;
        public bool depthBoundsTestEnable;
        public bool stencilTestEnable;
        public VkStencilOpState front;
        public VkStencilOpState back;
        public float minDepthBounds;
        public float maxDepthBounds;

        internal VkPipelineDepthStencilStateCreateInfo GetNative() {
            var result = new VkPipelineDepthStencilStateCreateInfo();
            result.sType = VkStructureType.StructureTypePipelineDepthStencilStateCreateInfo;
            result.depthTestEnable = depthTestEnable ? 1u : 0u;
            result.depthWriteEnable = depthWriteEnable ? 1u : 0u;
            result.depthCompareOp = depthCompareOp;
            result.depthBoundsTestEnable = depthBoundsTestEnable ? 1u : 0u;
            result.stencilTestEnable = stencilTestEnable ? 1u : 0u;
            result.front = front;
            result.back = back;
            result.minDepthBounds = minDepthBounds;
            result.maxDepthBounds = maxDepthBounds;

            return result;
        }
    }

    public class PipelineColorBlendAttachmentState {
        public bool blendEnable;
        public VkBlendFactor srcColorBlendFactor;
        public VkBlendFactor dstColorBlendFactor;
        public VkBlendOp colorBlendOp;
        public VkBlendFactor srcAlphaBlendFactor;
        public VkBlendFactor dstAlphaBlendFactor;
        public VkBlendOp alphaBlendOp;
        public VkColorComponentFlags colorWriteMask;

        internal VkPipelineColorBlendAttachmentState GetNative() {
            var result = new VkPipelineColorBlendAttachmentState();
            result.blendEnable = blendEnable ? 1u : 0u;
            result.srcColorBlendFactor = srcColorBlendFactor;
            result.dstColorBlendFactor = dstColorBlendFactor;
            result.colorBlendOp = colorBlendOp;
            result.srcAlphaBlendFactor = srcAlphaBlendFactor;
            result.dstAlphaBlendFactor = dstColorBlendFactor;
            result.alphaBlendOp = alphaBlendOp;
            result.colorWriteMask = colorWriteMask;

            return result;
        }
    }

    public class PipelineColorBlendStateCreateInfo {
        public bool logicOpEnable;
        public VkLogicOp logicOp;
        public PipelineColorBlendAttachmentState[] attachments;
        public float[] blendConstants;

        internal VkPipelineColorBlendStateCreateInfo GetNative(List<IDisposable> marshalled) {
            var result = new VkPipelineColorBlendStateCreateInfo();
            result.sType = VkStructureType.StructureTypePipelineColorBlendStateCreateInfo;
            result.logicOpEnable = logicOpEnable ? 1u : 0u;
            result.logicOp = logicOp;
            result.attachmentCount = (uint)this.attachments.Length;

            var attachments = new VkPipelineColorBlendAttachmentState[this.attachments.Length];
            for (int i = 0; i < attachments.Length; i++) {
                attachments[i] = this.attachments[i].GetNative();
            }

            var attachMarshalled = new MarshalledArray<VkPipelineColorBlendAttachmentState>(attachments);
            result.attachmentCount = (uint)attachMarshalled.Count;
            result.pAttachments = attachMarshalled.Address;

            if (blendConstants != null) {
                result.blendConstants = blendConstants[0];
                result.blendConstants_1 = blendConstants[1];
                result.blendConstants_2 = blendConstants[2];
                result.blendConstants_3 = blendConstants[3];
            }

            marshalled.Add(attachMarshalled);

            return result;
        }
    }

    public class PipelineDynamicStateCreateInfo {
        public VkDynamicState[] dynamicStates;

        internal VkPipelineDynamicStateCreateInfo GetNative(List<IDisposable> marshalled) {
            var result = new VkPipelineDynamicStateCreateInfo();
            result.sType = VkStructureType.StructureTypePipelineDynamicStateCreateInfo;

            var dynamicMarshalled = new MarshalledArray<int>(dynamicStates.Length);
            for (int i = 0; i < dynamicStates.Length; i++) {
                dynamicMarshalled[i] = (int)dynamicStates[i];
            }
            result.dynamicStateCount = (uint)dynamicMarshalled.Count;
            result.pDynamicStates = dynamicMarshalled.Address;

            marshalled.Add(dynamicMarshalled);

            return result;
        }
    }
}