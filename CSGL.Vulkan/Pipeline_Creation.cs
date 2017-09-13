using System;
using System.Collections.Generic;

namespace CSGL.Vulkan {
    public class SpecializationInfo {
        public IList<VkSpecializationMapEntry> mapEntries;
        public IList<byte> data;

        public SpecializationInfo() { }

        internal SpecializationInfo(SpecializationInfo other) {
            if (other.mapEntries != null) mapEntries = new List<VkSpecializationMapEntry>(other.mapEntries).AsReadOnly();
            if (other.data != null) data = new List<byte>(other.data).AsReadOnly();
        }

        internal IntPtr GetNative(DisposableList<IDisposable> marshalled) {
            var entriesMarshalled = new MarshalledArray<VkSpecializationMapEntry>(mapEntries);
            var dataMarshalled = new NativeArray<byte>(data);
            marshalled.Add(entriesMarshalled);
            marshalled.Add(dataMarshalled);

            var info = new VkSpecializationInfo();
            info.mapEntryCount = (uint)entriesMarshalled.Count;
            info.pMapEntries = entriesMarshalled.Address;
            info.dataSize = (IntPtr)dataMarshalled.Count;
            info.pData = dataMarshalled.Address;

            var infoMarshalled = new Marshalled<VkSpecializationInfo>(info);
            marshalled.Add(infoMarshalled);

            return infoMarshalled.Address;
        }
    }

    public class PipelineShaderStageCreateInfo {
        public VkShaderStageFlags stage;
        public ShaderModule module;
        public string name;
        public SpecializationInfo specializationInfo;

        public PipelineShaderStageCreateInfo() { }

        internal PipelineShaderStageCreateInfo(PipelineShaderStageCreateInfo other) {
            stage = other.stage;
            module = other.module;
            name = other.name;
            if (other.specializationInfo != null) specializationInfo = new SpecializationInfo(other.specializationInfo);
        }

        internal VkPipelineShaderStageCreateInfo GetNative(DisposableList<IDisposable> marshalled) {
            var result = new VkPipelineShaderStageCreateInfo();
            result.sType = VkStructureType.PipelineShaderStageCreateInfo;
            result.stage = stage;
            result.module = module.Native;

            var strInterop = new InteropString(name);
            result.pName = strInterop.Address;
            marshalled.Add(strInterop);

            if (specializationInfo != null) {
                result.pSpecializationInfo = specializationInfo.GetNative(marshalled);
            }

            return result;
        }
    }

    public class PipelineVertexInputStateCreateInfo {
        public IList<VkVertexInputBindingDescription> vertexBindingDescriptions;
        public IList<VkVertexInputAttributeDescription> vertexAttributeDescriptions;

        public PipelineVertexInputStateCreateInfo() { }

        internal PipelineVertexInputStateCreateInfo(PipelineVertexInputStateCreateInfo other) {
            if (other.vertexBindingDescriptions != null) vertexBindingDescriptions = new List<VkVertexInputBindingDescription>(other.vertexBindingDescriptions).AsReadOnly();
            if (other.vertexAttributeDescriptions != null) vertexAttributeDescriptions = new List<VkVertexInputAttributeDescription>(other.vertexAttributeDescriptions).AsReadOnly();
        }

        internal VkPipelineVertexInputStateCreateInfo GetNative(DisposableList<IDisposable> marshalled) {
            var result = new VkPipelineVertexInputStateCreateInfo();
            result.sType = VkStructureType.PipelineVertexInputStateCreateInfo;

            var attMarshalled = new MarshalledArray<VkVertexInputAttributeDescription>(vertexAttributeDescriptions);
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

        public PipelineInputAssemblyStateCreateInfo() { }

        internal PipelineInputAssemblyStateCreateInfo(PipelineInputAssemblyStateCreateInfo other) {
            topology = other.topology;
            primitiveRestartEnable = other.primitiveRestartEnable;
        }

        internal VkPipelineInputAssemblyStateCreateInfo GetNative() {
            var result = new VkPipelineInputAssemblyStateCreateInfo();
            result.sType = VkStructureType.PipelineInputAssemblyStateCreateInfo;
            result.topology = topology;
            result.primitiveRestartEnable = primitiveRestartEnable ? 1u : 0u;

            return result;
        }
    }

