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

        static VkMappedMemoryRange Marshal(MappedMemoryRange range) {
            var result = new VkMappedMemoryRange();
            result.sType = VkStructureType.MappedMemoryRange;
            result.memory = range.memory.Native;
            result.offset = range.offset;
            result.size = range.size;

            return result;
        }

        static unsafe void FlushInternal(Device device, int count, VkMappedMemoryRange* ranges) {
            var result = device.Commands.flushMemory(device.Native, (uint)count, (IntPtr)ranges);
            if (result != VkResult.Success) throw new DeviceMemoryException(string.Format("Error flushing memory: {0}", result));
        }

        static void FlushInternal(Device device, int count, MappedMemoryRange[] ranges) {
            unsafe
            {
                VkMappedMemoryRange* rangesNative = stackalloc VkMappedMemoryRange[count];

                for (int i = 0; i < count; i++) {
                    rangesNative[i] = Marshal(ranges[i]);
                }

                FlushInternal(device, count, rangesNative);
            }
        }

        public static void Flush(Device device, MappedMemoryRange[] ranges) {
            FlushInternal(device, ranges.Length, ranges);
        }

        public static void Flush(Device device, List<MappedMemoryRange> ranges) {
            FlushInternal(device, ranges.Count, Interop.GetInternalArray(ranges));
        }

        public static void Flush(Device device, MappedMemoryRange ranges) {
            VkMappedMemoryRange rangeNative = Marshal(ranges);

            unsafe
            {
                FlushInternal(device, 1, &rangeNative);
            }
        }

        public void Flush(MappedMemoryRange[] ranges) {
            for (int i = 0; i < ranges.Length; i++) {
                ranges[i].memory = this;
            }

            Flush(Device, ranges);
        }

        public void Flush(List<MappedMemoryRange> ranges) {
            for (int i = 0; i < ranges.Count; i++) {
                ranges[i].memory = this;
            }

            FlushInternal(Device, ranges.Count, Interop.GetInternalArray(ranges));
        }

        public void Flush(MappedMemoryRange ranges) {
            ranges.memory = this;
            VkMappedMemoryRange rangeNative = Marshal(ranges);

            unsafe
            {
                FlushInternal(Device, 1, &rangeNative);
            }
        }

        public void Flush(ulong offset, ulong size) {
            VkMappedMemoryRange rangeNative = new VkMappedMemoryRange();
            rangeNative.sType = VkStructureType.MappedMemoryRange;
            rangeNative.memory = deviceMemory;
            rangeNative.offset = offset;
            rangeNative.size = size;

            unsafe
            {
                FlushInternal(Device, 1, &rangeNative);
            }
        }

        static unsafe void InvalidateInternal(Device device, int count, VkMappedMemoryRange* ranges) {
            var result = device.Commands.invalidateMemory(device.Native, (uint)count, (IntPtr)ranges);
            if (result != VkResult.Success) throw new DeviceMemoryException(string.Format("Error invalidating memory: {0}", result));
        }

        static void InvalidateInternal(Device device, int count, MappedMemoryRange[] ranges) {
            unsafe
            {
                VkMappedMemoryRange* rangesNative = stackalloc VkMappedMemoryRange[count];

                for (int i = 0; i < count; i++) {
                    rangesNative[i] = Marshal(ranges[i]);
                }

                InvalidateInternal(device, count, rangesNative);
            }
        }

        public static void Invalidate(Device device, MappedMemoryRange[] ranges) {
            InvalidateInternal(device, ranges.Length, ranges);
        }

        public static void Invalidate(Device device, List<MappedMemoryRange> ranges) {
            InvalidateInternal(device, ranges.Count, Interop.GetInternalArray(ranges));
        }

        public static void Invalidate(Device device, MappedMemoryRange ranges) {
            VkMappedMemoryRange rangeNative = Marshal(ranges);

            unsafe
            {
                InvalidateInternal(device, 1, &rangeNative);
            }
        }

        public void Invalidate(MappedMemoryRange[] ranges) {
            for (int i = 0; i < ranges.Length; i++) {
                ranges[i].memory = this;
            }

            Invalidate(Device, ranges);
        }

        public void Invalidate(List<MappedMemoryRange> ranges) {
            for (int i = 0; i < ranges.Count; i++) {
                ranges[i].memory = this;
            }

            InvalidateInternal(Device, ranges.Count, Interop.GetInternalArray(ranges));
        }

        public void Invalidate(MappedMemoryRange ranges) {
            ranges.memory = this;
            VkMappedMemoryRange rangeNative = Marshal(ranges);

            unsafe
            {
                InvalidateInternal(Device, 1, &rangeNative);
            }
        }

        public void Invalidate(ulong offset, ulong size) {
            VkMappedMemoryRange rangeNative = new VkMappedMemoryRange();
            rangeNative.sType = VkStructureType.MappedMemoryRange;
            rangeNative.memory = deviceMemory;
            rangeNative.offset = offset;
            rangeNative.size = size;

            unsafe
            {
                InvalidateInternal(Device, 1, &rangeNative);
            }
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
