using System;
using System.Collections.Generic;

namespace CSGL.Vulkan {
    public class CommandPoolCreateInfo {
        public VkCommandPoolCreateFlags Flags { get; set; }
        public uint QueueFamilyIndex { get; set; }
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

        public CommandPool(Device device, CommandPoolCreateInfo info) {
            if (device == null) throw new ArgumentNullException(nameof(device));
            if (info == null) throw new ArgumentNullException(nameof(info));

            Device = device;

            CreateCommandPool(info);
        }

        void CreateCommandPool(CommandPoolCreateInfo mInfo) {
            VkCommandPoolCreateInfo info = new VkCommandPoolCreateInfo();
            info.sType = VkStructureType.CommandPoolCreateInfo;
            info.flags = mInfo.Flags;
            info.queueFamilyIndex = mInfo.QueueFamilyIndex;
            
            var result = Device.Commands.createCommandPool(Device.Native, ref info, Device.Instance.AllocationCallbacks, out commandPool);
            if (result != VkResult.Success) throw new CommandPoolException(string.Format("Error creating command pool: {0}", result));
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
            
            var commandBufferMarshalled = new Native<VkCommandBuffer>();

            try {
                var result = Device.Commands.allocateCommandBuffers(Device.Native, ref info, commandBufferMarshalled.Address);
                if (result != VkResult.Success) throw new CommandPoolException(string.Format("Error allocating command buffer: {0}", result));

                CommandBuffer commandBuffer = new CommandBuffer(Device, this, commandBufferMarshalled.Value);

                return commandBuffer;
            }
            finally {
                commandBufferMarshalled.Dispose();
            }
        }

        public CommandBuffer[] Allocate(VkCommandBufferLevel level, int count) {
            var info = new VkCommandBufferAllocateInfo();
            info.sType = VkStructureType.CommandBufferAllocateInfo;
            info.level = level;
            info.commandPool = commandPool;
            info.commandBufferCount = (uint)count;

            var commandBuffersMarshalled = new NativeArray<VkCommandBuffer>(count);

            CommandBuffer[] commandBuffers = new CommandBuffer[count];

            try {
                var result = Device.Commands.allocateCommandBuffers(Device.Native, ref info, commandBuffersMarshalled.Address);
                if (result != VkResult.Success) throw new CommandPoolException(string.Format("Error allocating command buffers: {0}", result));

                for (int i = 0; i < count; i++) {
                    commandBuffers[i] = new CommandBuffer(Device, this, commandBuffersMarshalled[i]);
                }

                return commandBuffers;
            }
            finally {
                commandBuffersMarshalled.Dispose();
            }
        }

        public void Free(CommandBuffer[] commandBuffers) {
            using (var commandBuffersMarshalled = new NativeArray<VkCommandBuffer>(commandBuffers)) {
                Device.Commands.freeCommandBuffers(Device.Native, commandPool, (uint)commandBuffers.Length, commandBuffersMarshalled.Address);
            }
        }

        public void Free(List<CommandBuffer> commandBuffers) {
            using (var commandBuffersMarshalled = new NativeArray<VkCommandBuffer>(commandBuffers.Count)) {
                Device.Commands.freeCommandBuffers(Device.Native, commandPool, (uint)commandBuffers.Count, commandBuffersMarshalled.Address);
            }
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
