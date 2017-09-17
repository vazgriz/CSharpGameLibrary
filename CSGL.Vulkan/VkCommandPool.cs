using System;
using System.Collections.Generic;

namespace CSGL.Vulkan {
    public class VkCommandPoolCreateInfo {
        public VkCommandPoolCreateFlags flags;
        public int queueFamilyIndex;
    }

    public class VkCommandPool : IDisposable, INative<Unmanaged.VkCommandPool> {
        Unmanaged.VkCommandPool commandPool;
        bool disposed = false;

        public Unmanaged.VkCommandPool Native {
            get {
                return commandPool;
            }
        }

        public VkDevice Device { get; private set; }
        public VkCommandPoolCreateFlags Flags { get; private set; }
        public int QueueFamilyIndex { get; private set; }

        List<VkCommandBuffer> commandBuffers;

        public VkCommandPool(VkDevice device, VkCommandPoolCreateInfo info) {
            if (device == null) throw new ArgumentNullException(nameof(device));
            if (info == null) throw new ArgumentNullException(nameof(info));

            Device = device;

            CreateCommandPool(info);

            Flags = info.flags;
            QueueFamilyIndex = info.queueFamilyIndex;

            commandBuffers = new List<VkCommandBuffer>();
        }

        void CreateCommandPool(VkCommandPoolCreateInfo mInfo) {
            var info = new Unmanaged.VkCommandPoolCreateInfo();
            info.sType = VkStructureType.CommandPoolCreateInfo;
            info.flags = mInfo.flags;
            info.queueFamilyIndex = (uint)mInfo.queueFamilyIndex;

            var result = Device.Commands.createCommandPool(Device.Native, ref info, Device.Instance.AllocationCallbacks, out commandPool);
            if (result != VkResult.Success) throw new CommandPoolException(result, string.Format("Error creating command pool: {0}", result));
        }

        public VkCommandBuffer Allocate(VkCommandBufferLevel level) {
            var info = new Unmanaged.VkCommandBufferAllocateInfo();
            info.sType = VkStructureType.CommandBufferAllocateInfo;
            info.level = level;
            info.commandPool = commandPool;
            info.commandBufferCount = 1;

            using (var commandBufferMarshalled = new Native<Unmanaged.VkCommandBuffer>()) {
                var result = Device.Commands.allocateCommandBuffers(Device.Native, ref info, commandBufferMarshalled.Address);
                if (result != VkResult.Success) throw new CommandPoolException(result, string.Format("Error allocating command buffer: {0}", result));

                VkCommandBuffer commandBuffer = new VkCommandBuffer(Device, this, commandBufferMarshalled.Value, level);
                commandBuffers.Add(commandBuffer);

                return commandBuffer;
            }
        }

        public IList<VkCommandBuffer> Allocate(VkCommandBufferAllocateInfo info) {
            if (info == null) throw new ArgumentNullException(nameof(info));

            var infoNative = new Unmanaged.VkCommandBufferAllocateInfo();
            infoNative.sType = VkStructureType.CommandBufferAllocateInfo;
            infoNative.level = info.level;
            infoNative.commandPool = commandPool;
            infoNative.commandBufferCount = (uint)info.commandBufferCount;

            using (var commandBuffersMarshalled = new NativeArray<Unmanaged.VkCommandBuffer>(info.commandBufferCount)) {
                var results = new List<VkCommandBuffer>((int)info.commandBufferCount);

                var result = Device.Commands.allocateCommandBuffers(Device.Native, ref infoNative, commandBuffersMarshalled.Address);
                if (result != VkResult.Success) throw new CommandPoolException(result, string.Format("Error allocating command buffers: {0}", result));

                for (int i = 0; i < (int)info.commandBufferCount; i++) {
                    results.Add(new VkCommandBuffer(Device, this, commandBuffersMarshalled[i], info.level));
                    commandBuffers.Add(results[i]);
                }

                return results;
            }
        }

        public void Free(IList<VkCommandBuffer> commandBuffers) {
            if (commandBuffers == null) throw new ArgumentNullException(nameof(commandBuffers));

            using (var commandBuffersMarshalled = new NativeArray<Unmanaged.VkCommandBuffer>(commandBuffers.Count)) {
                for (int i = 0; i < commandBuffers.Count; i++) {
                    commandBuffersMarshalled[i] = commandBuffers[i].Native;
                }
                Device.Commands.freeCommandBuffers(Device.Native, commandPool, (uint)commandBuffers.Count, commandBuffersMarshalled.Address);
            }
        }

        public void Free(VkCommandBuffer commandBuffer) {
            if (commandBuffer == null) throw new ArgumentNullException(nameof(commandBuffer));

            unsafe {
                Unmanaged.VkCommandBuffer commandBufferNative = commandBuffer.Native;
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

        ~VkCommandPool() {
            Dispose(false);
        }
    }

    public class CommandPoolException : VulkanException {
        public CommandPoolException(VkResult result, string message) : base(result, message) { }
    }
}
