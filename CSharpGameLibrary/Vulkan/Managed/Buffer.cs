using System;

using CSGL;

namespace CSGL.Vulkan.Managed {
    public class BufferCreateInfo {
        public VkBufferCreateFlags flags;
        public ulong size;
        public VkBufferUsageFlags usage;
        public VkSharingMode sharingMode;
        public uint[] queueFamilyIndices;
    }

    public class Buffer : IDisposable {
        VkBuffer buffer;
        bool disposed;
        
        public Device Device { get; set; }

        public VkBuffer Native {
            get {
                return buffer;
            }
        }

        public Buffer(Device device, BufferCreateInfo info) {
            if (device == null) throw new ArgumentNullException(nameof(device));
            if (info == null) throw new ArgumentNullException(nameof(info));

            Device = device;
        }

        void CreateBufferInternal(BufferCreateInfo mInfo) {
            var info = new VkBufferCreateInfo();
            info.sType = VkStructureType.StructureTypeBufferCreateInfo;
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

        void GetMemoryRequirements() {
            VkMemoryRequirements requirements;
            Device.Commands.getMemoryRequirements(Device.Native, buffer, out requirements);
        }

        //uint FindMemoryType(uint filter, VkMemoryPropertyFlags properties) {
        //    var memPropertiesMarshalled = new Marshalled<VkPhysicalDeviceMemoryProperties>();
        //    Device.Commands.getMemoryProperties(Device.PhysicalDevice.Native, memPropertiesMarshalled.Address);
        //}

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
