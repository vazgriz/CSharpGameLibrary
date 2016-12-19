using System;

using CSGL;

namespace CSGL.Vulkan {
    public class BufferCreateInfo {
        public VkBufferCreateFlags flags;
        public ulong size;
        public VkBufferUsageFlags usage;
        public VkSharingMode sharingMode;
        public uint[] queueFamilyIndices;
    }

    public class Buffer : IDisposable, INative<VkBuffer> {
        VkBuffer buffer;
        bool disposed;

        VkMemoryRequirements requirements;
        
        public Device Device { get; private set; }

        public VkBuffer Native {
            get {
                return buffer;
            }
        }

        public VkMemoryRequirements Requirements {
            get {
                return requirements;
            }
        }

        public Buffer(Device device, BufferCreateInfo info) {
            if (device == null) throw new ArgumentNullException(nameof(device));
            if (info == null) throw new ArgumentNullException(nameof(info));

            Device = device;

            CreateBuffer(info);
            
            Device.Commands.getMemoryRequirements(Device.Native, buffer, out requirements);
        }

        void CreateBuffer(BufferCreateInfo mInfo) {
            var info = new VkBufferCreateInfo();
            info.sType = VkStructureType.BufferCreateInfo;
            info.flags = mInfo.flags;
            info.size = mInfo.size;
            info.usage = mInfo.usage;
            info.sharingMode = mInfo.sharingMode;

            var indicesMarshalled = new PinnedArray<uint>(mInfo.queueFamilyIndices);
            info.queueFamilyIndexCount = (uint)indicesMarshalled.Length;
            info.pQueueFamilyIndices = indicesMarshalled.Address;

            try {
                var result = Device.Commands.createBuffer(Device.Native, ref info, Device.Instance.AllocationCallbacks, out buffer);
                if (result != VkResult.Success) throw new BufferException(string.Format("Error creating Buffer: {0}", result));
            }
            finally {
                indicesMarshalled.Dispose();
            }
        }

        public void Bind(DeviceMemory deviceMemory, ulong offset) {
            if (deviceMemory == null) throw new ArgumentNullException(nameof(deviceMemory));

            Device.Commands.bindBuffer(Device.Native, buffer, deviceMemory.Native, offset);
        }

        public void Dispose() {
            if (disposed) return;

            Device.Commands.destroyBuffer(Device.Native, buffer, Device.Instance.AllocationCallbacks);
            disposed = true;
        }
    }

    public class BufferException : Exception {
        public BufferException(string message) : base(message) { }
    }
}
