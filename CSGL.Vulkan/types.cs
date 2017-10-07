using System;
using System.Collections.Generic;

namespace CSGL.Vulkan {
    public struct VkOffset2D {
        public int x;
        public int y;

        internal Unmanaged.VkOffset2D GetNative() {
            return new Unmanaged.VkOffset2D {
                x = x,
                y = y
            };
        }
    }

    public struct VkOffset3D {
        public int x;
        public int y;
        public int z;

        internal Unmanaged.VkOffset3D GetNative() {
            return new Unmanaged.VkOffset3D {
                x = x,
                y = y,
                z = z
            };
        }
    }

    public struct VkExtent2D {
        public int width;
        public int height;

        internal VkExtent2D(Unmanaged.VkExtent2D other) {
            width = (int)other.width;
            height = (int)other.height;
        }

        internal Unmanaged.VkExtent2D GetNative() {
            return new Unmanaged.VkExtent2D {
                width = (uint)width,
                height = (uint)height
            };
        }
    }

    public struct VkExtent3D {
        public int width;
        public int height;
        public int depth;

        internal VkExtent3D(Unmanaged.VkExtent3D other) {
            width = (int)other.width;
            height = (int)other.height;
            depth = (int)other.depth;
        }

        internal Unmanaged.VkExtent3D GetNative() {
            return new Unmanaged.VkExtent3D {
                width = (uint)width,
                height = (uint)height,
                depth = (uint)depth
            };
        }
    }

    public struct VkViewport {
        public float x;
        public float y;
        public float width;
        public float height;
        public float minDepth;
        public float maxDepth;

        internal Unmanaged.VkViewport GetNative() {
            return new Unmanaged.VkViewport {
                x = x,
                y = y,
                width = width,
                height = height,
                minDepth = minDepth,
                maxDepth = maxDepth
            };
        }
    }

    public struct VkRect2D {
        public VkOffset2D offset;
        public VkExtent2D extent;

        internal Unmanaged.VkRect2D GetNative() {
            return new Unmanaged.VkRect2D {
                offset = offset.GetNative(),
                extent = extent.GetNative()
            };
        }
    }

    public struct VkRect3D {
        public VkOffset3D offset;
        public VkExtent3D extent;

        internal Unmanaged.VkRect3D GetNative() {
            return new Unmanaged.VkRect3D {
                offset = offset.GetNative(),
                extent = extent.GetNative()
            };
        }
    }

    public struct VkClearRect {
        public VkRect2D rect;
        public int baseArrayLayer;
        public int layerCount;

        internal Unmanaged.VkClearRect GetNative() {
            return new Unmanaged.VkClearRect {
                rect = rect.GetNative(),
                baseArrayLayer = (uint)baseArrayLayer,
                layerCount = (uint)layerCount
            };
        }
    }

    public struct VkComponentMapping {
        public VkComponentSwizzle r;
        public VkComponentSwizzle g;
        public VkComponentSwizzle b;
        public VkComponentSwizzle a;

        internal Unmanaged.VkComponentMapping GetNative() {
            return new Unmanaged.VkComponentMapping {
                r = r,
                g = g,
                b = b,
                a = a
            };
        }
    }

    public struct VkMemoryRequirements {
        public long size;
        public long alignment;
        public uint memoryTypeBits;

        internal VkMemoryRequirements(Unmanaged.VkMemoryRequirements native) {
            size = (long)native.size;
            alignment = (long)native.alignment;
            memoryTypeBits = native.memoryTypeBits;
        }
    }

    public struct VkImageSubresource {
        public VkImageAspectFlags aspectMask;
        public int mipLevel;
        public int arrayLayer;

        internal Unmanaged.VkImageSubresource GetNative() {
            return new Unmanaged.VkImageSubresource {
                aspectMask = aspectMask,
                mipLevel = (uint)mipLevel,
                arrayLayer = (uint)arrayLayer
            };
        }
    }

    public struct VkImageSubresourceRange {
        public VkImageAspectFlags aspectMask;
        public int baseMipLevel;
        public int levelCount;
        public int baseArrayLayer;
        public int layerCount;

        internal Unmanaged.VkImageSubresourceRange GetNative() {
            return new Unmanaged.VkImageSubresourceRange {
                aspectMask = aspectMask,
                baseMipLevel = (uint)baseMipLevel,
                levelCount = (uint)levelCount,
                baseArrayLayer = (uint)baseArrayLayer,
                layerCount = (uint)layerCount
            };
        }
    }

    public struct VkImageSubresourceLayers {
        public VkImageAspectFlags aspectMask;
        public int mipLevel;
        public int baseArrayLayer;
        public int layerCount;

