using System;
using System.Collections.Generic;

namespace CSGL.Vulkan {
    public struct VkOffset2D {
        public int x;
        public int y;
    }

    public struct VkOffset3D {
        public int x;
        public int y;
        public int z;
    }

    public struct VkExtent2D {
        public int width;
        public int height;
    }

    public struct VkExtent3D {
        public int width;
        public int height;
        public int depth;
    }

    public struct VkViewport {
        public float x;
        public float y;
        public float width;
        public float height;
        public float minDepth;
        public float maxDepth;
    }

    public struct VkRect2D {
        public VkOffset2D offset;
        public VkExtent2D extent;
    }

    public struct VkRect3D {
        public VkOffset3D offset;
        public VkExtent3D extent;
    }

    public struct VkClearRect {
        public VkRect2D rect;
        public int baseArrayLayer;
        public int layerCount;
    }

    public struct VkComponentMapping {
        public VkComponentSwizzle r;
        public VkComponentSwizzle g;
        public VkComponentSwizzle b;
        public VkComponentSwizzle a;
    }

    public  struct VkMemoryRequirements {
        public long size;
        public long alignment;
        public uint memoryTypeBits;
    }
}
