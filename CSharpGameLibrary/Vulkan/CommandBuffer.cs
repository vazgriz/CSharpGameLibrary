using System;
using System.Collections.Generic;

namespace CSGL.Vulkan {
    public class CommandBufferAllocateInfo {
        public VkCommandBufferLevel level;
        public uint commandBufferCount;
    }

    public class CommandBuffer: INative<VkCommandBuffer> {
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
            List<IDisposable> marshalled = new List<IDisposable>();
            var infoNative = info.GetNative(marshalled);

            Device.Commands.beginCommandBuffer(commandBuffer, ref infoNative);

            foreach (var m in marshalled) m.Dispose();
        }

        public void BeginRenderPass(RenderPassBeginInfo info, VkSubpassContents contents) {
            List<IDisposable> marshalled = new List<IDisposable>();
            var infoNative = info.GetNative(marshalled);

            Device.Commands.cmdBeginRenderPass(commandBuffer, ref infoNative, contents);

            foreach (var m in marshalled) m.Dispose();
        }

        public void BindPipeline(VkPipelineBindPoint bindPoint, Pipeline pipeline) {
            Device.Commands.cmdBindPipeline(commandBuffer, bindPoint, pipeline.Native);
        }

        public void BindVertexBuffers(uint firstBinding, Buffer[] buffers, ulong[] offsets) {
            using (var marshalled = new NativeArray<VkBuffer>(buffers)) {
                Device.Commands.cmdBindVertexBuffers(commandBuffer, firstBinding, (uint)buffers.Length, marshalled.Address, ref offsets[0]);
            }
        }

        public void BindIndexBuffer(Buffer buffer, ulong offset, VkIndexType indexType) {
            Device.Commands.cmdBindIndexBuffer(commandBuffer, buffer.Native, offset, indexType);
        }

        public void BindDescriptorSets(VkPipelineBindPoint pipelineBindPoint, PipelineLayout layout, uint firstSet, DescriptorSet[] descriptorSets, uint[] dynamicOffsets) {
            using (var descriptorSetsMarshalled = new NativeArray<VkDescriptorSet>(descriptorSets))
            using (var offsetsMarshalled = new PinnedArray<uint>(dynamicOffsets)) {
                Device.Commands.cmdBindDescriptorSets(commandBuffer, pipelineBindPoint, layout.Native,
                    firstSet, (uint)descriptorSets.Length, descriptorSetsMarshalled.Address,
                    (uint)offsetsMarshalled.Length, offsetsMarshalled.Address);
            }
        }

        public void BindDescriptorSets(VkPipelineBindPoint pipelineBindPoint, PipelineLayout layout, uint firstSet, DescriptorSet[] descriptorSets) {
            BindDescriptorSets(pipelineBindPoint, layout, firstSet, descriptorSets, null);
        }

        public void Draw(int vertexCount, int instanceCount, int firstVertex, int firstInstance) {
            Device.Commands.cmdDraw(commandBuffer, (uint)vertexCount, (uint)instanceCount, (uint)firstVertex, (uint)firstInstance);
        }

        public void DrawIndexed(uint indexCount, uint instanceCount, uint firstIndex, int vertexOffset, uint firstInstance) {
            Device.Commands.cmdDrawIndexed(commandBuffer, indexCount, instanceCount, firstIndex, vertexOffset, firstInstance);
        }

        public void Copy(Buffer srcBuffer, Buffer dstBuffer, VkBufferCopy[] regions) {
            using (var pinned = new MarshalledArray<VkBufferCopy>(regions)) {
                Device.Commands.cmdCopyBuffer(commandBuffer, srcBuffer.Native, dstBuffer.Native, (uint)pinned.Count, pinned.Address);
            }
        }

        public void Copy(Image srcImage, VkImageLayout srcImageLayout, Image dstImage, VkImageLayout dstImageLayout, VkImageCopy[] regions) {
            using (var marshalled = new MarshalledArray<VkImageCopy>(regions)) {
                Device.Commands.cmdCopyImage(commandBuffer,
                    srcImage.Native, srcImageLayout,
                    dstImage.Native, dstImageLayout,
                    (uint)marshalled.Count, marshalled.Address);
            }
        }

