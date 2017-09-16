using System;
using System.Collections.Generic;

namespace CSGL.Vulkan {
    public class SpecializationInfo {
        public IList<Unmanaged.VkSpecializationMapEntry> mapEntries;
        public IList<byte> data;

        public SpecializationInfo() { }

        internal SpecializationInfo(SpecializationInfo other) {
            mapEntries = other.mapEntries.CloneReadOnly();
            data = other.data.CloneReadOnly();
        }

        internal IntPtr GetNative(DisposableList<IDisposable> marshalled) {
            var entriesMarshalled = new MarshalledArray<Unmanaged.VkSpecializationMapEntry>(mapEntries);
            var dataMarshalled = new NativeArray<byte>(data);
            marshalled.Add(entriesMarshalled);
            marshalled.Add(dataMarshalled);

            var info = new Unmanaged.VkSpecializationInfo();
            info.mapEntryCount = (uint)entriesMarshalled.Count;
            info.pMapEntries = entriesMarshalled.Address;
            info.dataSize = (IntPtr)dataMarshalled.Count;
            info.pData = dataMarshalled.Address;

            var infoMarshalled = new Marshalled<Unmanaged.VkSpecializationInfo>(info);
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

        internal Unmanaged.VkPipelineShaderStageCreateInfo GetNative(DisposableList<IDisposable> marshalled) {
            var result = new Unmanaged.VkPipelineShaderStageCreateInfo();
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
        public IList<Unmanaged.VkVertexInputBindingDescription> vertexBindingDescriptions;
        public IList<Unmanaged.VkVertexInputAttributeDescription> vertexAttributeDescriptions;

        public PipelineVertexInputStateCreateInfo() { }

        internal PipelineVertexInputStateCreateInfo(PipelineVertexInputStateCreateInfo other) {
            vertexBindingDescriptions = other.vertexBindingDescriptions.CloneReadOnly();
            vertexAttributeDescriptions = other.vertexAttributeDescriptions.CloneReadOnly();
        }

        internal Unmanaged.VkPipelineVertexInputStateCreateInfo GetNative(DisposableList<IDisposable> marshalled) {
            var result = new Unmanaged.VkPipelineVertexInputStateCreateInfo();
            result.sType = VkStructureType.PipelineVertexInputStateCreateInfo;

            var attMarshalled = new MarshalledArray<Unmanaged.VkVertexInputAttributeDescription>(vertexAttributeDescriptions);
            result.vertexAttributeDescriptionCount = (uint)attMarshalled.Count;
            result.pVertexAttributeDescriptions = attMarshalled.Address;

            var bindMarshalled = new MarshalledArray<Unmanaged.VkVertexInputBindingDescription>(vertexBindingDescriptions);
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

        internal Unmanaged.VkPipelineInputAssemblyStateCreateInfo GetNative() {
            var result = new Unmanaged.VkPipelineInputAssemblyStateCreateInfo();
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

        internal Unmanaged.VkPipelineTessellationStateCreateInfo GetNative() {
            var result = new Unmanaged.VkPipelineTessellationStateCreateInfo();
            result.sType = VkStructureType.PipelineTessellationStateCreateInfo;
            result.patchControlPoints = patchControlPoints;

            return result;
        }
    }

    public class PipelineViewportStateCreateInfo {
        public IList<Unmanaged.VkViewport> viewports;
        public IList<Unmanaged.VkRect2D> scissors;

        public PipelineViewportStateCreateInfo() { }

        internal PipelineViewportStateCreateInfo(PipelineViewportStateCreateInfo other) {
            viewports = other.viewports.CloneReadOnly();
            scissors = other.scissors.CloneReadOnly();
        }

        internal Unmanaged.VkPipelineViewportStateCreateInfo GetNative(DisposableList<IDisposable> marshalled) {
            var result = new Unmanaged.VkPipelineViewportStateCreateInfo();
            result.sType = VkStructureType.PipelineViewportStateCreateInfo;

            var viewportMarshalled = new MarshalledArray<Unmanaged.VkViewport>(viewports);
            result.viewportCount = (uint)viewportMarshalled.Count;
            result.pViewports = viewportMarshalled.Address;

            var scissorMarshalled = new MarshalledArray<Unmanaged.VkRect2D>(scissors);
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

        internal Unmanaged.VkPipelineRasterizationStateCreateInfo GetNative() {
            var result = new Unmanaged.VkPipelineRasterizationStateCreateInfo();
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
            sampleMask = other.sampleMask.CloneReadOnly();
            alphaToCoverageEnable = other.alphaToCoverageEnable;
            alphaToOneEnable = other.alphaToOneEnable;
        }

        internal Unmanaged.VkPipelineMultisampleStateCreateInfo GetNative(DisposableList<IDisposable> marshalled) {
            var result = new Unmanaged.VkPipelineMultisampleStateCreateInfo();
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
        public Unmanaged.VkStencilOpState front;
        public Unmanaged.VkStencilOpState back;
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

        internal Unmanaged.VkPipelineDepthStencilStateCreateInfo GetNative() {
            var result = new Unmanaged.VkPipelineDepthStencilStateCreateInfo();
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

        internal Unmanaged.VkPipelineColorBlendAttachmentState GetNative() {
            var result = new Unmanaged.VkPipelineColorBlendAttachmentState();
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
            blendConstants = other.blendConstants.CloneReadOnly();
        }

        internal Unmanaged.VkPipelineColorBlendStateCreateInfo GetNative(DisposableList<IDisposable> marshalled) {
            var result = new Unmanaged.VkPipelineColorBlendStateCreateInfo();
            result.sType = VkStructureType.PipelineColorBlendStateCreateInfo;
            result.logicOpEnable = logicOpEnable ? 1u : 0u;
            result.logicOp = logicOp;
            result.attachmentCount = (uint)this.attachments.Count;

            var attachments = new Unmanaged.VkPipelineColorBlendAttachmentState[this.attachments.Count];
            for (int i = 0; i < attachments.Length; i++) {
                attachments[i] = this.attachments[i].GetNative();
            }

            var attachMarshalled = new MarshalledArray<Unmanaged.VkPipelineColorBlendAttachmentState>(attachments);
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
            dynamicStates = other.dynamicStates.CloneReadOnly();
        }

        internal Unmanaged.VkPipelineDynamicStateCreateInfo GetNative(DisposableList<IDisposable> marshalled) {
            var result = new Unmanaged.VkPipelineDynamicStateCreateInfo();
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