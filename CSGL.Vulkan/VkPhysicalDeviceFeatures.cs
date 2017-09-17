using System;
using System.Collections.Generic;

namespace CSGL.Vulkan {
    public class VkPhysicalDeviceFeatures {
        public bool robustBufferAccess;
        public bool fullDrawIndexUint32;
        public bool imageCubeArray;
        public bool independentBlend;
        public bool geometryShader;
        public bool tessellationShader;
        public bool sampleRateShading;
        public bool dualSrcBlend;
        public bool logicOp;
        public bool multiDrawIndirect;
        public bool drawIndirectFirstInstance;
        public bool depthClamp;
        public bool depthBiasClamp;
        public bool fillModeNonSolid;
        public bool depthBounds;
        public bool wideLines;
        public bool largePoints;
        public bool alphaToOne;
        public bool multiViewport;
        public bool samplerAnisotropy;
        public bool textureCompressionETC2;
        public bool textureCompressionASTC_LDR;
        public bool textureCompressionBC;
        public bool occlusionQueryPrecise;
        public bool pipelineStatisticsQuery;
        public bool vertexPipelineStoresAndAtomics;
        public bool fragmentStoresAndAtomics;
        public bool shaderTessellationAndGeometryPointSize;
        public bool shaderImageGatherExtended;
        public bool shaderStorageImageExtendedFormats;
        public bool shaderStorageImageMultisample;
        public bool shaderStorageImageReadWithoutFormat;
        public bool shaderStorageImageWriteWithoutFormat;
        public bool shaderUniformBufferArrayDynamicIndexing;
        public bool shaderSampledImageArrayDynamicIndexing;
        public bool shaderStorageBufferArrayDynamicIndexing;
        public bool shaderStorageImageArrayDynamicIndexing;
        public bool shaderClipDistance;
        public bool shaderCullDistance;
        public bool shaderFloat64;
        public bool shaderInt64;
        public bool shaderInt16;
        public bool shaderResourceResidency;
        public bool shaderResourceMinLod;
        public bool sparseBinding;
        public bool sparseResidencyBuffer;
        public bool sparseResidencyImage2D;
        public bool sparseResidencyImage3D;
        public bool sparseResidency2Samples;
        public bool sparseResidency4Samples;
        public bool sparseResidency8Samples;
        public bool sparseResidency16Samples;
        public bool sparseResidencyAliased;
        public bool variableMultisampleRate;
        public bool inheritedQueries;

        public VkPhysicalDeviceFeatures() { }

        internal VkPhysicalDeviceFeatures(VkPhysicalDeviceFeatures other) {
            robustBufferAccess = other.robustBufferAccess;
            fullDrawIndexUint32 = other.fullDrawIndexUint32;
            imageCubeArray = other.imageCubeArray;
            independentBlend = other.independentBlend;
            geometryShader = other.geometryShader;
            tessellationShader = other.tessellationShader;
            sampleRateShading = other.sampleRateShading;
            dualSrcBlend = other.dualSrcBlend;
            logicOp = other.logicOp;
            multiDrawIndirect = other.multiDrawIndirect;
            drawIndirectFirstInstance = other.drawIndirectFirstInstance;
            depthClamp = other.depthClamp;
            depthBiasClamp = other.depthBiasClamp;
            fillModeNonSolid = other.fillModeNonSolid;
            depthBounds = other.depthBounds;
            wideLines = other.wideLines;
            largePoints = other.largePoints;
            alphaToOne = other.alphaToOne;
            multiViewport = other.multiViewport;
            samplerAnisotropy = other.samplerAnisotropy;
            textureCompressionETC2 = other.textureCompressionETC2;
            textureCompressionASTC_LDR = other.textureCompressionASTC_LDR;
            textureCompressionBC = other.textureCompressionBC;
            occlusionQueryPrecise = other.occlusionQueryPrecise;
            pipelineStatisticsQuery = other.pipelineStatisticsQuery;
            vertexPipelineStoresAndAtomics = other.vertexPipelineStoresAndAtomics;
            fragmentStoresAndAtomics = other.fragmentStoresAndAtomics;
            shaderTessellationAndGeometryPointSize = other.shaderTessellationAndGeometryPointSize;
            shaderImageGatherExtended = other.shaderImageGatherExtended;
            shaderStorageImageExtendedFormats = other.shaderStorageImageExtendedFormats;
            shaderStorageImageMultisample = other.shaderStorageImageMultisample;
            shaderStorageImageReadWithoutFormat = other.shaderStorageImageReadWithoutFormat;
            shaderStorageImageWriteWithoutFormat = other.shaderStorageImageWriteWithoutFormat;
            shaderUniformBufferArrayDynamicIndexing = other.shaderUniformBufferArrayDynamicIndexing;
            shaderSampledImageArrayDynamicIndexing = other.shaderSampledImageArrayDynamicIndexing;
            shaderStorageBufferArrayDynamicIndexing = other.shaderStorageBufferArrayDynamicIndexing;
            shaderStorageImageArrayDynamicIndexing = other. shaderStorageImageArrayDynamicIndexing;
            shaderClipDistance = other.shaderClipDistance;
            shaderCullDistance = other.shaderCullDistance;
            shaderFloat64 = other.shaderFloat64;
            shaderInt64 = other.shaderInt64;
            shaderInt16 = other.shaderInt16;
            shaderResourceResidency = other.shaderResourceResidency;
            shaderResourceMinLod = other.shaderResourceMinLod;
            sparseBinding = other.sparseBinding;
            sparseResidencyBuffer = other.sparseResidencyBuffer;
            sparseResidencyImage2D = other. sparseResidencyImage2D;
            sparseResidencyImage3D = other. sparseResidencyImage3D;
            sparseResidency2Samples = other.sparseResidency2Samples;
            sparseResidency4Samples = other.sparseResidency4Samples;
            sparseResidency8Samples = other.sparseResidency8Samples;
            sparseResidency16Samples = other.sparseResidency16Samples;
            sparseResidencyAliased = other.sparseResidencyAliased;
            variableMultisampleRate = other.variableMultisampleRate;
            inheritedQueries = other.inheritedQueries;
        }

