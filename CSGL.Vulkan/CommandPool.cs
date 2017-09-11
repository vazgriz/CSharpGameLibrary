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
        public uint QueueFamilyIndex { get; private set; }

        List<CommandBuffer> commandBuffers;

        public CommandPool(Device device, CommandPoolCreateInfo info) {
            if (device == null) throw new ArgumentNullException(nameof(device));
            if (info == null) throw new ArgumentNullException(nameof(info));

            Device = device;

            CreateCommandPool(info);

            Flags = info.flags;
            QueueFamilyIndex = info.queueFamilyIndex;

            commandBuffers = new List<CommandBuffer>();
        }

        void CreateCommandPool(CommandPoolCreateInfo mInfo) {
            VkCommandPoolCreateInfo info = new VkCommandPoolCreateInfo();
            info.sType = VkStructureType.CommandPoolCreateInfo;
            info.flags = mInfo.flags;
            info.queueFamilyIndex = mInfo.queueFamilyIndex;

            var result = Device.Commands.createCommandPool(Device.Native, ref info, Device.Instance.AllocationCallbacks, out commandPool);
            if (result != VkResult.Success) throw new CommandPoolException(result, string.Format("Error creating command pool: {0}", result));
        }

        public CommandBuffer Allocate(VkCommandBufferLevel level) {
            var info = new VkCommandBufferAllocateInfo();
            info.sType = VkStructureType.CommandBufferAllocateInfo;
            info.level = level;
            info.commandPool = commandPool;
            info.commandBufferCount = 1;

            using (var commandBufferMarshalled = new Native<VkCommandBuffer>()) {
                var result = Device.Commands.allocateCommandBuffers(Device.Native, ref info, commandBufferMarshalled.Address);
                if (result != VkResult.Success) throw new CommandPoolException(result, string.Format("Error allocating command buffer: {0}", result));

                CommandBuffer commandBuffer = new CommandBuffer(Device, this, commandBufferMarshalled.Value, level);
                commandBuffers.Add(commandBuffer);

                return commandBuffer;
            }
        }

        public IList<CommandBuffer> Allocate(CommandBufferAllocateInfo info) {
            var infoNative = new VkCommandBufferAllocateInfo();
            infoNative.sType = VkStructureType.CommandBufferAllocateInfo;
            infoNative.level = info.level;
            infoNative.commandPool = commandPool;
            infoNative.commandBufferCount = info.commandBufferCount;

            using (var commandBuffersMarshalled = new NativeArray<VkCommandBuffer>(info.commandBufferCount)) {
                var results = new List<CommandBuffer>((int)info.commandBufferCount);

                var result = Device.Commands.allocateCommandBuffers(Device.Native, ref infoNative, commandBuffersMarshalled.Address);
                if (result != VkResult.Success) throw new CommandPoolException(result, string.Format("Error allocating command buffers: {0}", result));

                for (int i = 0; i < (int)info.commandBufferCount; i++) {
                    results.Add(new CommandBuffer(Device, this, commandBuffersMarshalled[i], info.level));
                    commandBuffers.Add(results[i]);
                }

                return results;
            }
        }

        public void Free(IList<CommandBuffer> commandBuffers) {
            using (var commandBuffersMarshalled = new NativeArray<VkCommandBuffer>(commandBuffers.Count)) {
                for (int i = 0; i < commandBuffers.Count; i++) {
                    commandBuffersMarshalled[i] = commandBuffers[i].Native;
                }
                Device.Commands.freeCommandBuffers(Device.Native, commandPool, (uint)commandBuffers.Count, commandBuffersMarshalled.Address);
            }
        }

        public void Free(CommandBuffer commandBuffer) {
            unsafe {
                VkCommandBuffer commandBufferNative = commandBuffer.Native;
                Device.Commands.freeCommandBuffers(Device.Native, commandPool, 1, (IntPtr)(&commandBufferNative));
            }
        }

        public void Reset(VkCommandPoolResetFlags flags) {
            var result = Device.Commands.resetCommandPool(Device.Native, commandPool, flags);
            if (result != VkResult.Success) throw new CommandPoolException(result, string.Format("Error resetting command pool: {0}", result));

            foreach (var commandBuffer in commandBuffers) {
                commandBuffer.CanDispose = false;
            }

            commandBuffers.Clear();
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        void Dispose(bool disposing) {
            if (disposed) return;

            Device.Commands.destroyCommandPool(Device.Native, commandPool, Device.Instance.AllocationCallbacks);

            foreach (var commandBuffer in commandBuffers) {
                commandBuffer.CanDispose = false;
            }

            disposed = true;
        }

        ~CommandPool() {
            Dispose(false);
        }
    }

    public class CommandPoolException : VulkanException {
        public CommandPoolException(VkResult result, string message) : base(result, message) { }
    }
}
