using System;
using System.Collections.Generic;

namespace CSGL.Vulkan {
    public class MemoryAllocateInfo {
        public ulong allocationSize;
        public uint memoryTypeIndex;
    }

    public class MappedMemoryRange {
        public DeviceMemory memory;
        public ulong offset;
        public ulong size;
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
        public uint MemoryTypeIndex { get; private set; }

        public ulong Commitment {
            get {
                ulong result = 0;
                Device.Commands.getCommitedMemory(Device.Native, deviceMemory, ref result);
                return result;
            }
        }

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

            var result = Device.Commands.allocateMemory(Device.Native, ref info, Device.Instance.AllocationCallbacks, out deviceMemory);
            if (result != VkResult.Success) throw new DeviceMemoryException(result, string.Format("Error allocating device memory: {0}", result));

            Size = allocationSize;
            MemoryTypeIndex = memoryTypeIndex;
        }

        public IntPtr Map(ulong offset, ulong size) {
            IntPtr data;
            var result = Device.Commands.mapMemory(Device.Native, deviceMemory, offset, size, VkMemoryMapFlags.None, out data);
            if (result != VkResult.Success) throw new DeviceMemoryException(result, string.Format("Error mapping memory: {0}", result));

            return data;
        }

        public void Unmap() {
            Device.Commands.unmapMemory(Device.Native, deviceMemory);
        }

        static VkMappedMemoryRange Marshal(MappedMemoryRange range) {
            if (range == null) throw new ArgumentNullException(nameof(range));

            var result = new VkMappedMemoryRange();
            result.sType = VkStructureType.MappedMemoryRange;
            result.memory = range.memory.Native;
            result.offset = range.offset;
            result.size = range.size;

            return result;
        }

        public static void Flush(Device device, IList<MappedMemoryRange> ranges) {
            unsafe {
                VkMappedMemoryRange* rangesNative = stackalloc VkMappedMemoryRange[ranges.Count];

                for (int i = 0; i < ranges.Count; i++) {
                    rangesNative[i] = Marshal(ranges[i]);
                }

                var result = device.Commands.flushMemory(device.Native, (uint)ranges.Count, (IntPtr)rangesNative);
                if (result != VkResult.Success) throw new DeviceMemoryException(result, string.Format("Error flushing memory: {0}", result));
            }
        }

        public static void Flush(Device device, MappedMemoryRange ranges) {
            VkMappedMemoryRange rangeNative = Marshal(ranges);

            unsafe {
                var result = device.Commands.flushMemory(device.Native, 1, (IntPtr)(&rangeNative));
                if (result != VkResult.Success) throw new DeviceMemoryException(result, string.Format("Error flushing memory: {0}", result));
            }
        }

        public void Flush(IList<MappedMemoryRange> ranges) {
            if (ranges == null) throw new ArgumentNullException(nameof(ranges));

            for (int i = 0; i < ranges.Count; i++) {
                ranges[i].memory = this;
            }

            Flush(Device, ranges);
        }

        public void Flush(MappedMemoryRange ranges) {
            ranges.memory = this;
            Flush(Device, ranges);
        }

        public void Flush(ulong offset, ulong size) {
            VkMappedMemoryRange rangeNative = new VkMappedMemoryRange();
            rangeNative.sType = VkStructureType.MappedMemoryRange;
            rangeNative.memory = deviceMemory;
            rangeNative.offset = offset;
            rangeNative.size = size;

            unsafe {
                var result = Device.Commands.flushMemory(Device.Native, 1, (IntPtr)(&rangeNative));
                if (result != VkResult.Success) throw new DeviceMemoryException(result, string.Format("Error flushing memory: {0}", result));
            }
        }

        public static void Invalidate(Device device, IList<MappedMemoryRange> ranges) {
            unsafe {
                VkMappedMemoryRange* rangesNative = stackalloc VkMappedMemoryRange[ranges.Count];

                for (int i = 0; i < ranges.Count; i++) {
                    rangesNative[i] = Marshal(ranges[i]);
                }
                
                var result = device.Commands.invalidateMemory(device.Native, (uint)ranges.Count, (IntPtr)(&rangesNative));
                if (result != VkResult.Success) throw new DeviceMemoryException(result, string.Format("Error invalidating memory: {0}", result));
            }
        }

        public static void Invalidate(Device device, MappedMemoryRange ranges) {
            VkMappedMemoryRange rangeNative = Marshal(ranges);

            unsafe {
                var result = device.Commands.invalidateMemory(device.Native, 1, (IntPtr)(&rangeNative));
                if (result != VkResult.Success) throw new DeviceMemoryException(result, string.Format("Error invalidating memory: {0}", result));
            }
        }

        public void Invalidate(IList<MappedMemoryRange> ranges) {
            if (ranges == null) throw new ArgumentNullException(nameof(ranges));

            for (int i = 0; i < ranges.Count; i++) {
                ranges[i].memory = this;
            }

            Invalidate(Device, ranges);
        }

        public void Invalidate(MappedMemoryRange ranges) {
            ranges.memory = this;
            Invalidate(Device, ranges);
        }

        public void Invalidate(ulong offset, ulong size) {
            VkMappedMemoryRange rangeNative = new VkMappedMemoryRange();
            rangeNative.sType = VkStructureType.MappedMemoryRange;
            rangeNative.memory = deviceMemory;
            rangeNative.offset = offset;
            rangeNative.size = size;

            unsafe {
                var result = Device.Commands.invalidateMemory(Device.Native, 1, (IntPtr)(&rangeNative));
                if (result != VkResult.Success) throw new DeviceMemoryException(result, string.Format("Error invalidating memory: {0}", result));
            }
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        void Dispose(bool disposing) {
            if (disposed) return;

            Device.Commands.freeMemory(Device.Native, deviceMemory, Device.Instance.AllocationCallbacks);
            disposed = true;
        }

        ~DeviceMemory() {
            Dispose(false);
        }
    }

    public class DeviceMemoryException : VulkanException {
        public DeviceMemoryException(VkResult result, string message) : base(result, message) { }
    }
}
