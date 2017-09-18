using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace CSGL.Vulkan {
    public class VkCommandBufferAllocateInfo {
        public VkCommandBufferLevel level;
        public int commandBufferCount;
    }

    public class VkCommandBufferInheritanceInfo {
        public VkRenderPass renderPass;
        public int subpass;
        public VkFramebuffer framebuffer;
        public bool occlusionQueryEnable;
        public VkQueryControlFlags queryFlags;
        public VkQueryPipelineStatisticFlags pipelineStatistics;
    }

    public class VkCommandBufferBeginInfo {
        public VkCommandBufferUsageFlags flags;
        public VkCommandBufferInheritanceInfo inheritanceInfo;
    }

    public class VkRenderPassBeginInfo {
        public VkRenderPass renderPass;
        public VkFramebuffer framebuffer;
        public VkRect2D renderArea;
        public IList<VkClearValue> clearValues;
    }

    public class VkMemoryBarrier {
        public VkAccessFlags srcAccessMask;
        public VkAccessFlags dstAccessMask;
    }

    public class VkBufferMemoryBarrier {
        public VkAccessFlags srcAccessMask;
        public VkAccessFlags dstAccessMask;
        public int srcQueueFamilyIndex;
        public int dstQueueFamilyIndex;
        public VkBuffer buffer;
        public long offset;
        public long size;
    }

    public class VkImageMemoryBarrier {
        public VkAccessFlags srcAccessMask;
        public VkAccessFlags dstAccessMask;
        public VkImageLayout oldLayout;
        public VkImageLayout newLayout;
        public int srcQueueFamilyIndex;
        public int dstQueueFamilyIndex;
        public VkImage image;
        public VkImageSubresourceRange subresourceRange;
    }

    public class VkCommandBuffer : IDisposable, INative<Unmanaged.VkCommandBuffer> {
        Unmanaged.VkCommandBuffer commandBuffer;
        bool disposed;

        public Unmanaged.VkCommandBuffer Native {
            get {
                return commandBuffer;
            }
        }

        public VkDevice Device { get; private set; }
        public VkCommandPool Pool { get; private set; }
        public VkCommandBufferLevel Level { get; private set; }

        //set to false if pool is reset
        //prevents double free
        internal bool CanDispose { get; set; } = true;

        internal VkCommandBuffer(VkDevice device, VkCommandPool pool, Unmanaged.VkCommandBuffer commandBuffer, VkCommandBufferLevel level) {
            Device = device;
            Pool = pool;
            this.commandBuffer = commandBuffer;
            Level = level;
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        void Dispose(bool disposing) {
            if (disposed) return;

            if (CanDispose) {
                Pool.Free(this);
            }

            disposed = true;
        }

        ~VkCommandBuffer() {
            Dispose(false);
        }

        public void Begin(VkCommandBufferBeginInfo info) {
            if (info == null) throw new ArgumentNullException(nameof(info));

            unsafe {
                var infoNative = new Unmanaged.VkCommandBufferBeginInfo();
                infoNative.sType = VkStructureType.CommandBufferBeginInfo;
                infoNative.flags = info.flags;

                var inheritanceNative = new Unmanaged.VkCommandBufferInheritanceInfo();
                if (info.inheritanceInfo != null) {
                    inheritanceNative.sType = VkStructureType.CommandBufferInheritanceInfo;
                    inheritanceNative.renderPass = info.inheritanceInfo.renderPass.Native;
                    inheritanceNative.subpass = (uint)info.inheritanceInfo.subpass;

                    if (info.inheritanceInfo.framebuffer != null) inheritanceNative.framebuffer = info.inheritanceInfo.framebuffer.Native;

                    inheritanceNative.occlusionQueryEnable = info.inheritanceInfo.occlusionQueryEnable ? 1u : 0u;
                    inheritanceNative.queryFlags = info.inheritanceInfo.queryFlags;
                    inheritanceNative.pipelineStatistics = info.inheritanceInfo.pipelineStatistics;

                    infoNative.pInheritanceInfo = (IntPtr)(&inheritanceNative);
                }

                var result = Device.Commands.beginCommandBuffer(commandBuffer, ref infoNative);
                if (result != VkResult.Success) throw new CommandBufferException(result, string.Format("Error beginning command buffer: {0}", result));
            }
        }

        public void BeginRenderPass(VkRenderPassBeginInfo info, VkSubpassContents contents) {
            if (info == null) throw new ArgumentNullException(nameof(info));

            unsafe {
                int clearValueCount = 0;
                if (info.clearValues != null) clearValueCount = info.clearValues.Count;

                var infoNative = new Unmanaged.VkRenderPassBeginInfo();
                infoNative.sType = VkStructureType.RenderPassBeginInfo;
                infoNative.renderPass = info.renderPass.Native;
                infoNative.framebuffer = info.framebuffer.Native;
                infoNative.renderArea = info.renderArea.GetNative();

                var clearValuesNative = stackalloc Unmanaged.VkClearValue[clearValueCount];
                if (info.clearValues != null) {
                    for (int i = 0; i < clearValueCount; i++) {
                        clearValuesNative[i] = info.clearValues[i].GetNative();
                    }
                }
                infoNative.clearValueCount = (uint)clearValueCount;
                infoNative.pClearValues = (IntPtr)clearValuesNative;

                Device.Commands.cmdBeginRenderPass(commandBuffer, ref infoNative, contents);
            }
        }

        public void NextSubpass(VkSubpassContents contents) {
            Device.Commands.cmdNextSubpass(commandBuffer, contents);
        }

        public void EndRenderPass() {
            Device.Commands.cmdEndRenderPass(commandBuffer);
        }

        public void End() {
            var result = Device.Commands.endCommandBuffer(commandBuffer);
            if (result != VkResult.Success) throw new CommandBufferException(result, string.Format("Error ending command buffer: {0}", result));
        }

        public void Reset(VkCommandBufferResetFlags flags) {
            var result = Device.Commands.resetCommandBuffers(commandBuffer, flags);
            if (result != VkResult.Success) throw new CommandBufferException(result, string.Format("Error resetting command buffer: {0}"));
        }

        public void BindPipeline(VkPipelineBindPoint bindPoint, VkPipeline pipeline) {
            if (pipeline == null) throw new ArgumentNullException(nameof(pipeline));

            Device.Commands.cmdBindPipeline(commandBuffer, bindPoint, pipeline.Native);
        }

        public void BindVertexBuffers(int firstBinding, IList<VkBuffer> buffers, IList<long> offsets) {
            if (buffers == null) throw new ArgumentNullException(nameof(buffers));
            if (offsets == null) throw new ArgumentNullException(nameof(offsets));

            unsafe {
                var buffersNative = stackalloc Unmanaged.VkBuffer[buffers.Count];
                var offsetsNative = stackalloc ulong[offsets.Count];

                Interop.Marshal<Unmanaged.VkBuffer, VkBuffer>(buffers, buffersNative);
                Interop.Copy(offsets, (IntPtr)offsetsNative);

                Device.Commands.cmdBindVertexBuffers(commandBuffer, (uint)firstBinding, (uint)buffers.Count, (IntPtr)(buffersNative), ref offsetsNative[0]);
            }
        }

        public void BindVertexBuffers(int firstBinding, VkBuffer buffer, long offset) {
            if (buffer == null) throw new ArgumentNullException(nameof(buffer));

            unsafe {
                Unmanaged.VkBuffer bufferNative = buffer.Native;
                ulong offsetNative = (ulong)offset;
                Device.Commands.cmdBindVertexBuffers(commandBuffer, (uint)firstBinding, 1, (IntPtr)(&bufferNative), ref offsetNative);
            }
        }

        public void BindIndexBuffer(VkBuffer buffer, long offset, VkIndexType indexType) {
            if (buffer == null) throw new ArgumentNullException(nameof(buffer));

            Device.Commands.cmdBindIndexBuffer(commandBuffer, buffer.Native, (ulong)offset, indexType);
        }

        public void BindDescriptorSets(VkPipelineBindPoint pipelineBindPoint, VkPipelineLayout layout, int firstSet, IList<VkDescriptorSet> descriptorSets, IList<int> dynamicOffsets) {
            if (descriptorSets == null) throw new ArgumentNullException(nameof(descriptorSets));

            unsafe {
                int dynamicOffsetCount = 0;
                if (dynamicOffsets != null) dynamicOffsetCount = dynamicOffsets.Count;

                var sets = stackalloc Unmanaged.VkDescriptorSet[descriptorSets.Count];
                var offsets = stackalloc uint[dynamicOffsetCount];

                Interop.Marshal<Unmanaged.VkDescriptorSet, VkDescriptorSet>(descriptorSets, sets);

                if (dynamicOffsets != null) Interop.Copy(dynamicOffsets, (IntPtr)offsets);

                Device.Commands.cmdBindDescriptorSets(commandBuffer, pipelineBindPoint, layout.Native,
                    (uint)firstSet, (uint)descriptorSets.Count, (IntPtr)sets,
                    (uint)dynamicOffsetCount, (IntPtr)offsets);
            }
        }

        public void BindDescriptorSets(VkPipelineBindPoint pipelineBindPoint, VkPipelineLayout layout, int firstSet, VkDescriptorSet descriptorSet, IList<int> dynamicOffsets) {
            if (layout == null) throw new ArgumentNullException(nameof(layout));
            if (descriptorSet == null) throw new ArgumentNullException(nameof(descriptorSet));

            unsafe {
                int dynamicOffsetCount = 0;
                if (dynamicOffsets != null) dynamicOffsetCount = dynamicOffsets.Count;

                var offsets = stackalloc uint[dynamicOffsetCount];

                Unmanaged.VkDescriptorSet setNative = descriptorSet.Native;
                if (dynamicOffsets != null) Interop.Copy(dynamicOffsets, (IntPtr)offsets);

                Device.Commands.cmdBindDescriptorSets(commandBuffer, VkPipelineBindPoint.Graphics, layout.Native,
                    (uint)firstSet, 1, (IntPtr)(&setNative),
                    (uint)dynamicOffsetCount, (IntPtr)offsets);
            }
        }

        public void Draw(int vertexCount, int instanceCount, int firstVertex, int firstInstance) {
            Device.Commands.cmdDraw(commandBuffer, (uint)vertexCount, (uint)instanceCount, (uint)firstVertex, (uint)firstInstance);
        }

        public void DrawIndexed(int indexCount, int instanceCount, int firstIndex, int vertexOffset, int firstInstance) {
            Device.Commands.cmdDrawIndexed(commandBuffer, (uint)indexCount, (uint)instanceCount, (uint)firstIndex, vertexOffset, (uint)firstInstance);
        }

        public void CopyBuffer(VkBuffer srcBuffer, VkBuffer dstBuffer, IList<VkBufferCopy> regions) {
            if (srcBuffer == null) throw new ArgumentNullException(nameof(srcBuffer));
            if (dstBuffer == null) throw new ArgumentNullException(nameof(dstBuffer));
            if (regions == null) throw new ArgumentNullException(nameof(regions));

            unsafe {
                var regionsNative = stackalloc Unmanaged.VkBufferCopy[regions.Count];
                for (int i = 0; i < regions.Count; i++) {
                    regionsNative[i] = regions[i].GetNative();
                }
                Device.Commands.cmdCopyBuffer(commandBuffer, srcBuffer.Native, dstBuffer.Native, (uint)regions.Count, (IntPtr)regionsNative);
            }
        }

        public void CopyBuffer(VkBuffer srcBuffer, VkBuffer dstBuffer, VkBufferCopy regions) {
            if (srcBuffer == null) throw new ArgumentNullException(nameof(srcBuffer));
            if (dstBuffer == null) throw new ArgumentNullException(nameof(dstBuffer));

            unsafe {
                var regionsNative = regions.GetNative();
                Device.Commands.cmdCopyBuffer(commandBuffer, srcBuffer.Native, dstBuffer.Native, 1, (IntPtr)(&regionsNative));
            }
        }

        public void CopyImage(VkImage srcImage, VkImageLayout srcImageLayout, VkImage dstImage, VkImageLayout dstImageLayout, IList<VkImageCopy> regions) {
            if (srcImage == null) throw new ArgumentNullException(nameof(srcImage));
            if (dstImage == null) throw new ArgumentNullException(nameof(dstImage));
            if (regions == null) throw new ArgumentNullException(nameof(regions));

            unsafe {
                var regionsNative = stackalloc Unmanaged.VkImageCopy[regions.Count];
                for (int i = 0; i < regions.Count; i++) {
                    regionsNative[i] = regions[i].GetNative();
                }

                Device.Commands.cmdCopyImage(commandBuffer,
                    srcImage.Native, srcImageLayout,
                    dstImage.Native, dstImageLayout,
                    (uint)regions.Count, (IntPtr)regionsNative);
            }
        }

        public void CopyImage(VkImage srcImage, VkImageLayout srcImageLayout, VkImage dstImage, VkImageLayout dstImageLayout, VkImageCopy regions) {
            if (srcImage == null) throw new ArgumentNullException(nameof(srcImage));
            if (dstImage == null) throw new ArgumentNullException(nameof(dstImage));

            unsafe {
                var regionsNative = regions.GetNative();

                Device.Commands.cmdCopyImage(commandBuffer,
                    srcImage.Native, srcImageLayout,
                    dstImage.Native, dstImageLayout,
                    1, (IntPtr)(&regions));
            }
        }

        public void PipelineBarrier(
            VkPipelineStageFlags srcStageMask, VkPipelineStageFlags dstStageMask,
            VkDependencyFlags flags,
            IList<VkMemoryBarrier> memoryBarriers,
            IList<VkBufferMemoryBarrier> bufferMemoryBarriers,
            IList<VkImageMemoryBarrier> imageMemoryBarriers) {

            int mbCount = 0;
            int bbCount = 0;
            int ibCount = 0;

            if (memoryBarriers != null) mbCount = memoryBarriers.Count;
            if (bufferMemoryBarriers != null) bbCount = bufferMemoryBarriers.Count;
            if (imageMemoryBarriers != null) ibCount = imageMemoryBarriers.Count;

            unsafe {
                var mbNative = stackalloc Unmanaged.VkMemoryBarrier[mbCount];

                for (int i = 0; i < mbCount; i++) {
                    var mb = memoryBarriers[i];
                    mbNative[i] = new Unmanaged.VkMemoryBarrier();
                    mbNative[i].sType = VkStructureType.MemoryBarrier;
                    mbNative[i].srcAccessMask = mb.srcAccessMask;
                    mbNative[i].dstAccessMask = mb.dstAccessMask;
                }

                var bbNative = stackalloc Unmanaged.VkBufferMemoryBarrier[bbCount];

                for (int i = 0; i < bbCount; i++) {
                    var bb = bufferMemoryBarriers[i];
                    bbNative[i] = new Unmanaged.VkBufferMemoryBarrier();
                    bbNative[i].sType = VkStructureType.BufferMemoryBarrier;
                    bbNative[i].srcAccessMask = bb.srcAccessMask;
                    bbNative[i].dstAccessMask = bb.dstAccessMask;
                    bbNative[i].srcQueueFamilyIndex = (uint)bb.srcQueueFamilyIndex;
                    bbNative[i].dstQueueFamilyIndex = (uint)bb.dstQueueFamilyIndex;
                    bbNative[i].buffer = bb.buffer.Native;
                    bbNative[i].offset = (ulong)bb.offset;
                    bbNative[i].size = (ulong)bb.size;
                }

                var ibNative = stackalloc Unmanaged.VkImageMemoryBarrier[ibCount];

                for (int i = 0; i < ibCount; i++) {
                    var ib = imageMemoryBarriers[i];
                    ibNative[i] = new Unmanaged.VkImageMemoryBarrier();
                    ibNative[i].sType = VkStructureType.ImageMemoryBarrier;
                    ibNative[i].srcAccessMask = ib.srcAccessMask;
                    ibNative[i].dstAccessMask = ib.dstAccessMask;
                    ibNative[i].oldLayout = ib.oldLayout;
                    ibNative[i].newLayout = ib.newLayout;
                    ibNative[i].srcQueueFamilyIndex = (uint)ib.srcQueueFamilyIndex;
                    ibNative[i].dstQueueFamilyIndex = (uint)ib.dstQueueFamilyIndex;
                    ibNative[i].image = ib.image.Native;
                    ibNative[i].subresourceRange = ib.subresourceRange.GetNative();
                }

                Device.Commands.cmdPipelineBarrier(commandBuffer,
                    srcStageMask, dstStageMask, flags,
                    (uint)mbCount, (IntPtr)mbNative,
                    (uint)bbCount, (IntPtr)bbNative,
                    (uint)ibCount, (IntPtr)ibNative);
            }
        }

        public void ClearColorImage(VkImage image, VkImageLayout imageLayout, VkClearColorValue clearColor, IList<VkImageSubresourceRange> ranges) {
            if (image == null) throw new ArgumentNullException(nameof(image));
            if (ranges == null) throw new ArgumentNullException(nameof(ranges));

            unsafe {
                var rangesNative = stackalloc Unmanaged.VkImageSubresourceRange[ranges.Count];
                for (int i = 0; i < ranges.Count; i++) {
                    rangesNative[i] = ranges[i].GetNative();
                }
                var clearColorNative = clearColor.GetNative();

                Device.Commands.cmdClearColorImage(commandBuffer, image.Native, imageLayout, ref clearColorNative, (uint)ranges.Count, (IntPtr)rangesNative);
            }
        }

        public void ClearColorImage(VkImage image, VkImageLayout imageLayout, VkClearColorValue clearColor, VkImageSubresourceRange ranges) {
            if (image == null) throw new ArgumentNullException(nameof(image));

            unsafe {
                var clearColorNative = clearColor.GetNative();
                var rangesNative = ranges.GetNative();
                Device.Commands.cmdClearColorImage(commandBuffer, image.Native, imageLayout, ref clearColorNative, 1, (IntPtr)(&rangesNative));
            }
        }

        public void Execute(IList<VkCommandBuffer> commandBuffers) {
            if (commandBuffers == null) throw new ArgumentNullException(nameof(commandBuffers));

            unsafe {
                var commandBuffersNative = stackalloc Unmanaged.VkCommandBuffer[commandBuffers.Count];
                Interop.Marshal<Unmanaged.VkCommandBuffer, VkCommandBuffer>(commandBuffers, commandBuffersNative);
                Device.Commands.cmdExecuteCommands(commandBuffer, (uint)commandBuffers.Count, (IntPtr)commandBuffersNative);
            }
        }

        public void PushConstants(VkPipelineLayout layout, VkShaderStageFlags stageFlags, int offset, int size, IntPtr data) {
            if (layout == null) throw new ArgumentNullException(nameof(layout));
            if (data == IntPtr.Zero) throw new ArgumentNullException(nameof(data));

            Device.Commands.cmdPushConstants(commandBuffer, layout.Native, stageFlags, (uint)offset, (uint)size, data);
        }

        public void PushConstants<T>(VkPipelineLayout layout, VkShaderStageFlags stageFlags, int offset, IList<T> data) where T : struct {
            if (layout == null) throw new ArgumentNullException(nameof(layout));
            if (data == null) throw new ArgumentNullException(nameof(data));

            unsafe {
                uint size = (uint)Interop.SizeOf(data);
                var dataNative = stackalloc byte[(int)size];
                Interop.Copy(data, (IntPtr)dataNative);

                Device.Commands.cmdPushConstants(commandBuffer, layout.Native, stageFlags, (uint)offset, size, (IntPtr)dataNative);
            }
        }

        public void PushConstants<T>(VkPipelineLayout layout, VkShaderStageFlags stageFlags, int offset, T data) where T : struct {
            if (layout == null) throw new ArgumentNullException(nameof(layout));

            Device.Commands.cmdPushConstants(commandBuffer, layout.Native, stageFlags, (uint)offset, (uint)Interop.SizeOf<T>(), Interop.AddressOf(ref data));
        }

        public void SetEvent(VkEvent _event, VkPipelineStageFlags stageMask) {
            if (_event == null) throw new ArgumentNullException(nameof(_event));

            Device.Commands.cmdSetEvent(commandBuffer, _event.Native, stageMask);
        }

        public void ResetEvent(VkEvent _event, VkPipelineStageFlags stageMask) {
            if (_event == null) throw new ArgumentNullException(nameof(_event));

            Device.Commands.cmdResetEvent(commandBuffer, _event.Native, stageMask);
        }

        public void WaitEvents(
            List<VkEvent> events, 
            VkPipelineStageFlags srcStageMask, VkPipelineStageFlags dstStageMask,
            IList<VkMemoryBarrier> memoryBarriers, 
            IList<VkBufferMemoryBarrier> bufferMemoryBarriers, 
            IList<VkImageMemoryBarrier> imageMemoryBarriers) {

            if (events == null) throw new ArgumentNullException(nameof(events));

            unsafe {
                var eventsNative = stackalloc Unmanaged.VkEvent[events.Count];
                Interop.Marshal<Unmanaged.VkEvent, VkEvent>(events, eventsNative);

                int mbCount = 0;
                int bbCount = 0;
                int ibCount = 0;

                if (memoryBarriers != null) mbCount = memoryBarriers.Count;
                if (bufferMemoryBarriers != null) bbCount = bufferMemoryBarriers.Count;
                if (imageMemoryBarriers != null) ibCount = imageMemoryBarriers.Count;

                var mbNative = stackalloc Unmanaged.VkMemoryBarrier[mbCount];
                var bbNative = stackalloc Unmanaged.VkBufferMemoryBarrier[bbCount];
                var ibNative = stackalloc Unmanaged.VkImageMemoryBarrier[ibCount];

                for (int i = 0; i < mbCount; i++) {
                    var mb = memoryBarriers[i];
                    mbNative[i] = new Unmanaged.VkMemoryBarrier();
                    mbNative[i].sType = VkStructureType.MemoryBarrier;
                    mbNative[i].srcAccessMask = mb.srcAccessMask;
                    mbNative[i].dstAccessMask = mb.dstAccessMask;
                }

                for (int i = 0; i < bbCount; i++) {
                    var bb = bufferMemoryBarriers[i];
                    bbNative[i] = new Unmanaged.VkBufferMemoryBarrier();
                    bbNative[i].sType = VkStructureType.BufferMemoryBarrier;
                    bbNative[i].srcAccessMask = bb.srcAccessMask;
                    bbNative[i].dstAccessMask = bb.dstAccessMask;
                    bbNative[i].srcQueueFamilyIndex = (uint)bb.srcQueueFamilyIndex;
                    bbNative[i].dstQueueFamilyIndex = (uint)bb.dstQueueFamilyIndex;
                    bbNative[i].buffer = bb.buffer.Native;
                    bbNative[i].offset = (ulong)bb.offset;
                    bbNative[i].size = (ulong)bb.size;
                }

                for (int i = 0; i < ibCount; i++) {
                    var ib = imageMemoryBarriers[i];
                    ibNative[i] = new Unmanaged.VkImageMemoryBarrier();
                    ibNative[i].sType = VkStructureType.ImageMemoryBarrier;
                    ibNative[i].srcAccessMask = ib.srcAccessMask;
                    ibNative[i].dstAccessMask = ib.dstAccessMask;
                    ibNative[i].oldLayout = ib.oldLayout;
                    ibNative[i].newLayout = ib.newLayout;
                    ibNative[i].srcQueueFamilyIndex = (uint)ib.srcQueueFamilyIndex;
                    ibNative[i].dstQueueFamilyIndex = (uint)ib.dstQueueFamilyIndex;
                    ibNative[i].image = ib.image.Native;
                    ibNative[i].subresourceRange = ib.subresourceRange.GetNative();
                }

                Device.Commands.cmdWaitEvents(commandBuffer,
                    (uint)events.Count, (IntPtr)eventsNative,
                    srcStageMask, dstStageMask,
                    (uint)mbCount, (IntPtr)mbNative,
                    (uint)bbCount, (IntPtr)bbNative,
                    (uint)ibCount, (IntPtr)ibNative);
            }
        }

        public void WaitEvents(IList<VkEvent> events, VkPipelineStageFlags srcStageMask, VkPipelineStageFlags dstStageMask) {
            if (events == null) throw new ArgumentNullException(nameof(events));

            unsafe {
                var eventsNative = stackalloc Unmanaged.VkEvent[events.Count];
                Interop.Marshal<Unmanaged.VkEvent, VkEvent>(events, eventsNative);

                Device.Commands.cmdWaitEvents(commandBuffer,
                    (uint)events.Count, (IntPtr)eventsNative,
                    srcStageMask, dstStageMask,
                    0, IntPtr.Zero,
                    0, IntPtr.Zero,
                    0, IntPtr.Zero);
            }
        }

        public void SetViewports(int firstViewport, IList<VkViewport> viewports) {
            if (viewports == null) throw new ArgumentNullException(nameof(viewports));

            unsafe {
                var viewportsNative = stackalloc Unmanaged.VkViewport[viewports.Count];
                Interop.Copy(viewports, (IntPtr)viewportsNative);
                Device.Commands.cmdSetViewports(commandBuffer, (uint)firstViewport, (uint)viewports.Count, (IntPtr)viewportsNative);
            }
        }

        public void SetViewports(int firstViewport, VkViewport viewports) {
            unsafe {
                var native = viewports.GetNative();
                Device.Commands.cmdSetViewports(commandBuffer, (uint)firstViewport, 1, (IntPtr)(&native));
            }
        }

        public void SetScissor(int firstScissor, IList<VkRect2D> scissors) {
            if (scissors == null) throw new ArgumentNullException(nameof(scissors));

            unsafe {
                var scissorsNative = stackalloc Unmanaged.VkRect2D[scissors.Count];
                Interop.Copy(scissors, (IntPtr)scissorsNative);
                Device.Commands.cmdSetScissor(commandBuffer, (uint)firstScissor, (uint)scissors.Count, (IntPtr)scissorsNative);
            }
        }

        public void SetScissor(int firstScissor, VkRect2D scissors) {
            unsafe {
                var native = scissors.GetNative();
                Device.Commands.cmdSetScissor(commandBuffer, (uint)firstScissor, 1, (IntPtr)(&native));
            }
        }

        public void SetLineWidth(float lineWidth) {
            Device.Commands.cmdSetLineWidth(commandBuffer, lineWidth);
        }

        public void SetDepthBias(float constantFactor, float clamp, float slopeFactor) {
            Device.Commands.cmdSetDepthBias(commandBuffer, constantFactor, clamp, slopeFactor);
        }

        public void SetBlendConstants(IList<float> blendConstants) {
            if (blendConstants == null) throw new ArgumentNullException(nameof(blendConstants));

            unsafe {
                var blendConstantsNative = stackalloc float[4]; //vulkan expects 4 floats
                Interop.Copy(blendConstants, (IntPtr)blendConstantsNative, 4);

                Device.Commands.cmdSetBlendConstants(commandBuffer, (IntPtr)blendConstantsNative);
            }
        }

        public void SetBlendConstants(float constant0, float constant1, float constant2, float constant3) {
            unsafe {
                float* constants = stackalloc float[4];
                constants[0] = constant0;
                constants[1] = constant1;
                constants[2] = constant2;
                constants[3] = constant3;

                Device.Commands.cmdSetBlendConstants(commandBuffer, (IntPtr)constants);
            }
        }

        public void SetDepthBounds(float minDepthBounds, float maxDepthBounds) {
            Device.Commands.cmdSetDepthBounds(commandBuffer, minDepthBounds, maxDepthBounds);
        }

        public void SetStencilCompareMask(VkStencilFaceFlags faceMask, uint compareMask) {
            Device.Commands.cmdSetStencilCompareMask(commandBuffer, faceMask, compareMask);
        }

        public void SetStencilWriteMask(VkStencilFaceFlags faceMask, uint writeMask) {
            Device.Commands.cmdSetStencilWriteMask(commandBuffer, faceMask, writeMask);
        }

        public void SetStencilReference(VkStencilFaceFlags faceMask, int reference) {
            Device.Commands.cmdSetStencilReference(commandBuffer, faceMask, (uint)reference);
        }

        public void DrawIndirect(VkBuffer buffer, long offset, int drawCount, int stride) {
            if (buffer == null) throw new ArgumentNullException(nameof(buffer));

            Device.Commands.cmdDrawIndirect(commandBuffer, buffer.Native, (ulong)offset, (uint)drawCount, (uint)stride);
        }

        public void DrawIndexedIndirect(VkBuffer buffer, long offset, int drawCount, int stride) {
            if (buffer == null) throw new ArgumentNullException(nameof(buffer));

            Device.Commands.cmdDrawIndexedIndirect(commandBuffer, buffer.Native, (ulong)offset, (uint)drawCount, (uint)stride);
        }

        public void UpdateBuffer(VkBuffer dstBuffer, long dstOffset, long dataSize, IntPtr data) {
            if (dstBuffer == null) throw new ArgumentNullException(nameof(dstBuffer));
            if (data == IntPtr.Zero) throw new ArgumentNullException(nameof(data));

            Device.Commands.cmdUpdateBuffer(commandBuffer, dstBuffer.Native, (ulong)dstOffset, (ulong)dataSize, data);
        }

        public void UpdateBuffer(VkBuffer dstBuffer, long dstOffset, byte[] data) {
            if (dstBuffer == null) throw new ArgumentNullException(nameof(dstBuffer));
            if (data == null) throw new ArgumentNullException(nameof(data));

            unsafe {
                fixed (byte* ptr = data) {
                    Device.Commands.cmdUpdateBuffer(commandBuffer, dstBuffer.Native, (ulong)dstOffset, (ulong)data.Length, (IntPtr)ptr);
                }
            }
        }

        public void UpdateBuffer<T>(VkBuffer dstBuffer, long dstOffset, IList<T> data) where T : struct {
            if (dstBuffer == null) throw new ArgumentNullException(nameof(dstBuffer));
            if (data == null) throw new ArgumentNullException(nameof(data));

            int size = (int)Interop.SizeOf(data);
            using (var dataNative = new NativeArray<byte>(size)) {
                Interop.Copy(data, dataNative.Address);
                    
                Device.Commands.cmdUpdateBuffer(commandBuffer, dstBuffer.Native, (ulong)dstOffset, (ulong)size, dataNative.Address);
            }
        }

        public void FillBuffer(VkBuffer dstBuffer, long dstOffset, long size, uint data) {
            if (dstBuffer == null) throw new ArgumentNullException(nameof(dstBuffer));

            Device.Commands.cmdFillBuffer(commandBuffer, dstBuffer.Native, (ulong)dstOffset, (ulong)size, data);
        }

        public void BlitImage(VkImage srcImage, VkImageLayout srcImageLayout, VkImage dstImage, VkImageLayout dstImageLayout, IList<VkImageBlit> regions, VkFilter filter) {
            if (srcImage == null) throw new ArgumentNullException(nameof(srcImage));
            if (dstImage == null) throw new ArgumentNullException(nameof(dstImage));
            if (regions == null) throw new ArgumentNullException(nameof(regions));

            unsafe {
                var regionsNative = stackalloc Unmanaged.VkImageBlit[regions.Count];
                for (int i = 0; i < regions.Count; i++) {
                    regionsNative[i] = regions[i].GetNative();
                }

                Device.Commands.cmdBlitImage(commandBuffer, srcImage.Native, srcImageLayout, dstImage.Native, dstImageLayout, (uint)regions.Count, (IntPtr)regionsNative, filter);
            }
        }

        public void BlitImage(VkImage srcImage, VkImageLayout srcImageLayout, VkImage dstImage, VkImageLayout dstImageLayout, VkImageBlit regions, VkFilter filter) {
            if (srcImage == null) throw new ArgumentNullException(nameof(srcImage));
            if (dstImage == null) throw new ArgumentNullException(nameof(dstImage));

            unsafe {
                var regionsNative = regions.GetNative();
                Device.Commands.cmdBlitImage(commandBuffer, srcImage.Native, srcImageLayout, dstImage.Native, dstImageLayout, 1, (IntPtr)(&regionsNative), filter);
            }
        }

        public void CopyBufferToImage(VkBuffer srcBuffer, VkImage dstImage, VkImageLayout dstImageLayout, IList<VkBufferImageCopy> regions) {
            if (srcBuffer == null) throw new ArgumentNullException(nameof(srcBuffer));
            if (dstImage == null) throw new ArgumentNullException(nameof(dstImage));
            if (regions == null) throw new ArgumentNullException(nameof(regions));

            unsafe {
                var regionsNative = stackalloc Unmanaged.VkBufferImageCopy[regions.Count];
                for (int i = 0; i < regions.Count; i++) {
                    regionsNative[i] = regions[i].GetNative();
                }

                Device.Commands.cmdCopyBufferToImage(commandBuffer, srcBuffer.Native, dstImage.Native, dstImageLayout, (uint)regions.Count, (IntPtr)regionsNative);
            }
        }

        public void CopyBufferToImage(VkBuffer srcBuffer, VkImage dstImage, VkImageLayout dstImageLayout, VkBufferImageCopy regions) {
            if (srcBuffer == null) throw new ArgumentNullException(nameof(srcBuffer));
            if (dstImage == null) throw new ArgumentNullException(nameof(dstImage));

            unsafe {
                var regionsNative = regions.GetNative();
                Device.Commands.cmdCopyBufferToImage(commandBuffer, srcBuffer.Native, dstImage.Native, dstImageLayout, 1, (IntPtr)(&regionsNative));
            }
        }

        public void CopyImageToBuffer(VkImage srcImage, VkImageLayout srcImageLayout, VkBuffer dstBuffer, IList<VkBufferImageCopy> regions) {
            if (srcImage == null) throw new ArgumentNullException(nameof(srcImage));
            if (dstBuffer == null) throw new ArgumentNullException(nameof(dstBuffer));
            if (regions == null) throw new ArgumentNullException(nameof(regions));

            unsafe {
                var regionsNative = stackalloc Unmanaged.VkBufferImageCopy[regions.Count];
                for (int i = 0; i < regions.Count; i++) {
                    regionsNative[i] = regions[i].GetNative();
                }

                Device.Commands.cmdCopyImageToBuffer(commandBuffer, srcImage.Native, srcImageLayout, dstBuffer.Native, (uint)regions.Count, (IntPtr)regionsNative);
            }
        }

        public void CopyImageToBuffer(VkImage srcImage, VkImageLayout srcImageLayout, VkBuffer dstBuffer, VkBufferImageCopy regions) {
            if (srcImage == null) throw new ArgumentNullException(nameof(srcImage));
            if (dstBuffer == null) throw new ArgumentNullException(nameof(dstBuffer));

            unsafe {
                var regionsNative = regions.GetNative();
                Device.Commands.cmdCopyImageToBuffer(commandBuffer, srcImage.Native, srcImageLayout, dstBuffer.Native, 1, (IntPtr)(&regionsNative));
            }
        }

        public void ClearAttachments(IList<VkClearAttachment> attachments, IList<VkClearRect> rects) {
            if (attachments == null) throw new ArgumentNullException(nameof(attachments));
            if (rects == null) throw new ArgumentNullException(nameof(rects));

            unsafe {
                var attachmentsNative = stackalloc Unmanaged.VkClearAttachment[attachments.Count];
                var rectsNative = stackalloc Unmanaged.VkClearRect[rects.Count];
                Interop.Copy(attachments, (IntPtr)attachmentsNative);
                for (int i = 0; i < rects.Count; i++) {
                    rectsNative[i] = rects[i].GetNative();
                }

                Device.Commands.cmdClearAttachments(commandBuffer, (uint)attachments.Count, (IntPtr)attachmentsNative, (uint)rects.Count, (IntPtr)rectsNative);
            }
        }

        public void ClearAttachments(VkClearAttachment attachments, VkClearRect rects) {
            unsafe {
                Device.Commands.cmdClearAttachments(commandBuffer, 1, (IntPtr)(&attachments), 1, (IntPtr)(&rects));
            }
        }

        public void ResolveImage(VkImage srcImage, VkImageLayout srcImageLayout, VkImage dstImage, VkImageLayout dstImageLayout, IList<VkImageResolve> regions) {
            if (srcImage == null) throw new ArgumentNullException(nameof(srcImage));
            if (dstImage == null) throw new ArgumentNullException(nameof(dstImage));
            if (regions == null) throw new ArgumentNullException(nameof(regions));

            unsafe {
                var regionsNative = stackalloc Unmanaged.VkImageResolve[regions.Count];
                for (int i = 0; i < regions.Count; i++) {
                    regionsNative[i] = regions[i].GetNative();
                }

                Device.Commands.cmdResolveImage(commandBuffer, srcImage.Native, srcImageLayout, dstImage.Native, dstImageLayout, (uint)regions.Count, (IntPtr)regionsNative);
            }
        }

        public void ResolveImage(VkImage srcImage, VkImageLayout srcImageLayout, VkImage dstImage, VkImageLayout dstImageLayout, VkImageResolve regions) {
            if (srcImage == null) throw new ArgumentNullException(nameof(srcImage));
            if (dstImage == null) throw new ArgumentNullException(nameof(dstImage));

            unsafe {
                var regionsNative = regions.GetNative();
                Device.Commands.cmdResolveImage(commandBuffer, srcImage.Native, srcImageLayout, dstImage.Native, dstImageLayout, 1, (IntPtr)(&regionsNative));
            }
        }

        public void ResetQueryPool(VkQueryPool queryPool, int firstQuery, int queryCount) {
            if (queryPool == null) throw new ArgumentNullException(nameof(queryPool));

            Device.Commands.cmdResetQueryPool(commandBuffer, queryPool.Native, (uint)firstQuery, (uint)queryCount);
        }

        public void BeginQuery(VkQueryPool queryPool, int query, VkQueryControlFlags flags) {
            if (queryPool == null) throw new ArgumentNullException(nameof(queryPool));

            Device.Commands.cmdBeginQuery(commandBuffer, queryPool.Native, (uint)query, flags);
        }

        public void EndQuery(VkQueryPool queryPool, int query) {
            if (queryPool == null) throw new ArgumentNullException(nameof(queryPool));

            Device.Commands.cmdEndQuery(commandBuffer, queryPool.Native, (uint)query);
        }

        public void CopyQueryPoolResults(VkQueryPool queryPool, int firstQuery, int queryCount, VkBuffer dstBuffer, long dstOffset, long stride, VkQueryResultFlags flags) {
            if (queryPool == null) throw new ArgumentNullException(nameof(queryPool));
            if (dstBuffer == null) throw new ArgumentNullException(nameof(dstBuffer));

            Device.Commands.cmdCopyQueryPoolResults(commandBuffer, queryPool.Native, (uint)firstQuery, (uint)queryCount, dstBuffer.Native, (ulong)dstOffset, (ulong)stride, flags);
        }

        public void WriteTimestamp(VkPipelineStageFlags pipelineStage, VkQueryPool queryPool, int query) {
            if (queryPool == null) throw new ArgumentNullException(nameof(queryPool));

            Device.Commands.cmdWriteTimestamp(commandBuffer, pipelineStage, queryPool.Native, (uint)query);
        }

        public void Dispatch(int x, int y, int z) {
            Device.Commands.cmdDispatch(commandBuffer, (uint)x, (uint)y, (uint)z);
        }

        public void DispatchIndirect(VkBuffer buffer, long offset) {
            if (buffer == null) throw new ArgumentNullException(nameof(buffer));

            Device.Commands.cmdDispatchIndirect(commandBuffer, buffer.Native, (ulong)offset);
        }
    }

    public class CommandBufferException : VulkanException {
        public CommandBufferException(VkResult result, string message) : base(result, message) { }
    }
}
