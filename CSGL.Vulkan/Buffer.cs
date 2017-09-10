using System;
using System.Collections.Generic;

using CSGL;

namespace CSGL.Vulkan {
    public class BufferCreateInfo {
        public VkBufferCreateFlags flags;
        public ulong size;
        public VkBufferUsageFlags usage;
        public VkSharingMode sharingMode;
        public List<uint> queueFamilyIndices;
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

        public VkBufferCreateFlags Flags { get; private set; }
        public VkBufferUsageFlags Usage { get; private set; }
        public VkSharingMode SharingMode { get; private set; }
        public List<uint> QueueFamilyIndices { get; private set; }
        public ulong Size { get; private set; }
        public ulong Offset { get; private set; }
        public DeviceMemory Memory { get; private set; }

        public Buffer(Device device, BufferCreateInfo info) {
            if (device == null) throw new ArgumentNullException(nameof(device));
            if (info == null) throw new ArgumentNullException(nameof(info));

            Device = device;

            CreateBuffer(info);

            Device.Commands.getMemoryRequirements(Device.Native, buffer, out requirements);

            Flags = info.flags;
            Usage = info.usage;
            Size = info.size;
            if (info.queueFamilyIndices != null) QueueFamilyIndices = new List<uint>(info.queueFamilyIndices);
        }

        void CreateBuffer(BufferCreateInfo mInfo) {
            var info = new VkBufferCreateInfo();
            info.sType = VkStructureType.BufferCreateInfo;
            info.flags = mInfo.flags;
            info.size = mInfo.size;
            info.usage = mInfo.usage;
            info.sharingMode = mInfo.sharingMode;

            var indicesMarshalled = new NativeArray<uint>(mInfo.queueFamilyIndices);
            info.queueFamilyIndexCount = (uint)indicesMarshalled.Count;
            info.pQueueFamilyIndices = indicesMarshalled.Address;

            using (indicesMarshalled) {
                var result = Device.Commands.createBuffer(Device.Native, ref info, Device.Instance.AllocationCallbacks, out buffer);
                if (result != VkResult.Success) throw new BufferException(result, string.Format("Error creating Buffer: {0}", result));
            }
        }

        public void Bind(DeviceMemory deviceMemory, ulong offset) {
            if (deviceMemory == null) throw new ArgumentNullException(nameof(deviceMemory));

            var result = Device.Commands.bindBuffer(Device.Native, buffer, deviceMemory.Native, offset);
            if (result != VkResult.Success) throw new BufferException(result, string.Format("Error binding buffer: {0}", result));

            Offset = offset;
            Memory = deviceMemory;
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        void Dispose(bool disposing) {
            if (disposed) return;

            Device.Commands.destroyBuffer(Device.Native, buffer, Device.Instance.AllocationCallbacks);
            disposed = true;
        }

        ~Buffer() {
            Dispose(false);
        }
    }

    public class BufferException : VulkanException {
        public BufferException(VkResult result, string message) : base(result, message) { }
    }
}
