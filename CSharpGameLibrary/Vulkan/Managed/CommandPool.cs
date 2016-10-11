using System;
using System.Collections.Generic;

namespace CSGL.Vulkan.Managed {
    public class CommandPoolCreateInfo {
        public VkCommandPoolCreateFlags Flags { get; set; }
        public uint QueueFamilyIndex { get; set; }
    }

    public class CommandPool : IDisposable {
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
            info.sType = VkStructureType.StructureTypeCommandPoolCreateInfo;
            info.flags = mInfo.Flags;
            info.queueFamilyIndex = mInfo.QueueFamilyIndex;
            
            var result = Device.Commands.createCommandPool(Device.Native, ref info, Device.Instance.AllocationCallbacks, out commandPool);
            if (result != VkResult.Success) throw new CommandPoolException(string.Format("Error creating command pool: {0}", result));
        }

        public CommandBuffer[] Allocate(CommandBufferAllocateInfo info) {
            VkCommandBufferAllocateInfo infoNative = new VkCommandBufferAllocateInfo();
            infoNative.sType = VkStructureType.StructureTypeCommandBufferAllocateInfo;
            infoNative.level = info.level;
            infoNative.commandPool = commandPool;
            infoNative.commandBufferCount = info.count;

            var infoMarshalled = new Marshalled<VkCommandBufferAllocateInfo>(infoNative);
            var commandBuffersMarshalled = new MarshalledArray<VkCommandBuffer>((int)info.count);

            CommandBuffer[] commandBuffers = new CommandBuffer[(int)info.count];

            try {
                var result = Device.Commands.allocateCommandBuffers(Device.Native, ref infoNative, commandBuffersMarshalled.Address);
                if (result != VkResult.Success) throw new CommandPoolException(string.Format("Error allocating command buffers: {0}", result));

                for (int i = 0; i < info.count; i++) {
                    commandBuffers[i] = new CommandBuffer(Device, this, commandBuffersMarshalled[i]);
                }

                return commandBuffers;
            }
            finally {
                infoMarshalled.Dispose();
                commandBuffersMarshalled.Dispose();
            }
        }

        public void Free(CommandBuffer[] commandBuffers) {
            var commandBuffersMarshalled = new MarshalledArray<VkCommandBuffer>(commandBuffers.Length);
            
            for (int i = 0; i < commandBuffers.Length; i++) {
                commandBuffersMarshalled[i] = commandBuffers[i].Native;
            }

            Device.Commands.freeCommandBuffers(Device.Native, commandPool, (uint)commandBuffers.Length, commandBuffersMarshalled.Address);
            commandBuffersMarshalled.Dispose();
        }

        public void Free(List<CommandBuffer> commandBuffers) {
            var commandBuffersMarshalled = new MarshalledArray<VkCommandBuffer>(commandBuffers.Count);

            for (int i = 0; i < commandBuffers.Count; i++) {
                commandBuffersMarshalled[i] = commandBuffers[i].Native;
            }

            Device.Commands.freeCommandBuffers(Device.Native, commandPool, (uint)commandBuffers.Count, commandBuffersMarshalled.Address);
            commandBuffersMarshalled.Dispose();
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
