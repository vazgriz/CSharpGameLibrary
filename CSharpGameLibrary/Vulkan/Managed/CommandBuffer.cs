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
    }
}