        public void PipelineBarrier(VkPipelineStageFlags srcStageMask, VkPipelineStageFlags dstStageMask, VkDependencyFlags flags,
            MemoryBarrier[] memoryBarriers, BufferMemoryBarrier[] bufferMemoryBarriers, ImageMemoryBarrier[] imageMemoryBarriers) {

            MarshalledArray<VkMemoryBarrier> memoryBarriersMarshalled = null;
            uint mbCount = 0;
            IntPtr mbAddress = IntPtr.Zero;
            MarshalledArray<VkBufferMemoryBarrier> bufferBarriersMarshalled = null;
            uint bbCount = 0;
            IntPtr bbAddress = IntPtr.Zero;
            MarshalledArray<VkImageMemoryBarrier> imageBarriersMarshalled = null;
            uint ibCount = 0;
            IntPtr ibAddress = IntPtr.Zero;

            if (memoryBarriers != null) {
                memoryBarriersMarshalled = new MarshalledArray<VkMemoryBarrier>(memoryBarriers.Length);
                mbCount = (uint)memoryBarriers.Length;
                mbAddress = memoryBarriersMarshalled.Address;

                for (int i = 0; i < memoryBarriers.Length; i++) {
                    var mb = memoryBarriers[i];
                    var barrier = new VkMemoryBarrier();
                    barrier.sType = VkStructureType.StructureTypeMemoryBarrier;
                    barrier.srcAccessMask = mb.srcAccessMask;
                    barrier.dstAccessMask = mb.dstAccessMask;

                    memoryBarriersMarshalled[i] = barrier;
                }
            }

            if (bufferMemoryBarriers != null) {
                bufferBarriersMarshalled = new MarshalledArray<VkBufferMemoryBarrier>(bufferMemoryBarriers.Length);
                bbCount = (uint)bufferMemoryBarriers.Length;
                bbAddress = bufferBarriersMarshalled.Address;

                for (int i = 0; i < bufferMemoryBarriers.Length; i++) {
                    var bb = bufferMemoryBarriers[i];
                    var barrier = new VkBufferMemoryBarrier();
                    barrier.sType = VkStructureType.StructureTypeBufferMemoryBarrier;
                    barrier.srcAccessMask = bb.srcAccessMask;
                    barrier.dstAccessMask = bb.dstAccessMask;
                    barrier.srcQueueFamilyIndex = bb.srcQueueFamilyIndex;
                    barrier.dstQueueFamilyIndex = bb.dstQueueFamilyIndex;
                    barrier.buffer = bb.buffer.Native;
                    barrier.offset = bb.offset;
                    barrier.size = bb.size;

                    bufferBarriersMarshalled[i] = barrier;
                }
            }

            if (imageMemoryBarriers != null) {
                imageBarriersMarshalled = new MarshalledArray<VkImageMemoryBarrier>(imageMemoryBarriers.Length);
                ibCount = (uint)imageMemoryBarriers.Length;
                ibAddress = imageBarriersMarshalled.Address;

                for (int i = 0; i < imageMemoryBarriers.Length; i++) {
                    var ib = imageMemoryBarriers[i];
                    var barrier = new VkImageMemoryBarrier();
                    barrier.sType = VkStructureType.StructureTypeImageMemoryBarrier;
                    barrier.srcAccessMask = ib.srcAccessMask;
                    barrier.dstAccessMask = ib.dstAccessMask;
                    barrier.oldLayout = ib.oldLayout;
                    barrier.newLayout = ib.newLayout;
                    barrier.srcQueueFamilyIndex = ib.srcQueueFamilyIndex;
                    barrier.dstQueueFamilyIndex = ib.dstQueueFamilyIndex;
                    barrier.image = ib.image.Native;
                    barrier.subresourceRange = ib.subresourceRange;

                    imageBarriersMarshalled[i] = barrier;
                }
            }

            using (memoryBarriersMarshalled)
            using (bufferBarriersMarshalled)
            using (imageBarriersMarshalled) {
                Device.Commands.cmdPipelineBarrier(commandBuffer,
                    srcStageMask, dstStageMask, flags,
                    mbCount, mbAddress,
                    bbCount, bbAddress,
                    ibCount, ibAddress);
            }
        }

        public void EndRenderPass() {
            Device.Commands.cmdEndRenderPass(commandBuffer);
        }

        public VkResult End() {
            return Device.Commands.endCommandBuffer(commandBuffer);
        }
    }
}