    public class PipelineTessellationStateCreateInfo {
        public uint patchControlPoints;

        public PipelineTessellationStateCreateInfo() { }

        internal PipelineTessellationStateCreateInfo(PipelineTessellationStateCreateInfo other) {
            patchControlPoints = other.patchControlPoints;
        }

        internal VkPipelineTessellationStateCreateInfo GetNative() {
            var result = new VkPipelineTessellationStateCreateInfo();
            result.sType = VkStructureType.PipelineTessellationStateCreateInfo;
            result.patchControlPoints = patchControlPoints;

            return result;
        }
    }

    public class PipelineViewportStateCreateInfo {
        public IList<VkViewport> viewports;
        public IList<VkRect2D> scissors;

        public PipelineViewportStateCreateInfo() { }

        internal PipelineViewportStateCreateInfo(PipelineViewportStateCreateInfo other) {
            if (other.viewports != null) viewports = new List<VkViewport>(other.viewports).AsReadOnly();
            if (other.scissors != null) scissors = new List<VkRect2D>(other.scissors).AsReadOnly();
        }

        internal VkPipelineViewportStateCreateInfo GetNative(DisposableList<IDisposable> marshalled) {
            var result = new VkPipelineViewportStateCreateInfo();
            result.sType = VkStructureType.PipelineViewportStateCreateInfo;

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

        public PipelineRasterizationStateCreateInfo() { }

        internal PipelineRasterizationStateCreateInfo(PipelineRasterizationStateCreateInfo other) {
            depthClampEnable = other.depthClampEnable;
            rasterizerDiscardEnable = other.rasterizerDiscardEnable;
            polygonMode = other.polygonMode;
            cullMode = other.cullMode;
            frontFace = other.frontFace;
            depthBiasEnable = other.depthBiasEnable;
            depthBiasConstantFactor = other.depthBiasConstantFactor;
            depthBiasClamp = other.depthBiasClamp;
            depthBiasSlopeFactor = other.depthBiasSlopeFactor;
            lineWidth = other.lineWidth;
        }

        internal VkPipelineRasterizationStateCreateInfo GetNative() {
            var result = new VkPipelineRasterizationStateCreateInfo();
            result.sType = VkStructureType.PipelineRasterizationStateCreateInfo;
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
        public IList<uint> sampleMask;
        public bool alphaToCoverageEnable;
        public bool alphaToOneEnable;

        public PipelineMultisampleStateCreateInfo() { }

        internal PipelineMultisampleStateCreateInfo(PipelineMultisampleStateCreateInfo other) {
            rasterizationSamples = other.rasterizationSamples;
            sampleShadingEnable = other.sampleShadingEnable;
            minSampleShading = other.minSampleShading;
            if (other.sampleMask != null) sampleMask = new List<uint>(other.sampleMask).AsReadOnly();
            alphaToCoverageEnable = other.alphaToCoverageEnable;
            alphaToOneEnable = other.alphaToOneEnable;
        }

        internal VkPipelineMultisampleStateCreateInfo GetNative(DisposableList<IDisposable> marshalled) {
            var result = new VkPipelineMultisampleStateCreateInfo();
            result.sType = VkStructureType.PipelineMultisampleStateCreateInfo;
            result.rasterizationSamples = rasterizationSamples;
            result.sampleShadingEnable = sampleShadingEnable ? 1u : 0u;
            result.minSampleShading = minSampleShading;

            if (sampleMask != null) {
                NativeArray<uint> masks = new NativeArray<uint>(sampleMask);
                result.pSampleMask = masks.Address;
                marshalled.Add(masks);
            }

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

        public PipelineDepthStencilStateCreateInfo() { }

        internal PipelineDepthStencilStateCreateInfo(PipelineDepthStencilStateCreateInfo other) {
            depthTestEnable = other.depthTestEnable;
            depthWriteEnable = other.depthWriteEnable;
            depthCompareOp = other.depthCompareOp;
            depthBoundsTestEnable = other.depthBoundsTestEnable;
            stencilTestEnable = other.stencilTestEnable;
            front = other.front;
            back = other.back;
            minDepthBounds = other.minDepthBounds;
            maxDepthBounds = other.maxDepthBounds;
        }

        internal VkPipelineDepthStencilStateCreateInfo GetNative() {
            var result = new VkPipelineDepthStencilStateCreateInfo();
            result.sType = VkStructureType.PipelineDepthStencilStateCreateInfo;
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

        public PipelineColorBlendAttachmentState() { }

        internal PipelineColorBlendAttachmentState(PipelineColorBlendAttachmentState other) {
            blendEnable = other.blendEnable;
            srcColorBlendFactor = other.srcColorBlendFactor;
            dstColorBlendFactor = other.dstColorBlendFactor;
            colorBlendOp = other.colorBlendOp;
            srcAlphaBlendFactor = other.srcAlphaBlendFactor;
            dstAlphaBlendFactor = other.dstAlphaBlendFactor;
            alphaBlendOp = other.alphaBlendOp;
            colorWriteMask = other.colorWriteMask;
        }

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
        public IList<PipelineColorBlendAttachmentState> attachments;
        public IList<float> blendConstants;

        public PipelineColorBlendStateCreateInfo() { }

        internal PipelineColorBlendStateCreateInfo(PipelineColorBlendStateCreateInfo other) {
            logicOpEnable = other.logicOpEnable;
            logicOp = other.logicOp;
            if (other.attachments != null) {
                var attachments = new List<PipelineColorBlendAttachmentState>(other.attachments.Count);
                foreach (var attachment in other.attachments) {
                    attachments.Add(new PipelineColorBlendAttachmentState(attachment));
                }
                this.attachments = attachments.AsReadOnly();
            }
            if (other.blendConstants != null) blendConstants = new List<float>(other.blendConstants).AsReadOnly();
        }

        internal VkPipelineColorBlendStateCreateInfo GetNative(DisposableList<IDisposable> marshalled) {
            var result = new VkPipelineColorBlendStateCreateInfo();
            result.sType = VkStructureType.PipelineColorBlendStateCreateInfo;
            result.logicOpEnable = logicOpEnable ? 1u : 0u;
            result.logicOp = logicOp;
            result.attachmentCount = (uint)this.attachments.Count;

            var attachments = new VkPipelineColorBlendAttachmentState[this.attachments.Count];
            for (int i = 0; i < attachments.Length; i++) {
                attachments[i] = this.attachments[i].GetNative();
            }

            var attachMarshalled = new MarshalledArray<VkPipelineColorBlendAttachmentState>(attachments);
            result.attachmentCount = (uint)attachMarshalled.Count;
            result.pAttachments = attachMarshalled.Address;

            if (blendConstants != null) {
                result.blendConstants_0 = blendConstants[0];
                result.blendConstants_1 = blendConstants[1];
                result.blendConstants_2 = blendConstants[2];
                result.blendConstants_3 = blendConstants[3];
            }

            marshalled.Add(attachMarshalled);

            return result;
        }
    }

    public class PipelineDynamicStateCreateInfo {
        public IList<VkDynamicState> dynamicStates;

        public PipelineDynamicStateCreateInfo() { }

        internal PipelineDynamicStateCreateInfo(PipelineDynamicStateCreateInfo other) {
            if (other.dynamicStates != null) dynamicStates = new List<VkDynamicState>(other.dynamicStates).AsReadOnly();
        }

        internal VkPipelineDynamicStateCreateInfo GetNative(DisposableList<IDisposable> marshalled) {
            var result = new VkPipelineDynamicStateCreateInfo();
            result.sType = VkStructureType.PipelineDynamicStateCreateInfo;

            var dynamicMarshalled = new NativeArray<int>(dynamicStates.Count);
            for (int i = 0; i < dynamicStates.Count; i++) {
                dynamicMarshalled[i] = (int)dynamicStates[i];
            }
            result.dynamicStateCount = (uint)dynamicMarshalled.Count;
            result.pDynamicStates = dynamicMarshalled.Address;

            marshalled.Add(dynamicMarshalled);

            return result;
        }
    }
}