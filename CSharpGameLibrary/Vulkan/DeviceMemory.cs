using System;

namespace CSGL.Vulkan {
    public class MemoryAllocateInfo {
        public ulong allocationSize;
        public uint memoryTypeIndex;
    }

    public class DeviceMemory : IDisposable, INative<VkDeviceMemory> {
        VkDeviceMemory deviceMemory;
        bool disposed = false;

        public Device Device { get; private set; }

        public VkDeviceMemory Native {
            get {
                return deviceMemory;
            }
        }

        public ulong Size { get; private set; }

        public DeviceMemory(Device device, MemoryAllocateInfo info) {
            if (device == null) throw new ArgumentNullException(nameof(device));
            if (info == null) throw new ArgumentNullException(nameof(info));

            Device = device;
            Size = info.allocationSize;

            CreateDeviceMemory(info);
        }

        void CreateDeviceMemory(MemoryAllocateInfo mInfo) {
            var info = new VkMemoryAllocateInfo();
            info.sType = VkStructureType.MemoryAllocateInfo;
            info.allocationSize = mInfo.allocationSize;
            info.memoryTypeIndex = mInfo.memoryTypeIndex;

            Device.Commands.allocateMemory(Device.Native, ref info, Device.Instance.AllocationCallbacks, out deviceMemory);
        }

        public IntPtr Map(ulong offset, ulong size, VkMemoryMapFlags flags) {
            IntPtr data;
            var result = Device.Commands.mapMemory(Device.Native, deviceMemory, offset, size, flags, out data);
            if (result != VkResult.Success) throw new DeviceMemoryException(string.Format("Error mapping memory: {0}", result));

            return data;
        }

        public void Unmap() {
            Device.Commands.unmapMemory(Device.Native, deviceMemory);
        }

        public void Dispose() {
            if (disposed) return;

            Device.Commands.freeMemory(Device.Native, deviceMemory, Device.Instance.AllocationCallbacks);
            disposed = true;
        }
    }

    public class DeviceMemoryException : Exception {
        public DeviceMemoryException(string message) : base(message) { }
    }
}
