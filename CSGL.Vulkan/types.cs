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
}
