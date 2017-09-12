using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace CSGL.Vulkan {
    public class CommandBufferAllocateInfo {
        public VkCommandBufferLevel level;
        public uint commandBufferCount;
    }

    public class CommandBuffer : IDisposable, INative<VkCommandBuffer> {
        VkCommandBuffer commandBuffer;
        bool disposed;

        public VkCommandBuffer Native {
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

        internal CommandBuffer(Device device, CommandPool pool, VkCommandBuffer commandBuffer, VkCommandBufferLevel level) {
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
            using (var marshalled = new DisposableList<IDisposable>()) {
                var infoNative = info.GetNative(marshalled);

                var result = Device.Commands.beginCommandBuffer(commandBuffer, ref infoNative);
                if (result != VkResult.Success) throw new CommandBufferException(result, string.Format("Error beginning command buffer: {0}", result));
            }
        }

        public void BeginRenderPass(RenderPassBeginInfo info, VkSubpassContents contents) {
            using (var marshalled = new DisposableList<IDisposable>()) {
                var infoNative = info.GetNative(marshalled);
                Device.Commands.cmdBeginRenderPass(commandBuffer, ref infoNative, contents);
            }
        }

        public void NextSubpass(VkSubpassContents contents) {
            Device.Commands.cmdNextSubpass(commandBuffer, contents);
        }

        public void BindPipeline(VkPipelineBindPoint bindPoint, Pipeline pipeline) {
            Device.Commands.cmdBindPipeline(commandBuffer, bindPoint, pipeline.Native);
        }

        public void BindVertexBuffers(uint firstBinding, IList<Buffer> buffers, IList<ulong> offsets) {
            unsafe {
                var buffersNative = stackalloc VkBuffer[buffers.Count];
                var offsetsNative = stackalloc ulong[offsets.Count];

                Interop.Marshal<VkBuffer, Buffer>(buffers, buffersNative);
                Interop.Copy(offsets, (IntPtr)offsetsNative);

                Device.Commands.cmdBindVertexBuffers(commandBuffer, firstBinding, (uint)buffers.Count, (IntPtr)(buffersNative), ref offsetsNative[0]);
            }
        }

        public void BindVertexBuffers(uint firstBinding, Buffer buffer, ulong offset) {
            unsafe {
                VkBuffer bufferNative = buffer.Native;
                Device.Commands.cmdBindVertexBuffers(commandBuffer, firstBinding, 1, (IntPtr)(&bufferNative), ref offset);
            }
        }

        public void BindIndexBuffer(Buffer buffer, ulong offset, VkIndexType indexType) {
            Device.Commands.cmdBindIndexBuffer(commandBuffer, buffer.Native, offset, indexType);
        }

        public void BindDescriptorSets(VkPipelineBindPoint pipelineBindPoint, PipelineLayout layout, uint firstSet, IList<DescriptorSet> descriptorSets, IList<uint> dynamicOffsets) {
            unsafe {
                int dynamicOffsetCount = 0;
                if (dynamicOffsets != null) dynamicOffsetCount = dynamicOffsets.Count;

                var sets = stackalloc VkDescriptorSet[descriptorSets.Count];
                var offsets = stackalloc uint[dynamicOffsetCount];

                Interop.Marshal<VkDescriptorSet, DescriptorSet>(descriptorSets, sets);

                if (dynamicOffsets != null) Interop.Copy(dynamicOffsets, (IntPtr)offsets);

                Device.Commands.cmdBindDescriptorSets(commandBuffer, pipelineBindPoint, layout.Native,
                    firstSet, (uint)descriptorSets.Count, (IntPtr)sets,
                    (uint)dynamicOffsetCount, (IntPtr)offsets);
            }
        }

        public void BindDescriptorSets(VkPipelineBindPoint pipelineBindPoint, PipelineLayout layout, uint firstSet, DescriptorSet descriptorSet, IList<uint> dynamicOffsets) {
            unsafe {
                int dynamicOffsetCount = 0;
                if (dynamicOffsets != null) dynamicOffsetCount = dynamicOffsets.Count;

                var offsets = stackalloc uint[dynamicOffsetCount];

                VkDescriptorSet setNative = descriptorSet.Native;
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

        public void CopyBuffer(Buffer srcBuffer, Buffer dstBuffer, IList<VkBufferCopy> regions) {
            unsafe {
                var regionsNative = stackalloc VkBufferCopy[regions.Count];
                Interop.Copy(regions, (IntPtr)regionsNative);
                Device.Commands.cmdCopyBuffer(commandBuffer, srcBuffer.Native, dstBuffer.Native, (uint)regions.Count, (IntPtr)regionsNative);
            }
        }

        public void CopyBuffer(Buffer srcBuffer, Buffer dstBuffer, VkBufferCopy region) {
            unsafe {
                Device.Commands.cmdCopyBuffer(commandBuffer, srcBuffer.Native, dstBuffer.Native, 1, (IntPtr)(&region));
            }
        }

        public void CopyImage(Image srcImage, VkImageLayout srcImageLayout, Image dstImage, VkImageLayout dstImageLayout, IList<VkImageCopy> regions) {
            unsafe {
                var regionsNative = stackalloc VkImageCopy[regions.Count];
                Interop.Copy(regions, (IntPtr)regionsNative);

                Device.Commands.cmdCopyImage(commandBuffer,
                    srcImage.Native, srcImageLayout,
                    dstImage.Native, dstImageLayout,
                    (uint)regions.Count, (IntPtr)regionsNative);
            }
        }

        public void CopyImage(Image srcImage, VkImageLayout srcImageLayout, Image dstImage, VkImageLayout dstImageLayout, VkImageCopy regions) {
            unsafe {
                Device.Commands.cmdCopyImage(commandBuffer,
                    srcImage.Native, srcImageLayout,
                    dstImage.Native, dstImageLayout,
                    1, (IntPtr)(&regions));
            }
        }

        public void PipelineBarrier(VkPipelineStageFlags srcStageMask, VkPipelineStageFlags dstStageMask, VkDependencyFlags flags,
            IList<MemoryBarrier> memoryBarriers, IList<BufferMemoryBarrier> bufferMemoryBarriers, IList<ImageMemoryBarrier> imageMemoryBarriers) {

            int mbCount = 0;
            int bbCount = 0;
            int ibCount = 0;

            if (memoryBarriers != null) mbCount = memoryBarriers.Count;
            if (bufferMemoryBarriers != null) bbCount = bufferMemoryBarriers.Count;
            if (imageMemoryBarriers != null) ibCount = imageMemoryBarriers.Count;

            unsafe {
                var mbNative = stackalloc VkMemoryBarrier[mbCount];

                for (int i = 0; i < mbCount; i++) {
                    var mb = memoryBarriers[i];
                    mbNative[i] = new VkMemoryBarrier();
                    mbNative[i].sType = VkStructureType.MemoryBarrier;
                    mbNative[i].srcAccessMask = mb.srcAccessMask;
                    mbNative[i].dstAccessMask = mb.dstAccessMask;
                }

                var bbNative = stackalloc VkBufferMemoryBarrier[bbCount];

                for (int i = 0; i < bbCount; i++) {
                    var bb = bufferMemoryBarriers[i];
                    bbNative[i] = new VkBufferMemoryBarrier();
                    bbNative[i].sType = VkStructureType.BufferMemoryBarrier;
                    bbNative[i].srcAccessMask = bb.srcAccessMask;
                    bbNative[i].dstAccessMask = bb.dstAccessMask;
                    bbNative[i].srcQueueFamilyIndex = bb.srcQueueFamilyIndex;
                    bbNative[i].dstQueueFamilyIndex = bb.dstQueueFamilyIndex;
                    bbNative[i].buffer = bb.buffer.Native;
                    bbNative[i].offset = bb.offset;
                    bbNative[i].size = bb.size;
                }

                var ibNative = stackalloc VkImageMemoryBarrier[ibCount];

                for (int i = 0; i < ibCount; i++) {
                    var ib = imageMemoryBarriers[i];
                    ibNative[i] = new VkImageMemoryBarrier();
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

        public void ClearColorImage(Image image, VkImageLayout imageLayout, ref VkClearColorValue clearColor, IList<VkImageSubresourceRange> ranges) {
            unsafe {
                var rangesNative = stackalloc VkImageSubresourceRange[ranges.Count];
                Interop.Copy(ranges, (IntPtr)rangesNative);
                Device.Commands.cmdClearColorImage(commandBuffer, image.Native, imageLayout, ref clearColor, (uint)ranges.Count, (IntPtr)rangesNative);
            }
        }

        public void ClearColorImage(Image image, VkImageLayout imageLayout, ref VkClearColorValue clearColor, VkImageSubresourceRange ranges) {
            unsafe {
                Device.Commands.cmdClearColorImage(commandBuffer, image.Native, imageLayout, ref clearColor, 1, (IntPtr)(&ranges));
            }
        }

        public void Execute(IList<CommandBuffer> commandBuffers) {
            unsafe {
                var commandBuffersNative = stackalloc VkCommandBuffer[commandBuffers.Count];
                Interop.Marshal<VkCommandBuffer, CommandBuffer>(commandBuffers, commandBuffersNative);
                Device.Commands.cmdExecuteCommands(commandBuffer, (uint)commandBuffers.Count, (IntPtr)commandBuffersNative);
            }
        }

        public void PushConstants(PipelineLayout layout, VkShaderStageFlags stageFlags, uint offset, uint size, IntPtr data) {
            Device.Commands.cmdPushConstants(commandBuffer, layout.Native, stageFlags, offset, size, data);
        }

        public void PushConstants<T>(PipelineLayout layout, VkShaderStageFlags stageFlags, uint offset, IList<T> data) where T : struct {
            unsafe {
                uint size = (uint)Interop.SizeOf(data);
                var dataNative = stackalloc byte[(int)size];
                Interop.Copy(data, (IntPtr)dataNative);

                Device.Commands.cmdPushConstants(commandBuffer, layout.Native, stageFlags, offset, size, (IntPtr)dataNative);
            }
        }

        public void PushConstants<T>(PipelineLayout layout, VkShaderStageFlags stageFlags, uint offset, List<T> data) where T : struct {
            unsafe {
                int size = (int)Interop.SizeOf<T>();
                byte* native = stackalloc byte[size * data.Count];
                Interop.Copy(data, (IntPtr)native);
                Device.Commands.cmdPushConstants(commandBuffer, layout.Native, stageFlags, offset, (uint)(size * data.Count), (IntPtr)native);
            }
        }

        public void PushConstants<T>(PipelineLayout layout, VkShaderStageFlags stageFlags, uint offset, T data) where T : struct {
            Device.Commands.cmdPushConstants(commandBuffer, layout.Native, stageFlags, offset, (uint)Interop.SizeOf<T>(), Interop.AddressOf(ref data));
        }

        public void SetEvent(Event _event, VkPipelineStageFlags stageMask) {
            Device.Commands.cmdSetEvent(commandBuffer, _event.Native, stageMask);
        }

        public void ResetEvent(Event _event, VkPipelineStageFlags stageMask) {
            Device.Commands.cmdResetEvent(commandBuffer, _event.Native, stageMask);
        }

        public void WaitEvents(List<Event> events, VkPipelineStageFlags srcStageMask, VkPipelineStageFlags dstStageMask,
            List<MemoryBarrier> memoryBarriers, List<BufferMemoryBarrier> bufferMemoryBarriers, List<ImageMemoryBarrier> imageMemoryBarriers) {
            unsafe {
                var eventsNative = stackalloc VkEvent[events.Count];
                Interop.Marshal<VkEvent, Event>(events, eventsNative);

                int mbCount = 0;
                int bbCount = 0;
                int ibCount = 0;

                if (memoryBarriers != null) mbCount = memoryBarriers.Count;
                if (bufferMemoryBarriers != null) bbCount = bufferMemoryBarriers.Count;
                if (imageMemoryBarriers != null) ibCount = imageMemoryBarriers.Count;

                var mbNative = stackalloc VkMemoryBarrier[mbCount];
                var bbNative = stackalloc VkBufferMemoryBarrier[bbCount];
                var ibNative = stackalloc VkImageMemoryBarrier[ibCount];

                for (int i = 0; i < mbCount; i++) {
                    var mb = memoryBarriers[i];
                    mbNative[i] = new VkMemoryBarrier();
                    mbNative[i].sType = VkStructureType.MemoryBarrier;
                    mbNative[i].srcAccessMask = mb.srcAccessMask;
                    mbNative[i].dstAccessMask = mb.dstAccessMask;
                }

                for (int i = 0; i < bbCount; i++) {
                    var bb = bufferMemoryBarriers[i];
                    bbNative[i] = new VkBufferMemoryBarrier();
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
                    ibNative[i] = new VkImageMemoryBarrier();
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

        public void WaitEvents(List<Event> events, VkPipelineStageFlags srcStageMask, VkPipelineStageFlags dstStageMask) {
            unsafe {
                var eventsNative = stackalloc VkEvent[events.Count];
                Interop.Marshal<VkEvent, Event>(events, eventsNative);

                Device.Commands.cmdWaitEvents(commandBuffer,
                    (uint)events.Count, (IntPtr)eventsNative,
                    srcStageMask, dstStageMask,
                    0, IntPtr.Zero,
                    0, IntPtr.Zero,
                    0, IntPtr.Zero);
            }
        }

        public void SetViewports(uint firstViewport, VkViewport[] viewports) {
            unsafe {
                fixed (VkViewport* ptr = viewports) {
                    Device.Commands.cmdSetViewports(commandBuffer, firstViewport, (uint)viewports.Length, (IntPtr)ptr);
                }
            }
        }

        public void SetViewports(uint firstViewport, List<VkViewport> viewports) {
            unsafe {
                fixed (VkViewport* ptr = Interop.GetInternalArray(viewports)) {
                    Device.Commands.cmdSetViewports(commandBuffer, firstViewport, (uint)viewports.Count, (IntPtr)ptr);
                }
            }
        }

        public void SetViewports(uint firstViewport, VkViewport viewports) {
            unsafe {
                Device.Commands.cmdSetViewports(commandBuffer, firstViewport, 1, (IntPtr)(&viewports));
            }
        }

        public void SetScissor(uint firstScissor, VkRect2D[] scissors) {
            unsafe {
                fixed (VkRect2D* ptr = scissors) {
                    Device.Commands.cmdSetScissor(commandBuffer, firstScissor, (uint)scissors.Length, (IntPtr)(ptr));
                }
            }
        }

        public void SetScissor(uint firstScissor, List<VkRect2D> scissors) {
            unsafe {
                fixed (VkRect2D* ptr = Interop.GetInternalArray(scissors)) {
                    Device.Commands.cmdSetScissor(commandBuffer, firstScissor, (uint)scissors.Count, (IntPtr)(ptr));
                }
            }
        }

        public void SetScissor(uint firstScissor, VkRect2D scissors) {
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

        public void SetBlendConstants(float[] blendConstants) {
            unsafe {
                fixed (float* ptr = blendConstants) {
                    Device.Commands.cmdSetBlendConstants(commandBuffer, (IntPtr)ptr);
                }
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
            Device.Commands.cmdDrawIndirect(commandBuffer, buffer.Native, offset, drawCount, stride);
        }

        public void DrawIndexedIndirect(Buffer buffer, ulong offset, uint drawCount, uint stride) {
            Device.Commands.cmdDrawIndexedIndirect(commandBuffer, buffer.Native, offset, drawCount, stride);
        }

        public void UpdateBuffer(Buffer dstBuffer, ulong dstOffset, ulong dataSize, IntPtr data) {
            Device.Commands.cmdUpdateBuffer(commandBuffer, dstBuffer.Native, dstOffset, dataSize, data);
        }

        public void UpdateBuffer(Buffer dstBuffer, ulong dstOffset, byte[] data) {
            unsafe {
                fixed (byte* ptr = data) {
                    Device.Commands.cmdUpdateBuffer(commandBuffer, dstBuffer.Native, dstOffset, (ulong)data.Length, (IntPtr)ptr);
                }
            }
        }

        public void UpdateBuffer<T>(Buffer dstBuffer, ulong dstOffset, T[] data) where T : struct {
            ulong size = (ulong)(data.Length * Interop.SizeOf<T>());

            GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);
            Device.Commands.cmdUpdateBuffer(commandBuffer, dstBuffer.Native, dstOffset, size, handle.AddrOfPinnedObject());
            handle.Free();
        }

        public void FillBuffer(Buffer dstBuffer, ulong dstOffset, ulong size, uint data) {
            Device.Commands.cmdFillBuffer(commandBuffer, dstBuffer.Native, dstOffset, size, data);
        }

        public void BlitImage(Image srcImage, VkImageLayout srcImageLayout, Image dstImage, VkImageLayout dstImageLayout, VkImageBlit[] regions, VkFilter filter) {
            unsafe {
                fixed (VkImageBlit* ptr = regions) {
                    Device.Commands.cmdBlitImage(commandBuffer, srcImage.Native, srcImageLayout, dstImage.Native, dstImageLayout, (uint)regions.Length, (IntPtr)ptr, filter);
                }
            }
        }

        public void BlitImage(Image srcImage, VkImageLayout srcImageLayout, Image dstImage, VkImageLayout dstImageLayout, List<VkImageBlit> regions, VkFilter filter) {
            unsafe {
                fixed (VkImageBlit* ptr = Interop.GetInternalArray(regions)) {
                    Device.Commands.cmdBlitImage(commandBuffer, srcImage.Native, srcImageLayout, dstImage.Native, dstImageLayout, (uint)regions.Count, (IntPtr)ptr, filter);
                }
            }
        }

        public void BlitImage(Image srcImage, VkImageLayout srcImageLayout, Image dstImage, VkImageLayout dstImageLayout, VkImageBlit regions, VkFilter filter) {
            unsafe {
                Device.Commands.cmdBlitImage(commandBuffer, srcImage.Native, srcImageLayout, dstImage.Native, dstImageLayout, 1, (IntPtr)(&regions), filter);
            }
        }

        public void CopyBufferToImage(Buffer srcBuffer, Image dstImage, VkImageLayout dstImageLayout, VkBufferImageCopy[] regions) {
            unsafe {
                fixed (VkBufferImageCopy* ptr = regions) {
                    Device.Commands.cmdCopyBufferToImage(commandBuffer, srcBuffer.Native, dstImage.Native, dstImageLayout, (uint)regions.Length, (IntPtr)ptr);
                }
            }
        }

        public void CopyBufferToImage(Buffer srcBuffer, Image dstImage, VkImageLayout dstImageLayout, List<VkBufferImageCopy> regions) {
            unsafe {
                fixed (VkBufferImageCopy* ptr = Interop.GetInternalArray(regions)) {
                    Device.Commands.cmdCopyBufferToImage(commandBuffer, srcBuffer.Native, dstImage.Native, dstImageLayout, (uint)regions.Count, (IntPtr)ptr);
                }
            }
        }

        public void CopyBufferToImage(Buffer srcBuffer, Image dstImage, VkImageLayout dstImageLayout, VkBufferImageCopy regions) {
            unsafe {
                Device.Commands.cmdCopyBufferToImage(commandBuffer, srcBuffer.Native, dstImage.Native, dstImageLayout, 1, (IntPtr)(&regions));
            }
        }

        public void CopyImageToBuffer(Image srcImage, VkImageLayout srcImageLayout, Buffer dstBuffer, VkBufferImageCopy[] regions) {
            unsafe {
                fixed (VkBufferImageCopy* ptr = regions) {
                    Device.Commands.cmdCopyImageToBuffer(commandBuffer, srcImage.Native, srcImageLayout, dstBuffer.Native, (uint)regions.Length, (IntPtr)ptr);
                }
            }
        }

        public void CopyImageToBuffer(Image srcImage, VkImageLayout srcImageLayout, Buffer dstBuffer, List<VkBufferImageCopy> regions) {
            unsafe {
                fixed (VkBufferImageCopy* ptr = Interop.GetInternalArray(regions)) {
                    Device.Commands.cmdCopyImageToBuffer(commandBuffer, srcImage.Native, srcImageLayout, dstBuffer.Native, (uint)regions.Count, (IntPtr)ptr);
                }
            }
        }

        public void CopyImageToBuffer(Image srcImage, VkImageLayout srcImageLayout, Buffer dstBuffer, VkBufferImageCopy regions) {
            unsafe {
                Device.Commands.cmdCopyImageToBuffer(commandBuffer, srcImage.Native, srcImageLayout, dstBuffer.Native, 1, (IntPtr)(&regions));
            }
        }

        public void ClearAttachments(VkClearAttachment[] attachments, VkClearRect[] rects) {
            unsafe {
                fixed (VkClearAttachment* attachmentPtr = attachments)
                fixed (VkClearRect* rectPtr = rects) {
                    Device.Commands.cmdClearAttachments(commandBuffer, (uint)attachments.Length, (IntPtr)attachmentPtr, (uint)rects.Length, (IntPtr)rectPtr);
                }
            }
        }

        public void ClearAttachments(List<VkClearAttachment> attachments, List<VkClearRect> rects) {
            unsafe {
                fixed (VkClearAttachment* attachmentPtr = Interop.GetInternalArray(attachments))
                fixed (VkClearRect* rectPtr = Interop.GetInternalArray(rects)) {
                    Device.Commands.cmdClearAttachments(commandBuffer, (uint)attachments.Count, (IntPtr)attachmentPtr, (uint)rects.Count, (IntPtr)rectPtr);
                }
            }
        }

        public void ClearAttachments(VkClearAttachment attachments, VkClearRect rects) {
            unsafe {
                Device.Commands.cmdClearAttachments(commandBuffer, 1, (IntPtr)(&attachments), 1, (IntPtr)(&rects));
            }
        }

        public void ResolveImage(Image srcImage, VkImageLayout srcImageLayout, Image dstImage, VkImageLayout dstImageLayout, VkImageResolve[] regions) {
            unsafe {
                fixed (VkImageResolve* ptr = regions) {
                    Device.Commands.cmdResolveImage(commandBuffer, srcImage.Native, srcImageLayout, dstImage.Native, dstImageLayout, (uint)regions.Length, (IntPtr)ptr);
                }
            }
        }

        public void ResolveImage(Image srcImage, VkImageLayout srcImageLayout, Image dstImage, VkImageLayout dstImageLayout, List<VkImageResolve> regions) {
            unsafe {
                fixed (VkImageResolve* ptr = Interop.GetInternalArray(regions)) {
                    Device.Commands.cmdResolveImage(commandBuffer, srcImage.Native, srcImageLayout, dstImage.Native, dstImageLayout, (uint)regions.Count, (IntPtr)ptr);
                }
            }
        }

        public void ResolveImage(Image srcImage, VkImageLayout srcImageLayout, Image dstImage, VkImageLayout dstImageLayout, VkImageResolve regions) {
            unsafe {
                Device.Commands.cmdResolveImage(commandBuffer, srcImage.Native, srcImageLayout, dstImage.Native, dstImageLayout, 1, (IntPtr)(&regions));
            }
        }

        public void ResetQueryPool(QueryPool queryPool, uint firstQuery, uint queryCount) {
            Device.Commands.cmdResetQueryPool(commandBuffer, queryPool.Native, firstQuery, queryCount);
        }

        public void BeginQuery(QueryPool queryPool, uint query, VkQueryControlFlags flags) {
            Device.Commands.cmdBeginQuery(commandBuffer, queryPool.Native, query, flags);
        }

        public void EndQuery(QueryPool queryPool, uint query) {
            Device.Commands.cmdEndQuery(commandBuffer, queryPool.Native, query);
        }

        public void CopyQueryPoolResults(QueryPool queryPool, uint firstQuery, uint queryCount, Buffer dstBuffer, ulong dstOffset, ulong stride, VkQueryResultFlags flags) {
            Device.Commands.cmdCopyQueryPoolResults(commandBuffer, queryPool.Native, firstQuery, queryCount, dstBuffer.Native, dstOffset, stride, flags);
        }

        public void WriteTimestamp(VkPipelineStageFlags pipelineStage, QueryPool queryPool, uint query) {
            Device.Commands.cmdWriteTimestamp(commandBuffer, pipelineStage, queryPool.Native, query);
        }

        public void Dispatch(uint x, uint y, uint z) {
            Device.Commands.cmdDispatch(commandBuffer, x, y, z);
        }

        public void DispatchIndirect(Buffer buffer, ulong offset) {
            Device.Commands.cmdDispatchIndirect(commandBuffer, buffer.Native, offset);
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
    }

    public class CommandBufferException : VulkanException {
        public CommandBufferException(VkResult result, string message) : base(result, message) { }
    }
}
