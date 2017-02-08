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

            CreateDeviceMemory(info.allocationSize, info.memoryTypeIndex);
        }

        public DeviceMemory(Device device, ulong allocationSize, uint memoryTypeIndex) {
            if (device == null) throw new ArgumentNullException(nameof(device));

            Device = device;

            CreateDeviceMemory(allocationSize, memoryTypeIndex);
        }

        void CreateDeviceMemory(ulong allocationSize, uint memoryTypeIndex) {
            var info = new VkMemoryAllocateInfo();
            info.sType = VkStructureType.MemoryAllocateInfo;
            info.allocationSize = allocationSize;
            info.memoryTypeIndex = memoryTypeIndex;

            Device.Commands.allocateMemory(Device.Native, ref info, Device.Instance.AllocationCallbacks, out deviceMemory);

            Size = info.allocationSize;
        }

        public IntPtr Map(ulong offset, ulong size) {
            IntPtr data;
            var result = Device.Commands.mapMemory(Device.Native, deviceMemory, offset, size, VkMemoryMapFlags.None, out data);
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
