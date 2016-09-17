using System;
using System.Collections.Generic;

namespace CSGL.Vulkan.Managed {
    public class CommandBufferInheritanceInfo {
        public RenderPass RenderPass { get; set; }
        public uint Subpass { get; set; }
        public Framebuffer Framebuffer { get; set; }
        public bool OcclusionQueryEnable { get; set; }
        public VkQueryControlFlags QueryFlags { get; set; }
        public VkQueryPipelineStatisticFlags PipelineStatistics { get; set; }

        internal VkCommandBufferInheritanceInfo GetNative() {
            VkCommandBufferInheritanceInfo info = new VkCommandBufferInheritanceInfo();
            info.sType = VkStructureType.StructureTypeCommandBufferInheritanceInfo;
            info.renderPass = RenderPass.Native;
            info.subpass = Subpass;
            info.framebuffer = Framebuffer.Native;
            info.occlusionQueryEnable = OcclusionQueryEnable ? (uint)1 : (uint)0;
            info.queryFlags = QueryFlags;
            info.pipelineStatistics = PipelineStatistics;

            return info;
        }
    }

    public class CommandBeginInfo {
        public VkCommandBufferUsageFlags Flags { get; set; }
        public CommandBufferInheritanceInfo InheritanceInfo { get; set; }

        internal VkCommandBufferBeginInfo GetNative(List<IDisposable> marshalled) {
            VkCommandBufferBeginInfo info = new VkCommandBufferBeginInfo();
            info.sType = VkStructureType.StructureTypeCommandBufferBeginInfo;
            info.flags = Flags;

            if (InheritanceInfo != null) {
                var inheritanceInfoMarshalled = new Marshalled<VkCommandBufferInheritanceInfo>(InheritanceInfo.GetNative());
                info.pInheritanceInfo = inheritanceInfoMarshalled.Address;
                marshalled.Add(inheritanceInfoMarshalled);
            }

            return info;
        }
    }

    public class RenderPassBeginInfo {
        public RenderPass RenderPass { get; set; }
        public Framebuffer Framebuffer { get; set; }
        public VkRect2D RenderArea { get; set; }
        public VkClearValue[] ClearValues { get; set; }

        internal VkRenderPassBeginInfo GetNative(List<IDisposable> marshalled) {
            VkRenderPassBeginInfo info = new VkRenderPassBeginInfo();
            info.sType = VkStructureType.StructureTypeRenderPassBeginInfo;
            info.renderPass = RenderPass.Native;
            info.framebuffer = Framebuffer.Native;
            info.renderArea = RenderArea;

            var clearValuesMarshalled = new PinnedArray<VkClearValue>(ClearValues);
            info.clearValueCount = (uint)ClearValues.Length;
            info.pClearValues = clearValuesMarshalled.Address;

            marshalled.Add(clearValuesMarshalled);

            return info;
        }
    }
}