        internal Unmanaged.VkImageSubresourceLayers GetNative() {
            return new Unmanaged.VkImageSubresourceLayers {
                aspectMask = aspectMask,
                mipLevel = (uint)mipLevel,
                baseArrayLayer = (uint)baseArrayLayer,
                layerCount = (uint)layerCount
            };
        }
    }

    public struct VkClearColorValue {
        internal uint uint32_0;
        internal uint uint32_1;
        internal uint uint32_2;
        internal uint uint32_3;

        public VkClearColorValue(int r, int g, int b, int a) {
            uint32_0 = 0;
            uint32_1 = 0;
            uint32_2 = 0;
            uint32_3 = 0;
            Set(r, g, b, a);
        }

        public VkClearColorValue(uint r, uint g, uint b, uint a) {
            uint32_0 = 0;
            uint32_1 = 0;
            uint32_2 = 0;
            uint32_3 = 0;
            Set(r, g, b, a);
        }

        public VkClearColorValue(float r, float g, float b, float a) {
            uint32_0 = 0;
            uint32_1 = 0;
            uint32_2 = 0;
            uint32_3 = 0;
            Set(r, g, b, a);
        }

        public void Set(int r, int g, int b, int a) {
            uint32_0 = (uint)r;
            uint32_1 = (uint)g;
            uint32_2 = (uint)b;
            uint32_3 = (uint)a;
        }

        public void Set(float r, float g, float b, float a) {
            unsafe {
                uint32_0 = *((uint*)(&r));
                uint32_1 = *((uint*)(&g));
                uint32_2 = *((uint*)(&b));
                uint32_3 = *((uint*)(&a));
            }
        }

        public void Set(uint r, uint g, uint b, uint a) {
            uint32_0 = r;
            uint32_1 = g;
            uint32_2 = b;
            uint32_3 = a;
        }

        public void Get(out int r, out int g, out int b, out int a) {
            r = (int)uint32_0;
            g = (int)uint32_1;
            b = (int)uint32_2;
            a = (int)uint32_3;
        }

        public void Get(out float r, out float g, out float b, out float a) {
            uint uint32_0 = this.uint32_0;
            uint uint32_1 = this.uint32_1;
            uint uint32_2 = this.uint32_2;
            uint uint32_3 = this.uint32_3;
            unsafe {
                r = *((float*)(&uint32_0));
                g = *((float*)(&uint32_1));
                b = *((float*)(&uint32_2));
                a = *((float*)(&uint32_3));
            }
        }

        public void Get(out uint r, out uint g, out uint b, out uint a) {
            r = uint32_0;
            g = uint32_1;
            b = uint32_2;
            a = uint32_3;
        }
    }

    public struct VkClearDepthStencilValue {
        public float depth;
        public int stencil;

        internal Unmanaged.VkClearDepthStencilValue GetNative() {
            return new Unmanaged.VkClearDepthStencilValue {
                depth = depth,
                stencil = (uint)stencil
            };
        }
    }

    public struct VkClearValue {
        public VkClearColorValue color;
        public VkClearDepthStencilValue depthStencil;

        internal Unmanaged.VkClearValue GetNative() {
            return new Unmanaged.VkClearValue {
                depthStencil = depthStencil.GetNative()
            };
        }
    }

    public struct VkBufferCopy {
        public long srcOffset;
        public long dstOffset;
        public long size;

        internal Unmanaged.VkBufferCopy GetNative() {
            return new Unmanaged.VkBufferCopy {
                srcOffset = (ulong)srcOffset,
                dstOffset = (ulong)dstOffset,
                size = (ulong)size
            };
        }
    }

    public struct VkImageCopy {
        public VkImageSubresourceLayers srcSubresource;
        public VkOffset3D srcOffset;
        public VkImageSubresourceLayers dstSubresource;
        public VkOffset3D dstOffset;
        public VkExtent3D extent;

        internal Unmanaged.VkImageCopy GetNative() {
            return new Unmanaged.VkImageCopy {
                srcSubresource = srcSubresource.GetNative(),
                srcOffset = srcOffset.GetNative(),
                dstSubresource = dstSubresource.GetNative(),
                dstOffset = dstOffset.GetNative(),
                extent = extent.GetNative()
            };
        }
    }

    public struct VkImageBlit {
        public VkImageSubresourceLayers srcSubresource;
        public VkOffset3D srcOffsets0;
        public VkOffset3D srcOffsets1;
        public VkImageSubresourceLayers dstSubresource;
        public VkOffset3D dstOffsets0;
        public VkOffset3D dstOffsets1;

        internal Unmanaged.VkImageBlit GetNative() {
            return new Unmanaged.VkImageBlit {
                srcSubresource = srcSubresource.GetNative(),
                srcOffsets_0 = srcOffsets0.GetNative(),
                srcOffsets_1 = srcOffsets1.GetNative(),
                dstSubresource = dstSubresource.GetNative(),
                dstOffsets_0 = dstOffsets0.GetNative(),
                dstOffsets_1 = dstOffsets1.GetNative()
            };
        }
    }

