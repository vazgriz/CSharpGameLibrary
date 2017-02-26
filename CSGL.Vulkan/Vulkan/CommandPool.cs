using System;
using System.Collections.Generic;

namespace CSGL.Vulkan {
    public class CommandPoolCreateInfo {
        public VkCommandPoolCreateFlags flags;
        public uint queueFamilyIndex;
    }

    public class CommandPool : IDisposable, INative<VkCommandPool> {
        VkCommandPool commandPool;
        bool disposed = false;

        public VkCommandPool Native {
            get {
                return commandPool;
            }
        }

        public Device Device { get; private set; }
        public VkCommandPoolCreateFlags Flags { get; private set; }

        public CommandPool(Device device, CommandPoolCreateInfo info) {
            if (device == null) throw new ArgumentNullException(nameof(device));
            if (info == null) throw new ArgumentNullException(nameof(info));

            Device = device;

            CreateCommandPool(info);
        }

        void CreateCommandPool(CommandPoolCreateInfo mInfo) {
            VkCommandPoolCreateInfo info = new VkCommandPoolCreateInfo();
            info.sType = VkStructureType.CommandPoolCreateInfo;
            info.flags = mInfo.flags;
            info.queueFamilyIndex = mInfo.queueFamilyIndex;
            
            var result = Device.Commands.createCommandPool(Device.Native, ref info, Device.Instance.AllocationCallbacks, out commandPool);
            if (result != VkResult.Success) throw new CommandPoolException(string.Format("Error creating command pool: {0}", result));

            Flags = mInfo.flags;
        }

        public CommandBuffer[] Allocate(CommandBufferAllocateInfo info) {
            return Allocate(info.level, (int)info.commandBufferCount);
        }

        public CommandBuffer Allocate(VkCommandBufferLevel level) {
            var info = new VkCommandBufferAllocateInfo();
            info.sType = VkStructureType.CommandBufferAllocateInfo;
            info.level = level;
            info.commandPool = commandPool;
            info.commandBufferCount = 1;

            using (var commandBufferMarshalled = new Native<VkCommandBuffer>()) {
                var result = Device.Commands.allocateCommandBuffers(Device.Native, ref info, commandBufferMarshalled.Address);
                if (result != VkResult.Success) throw new CommandPoolException(string.Format("Error allocating command buffer: {0}", result));

                CommandBuffer commandBuffer = new CommandBuffer(Device, this, commandBufferMarshalled.Value, level);

                return commandBuffer;
            }
        }

        public CommandBuffer[] Allocate(VkCommandBufferLevel level, int count) {
            var info = new VkCommandBufferAllocateInfo();
            info.sType = VkStructureType.CommandBufferAllocateInfo;
            info.level = level;
            info.commandPool = commandPool;
            info.commandBufferCount = (uint)count;

            using (var commandBuffersMarshalled = new NativeArray<VkCommandBuffer>(count)) {
                CommandBuffer[] commandBuffers = new CommandBuffer[count];
                var result = Device.Commands.allocateCommandBuffers(Device.Native, ref info, commandBuffersMarshalled.Address);
                if (result != VkResult.Success) throw new CommandPoolException(string.Format("Error allocating command buffers: {0}", result));

                for (int i = 0; i < count; i++) {
                    commandBuffers[i] = new CommandBuffer(Device, this, commandBuffersMarshalled[i], level);
                }

                return commandBuffers;
            }
        }

        public void Free(CommandBuffer[] commandBuffers) {
            using (var commandBuffersMarshalled = new NativeArray<VkCommandBuffer>(commandBuffers)) {
                Device.Commands.freeCommandBuffers(Device.Native, commandPool, (uint)commandBuffers.Length, commandBuffersMarshalled.Address);
            }
        }

        public void Free(List<CommandBuffer> commandBuffers) {
            using (var commandBuffersMarshalled = new NativeArray<VkCommandBuffer>(commandBuffers.Count)) {
                for (int i = 0; i < commandBuffers.Count; i++) {
                    commandBuffersMarshalled[i] = commandBuffers[i].Native;
                }
                Device.Commands.freeCommandBuffers(Device.Native, commandPool, (uint)commandBuffers.Count, commandBuffersMarshalled.Address);
            }
        }

        public void Reset(VkCommandPoolResetFlags flags) {
            Device.Commands.resetCommandPool(Device.Native, commandPool, flags);
        }

        public void Dispose() {
            if (disposed) return;

            Device.Commands.destroyCommandPool(Device.Native, commandPool, Device.Instance.AllocationCallbacks);

            disposed = true;
        }
    }

    public class CommandPoolException : Exception {
        public CommandPoolException(string message) : base(message) { }
    }
}
