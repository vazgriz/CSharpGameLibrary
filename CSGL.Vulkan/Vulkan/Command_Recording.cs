using System;
using System.Collections.Generic;

namespace CSGL.Vulkan {
    public class CommandBufferInheritanceInfo {
        public RenderPass renderPass;
        public uint subpass;
        public Framebuffer framebuffer;
        public bool occlusionQueryEnable;
        public VkQueryControlFlags queryFlags;
        public VkQueryPipelineStatisticFlags pipelineStatistics;

        internal VkCommandBufferInheritanceInfo GetNative() {
            VkCommandBufferInheritanceInfo info = new VkCommandBufferInheritanceInfo();
            info.sType = VkStructureType.CommandBufferInheritanceInfo;
            info.renderPass = renderPass.Native;
            info.subpass = subpass;
            info.framebuffer = framebuffer.Native;
            info.occlusionQueryEnable = occlusionQueryEnable ? 1u : 0u;
            info.queryFlags = queryFlags;
            info.pipelineStatistics = pipelineStatistics;

            return info;
        }
    }

    public class CommandBufferBeginInfo {
        public VkCommandBufferUsageFlags flags;
        public CommandBufferInheritanceInfo inheritanceInfo;

        internal VkCommandBufferBeginInfo GetNative(DisposableList<IDisposable> marshalled) {
            VkCommandBufferBeginInfo info = new VkCommandBufferBeginInfo();
            info.sType = VkStructureType.CommandBufferBeginInfo;
            info.flags = flags;

            if (inheritanceInfo != null) {
                var inheritanceInfoMarshalled = new Marshalled<VkCommandBufferInheritanceInfo>(inheritanceInfo.GetNative());
                info.pInheritanceInfo = inheritanceInfoMarshalled.Address;
                marshalled.Add(inheritanceInfoMarshalled);
            }

            return info;
        }
    }

    public class RenderPassBeginInfo {
        public RenderPass renderPass;
        public Framebuffer framebuffer;
        public VkRect2D renderArea;
        public VkClearValue[] clearValues;

        internal VkRenderPassBeginInfo GetNative(DisposableList<IDisposable> marshalled) {
            VkRenderPassBeginInfo info = new VkRenderPassBeginInfo();
            info.sType = VkStructureType.RenderPassBeginInfo;
            info.renderPass = renderPass.Native;
            info.framebuffer = framebuffer.Native;
            info.renderArea = renderArea;

            var clearValuesMarshalled = new PinnedArray<VkClearValue>(clearValues);
            info.clearValueCount = (uint)clearValues.Length;
            info.pClearValues = clearValuesMarshalled.Address;

            marshalled.Add(clearValuesMarshalled);

            return info;
        }
    }
    public class MemoryBarrier {
        public VkAccessFlags srcAccessMask;
        public VkAccessFlags dstAccessMask;
    }
    
    public class BufferMemoryBarrier {
        public VkAccessFlags srcAccessMask;
        public VkAccessFlags dstAccessMask;
        public uint srcQueueFamilyIndex;
        public uint dstQueueFamilyIndex;
        public Buffer buffer;
        public ulong offset;
        public ulong size;
    }

    public class ImageMemoryBarrier {
        public VkAccessFlags srcAccessMask;
        public VkAccessFlags dstAccessMask;
        public VkImageLayout oldLayout;
        public VkImageLayout newLayout;
        public uint srcQueueFamilyIndex;
        public uint dstQueueFamilyIndex;
        public Image image;
        public VkImageSubresourceRange subresourceRange;
    }
}