    public struct VkBufferImageCopy {
        public long bufferOffset;
        public int bufferRowLength;
        public int bufferImageHeight;
        public VkImageSubresourceLayers imageSubresource;
        public VkOffset3D imageOffset;
        public VkExtent3D imageExtent;

        internal Unmanaged.VkBufferImageCopy GetNative() {
            return new Unmanaged.VkBufferImageCopy {
                bufferOffset = (ulong)bufferOffset,
                bufferRowLength = (uint)bufferRowLength,
                bufferImageHeight = (uint)bufferImageHeight,
                imageSubresource = imageSubresource.GetNative(),
                imageOffset = imageOffset.GetNative(),
                imageExtent = imageExtent.GetNative()
            };
        }
    }

    public struct VkClearAttachment {
        public VkImageAspectFlags aspectMask;
        public int colorAttachment;
        public VkClearValue clearValue;

        internal Unmanaged.VkClearAttachment GetNative() {
            return new Unmanaged.VkClearAttachment {
                aspectMask = aspectMask,
                colorAttachment = (uint)colorAttachment,
                clearValue = clearValue.GetNative()
            };
        }
    }

    public struct VkImageResolve {
        public VkImageSubresourceLayers srcSubresource;
        public VkOffset3D srcOffset;
        public VkImageSubresourceLayers dstSubresource;
        public VkOffset3D dstOffset;
        public VkExtent3D extent;

        internal Unmanaged.VkImageResolve GetNative() {
            return new Unmanaged.VkImageResolve {
                srcSubresource = srcSubresource.GetNative(),
                srcOffset = srcOffset.GetNative(),
                dstSubresource = dstSubresource.GetNative(),
                dstOffset = dstOffset.GetNative(),
                extent = extent.GetNative()
            };
        }
    }

    public struct VkStencilOpState {
        public VkStencilOp failOp;
        public VkStencilOp passOp;
        public VkStencilOp depthFailOp;
        public VkCompareOp compareOp;
        public uint compareMask;
        public uint writeMask;
        public int reference;

        internal Unmanaged.VkStencilOpState GetNative() {
            return new Unmanaged.VkStencilOpState {
                failOp = failOp,
                passOp = passOp,
                depthFailOp = depthFailOp,
                compareOp = compareOp,
                compareMask = compareMask,
                writeMask = writeMask,
                reference = (uint)reference
            };
        }
    }

    public struct VkDescriptorPoolSize {
        public VkDescriptorType type;
        public int descriptorCount;

        internal Unmanaged.VkDescriptorPoolSize GetNative() {
            return new Unmanaged.VkDescriptorPoolSize {
                type = type,
                descriptorCount = (uint)descriptorCount
            };
        }
    }

    public struct VkSubresourceLayout {
        public long offset;
        public long size;
        public long rowPitch;
        public long arrayPitch;
        public long depthPitch;

        internal VkSubresourceLayout(Unmanaged.VkSubresourceLayout other) {
            offset = (long)other.offset;
            size = (long)other.size;
            rowPitch = (long)other.rowPitch;
            arrayPitch = (long)other.arrayPitch;
            depthPitch = (long)other.depthPitch;
        }
    }

    public struct VkSparseImageFormatProperties {
        public VkImageAspectFlags aspectMask;
        public VkExtent3D imageGranularity;
        public VkSparseImageFormatFlags flags;

        internal VkSparseImageFormatProperties(Unmanaged.VkSparseImageFormatProperties other) {
            aspectMask = other.aspectMask;
            imageGranularity = new VkExtent3D(other.imageGranularity);
            flags = other.flags;
        }
    }

    public struct VkSparseImageMemoryRequirements {
        public VkSparseImageFormatProperties formatProperties;
        public int imageMipTailFirstLod;
        public long imageMipTailSize;
        public long imageMipTailOffset;
        public long imageMipTailStride;

        internal VkSparseImageMemoryRequirements(Unmanaged.VkSparseImageMemoryRequirements other) {
            formatProperties = new VkSparseImageFormatProperties(other.formatProperties);
            imageMipTailFirstLod = (int)other.imageMipTailFirstLod;
            imageMipTailSize = (long)other.imageMipTailSize;
            imageMipTailOffset = (long)other.imageMipTailOffset;
            imageMipTailStride = (long)other.imageMipTailStride;
        }
    }

    public struct VkFormatProperties {
        public VkFormatFeatureFlags linearTilingFeatures;
        public VkFormatFeatureFlags optimalTilingFeatures;
        public VkFormatFeatureFlags bufferFeatures;

