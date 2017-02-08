using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

namespace CSGL.Vulkan {
    public class CommandBufferAllocateInfo {
        public VkCommandBufferLevel level;
        public uint commandBufferCount;
    }

    public class CommandBuffer : INative<VkCommandBuffer> {
        VkCommandBuffer commandBuffer;

        public VkCommandBuffer Native {
            get {
                return commandBuffer;
            }
        }

        public Device Device { get; private set; }
        public CommandPool Pool { get; private set; }

        internal CommandBuffer(Device device, CommandPool pool, VkCommandBuffer commandBuffer) {
            Device = device;
            Pool = pool;
            this.commandBuffer = commandBuffer;
        }

        public void Begin(CommandBufferBeginInfo info) {
            using (var marshalled = new DisposableList<IDisposable>()) {
                var infoNative = info.GetNative(marshalled);
                Device.Commands.beginCommandBuffer(commandBuffer, ref infoNative);
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

        public void BindVertexBuffers(uint firstBinding, Buffer[] buffers, ulong[] offsets) {
            unsafe
            {
                var buffersNative = stackalloc VkBuffer[buffers.Length];

                Interop.Marshal<VkBuffer>(buffers, buffersNative);

                Device.Commands.cmdBindVertexBuffers(commandBuffer, firstBinding, (uint)buffers.Length, (IntPtr)(buffersNative), ref offsets[0]);
            }
        }

        public void BindVertexBuffers(uint firstBinding, List<Buffer> buffers, List<ulong> offsets) {
            unsafe
            {
                var buffersNative = stackalloc VkBuffer[buffers.Count];
                var offsetsNative = stackalloc ulong[offsets.Count];

                Interop.Marshal<VkBuffer, Buffer>(buffers, buffersNative);
                Interop.Copy(offsets, (IntPtr)offsetsNative);

                Device.Commands.cmdBindVertexBuffers(commandBuffer, firstBinding, (uint)buffers.Count, (IntPtr)(buffersNative), ref offsetsNative[0]);
            }
        }

        public void BindVertexBuffer(uint firstBinding, Buffer buffer, ulong offset) {
            unsafe
            {
                VkBuffer bufferNative = buffer.Native;
                Device.Commands.cmdBindVertexBuffers(commandBuffer, firstBinding, 1, (IntPtr)(&bufferNative), ref offset);
            }
        }

        public void BindIndexBuffer(Buffer buffer, ulong offset, VkIndexType indexType) {
            Device.Commands.cmdBindIndexBuffer(commandBuffer, buffer.Native, offset, indexType);
        }

        public void BindDescriptorSets(VkPipelineBindPoint pipelineBindPoint, PipelineLayout layout, uint firstSet, DescriptorSet[] descriptorSets, uint[] dynamicOffsets) {
            unsafe
            {
                var sets = stackalloc VkDescriptorSet[descriptorSets.Length];

                for (int i = 0; i < descriptorSets.Length; i++) {
                    sets[i] = descriptorSets[i].Native;
                }

                int dynamicOffsetCount = 0;
                if (dynamicOffsets != null) dynamicOffsetCount = dynamicOffsets.Length;
                GCHandle handle = GCHandle.Alloc(dynamicOffsets, GCHandleType.Pinned);

                Device.Commands.cmdBindDescriptorSets(commandBuffer, pipelineBindPoint, layout.Native,
                    firstSet, (uint)descriptorSets.Length, (IntPtr)sets,
                    (uint)dynamicOffsetCount, handle.AddrOfPinnedObject());

                handle.Free();
            }
        }

        public void BindDescriptorSets(VkPipelineBindPoint pipelineBindPoint, PipelineLayout layout, uint firstSet, DescriptorSet[] descriptorSets) {
            BindDescriptorSets(pipelineBindPoint, layout, firstSet, descriptorSets, null);
        }

        public void BindDescriptorSets(VkPipelineBindPoint pipelineBindPoint, PipelineLayout layout, uint firstSet, List<DescriptorSet> descriptorSets, List<uint> dynamicOffsets) {
            unsafe
            {
                int dynamicOffsetCount = 0;
                if (dynamicOffsets != null) dynamicOffsetCount = dynamicOffsets.Count;

                var sets = stackalloc VkDescriptorSet[descriptorSets.Count];
                var offsets = stackalloc uint[dynamicOffsetCount];

                for (int i = 0; i < descriptorSets.Count; i++) {
                    sets[i] = descriptorSets[i].Native;
                }

                for (int i = 0; i < dynamicOffsetCount; i++) {
                    offsets[i] = dynamicOffsets[i];
                }

                Device.Commands.cmdBindDescriptorSets(commandBuffer, pipelineBindPoint, layout.Native,
                    firstSet, (uint)descriptorSets.Count, (IntPtr)sets,
                    (uint)dynamicOffsetCount, (IntPtr)offsets);
            }
        }

        public void BindDescriptorSets(VkPipelineBindPoint pipelineBindPoint, PipelineLayout layout, uint firstSet, List<DescriptorSet> descriptorSets) {
            BindDescriptorSets(pipelineBindPoint, layout, firstSet, descriptorSets, null);
        }

        public void BindDescriptorSets(VkPipelineBindPoint pipelineBindPoint, PipelineLayout layout, uint firstSet, DescriptorSet descriptorSet) {
            unsafe {
                VkDescriptorSet set = descriptorSet.Native;
                Device.Commands.cmdBindDescriptorSets(commandBuffer, pipelineBindPoint, layout.Native, firstSet, 1, (IntPtr)(&set), 0, IntPtr.Zero);
            }
        }

        public void BindDescriptorSets(VkPipelineBindPoint pipelineBindPoint, PipelineLayout layout, uint firstSet, DescriptorSet descriptorSet, List<uint> dynamicOffsets) {
            unsafe
            {
                int dynamicOffsetCount = 0;
                if (dynamicOffsets != null) dynamicOffsetCount = dynamicOffsets.Count;
                
                var offsets = stackalloc uint[dynamicOffsetCount];

                VkDescriptorSet setNative = descriptorSet.Native;
                Interop.Copy(dynamicOffsets, (IntPtr)offsets);

                Device.Commands.cmdBindDescriptorSets(commandBuffer, VkPipelineBindPoint.Graphics, layout.Native,
                    firstSet, 1, (IntPtr)(&setNative),
                    (uint)dynamicOffsetCount, (IntPtr)offsets);
            }
        }

        public void BindDescriptorSets(VkPipelineBindPoint pipelineBindPoint, PipelineLayout layout, uint firstSet, DescriptorSet descriptorSet, uint dynamicOffset) {
            unsafe
            {
                VkDescriptorSet setNative = descriptorSet.Native;
                Device.Commands.cmdBindDescriptorSets(commandBuffer, pipelineBindPoint, layout.Native,
                    firstSet, 1, (IntPtr)(&setNative),
                    1, (IntPtr)(&dynamicOffset));
            }
        }

        public void Draw(uint vertexCount, uint instanceCount, uint firstVertex, uint firstInstance) {
            Device.Commands.cmdDraw(commandBuffer, vertexCount, instanceCount, firstVertex, firstInstance);
        }

        public void Draw(int vertexCount, int instanceCount, int firstVertex, int firstInstance) {
            Device.Commands.cmdDraw(commandBuffer, (uint)vertexCount, (uint)instanceCount, (uint)firstVertex, (uint)firstInstance);
        }

        public void DrawIndexed(uint indexCount, uint instanceCount, uint firstIndex, int vertexOffset, uint firstInstance) {
            Device.Commands.cmdDrawIndexed(commandBuffer, indexCount, instanceCount, firstIndex, vertexOffset, firstInstance);
        }

        public void CopyBuffer(Buffer srcBuffer, Buffer dstBuffer, VkBufferCopy[] regions) {
            unsafe
            {
                var regionsNative = stackalloc VkBufferCopy[regions.Length];

                Interop.Copy(regions, (IntPtr)regionsNative);

                Device.Commands.cmdCopyBuffer(commandBuffer, srcBuffer.Native, dstBuffer.Native, (uint)regions.Length, (IntPtr)regionsNative);
            }
        }

        public void CopyBuffer(Buffer srcBuffer, Buffer dstBuffer, List<VkBufferCopy> regions) {
            unsafe
            {
                var regionsNative = stackalloc VkBufferCopy[regions.Count];

                Interop.Copy(regions, (IntPtr)regionsNative);

                Device.Commands.cmdCopyBuffer(commandBuffer, srcBuffer.Native, dstBuffer.Native, (uint)regions.Count, (IntPtr)regionsNative);
            }
        }

        public void CopyBuffer(Buffer srcBuffer, Buffer dstBuffer) {
            unsafe
            {
                VkBufferCopy region = new VkBufferCopy();
                region.srcOffset = 0;
                region.dstOffset = 0;
                region.size = System.Math.Min(srcBuffer.Size, dstBuffer.Size);

                Device.Commands.cmdCopyBuffer(commandBuffer, srcBuffer.Native, dstBuffer.Native, 1, (IntPtr)(&region));
            }
        }

        public void CopyBuffer(Buffer srcBuffer, Buffer dstBuffer, VkBufferCopy region) {
            unsafe
            {
                Device.Commands.cmdCopyBuffer(commandBuffer, srcBuffer.Native, dstBuffer.Native, 1, (IntPtr)(&region));
            }
        }

        public void CopyImage(Image srcImage, VkImageLayout srcImageLayout, Image dstImage, VkImageLayout dstImageLayout, VkImageCopy[] regions) {
            unsafe {
                var regionsNative = stackalloc VkImageCopy[regions.Length];

                Interop.Copy(regions, (IntPtr)regionsNative);

                Device.Commands.cmdCopyImage(commandBuffer,
                    srcImage.Native, srcImageLayout,
                    dstImage.Native, dstImageLayout,
                    (uint)regions.Length, (IntPtr)regionsNative);
            }
        }

        public void CopyImage(Image srcImage, VkImageLayout srcImageLayout, Image dstImage, VkImageLayout dstImageLayout, List<VkImageCopy> regions) {
            unsafe
            {
                var regionsNative = stackalloc VkImageCopy[regions.Count];

                Interop.Copy(regions, (IntPtr)regionsNative);

                Device.Commands.cmdCopyImage(commandBuffer,
                    srcImage.Native, srcImageLayout,
                    dstImage.Native, dstImageLayout,
                    (uint)regions.Count, (IntPtr)regionsNative);
            }
        }

        public void PipelineBarrier(VkPipelineStageFlags srcStageMask, VkPipelineStageFlags dstStageMask, VkDependencyFlags flags,
            List<MemoryBarrier> memoryBarriers, List<BufferMemoryBarrier> bufferMemoryBarriers, List<ImageMemoryBarrier> imageMemoryBarriers) {

            int mbCount = 0;
            int bbCount = 0;
            int ibCount = 0;

            if (memoryBarriers != null) mbCount = memoryBarriers.Count;
            if (bufferMemoryBarriers != null) bbCount = bufferMemoryBarriers.Count;
            if (imageMemoryBarriers != null) ibCount = imageMemoryBarriers.Count;

            unsafe
            {
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

        public void ClearColorImage(Image image, VkImageLayout imageLayout, ref VkClearColorValue clearColor, VkImageSubresourceRange[] ranges) {
            GCHandle handle = GCHandle.Alloc(ranges, GCHandleType.Pinned);
            Device.Commands.cmdClearColorImage(commandBuffer, image.Native, imageLayout, ref clearColor, (uint)ranges.Length, handle.AddrOfPinnedObject());
        }

        public void ClearColorImage(Image image, VkImageLayout imageLayout, ref VkClearColorValue clearColor, List<VkImageSubresourceRange> ranges) {
            unsafe
            {
                var rangesNative = stackalloc VkImageSubresourceRange[ranges.Count];
                Interop.Copy(ranges, (IntPtr)rangesNative);
                Device.Commands.cmdClearColorImage(commandBuffer, image.Native, imageLayout, ref clearColor, (uint)ranges.Count, (IntPtr)rangesNative);
            }
        }

        public void Execute(List<CommandBuffer> commandBuffers) {
            unsafe {
                var commandBuffersNative = stackalloc VkCommandBuffer[commandBuffers.Count];
                Interop.Marshal<VkCommandBuffer, CommandBuffer>(commandBuffers, commandBuffersNative);
                Device.Commands.cmdExecuteCommands(commandBuffer, (uint)commandBuffers.Count, (IntPtr)commandBuffersNative);
            }
        }

        public void PushConstants(PipelineLayout layout, VkShaderStageFlags stageFlags, uint offset, uint size, IntPtr data) {
            Device.Commands.cmdPushConstants(commandBuffer, layout.Native, stageFlags, offset, size, data);
        }

        public void PushConstants<T>(PipelineLayout layout, VkShaderStageFlags stageFlags, uint offset, T[] data) where T : struct {
            GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);
            Device.Commands.cmdPushConstants(commandBuffer, layout.Native, stageFlags, offset, (uint)(data.Length * Interop.SizeOf<T>()), handle.AddrOfPinnedObject());
            handle.Free();
        }

        public void PushConstants<T>(PipelineLayout layout, VkShaderStageFlags stageFlags, uint offset, List<T> data) where T : struct {
            unsafe
            {
                int size = Interop.SizeOf<T>();
                byte* native = stackalloc byte[size * data.Count];
                Interop.Copy(data, (IntPtr)native);
                Device.Commands.cmdPushConstants(commandBuffer, layout.Native, stageFlags, offset, (uint)(size * data.Count), (IntPtr)native);
            }
        }

        public void PushConstants<T>(PipelineLayout layout, VkShaderStageFlags stageFlags, uint offset, T data) where T : struct {
            unsafe
            {
                //can't take address of generic variable in c#
                IntPtr native = (IntPtr)Unsafe.AsPointer(ref data);
                Device.Commands.cmdPushConstants(commandBuffer, layout.Native, stageFlags, offset, (uint)Interop.SizeOf<T>(), native);
            }
        }

        public void SetEvent(Event _event, VkPipelineStageFlags stageMask) {
            Device.Commands.cmdSetEvent(commandBuffer, _event.Native, stageMask);
        }

        public void ResetEvent(Event _event, VkPipelineStageFlags stageMask) {
            Device.Commands.cmdResetEvent(commandBuffer, _event.Native, stageMask);
        }

        public void WaitEvents(List<Event> events, VkPipelineStageFlags srcStageMask, VkPipelineStageFlags dstStageMask,
            List<MemoryBarrier> memoryBarriers, List<BufferMemoryBarrier> bufferMemoryBarriers, List<ImageMemoryBarrier> imageMemoryBarriers) {
            unsafe
            {
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
            unsafe
            {
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

        public void EndRenderPass() {
            Device.Commands.cmdEndRenderPass(commandBuffer);
        }

        public VkResult End() {
            return Device.Commands.endCommandBuffer(commandBuffer);
        }

        public VkResult Reset(VkCommandBufferResetFlags flags) {
            return Device.Commands.resetCommandBuffers(commandBuffer, flags);
        }
    }
}
