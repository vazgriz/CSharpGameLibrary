using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace CSGL.Vulkan {
    public class CommandBufferAllocateInfo {
        public VkCommandBufferLevel level;
        public uint commandBufferCount;
    }

    public class CommandBufferInheritanceInfo {
        public RenderPass renderPass;
        public uint subpass;
        public Framebuffer framebuffer;
        public bool occlusionQueryEnable;
        public VkQueryControlFlags queryFlags;
        public VkQueryPipelineStatisticFlags pipelineStatistics;
    }

    public class CommandBufferBeginInfo {
        public VkCommandBufferUsageFlags flags;
        public CommandBufferInheritanceInfo inheritanceInfo;
    }

    public class RenderPassBeginInfo {
        public RenderPass renderPass;
        public Framebuffer framebuffer;
        public Unmanaged.VkRect2D renderArea;
        public IList<Unmanaged.VkClearValue> clearValues;
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
        public Unmanaged.VkImageSubresourceRange subresourceRange;
    }

    public class CommandBuffer : IDisposable, INative<Unmanaged.VkCommandBuffer> {
        Unmanaged.VkCommandBuffer commandBuffer;
        bool disposed;

        public Unmanaged.VkCommandBuffer Native {
            get {
                return commandBuffer;
            }
        }

        public Device Device { get; private set; }
        public CommandPool Pool { get; private set; }
        public VkCommandBufferLevel Level { get; private set; }

        //set to false if pool is reset
        //prevents double free
        internal bool CanDispose { get; set; } = true;

        internal CommandBuffer(Device device, CommandPool pool, Unmanaged.VkCommandBuffer commandBuffer, VkCommandBufferLevel level) {
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

        ~CommandBuffer() {
            Dispose(false);
        }

        public void Begin(CommandBufferBeginInfo info) {
            if (info == null) throw new ArgumentNullException(nameof(info));

            unsafe {
                var infoNative = new Unmanaged.VkCommandBufferBeginInfo();
                infoNative.sType = VkStructureType.CommandBufferBeginInfo;
                infoNative.flags = info.flags;

                var inheritanceNative = new Unmanaged.VkCommandBufferInheritanceInfo();
                if (info.inheritanceInfo != null) {
                    inheritanceNative.sType = VkStructureType.CommandBufferInheritanceInfo;
                    inheritanceNative.renderPass = info.inheritanceInfo.renderPass.Native;
                    inheritanceNative.subpass = info.inheritanceInfo.subpass;

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

        public void BeginRenderPass(RenderPassBeginInfo info, VkSubpassContents contents) {
            if (info == null) throw new ArgumentNullException(nameof(info));

            unsafe {
                int clearValueCount = 0;
                if (info.clearValues != null) clearValueCount = info.clearValues.Count;

                var clearValuesNative = stackalloc Unmanaged.VkClearValue[clearValueCount];

                var infoNative = new Unmanaged.VkRenderPassBeginInfo();
                infoNative.sType = VkStructureType.RenderPassBeginInfo;
                infoNative.renderPass = info.renderPass.Native;
                infoNative.framebuffer = info.framebuffer.Native;
                infoNative.renderArea = info.renderArea;

                if (info.clearValues != null) Interop.Copy(info.clearValues, (IntPtr)clearValuesNative);
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

        public void BindPipeline(VkPipelineBindPoint bindPoint, Pipeline pipeline) {
            if (pipeline == null) throw new ArgumentNullException(nameof(pipeline));

            Device.Commands.cmdBindPipeline(commandBuffer, bindPoint, pipeline.Native);
        }

        public void BindVertexBuffers(uint firstBinding, IList<Buffer> buffers, IList<ulong> offsets) {
            if (buffers == null) throw new ArgumentNullException(nameof(buffers));
            if (offsets == null) throw new ArgumentNullException(nameof(offsets));

            unsafe {
                var buffersNative = stackalloc Unmanaged.VkBuffer[buffers.Count];
                var offsetsNative = stackalloc ulong[offsets.Count];

                Interop.Marshal<Unmanaged.VkBuffer, Buffer>(buffers, buffersNative);
                Interop.Copy(offsets, (IntPtr)offsetsNative);

                Device.Commands.cmdBindVertexBuffers(commandBuffer, firstBinding, (uint)buffers.Count, (IntPtr)(buffersNative), ref offsetsNative[0]);
            }
        }

        public void BindVertexBuffers(uint firstBinding, Buffer buffer, ulong offset) {
            if (buffer == null) throw new ArgumentNullException(nameof(buffer));

            unsafe {
                Unmanaged.VkBuffer bufferNative = buffer.Native;
                Device.Commands.cmdBindVertexBuffers(commandBuffer, firstBinding, 1, (IntPtr)(&bufferNative), ref offset);
            }
        }

        public void BindIndexBuffer(Buffer buffer, ulong offset, VkIndexType indexType) {
            if (buffer == null) throw new ArgumentNullException(nameof(buffer));

            Device.Commands.cmdBindIndexBuffer(commandBuffer, buffer.Native, offset, indexType);
        }

        public void BindDescriptorSets(VkPipelineBindPoint pipelineBindPoint, PipelineLayout layout, uint firstSet, IList<DescriptorSet> descriptorSets, IList<uint> dynamicOffsets) {
            if (descriptorSets == null) throw new ArgumentNullException(nameof(descriptorSets));

            unsafe {
                int dynamicOffsetCount = 0;
                if (dynamicOffsets != null) dynamicOffsetCount = dynamicOffsets.Count;

                var sets = stackalloc Unmanaged.VkDescriptorSet[descriptorSets.Count];
                var offsets = stackalloc uint[dynamicOffsetCount];

                Interop.Marshal<Unmanaged.VkDescriptorSet, DescriptorSet>(descriptorSets, sets);

                if (dynamicOffsets != null) Interop.Copy(dynamicOffsets, (IntPtr)offsets);

                Device.Commands.cmdBindDescriptorSets(commandBuffer, pipelineBindPoint, layout.Native,
                    firstSet, (uint)descriptorSets.Count, (IntPtr)sets,
                    (uint)dynamicOffsetCount, (IntPtr)offsets);
            }
        }

        public void BindDescriptorSets(VkPipelineBindPoint pipelineBindPoint, PipelineLayout layout, uint firstSet, DescriptorSet descriptorSet, IList<uint> dynamicOffsets) {
            if (descriptorSet == null) throw new ArgumentNullException(nameof(descriptorSet));

            unsafe {
                int dynamicOffsetCount = 0;
                if (dynamicOffsets != null) dynamicOffsetCount = dynamicOffsets.Count;

                var offsets = stackalloc uint[dynamicOffsetCount];

                Unmanaged.VkDescriptorSet setNative = descriptorSet.Native;
                if (dynamicOffsets != null) Interop.Copy(dynamicOffsets, (IntPtr)offsets);

                Device.Commands.cmdBindDescriptorSets(commandBuffer, VkPipelineBindPoint.Graphics, layout.Native,
                    firstSet, 1, (IntPtr)(&setNative),
                    (uint)dynamicOffsetCount, (IntPtr)offsets);
            }
        }

        public void Draw(uint vertexCount, uint instanceCount, uint firstVertex, uint firstInstance) {
            Device.Commands.cmdDraw(commandBuffer, vertexCount, instanceCount, firstVertex, firstInstance);
        }

        public void DrawIndexed(uint indexCount, uint instanceCount, uint firstIndex, int vertexOffset, uint firstInstance) {
            Device.Commands.cmdDrawIndexed(commandBuffer, indexCount, instanceCount, firstIndex, vertexOffset, firstInstance);
        }

        public void CopyBuffer(Buffer srcBuffer, Buffer dstBuffer, IList<Unmanaged.VkBufferCopy> regions) {
            if (srcBuffer == null) throw new ArgumentNullException(nameof(srcBuffer));
            if (dstBuffer == null) throw new ArgumentNullException(nameof(dstBuffer));
            if (regions == null) throw new ArgumentNullException(nameof(regions));

            unsafe {
                var regionsNative = stackalloc Unmanaged.VkBufferCopy[regions.Count];
                Interop.Copy(regions, (IntPtr)regionsNative);
                Device.Commands.cmdCopyBuffer(commandBuffer, srcBuffer.Native, dstBuffer.Native, (uint)regions.Count, (IntPtr)regionsNative);
            }
        }

        public void CopyBuffer(Buffer srcBuffer, Buffer dstBuffer, Unmanaged.VkBufferCopy region) {
            if (srcBuffer == null) throw new ArgumentNullException(nameof(srcBuffer));
            if (dstBuffer == null) throw new ArgumentNullException(nameof(dstBuffer));

            unsafe {
                Device.Commands.cmdCopyBuffer(commandBuffer, srcBuffer.Native, dstBuffer.Native, 1, (IntPtr)(&region));
            }
        }

        public void CopyImage(Image srcImage, VkImageLayout srcImageLayout, Image dstImage, VkImageLayout dstImageLayout, IList<Unmanaged.VkImageCopy> regions) {
            if (srcImage == null) throw new ArgumentNullException(nameof(srcImage));
            if (dstImage == null) throw new ArgumentNullException(nameof(dstImage));
            if (regions == null) throw new ArgumentNullException(nameof(regions));

            unsafe {
                var regionsNative = stackalloc Unmanaged.VkImageCopy[regions.Count];
                Interop.Copy(regions, (IntPtr)regionsNative);

                Device.Commands.cmdCopyImage(commandBuffer,
                    srcImage.Native, srcImageLayout,
                    dstImage.Native, dstImageLayout,
                    (uint)regions.Count, (IntPtr)regionsNative);
            }
        }

        public void CopyImage(Image srcImage, VkImageLayout srcImageLayout, Image dstImage, VkImageLayout dstImageLayout, Unmanaged.VkImageCopy regions) {
            if (srcImage == null) throw new ArgumentNullException(nameof(srcImage));
            if (dstImage == null) throw new ArgumentNullException(nameof(dstImage));

            unsafe {
                Device.Commands.cmdCopyImage(commandBuffer,
                    srcImage.Native, srcImageLayout,
                    dstImage.Native, dstImageLayout,
                    1, (IntPtr)(&regions));
            }
        }

        public void PipelineBarrier(
            VkPipelineStageFlags srcStageMask, VkPipelineStageFlags dstStageMask,
            VkDependencyFlags flags,
            IList<MemoryBarrier> memoryBarriers,
            IList<BufferMemoryBarrier> bufferMemoryBarriers,
            IList<ImageMemoryBarrier> imageMemoryBarriers) {

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
                    bbNative[i].srcQueueFamilyIndex = bb.srcQueueFamilyIndex;
                    bbNative[i].dstQueueFamilyIndex = bb.dstQueueFamilyIndex;
                    bbNative[i].buffer = bb.buffer.Native;
                    bbNative[i].offset = bb.offset;
                    bbNative[i].size = bb.size;
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
                    ibNative[i].srcQueueFamilyIndex = ib.srcQueueFamilyIndex;
                    ibNative[i].dstQueueFamilyIndex = ib.dstQueueFamilyIndex;
                    ibNative[i].image = ib.image.Native;
                    ibNative[i].subresourceRange = ib.subresourceRange;
                }

                Device.Commands.cmdPipelineBarrier(commandBuffer,
                    srcStageMask, dstStageMask, flags,
                    (uint)mbCount, (IntPtr)mbNative,
                    (uint)bbCount, (IntPtr)bbNative,
                    (uint)ibCount, (IntPtr)ibNative);
            }
        }

        public void ClearColorImage(Image image, VkImageLayout imageLayout, ref Unmanaged.VkClearColorValue clearColor, IList<Unmanaged.VkImageSubresourceRange> ranges) {
            if (image == null) throw new ArgumentNullException(nameof(image));
            if (ranges == null) throw new ArgumentNullException(nameof(ranges));

            unsafe {
                var rangesNative = stackalloc Unmanaged.VkImageSubresourceRange[ranges.Count];
                Interop.Copy(ranges, (IntPtr)rangesNative);
                Device.Commands.cmdClearColorImage(commandBuffer, image.Native, imageLayout, ref clearColor, (uint)ranges.Count, (IntPtr)rangesNative);
            }
        }

        public void ClearColorImage(Image image, VkImageLayout imageLayout, ref Unmanaged.VkClearColorValue clearColor, Unmanaged.VkImageSubresourceRange ranges) {
            if (image == null) throw new ArgumentNullException(nameof(image));

            unsafe {
                Device.Commands.cmdClearColorImage(commandBuffer, image.Native, imageLayout, ref clearColor, 1, (IntPtr)(&ranges));
            }
        }

        public void Execute(IList<CommandBuffer> commandBuffers) {
            if (commandBuffers == null) throw new ArgumentNullException(nameof(commandBuffers));

            unsafe {
                var commandBuffersNative = stackalloc Unmanaged.VkCommandBuffer[commandBuffers.Count];
                Interop.Marshal<Unmanaged.VkCommandBuffer, CommandBuffer>(commandBuffers, commandBuffersNative);
                Device.Commands.cmdExecuteCommands(commandBuffer, (uint)commandBuffers.Count, (IntPtr)commandBuffersNative);
            }
        }

        public void PushConstants(PipelineLayout layout, VkShaderStageFlags stageFlags, uint offset, uint size, IntPtr data) {
            if (layout == null) throw new ArgumentNullException(nameof(layout));
            if (data == IntPtr.Zero) throw new ArgumentNullException(nameof(data));

            Device.Commands.cmdPushConstants(commandBuffer, layout.Native, stageFlags, offset, size, data);
        }

        public void PushConstants<T>(PipelineLayout layout, VkShaderStageFlags stageFlags, uint offset, IList<T> data) where T : struct {
            if (layout == null) throw new ArgumentNullException(nameof(layout));
            if (data == null) throw new ArgumentNullException(nameof(data));

            unsafe {
                uint size = (uint)Interop.SizeOf(data);
                var dataNative = stackalloc byte[(int)size];
                Interop.Copy(data, (IntPtr)dataNative);

                Device.Commands.cmdPushConstants(commandBuffer, layout.Native, stageFlags, offset, size, (IntPtr)dataNative);
            }
        }

        public void PushConstants<T>(PipelineLayout layout, VkShaderStageFlags stageFlags, uint offset, T data) where T : struct {
            if (layout == null) throw new ArgumentNullException(nameof(layout));

            Device.Commands.cmdPushConstants(commandBuffer, layout.Native, stageFlags, offset, (uint)Interop.SizeOf<T>(), Interop.AddressOf(ref data));
        }

        public void SetEvent(Event _event, VkPipelineStageFlags stageMask) {
            if (_event == null) throw new ArgumentNullException(nameof(_event));

            Device.Commands.cmdSetEvent(commandBuffer, _event.Native, stageMask);
        }

        public void ResetEvent(Event _event, VkPipelineStageFlags stageMask) {
            if (_event == null) throw new ArgumentNullException(nameof(_event));

            Device.Commands.cmdResetEvent(commandBuffer, _event.Native, stageMask);
        }

        public void WaitEvents(
            List<Event> events, 
            VkPipelineStageFlags srcStageMask, VkPipelineStageFlags dstStageMask,
            IList<MemoryBarrier> memoryBarriers, 
            IList<BufferMemoryBarrier> bufferMemoryBarriers, 
            IList<ImageMemoryBarrier> imageMemoryBarriers) {

            if (events == null) throw new ArgumentNullException(nameof(events));

            unsafe {
                var eventsNative = stackalloc Unmanaged.VkEvent[events.Count];
                Interop.Marshal<Unmanaged.VkEvent, Event>(events, eventsNative);

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
                    bbNative[i].srcQueueFamilyIndex = bb.srcQueueFamilyIndex;
                    bbNative[i].dstQueueFamilyIndex = bb.dstQueueFamilyIndex;
                    bbNative[i].buffer = bb.buffer.Native;
                    bbNative[i].offset = bb.offset;
                    bbNative[i].size = bb.size;
                }

                for (int i = 0; i < ibCount; i++) {
                    var ib = imageMemoryBarriers[i];
                    ibNative[i] = new Unmanaged.VkImageMemoryBarrier();
                    ibNative[i].sType = VkStructureType.ImageMemoryBarrier;
                    ibNative[i].srcAccessMask = ib.srcAccessMask;
                    ibNative[i].dstAccessMask = ib.dstAccessMask;
                    ibNative[i].oldLayout = ib.oldLayout;
                    ibNative[i].newLayout = ib.newLayout;
                    ibNative[i].srcQueueFamilyIndex = ib.srcQueueFamilyIndex;
                    ibNative[i].dstQueueFamilyIndex = ib.dstQueueFamilyIndex;
                    ibNative[i].image = ib.image.Native;
                    ibNative[i].subresourceRange = ib.subresourceRange;
                }

                Device.Commands.cmdWaitEvents(commandBuffer,
                    (uint)events.Count, (IntPtr)eventsNative,
                    srcStageMask, dstStageMask,
                    (uint)mbCount, (IntPtr)mbNative,
                    (uint)bbCount, (IntPtr)bbNative,
                    (uint)ibCount, (IntPtr)ibNative);
            }
        }

        public void WaitEvents(IList<Event> events, VkPipelineStageFlags srcStageMask, VkPipelineStageFlags dstStageMask) {
            if (events == null) throw new ArgumentNullException(nameof(events));

            unsafe {
                var eventsNative = stackalloc Unmanaged.VkEvent[events.Count];
                Interop.Marshal<Unmanaged.VkEvent, Event>(events, eventsNative);

                Device.Commands.cmdWaitEvents(commandBuffer,
                    (uint)events.Count, (IntPtr)eventsNative,
                    srcStageMask, dstStageMask,
                    0, IntPtr.Zero,
                    0, IntPtr.Zero,
                    0, IntPtr.Zero);
            }
        }

        public void SetViewports(uint firstViewport, IList<Unmanaged.VkViewport> viewports) {
            if (viewports == null) throw new ArgumentNullException(nameof(viewports));

            unsafe {
                var viewportsNative = stackalloc Unmanaged.VkViewport[viewports.Count];
                Interop.Copy(viewports, (IntPtr)viewportsNative);
                Device.Commands.cmdSetViewports(commandBuffer, firstViewport, (uint)viewports.Count, (IntPtr)viewportsNative);
            }
        }

        public void SetViewports(uint firstViewport, Unmanaged.VkViewport viewports) {
            unsafe {
                Device.Commands.cmdSetViewports(commandBuffer, firstViewport, 1, (IntPtr)(&viewports));
            }
        }

        public void SetScissor(uint firstScissor, IList<Unmanaged.VkRect2D> scissors) {
            if (scissors == null) throw new ArgumentNullException(nameof(scissors));

            unsafe {
                var scissorsNative = stackalloc Unmanaged.VkRect2D[scissors.Count];
                Interop.Copy(scissors, (IntPtr)scissorsNative);
                Device.Commands.cmdSetScissor(commandBuffer, firstScissor, (uint)scissors.Count, (IntPtr)scissorsNative);
            }
        }

        public void SetScissor(uint firstScissor, Unmanaged.VkRect2D scissors) {
            unsafe {
                Device.Commands.cmdSetScissor(commandBuffer, firstScissor, 1, (IntPtr)(&scissors));
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

        public void SetStencilReference(VkStencilFaceFlags faceMask, uint reference) {
            Device.Commands.cmdSetStencilReference(commandBuffer, faceMask, reference);
        }

        public void DrawIndirect(Buffer buffer, ulong offset, uint drawCount, uint stride) {
            if (buffer == null) throw new ArgumentNullException(nameof(buffer));

            Device.Commands.cmdDrawIndirect(commandBuffer, buffer.Native, offset, drawCount, stride);
        }

        public void DrawIndexedIndirect(Buffer buffer, ulong offset, uint drawCount, uint stride) {
            if (buffer == null) throw new ArgumentNullException(nameof(buffer));

            Device.Commands.cmdDrawIndexedIndirect(commandBuffer, buffer.Native, offset, drawCount, stride);
        }

        public void UpdateBuffer(Buffer dstBuffer, ulong dstOffset, ulong dataSize, IntPtr data) {
            if (dstBuffer == null) throw new ArgumentNullException(nameof(dstBuffer));
            if (data == IntPtr.Zero) throw new ArgumentNullException(nameof(data));

            Device.Commands.cmdUpdateBuffer(commandBuffer, dstBuffer.Native, dstOffset, dataSize, data);
        }

        public void UpdateBuffer(Buffer dstBuffer, ulong dstOffset, byte[] data) {
            if (dstBuffer == null) throw new ArgumentNullException(nameof(dstBuffer));
            if (data == null) throw new ArgumentNullException(nameof(data));

            unsafe {
                fixed (byte* ptr = data) {
                    Device.Commands.cmdUpdateBuffer(commandBuffer, dstBuffer.Native, dstOffset, (ulong)data.Length, (IntPtr)ptr);
                }
            }
        }

        public void UpdateBuffer<T>(Buffer dstBuffer, ulong dstOffset, IList<T> data) where T : struct {
            if (dstBuffer == null) throw new ArgumentNullException(nameof(dstBuffer));
            if (data == null) throw new ArgumentNullException(nameof(data));

            int size = (int)Interop.SizeOf(data);
            using (var dataNative = new NativeArray<byte>(size)) {
                Interop.Copy(data, dataNative.Address);
                    
                Device.Commands.cmdUpdateBuffer(commandBuffer, dstBuffer.Native, dstOffset, (ulong)size, dataNative.Address);
            }
        }

        public void FillBuffer(Buffer dstBuffer, ulong dstOffset, ulong size, uint data) {
            if (dstBuffer == null) throw new ArgumentNullException(nameof(dstBuffer));

            Device.Commands.cmdFillBuffer(commandBuffer, dstBuffer.Native, dstOffset, size, data);
        }

        public void BlitImage(Image srcImage, VkImageLayout srcImageLayout, Image dstImage, VkImageLayout dstImageLayout, IList<Unmanaged.VkImageBlit> regions, VkFilter filter) {
            if (srcImage == null) throw new ArgumentNullException(nameof(srcImage));
            if (dstImage == null) throw new ArgumentNullException(nameof(dstImage));
            if (regions == null) throw new ArgumentNullException(nameof(regions));

            unsafe {
                var regionsNative = stackalloc Unmanaged.VkImageBlit[regions.Count];
                Interop.Copy(regions, (IntPtr)regionsNative);
                Device.Commands.cmdBlitImage(commandBuffer, srcImage.Native, srcImageLayout, dstImage.Native, dstImageLayout, (uint)regions.Count, (IntPtr)regionsNative, filter);
            }
        }

        public void BlitImage(Image srcImage, VkImageLayout srcImageLayout, Image dstImage, VkImageLayout dstImageLayout, Unmanaged.VkImageBlit regions, VkFilter filter) {
            if (srcImage == null) throw new ArgumentNullException(nameof(srcImage));
            if (dstImage == null) throw new ArgumentNullException(nameof(dstImage));

            unsafe {
                Device.Commands.cmdBlitImage(commandBuffer, srcImage.Native, srcImageLayout, dstImage.Native, dstImageLayout, 1, (IntPtr)(&regions), filter);
            }
        }

        public void CopyBufferToImage(Buffer srcBuffer, Image dstImage, VkImageLayout dstImageLayout, IList<Unmanaged.VkBufferImageCopy> regions) {
            if (srcBuffer == null) throw new ArgumentNullException(nameof(srcBuffer));
            if (dstImage == null) throw new ArgumentNullException(nameof(dstImage));
            if (regions == null) throw new ArgumentNullException(nameof(regions));

            unsafe {
                var regionsNative = stackalloc Unmanaged.VkBufferImageCopy[regions.Count];
                Interop.Copy(regions, (IntPtr)regionsNative);
                Device.Commands.cmdCopyBufferToImage(commandBuffer, srcBuffer.Native, dstImage.Native, dstImageLayout, (uint)regions.Count, (IntPtr)regionsNative);
            }
        }

        public void CopyBufferToImage(Buffer srcBuffer, Image dstImage, VkImageLayout dstImageLayout, Unmanaged.VkBufferImageCopy regions) {
            if (srcBuffer == null) throw new ArgumentNullException(nameof(srcBuffer));
            if (dstImage == null) throw new ArgumentNullException(nameof(dstImage));

            unsafe {
                Device.Commands.cmdCopyBufferToImage(commandBuffer, srcBuffer.Native, dstImage.Native, dstImageLayout, 1, (IntPtr)(&regions));
            }
        }

        public void CopyImageToBuffer(Image srcImage, VkImageLayout srcImageLayout, Buffer dstBuffer, IList<Unmanaged.VkBufferImageCopy> regions) {
            if (srcImage == null) throw new ArgumentNullException(nameof(srcImage));
            if (dstBuffer == null) throw new ArgumentNullException(nameof(dstBuffer));
            if (regions == null) throw new ArgumentNullException(nameof(regions));

            unsafe {
                var regionsNative = stackalloc Unmanaged.VkBufferImageCopy[regions.Count];
                Interop.Copy(regions, (IntPtr)regionsNative);
                Device.Commands.cmdCopyImageToBuffer(commandBuffer, srcImage.Native, srcImageLayout, dstBuffer.Native, (uint)regions.Count, (IntPtr)regionsNative);
            }
        }

        public void CopyImageToBuffer(Image srcImage, VkImageLayout srcImageLayout, Buffer dstBuffer, Unmanaged.VkBufferImageCopy regions) {
            if (srcImage == null) throw new ArgumentNullException(nameof(srcImage));
            if (dstBuffer == null) throw new ArgumentNullException(nameof(dstBuffer));

            unsafe {
                Device.Commands.cmdCopyImageToBuffer(commandBuffer, srcImage.Native, srcImageLayout, dstBuffer.Native, 1, (IntPtr)(&regions));
            }
        }

        public void ClearAttachments(IList<Unmanaged.VkClearAttachment> attachments, IList<Unmanaged.VkClearRect> rects) {
            if (attachments == null) throw new ArgumentNullException(nameof(attachments));
            if (rects == null) throw new ArgumentNullException(nameof(rects));

            unsafe {
                var attachmentsNative = stackalloc Unmanaged.VkClearAttachment[attachments.Count];
                var rectsNative = stackalloc Unmanaged.VkClearRect[rects.Count];
                Interop.Copy(attachments, (IntPtr)attachmentsNative);
                Interop.Copy(rects, (IntPtr)rectsNative);

                Device.Commands.cmdClearAttachments(commandBuffer, (uint)attachments.Count, (IntPtr)attachmentsNative, (uint)rects.Count, (IntPtr)rectsNative);
            }
        }

        public void ClearAttachments(Unmanaged.VkClearAttachment attachments, Unmanaged.VkClearRect rects) {
            unsafe {
                Device.Commands.cmdClearAttachments(commandBuffer, 1, (IntPtr)(&attachments), 1, (IntPtr)(&rects));
            }
        }

        public void ResolveImage(Image srcImage, VkImageLayout srcImageLayout, Image dstImage, VkImageLayout dstImageLayout, IList<Unmanaged.VkImageResolve> regions) {
            if (srcImage == null) throw new ArgumentNullException(nameof(srcImage));
            if (dstImage == null) throw new ArgumentNullException(nameof(dstImage));
            if (regions == null) throw new ArgumentNullException(nameof(regions));

            unsafe {
                var regionsNative = stackalloc Unmanaged.VkImageResolve[regions.Count];
                Interop.Copy(regions, (IntPtr)regionsNative);
                Device.Commands.cmdResolveImage(commandBuffer, srcImage.Native, srcImageLayout, dstImage.Native, dstImageLayout, (uint)regions.Count, (IntPtr)regionsNative);
            }
        }

        public void ResolveImage(Image srcImage, VkImageLayout srcImageLayout, Image dstImage, VkImageLayout dstImageLayout, Unmanaged.VkImageResolve regions) {
            if (srcImage == null) throw new ArgumentNullException(nameof(srcImage));
            if (dstImage == null) throw new ArgumentNullException(nameof(dstImage));

            unsafe {
                Device.Commands.cmdResolveImage(commandBuffer, srcImage.Native, srcImageLayout, dstImage.Native, dstImageLayout, 1, (IntPtr)(&regions));
            }
        }

        public void ResetQueryPool(QueryPool queryPool, uint firstQuery, uint queryCount) {
            if (queryPool == null) throw new ArgumentNullException(nameof(queryPool));

            Device.Commands.cmdResetQueryPool(commandBuffer, queryPool.Native, firstQuery, queryCount);
        }

        public void BeginQuery(QueryPool queryPool, uint query, VkQueryControlFlags flags) {
            if (queryPool == null) throw new ArgumentNullException(nameof(queryPool));

            Device.Commands.cmdBeginQuery(commandBuffer, queryPool.Native, query, flags);
        }

        public void EndQuery(QueryPool queryPool, uint query) {
            if (queryPool == null) throw new ArgumentNullException(nameof(queryPool));

            Device.Commands.cmdEndQuery(commandBuffer, queryPool.Native, query);
        }

        public void CopyQueryPoolResults(QueryPool queryPool, uint firstQuery, uint queryCount, Buffer dstBuffer, ulong dstOffset, ulong stride, VkQueryResultFlags flags) {
            if (queryPool == null) throw new ArgumentNullException(nameof(queryPool));
            if (dstBuffer == null) throw new ArgumentNullException(nameof(dstBuffer));

            Device.Commands.cmdCopyQueryPoolResults(commandBuffer, queryPool.Native, firstQuery, queryCount, dstBuffer.Native, dstOffset, stride, flags);
        }

        public void WriteTimestamp(VkPipelineStageFlags pipelineStage, QueryPool queryPool, uint query) {
            if (queryPool == null) throw new ArgumentNullException(nameof(queryPool));

            Device.Commands.cmdWriteTimestamp(commandBuffer, pipelineStage, queryPool.Native, query);
        }

        public void Dispatch(uint x, uint y, uint z) {
            Device.Commands.cmdDispatch(commandBuffer, x, y, z);
        }

        public void DispatchIndirect(Buffer buffer, ulong offset) {
            if (buffer == null) throw new ArgumentNullException(nameof(buffer));

            Device.Commands.cmdDispatchIndirect(commandBuffer, buffer.Native, offset);
        }
    }

    public class CommandBufferException : VulkanException {
        public CommandBufferException(VkResult result, string message) : base(result, message) { }
    }
}
