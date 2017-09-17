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

    public struct VkClearColorValue {
        Unmanaged.VkClearColorValue value;

        internal Unmanaged.VkClearColorValue GetNative() {
            return value;
        }

        public void Set(int r, int g, int b, int a) {
            value.int32_0 = r;
            value.int32_1 = g;
            value.int32_2 = b;
            value.int32_3 = a;
        }

        public void Set(float r, float g, float b, float a) {
            value.float32_0 = r;
            value.float32_1 = g;
            value.float32_2 = b;
            value.float32_3 = a;
        }

        public void Set(uint r, uint g, uint b, uint a) {
            value.uint32_0 = r;
            value.uint32_1 = g;
            value.uint32_2 = b;
            value.uint32_3 = a;
        }

        public void Get(out int r, out int g, out int b, out int a) {
            r = value.int32_0;
            g = value.int32_1;
            b = value.int32_2;
            a = value.int32_3;
        }

        public void Get(out float r, out float g, out float b, out float a) {
            r = value.float32_0;
            g = value.float32_1;
            b = value.float32_2;
            a = value.float32_3;
        }

        public void Get(out uint r, out uint g, out uint b, out uint a) {
            r = value.uint32_0;
            g = value.uint32_1;
            b = value.uint32_2;
            a = value.uint32_3;
        }
    }

    public struct VkClearDepthStencilValue {
        float depth;
        int stencil;

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
                color = color.GetNative(),
                depthStencil = depthStencil.GetNative()
            };
        }
    }
}
