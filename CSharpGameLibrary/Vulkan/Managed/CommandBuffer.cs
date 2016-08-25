using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSGL.Vulkan.Managed {
    public class CommandBufferAllocateInfo {
        public CommandPool CommandPool { get; set; }
        public VkCommandBufferLevel Level { get; set; }
        public uint Count { get; set; }
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

        public void Begin(CommandBeginInfo info) {
            List<IDisposable> marshalled = new List<IDisposable>();
            var infoMarshalled = new Marshalled<VkCommandBufferBeginInfo>(info.GetNative(marshalled));

            Device.Commands.beginCommandBuffer(commandBuffer, infoMarshalled.Address);

            infoMarshalled.Dispose();
            foreach (var m in marshalled) m.Dispose();
        }

        public void BeginRenderPass(RenderPassBeginInfo info, VkSubpassContents contents) {
            List<IDisposable> marshalled = new List<IDisposable>();
            var infoMarshalled = new Marshalled<VkRenderPassBeginInfo>(info.GetNative(marshalled));

            Device.Commands.cmdBeginRenderPass(commandBuffer, infoMarshalled.Address, contents);

            foreach (var m in marshalled) m.Dispose();
        }

        public void BindPipeline(VkPipelineBindPoint bindPoint, Pipeline pipeline) {
            Device.Commands.cmdBindPipeline(commandBuffer, bindPoint, pipeline.Native);
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
