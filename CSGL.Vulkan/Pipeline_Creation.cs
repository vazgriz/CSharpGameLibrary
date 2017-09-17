using System;
using System.Collections.Generic;

namespace CSGL.Vulkan {
    public class VkSpecializationInfo {
        public IList<Unmanaged.VkSpecializationMapEntry> mapEntries;
        public IList<byte> data;

        public VkSpecializationInfo() { }

        internal VkSpecializationInfo(VkSpecializationInfo other) {
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

    public class VkPipelineShaderStageCreateInfo {
        public VkShaderStageFlags stage;
        public VkShaderModule module;
        public string name;
        public VkSpecializationInfo specializationInfo;

        public VkPipelineShaderStageCreateInfo() { }

        internal VkPipelineShaderStageCreateInfo(VkPipelineShaderStageCreateInfo other) {
            stage = other.stage;
            module = other.module;
            name = other.name;
            if (other.specializationInfo != null) specializationInfo = new VkSpecializationInfo(other.specializationInfo);
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

    public class VkPipelineVertexInputStateCreateInfo {
        public IList<Unmanaged.VkVertexInputBindingDescription> vertexBindingDescriptions;
        public IList<Unmanaged.VkVertexInputAttributeDescription> vertexAttributeDescriptions;

        public VkPipelineVertexInputStateCreateInfo() { }

        internal VkPipelineVertexInputStateCreateInfo(VkPipelineVertexInputStateCreateInfo other) {
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

    public class VkPipelineInputAssemblyStateCreateInfo {
        public VkPrimitiveTopology topology;
        public bool primitiveRestartEnable;

        public VkPipelineInputAssemblyStateCreateInfo() { }

        internal VkPipelineInputAssemblyStateCreateInfo(VkPipelineInputAssemblyStateCreateInfo other) {
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

    public class VkPipelineTessellationStateCreateInfo {
        public int patchControlPoints;

        public VkPipelineTessellationStateCreateInfo() { }

        internal VkPipelineTessellationStateCreateInfo(VkPipelineTessellationStateCreateInfo other) {
            patchControlPoints = other.patchControlPoints;
        }

        internal Unmanaged.VkPipelineTessellationStateCreateInfo GetNative() {
            var result = new Unmanaged.VkPipelineTessellationStateCreateInfo();
            result.sType = VkStructureType.PipelineTessellationStateCreateInfo;
            result.patchControlPoints = (uint)patchControlPoints;

            return result;
        }
    }

    public class VkPipelineViewportStateCreateInfo {
        public IList<VkViewport> viewports;
        public IList<VkRect2D> scissors;

        public VkPipelineViewportStateCreateInfo() { }

        internal VkPipelineViewportStateCreateInfo(VkPipelineViewportStateCreateInfo other) {
            viewports = other.viewports.CloneReadOnly();
            scissors = other.scissors.CloneReadOnly();
        }

        internal Unmanaged.VkPipelineViewportStateCreateInfo GetNative(DisposableList<IDisposable> marshalled) {
            var result = new Unmanaged.VkPipelineViewportStateCreateInfo();
            result.sType = VkStructureType.PipelineViewportStateCreateInfo;

            var viewportMarshalled = new MarshalledArray<Unmanaged.VkViewport>(viewports.Count);
            for (int i = 0; i < viewports.Count; i++) {
                viewportMarshalled[i] = viewports[i].GetNative();
            }
            result.viewportCount = (uint)viewportMarshalled.Count;
            result.pViewports = viewportMarshalled.Address;

            var scissorMarshalled = new MarshalledArray<Unmanaged.VkRect2D>(scissors.Count);
            for (int i = 0; i < scissors.Count; i++) {
                scissorMarshalled[i] = scissors[i].GetNative();
            }
            result.scissorCount = (uint)scissorMarshalled.Count;
            result.pScissors = scissorMarshalled.Address;

            marshalled.Add(viewportMarshalled);
            marshalled.Add(scissorMarshalled);

            return result;
        }
    }

    public class VkPipelineRasterizationStateCreateInfo {
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

        public VkPipelineRasterizationStateCreateInfo() { }

        internal VkPipelineRasterizationStateCreateInfo(VkPipelineRasterizationStateCreateInfo other) {
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

    public class VkPipelineMultisampleStateCreateInfo {
        public VkSampleCountFlags rasterizationSamples;
        public bool sampleShadingEnable;
        public float minSampleShading;
        public IList<uint> sampleMask;
        public bool alphaToCoverageEnable;
        public bool alphaToOneEnable;

        public VkPipelineMultisampleStateCreateInfo() { }

        internal VkPipelineMultisampleStateCreateInfo(VkPipelineMultisampleStateCreateInfo other) {
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

    public class VkPipelineDepthStencilStateCreateInfo {
        public bool depthTestEnable;
        public bool depthWriteEnable;
        public VkCompareOp depthCompareOp;
        public bool depthBoundsTestEnable;
        public bool stencilTestEnable;
        public VkStencilOpState front;
        public VkStencilOpState back;
        public float minDepthBounds;
        public float maxDepthBounds;

        public VkPipelineDepthStencilStateCreateInfo() { }

        internal VkPipelineDepthStencilStateCreateInfo(VkPipelineDepthStencilStateCreateInfo other) {
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
            result.front = front.GetNative();
            result.back = back.GetNative();
            result.minDepthBounds = minDepthBounds;
            result.maxDepthBounds = maxDepthBounds;

            return result;
        }
    }

    public class VkPipelineColorBlendAttachmentState {
        public bool blendEnable;
        public VkBlendFactor srcColorBlendFactor;
        public VkBlendFactor dstColorBlendFactor;
        public VkBlendOp colorBlendOp;
        public VkBlendFactor srcAlphaBlendFactor;
        public VkBlendFactor dstAlphaBlendFactor;
        public VkBlendOp alphaBlendOp;
        public VkColorComponentFlags colorWriteMask;

        public VkPipelineColorBlendAttachmentState() { }

        internal VkPipelineColorBlendAttachmentState(VkPipelineColorBlendAttachmentState other) {
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

    public class VkPipelineColorBlendStateCreateInfo {
        public bool logicOpEnable;
        public VkLogicOp logicOp;
        public IList<VkPipelineColorBlendAttachmentState> attachments;
        public IList<float> blendConstants;

        public VkPipelineColorBlendStateCreateInfo() { }

        internal VkPipelineColorBlendStateCreateInfo(VkPipelineColorBlendStateCreateInfo other) {
            logicOpEnable = other.logicOpEnable;
            logicOp = other.logicOp;
            if (other.attachments != null) {
                var attachments = new List<VkPipelineColorBlendAttachmentState>(other.attachments.Count);
                foreach (var attachment in other.attachments) {
                    attachments.Add(new VkPipelineColorBlendAttachmentState(attachment));
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

    public class VkPipelineDynamicStateCreateInfo {
        public IList<VkDynamicState> dynamicStates;

        public VkPipelineDynamicStateCreateInfo() { }

        internal VkPipelineDynamicStateCreateInfo(VkPipelineDynamicStateCreateInfo other) {
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