        internal VkPhysicalDeviceFeatures(Unmanaged.VkPhysicalDeviceFeatures other) {
            robustBufferAccess = other.robustBufferAccess != 0;
            fullDrawIndexUint32 = other.fullDrawIndexUint32 != 0;
            imageCubeArray = other.imageCubeArray != 0;
            independentBlend = other.independentBlend != 0;
            geometryShader = other.geometryShader != 0;
            tessellationShader = other.tessellationShader != 0;
            sampleRateShading = other.sampleRateShading != 0;
            dualSrcBlend = other.dualSrcBlend != 0;
            logicOp = other.logicOp != 0;
            multiDrawIndirect = other.multiDrawIndirect != 0;
            drawIndirectFirstInstance = other.drawIndirectFirstInstance != 0;
            depthClamp = other.depthClamp != 0;
            depthBiasClamp = other.depthBiasClamp != 0;
            fillModeNonSolid = other.fillModeNonSolid != 0;
            depthBounds = other.depthBounds != 0;
            wideLines = other.wideLines != 0;
            largePoints = other.largePoints != 0;
            alphaToOne = other.alphaToOne != 0;
            multiViewport = other.multiViewport != 0;
            samplerAnisotropy = other.samplerAnisotropy != 0;
            textureCompressionETC2 = other.textureCompressionETC2 != 0;
            textureCompressionASTC_LDR = other.textureCompressionASTC_LDR != 0;
            textureCompressionBC = other.textureCompressionBC != 0;
            occlusionQueryPrecise = other.occlusionQueryPrecise != 0;
            pipelineStatisticsQuery = other.pipelineStatisticsQuery != 0;
            vertexPipelineStoresAndAtomics = other.vertexPipelineStoresAndAtomics != 0;
            fragmentStoresAndAtomics = other.fragmentStoresAndAtomics != 0;
            shaderTessellationAndGeometryPointSize = other.shaderTessellationAndGeometryPointSize != 0;
            shaderImageGatherExtended = other.shaderImageGatherExtended != 0;
            shaderStorageImageExtendedFormats = other.shaderStorageImageExtendedFormats != 0;
            shaderStorageImageMultisample = other.shaderStorageImageMultisample != 0;
            shaderStorageImageReadWithoutFormat = other.shaderStorageImageReadWithoutFormat != 0;
            shaderStorageImageWriteWithoutFormat = other.shaderStorageImageWriteWithoutFormat != 0;
            shaderUniformBufferArrayDynamicIndexing = other.shaderUniformBufferArrayDynamicIndexing != 0;
            shaderSampledImageArrayDynamicIndexing = other.shaderSampledImageArrayDynamicIndexing != 0;
            shaderStorageBufferArrayDynamicIndexing = other.shaderStorageBufferArrayDynamicIndexing != 0;
            shaderStorageImageArrayDynamicIndexing = other.shaderStorageImageArrayDynamicIndexing != 0;
            shaderClipDistance = other.shaderClipDistance != 0;
            shaderCullDistance = other.shaderCullDistance != 0;
            shaderFloat64 = other.shaderFloat64 != 0;
            shaderInt64 = other.shaderInt64 != 0;
            shaderInt16 = other.shaderInt16 != 0;
            shaderResourceResidency = other.shaderResourceResidency != 0;
            shaderResourceMinLod = other.shaderResourceMinLod != 0;
            sparseBinding = other.sparseBinding != 0;
            sparseResidencyBuffer = other.sparseResidencyBuffer != 0;
            sparseResidencyImage2D = other.sparseResidencyImage2D != 0;
            sparseResidencyImage3D = other.sparseResidencyImage3D != 0;
            sparseResidency2Samples = other.sparseResidency2Samples != 0;
            sparseResidency4Samples = other.sparseResidency4Samples != 0;
            sparseResidency8Samples = other.sparseResidency8Samples != 0;
            sparseResidency16Samples = other.sparseResidency16Samples != 0;
            sparseResidencyAliased = other.sparseResidencyAliased != 0;
            variableMultisampleRate = other.variableMultisampleRate != 0;
            inheritedQueries = other.inheritedQueries != 0;
        }

