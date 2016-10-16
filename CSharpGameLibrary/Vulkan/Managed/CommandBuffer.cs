using System;
using System.Collections.Generic;

namespace CSGL.Vulkan.Managed {
    public class CommandBufferAllocateInfo {
        public CommandPool commandPool;
        public VkCommandBufferLevel level;
        public uint count;
    }

    public class CommandBuffer {
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
            var marshalled = new MarshalledArray<VkBuffer>(buffers.Length);
            for (int i = 0; i < buffers.Length; i++) {
                marshalled[i] = buffers[i].Native;
            }
            Device.Commands.cmdBindVertexBuffers(commandBuffer, firstBinding, (uint)buffers.Length, marshalled.Address, ref offsets[0]);
            marshalled.Dispose();
        }

        public void Draw(int vertexCount, int instanceCount, int firstVertex, int firstInstance) {
            Device.Commands.cmdDraw(commandBuffer, (uint)vertexCount, (uint)instanceCount, (uint)firstVertex, (uint)firstInstance);
        }

        public void EndRenderPass() {
            Device.Commands.cmdEndRenderPass(commandBuffer);
        }

        public VkResult End() {
            return Device.Commands.endCommandBuffer(commandBuffer);
        }
    }
}