        internal VkFormatProperties(Unmanaged.VkFormatProperties other) {
            linearTilingFeatures = other.linearTilingFeatures;
            optimalTilingFeatures = other.optimalTilingFeatures;
            bufferFeatures = other.bufferFeatures;
        }
    }

    public struct VkImageFormatProperties {
        public VkExtent3D maxExtent;
        public int maxMipLevels;
        public int maxArrayLayers;
        public VkSampleCountFlags sampleCounts;
        public long maxResourceSize;

        internal VkImageFormatProperties(Unmanaged.VkImageFormatProperties other) {
            maxExtent = new VkExtent3D(other.maxExtent);
            maxMipLevels = (int)other.maxMipLevels;
            maxArrayLayers = (int)other.maxArrayLayers;
            sampleCounts = other.sampleCounts;
            maxResourceSize = (long)other.maxResourceSize;
        }
    }

    public struct VkPhysicalDeviceSparseProperties {
        public bool residencyStandard2DBlockShape;
        public bool residencyStandard2DMultisampleBlockShape;
        public bool residencyStandard3DBlockShape;
        public bool residencyAlignedMipSize;
        public bool residencyNonResidentStrict;

        internal VkPhysicalDeviceSparseProperties(Unmanaged.VkPhysicalDeviceSparseProperties other) {
            residencyStandard2DBlockShape = other.residencyStandard2DBlockShape != 0;
            residencyStandard2DMultisampleBlockShape = other.residencyStandard2DMultisampleBlockShape != 0;
            residencyStandard3DBlockShape = other.residencyStandard3DBlockShape != 0;
            residencyAlignedMipSize = other.residencyAlignedMipSize != 0;
            residencyNonResidentStrict = other.residencyNonResidentStrict != 0;
        }
    }

    public struct VkPushConstantRange {
        public VkShaderStageFlags stageFlags;
        public int offset;
        public int size;

        internal Unmanaged.VkPushConstantRange GetNative() {
            return new Unmanaged.VkPushConstantRange {
                stageFlags = stageFlags,
                offset = (uint)offset,
                size = (uint)size
            };
        }
    }

    public struct VkSurfaceCapabilitiesKHR {
        public int minImageCount;
        public int maxImageCount;
        public VkExtent2D currentExtent;
        public VkExtent2D minImageExtent;
        public VkExtent2D maxImageExtent;
        public int maxImageArrayLayers;
        public VkSurfaceTransformFlagsKHR supportedTransforms;
        public VkSurfaceTransformFlagsKHR currentTransform;
        public VkCompositeAlphaFlagsKHR supportedCompositeAlpha;
        public VkImageUsageFlags supportedUsageFlags;

        internal VkSurfaceCapabilitiesKHR(Unmanaged.VkSurfaceCapabilitiesKHR other) {
            minImageCount = (int)other.minImageCount;
            maxImageCount = (int)other.maxImageCount;
            currentExtent = new VkExtent2D(other.currentExtent);
            minImageExtent = new VkExtent2D(other.minImageExtent);
            maxImageExtent = new VkExtent2D(other.maxImageExtent);
            maxImageArrayLayers = (int)other.maxImageArrayLayers;
            supportedTransforms = other.supportedTransforms;
            currentTransform = other.currentTransform;
            supportedCompositeAlpha = other.supportedCompositeAlpha;
            supportedUsageFlags = other.supportedUsageFlags;
        }
    }

    public struct VkSurfaceFormatKHR {
        public VkFormat format;
        public VkColorSpaceKHR colorSpace;

        internal VkSurfaceFormatKHR(Unmanaged.VkSurfaceFormatKHR other) {
            format = other.format;
            colorSpace = other.colorSpace;
        }
    }

    public struct VkSpecializationMapEntry {
        public int constantID;
        public int offset;
        public long size;

        internal Unmanaged.VkSpecializationMapEntry GetNative() {
            return new Unmanaged.VkSpecializationMapEntry {
                constantID = (uint)constantID,
                offset = (uint)offset,
                size = (IntPtr)size
            };
        }
    }

    public struct VkVertexInputBindingDescription {
        public int binding;
        public int stride;
        public VkVertexInputRate inputRate;

        internal Unmanaged.VkVertexInputBindingDescription GetNative() {
            return new Unmanaged.VkVertexInputBindingDescription {
                binding = (uint)binding,
                stride = (uint)stride,
                inputRate = inputRate
            };
        }
    }
    
    public struct VkVertexInputAttributeDescription {
        public int location;
        public int binding;
        public VkFormat format;
        public int offset;

        internal Unmanaged.VkVertexInputAttributeDescription GetNative() {
            return new Unmanaged.VkVertexInputAttributeDescription {
                location = (uint)location,
                binding = (uint)binding,
                format = format,
                offset = (uint)offset
            };
        }
    }
}