        internal Unmanaged.VkPhysicalDeviceFeatures GetNative() {
            return new Unmanaged.VkPhysicalDeviceFeatures {
                robustBufferAccess = robustBufferAccess ? 1u : 0u,
                fullDrawIndexUint32 = fullDrawIndexUint32 ? 1u : 0u,
                imageCubeArray = imageCubeArray ? 1u : 0u,
                independentBlend = independentBlend ? 1u : 0u,
                geometryShader = geometryShader ? 1u : 0u,
                tessellationShader = tessellationShader ? 1u : 0u,
                sampleRateShading = sampleRateShading ? 1u : 0u,
                dualSrcBlend = dualSrcBlend ? 1u : 0u,
                logicOp = logicOp ? 1u : 0u,
                multiDrawIndirect = multiDrawIndirect ? 1u : 0u,
                drawIndirectFirstInstance = drawIndirectFirstInstance ? 1u : 0u,
                depthClamp = depthClamp ? 1u : 0u,
                depthBiasClamp = depthBiasClamp ? 1u : 0u,
                fillModeNonSolid = fillModeNonSolid ? 1u : 0u,
                depthBounds = depthBounds ? 1u : 0u,
                wideLines = wideLines ? 1u : 0u,
                largePoints = largePoints ? 1u : 0u,
                alphaToOne = alphaToOne ? 1u : 0u,
                multiViewport = multiViewport ? 1u : 0u,
                samplerAnisotropy = samplerAnisotropy ? 1u : 0u,
                textureCompressionETC2 = textureCompressionETC2 ? 1u : 0u,
                textureCompressionASTC_LDR = textureCompressionASTC_LDR ? 1u : 0u,
                textureCompressionBC = textureCompressionBC ? 1u : 0u,
                occlusionQueryPrecise = occlusionQueryPrecise ? 1u : 0u,
                pipelineStatisticsQuery = pipelineStatisticsQuery ? 1u : 0u,
                vertexPipelineStoresAndAtomics = vertexPipelineStoresAndAtomics ? 1u : 0u,
                fragmentStoresAndAtomics = fragmentStoresAndAtomics ? 1u : 0u,
                shaderTessellationAndGeometryPointSize = shaderTessellationAndGeometryPointSize ? 1u : 0u,
                shaderImageGatherExtended = shaderImageGatherExtended ? 1u : 0u,
                shaderStorageImageExtendedFormats = shaderStorageImageExtendedFormats ? 1u : 0u,
                shaderStorageImageMultisample = shaderStorageImageMultisample ? 1u : 0u,
                shaderStorageImageReadWithoutFormat = shaderStorageImageReadWithoutFormat ? 1u : 0u,
                shaderStorageImageWriteWithoutFormat = shaderStorageImageWriteWithoutFormat ? 1u : 0u,
                shaderUniformBufferArrayDynamicIndexing = shaderUniformBufferArrayDynamicIndexing ? 1u : 0u,
                shaderSampledImageArrayDynamicIndexing = shaderSampledImageArrayDynamicIndexing ? 1u : 0u,
                shaderStorageBufferArrayDynamicIndexing = shaderStorageBufferArrayDynamicIndexing ? 1u : 0u,
                shaderStorageImageArrayDynamicIndexing = shaderStorageImageArrayDynamicIndexing ? 1u : 0u,
                shaderClipDistance = shaderClipDistance ? 1u : 0u,
                shaderCullDistance = shaderCullDistance ? 1u : 0u,
                shaderFloat64 = shaderFloat64 ? 1u : 0u,
                shaderInt64 = shaderInt64 ? 1u : 0u,
                shaderInt16 = shaderInt16 ? 1u : 0u,
                shaderResourceResidency = shaderResourceResidency ? 1u : 0u,
                shaderResourceMinLod = shaderResourceMinLod ? 1u : 0u,
                sparseBinding = sparseBinding ? 1u : 0u,
                sparseResidencyBuffer = sparseResidencyBuffer ? 1u : 0u,
                sparseResidencyImage2D = sparseResidencyImage2D ? 1u : 0u,
                sparseResidencyImage3D = sparseResidencyImage3D ? 1u : 0u,
                sparseResidency2Samples = sparseResidency2Samples ? 1u : 0u,
                sparseResidency4Samples = sparseResidency4Samples ? 1u : 0u,
                sparseResidency8Samples = sparseResidency8Samples ? 1u : 0u,
                sparseResidency16Samples = sparseResidency16Samples ? 1u : 0u,
                sparseResidencyAliased = sparseResidencyAliased ? 1u : 0u,
                variableMultisampleRate = variableMultisampleRate ? 1u : 0u,
                inheritedQueries = inheritedQueries ? 1u : 0u,
            };
        }
    }
}
