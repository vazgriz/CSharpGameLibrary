using System;
using System.Collections.Generic;

namespace CSGL.Vulkan {
    public class VkMemoryAllocateInfo {
        public ulong allocationSize;
        public uint memoryTypeIndex;
    }

    public class VkMappedMemoryRange {
        public VkDeviceMemory memory;
        public ulong offset;
        public ulong size;
    }

    public class VkDeviceMemory : IDisposable, INative<Unmanaged.VkDeviceMemory> {
        Unmanaged.VkDeviceMemory deviceMemory;
        bool disposed = false;

        public VkDevice Device { get; private set; }

        public Unmanaged.VkDeviceMemory Native {
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

        public VkDeviceMemory(VkDevice device, VkMemoryAllocateInfo info) {
            if (device == null) throw new ArgumentNullException(nameof(device));
            if (info == null) throw new ArgumentNullException(nameof(info));

            Device = device;

            CreateDeviceMemory(info.allocationSize, info.memoryTypeIndex);
        }

        public VkDeviceMemory(VkDevice device, ulong allocationSize, uint memoryTypeIndex) {
            if (device == null) throw new ArgumentNullException(nameof(device));

            Device = device;

            CreateDeviceMemory(allocationSize, memoryTypeIndex);
        }

        void CreateDeviceMemory(ulong allocationSize, uint memoryTypeIndex) {
            var info = new Unmanaged.VkMemoryAllocateInfo();
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

        static Unmanaged.VkMappedMemoryRange Marshal(VkMappedMemoryRange range) {
            if (range == null) throw new ArgumentNullException(nameof(range));

            var result = new Unmanaged.VkMappedMemoryRange();
            result.sType = VkStructureType.MappedMemoryRange;
            result.memory = range.memory.Native;
            result.offset = range.offset;
            result.size = range.size;

            return result;
        }

        public static void Flush(VkDevice device, IList<VkMappedMemoryRange> ranges) {
            unsafe {
                var rangesNative = stackalloc Unmanaged.VkMappedMemoryRange[ranges.Count];

                for (int i = 0; i < ranges.Count; i++) {
                    rangesNative[i] = Marshal(ranges[i]);
                }

                var result = device.Commands.flushMemory(device.Native, (uint)ranges.Count, (IntPtr)rangesNative);
                if (result != VkResult.Success) throw new DeviceMemoryException(result, string.Format("Error flushing memory: {0}", result));
            }
        }

        public static void Flush(VkDevice device, VkMappedMemoryRange ranges) {
            var rangeNative = Marshal(ranges);

            unsafe {
                var result = device.Commands.flushMemory(device.Native, 1, (IntPtr)(&rangeNative));
                if (result != VkResult.Success) throw new DeviceMemoryException(result, string.Format("Error flushing memory: {0}", result));
            }
        }

        public void Flush(IList<VkMappedMemoryRange> ranges) {
            if (ranges == null) throw new ArgumentNullException(nameof(ranges));

            for (int i = 0; i < ranges.Count; i++) {
                ranges[i].memory = this;
            }

            Flush(Device, ranges);
        }

        public void Flush(VkMappedMemoryRange ranges) {
            ranges.memory = this;
            Flush(Device, ranges);
        }

        public void Flush(ulong offset, ulong size) {
            var rangeNative = new Unmanaged.VkMappedMemoryRange();
            rangeNative.sType = VkStructureType.MappedMemoryRange;
            rangeNative.memory = deviceMemory;
            rangeNative.offset = offset;
            rangeNative.size = size;

            unsafe {
                var result = Device.Commands.flushMemory(Device.Native, 1, (IntPtr)(&rangeNative));
                if (result != VkResult.Success) throw new DeviceMemoryException(result, string.Format("Error flushing memory: {0}", result));
            }
        }

        public static void Invalidate(VkDevice device, IList<VkMappedMemoryRange> ranges) {
            unsafe {
                var rangesNative = stackalloc Unmanaged.VkMappedMemoryRange[ranges.Count];

                for (int i = 0; i < ranges.Count; i++) {
                    rangesNative[i] = Marshal(ranges[i]);
                }
                
                var result = device.Commands.invalidateMemory(device.Native, (uint)ranges.Count, (IntPtr)(&rangesNative));
                if (result != VkResult.Success) throw new DeviceMemoryException(result, string.Format("Error invalidating memory: {0}", result));
            }
        }

        public static void Invalidate(VkDevice device, VkMappedMemoryRange ranges) {
            var rangeNative = Marshal(ranges);

            unsafe {
                var result = device.Commands.invalidateMemory(device.Native, 1, (IntPtr)(&rangeNative));
                if (result != VkResult.Success) throw new DeviceMemoryException(result, string.Format("Error invalidating memory: {0}", result));
            }
        }

        public void Invalidate(IList<VkMappedMemoryRange> ranges) {
            if (ranges == null) throw new ArgumentNullException(nameof(ranges));

            for (int i = 0; i < ranges.Count; i++) {
                ranges[i].memory = this;
            }

            Invalidate(Device, ranges);
        }

        public void Invalidate(VkMappedMemoryRange ranges) {
            ranges.memory = this;
            Invalidate(Device, ranges);
        }

        public void Invalidate(ulong offset, ulong size) {
            var rangeNative = new Unmanaged.VkMappedMemoryRange();
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

        ~VkDeviceMemory() {
            Dispose(false);
        }
    }

    public class DeviceMemoryException : VulkanException {
        public DeviceMemoryException(VkResult result, string message) : base(result, message) { }
    }
}
