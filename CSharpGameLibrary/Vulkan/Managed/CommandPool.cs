using System;

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

            var infoMarshalled = new Marshalled<VkCommandPoolCreateInfo>(info);
            var commandPoolMarshalled = new Marshalled<VkCommandPool>();

            try {
                var result = Device.Commands.createCommandPool(Device.Native, infoMarshalled.Address, Device.Instance.AllocationCallbacks, commandPoolMarshalled.Address);
                if (result != VkResult.Success) throw new CommandPoolException(string.Format("Error creating command pool: {0}", result));
                commandPool = commandPoolMarshalled.Value;
            }
            finally {
                infoMarshalled.Dispose();
                commandPoolMarshalled.Dispose();
            }
        }

        public CommandBuffer[] Allocate(CommandBufferAllocateInfo mInfo) {
            VkCommandBufferAllocateInfo info = new VkCommandBufferAllocateInfo();
            info.sType = VkStructureType.StructureTypeCommandBufferAllocateInfo;
            info.level = mInfo.Level;
            info.commandPool = commandPool;
            info.commandBufferCount = mInfo.Count;

            var infoMarshalled = new Marshalled<VkCommandBufferAllocateInfo>(info);
            var commandBuffersMarshalled = new MarshalledArray<VkCommandBuffer>((int)mInfo.Count);

            CommandBuffer[] commandBuffers = new CommandBuffer[(int)mInfo.Count];

            try {
                var result = Device.Commands.allocateCommandBuffers(Device.Native, infoMarshalled.Address, commandBuffersMarshalled.Address);
                if (result != VkResult.Success) throw new CommandPoolException(string.Format("Error allocating command buffers: {0}", result));

                for (int i = 0; i < mInfo.Count; i++) {
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
