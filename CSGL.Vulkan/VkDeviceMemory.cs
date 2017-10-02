using System;
using System.Collections.Generic;

namespace CSGL.Vulkan {
    public class VkMemoryAllocateInfo {
        public long allocationSize;
        public int memoryTypeIndex;
    }

    public class VkMappedMemoryRange {
        public VkDeviceMemory memory;
        public long offset;
        public long size;
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

        public long Size { get; private set; }
        public int MemoryTypeIndex { get; private set; }

        public long Commitment {
            get {
                ulong result = 0;
                Device.Commands.getCommitedMemory(Device.Native, deviceMemory, ref result);
                return (long)result;
            }
        }

        public VkDeviceMemory(VkDevice device, VkMemoryAllocateInfo info) {
            if (device == null) throw new ArgumentNullException(nameof(device));
            if (info == null) throw new ArgumentNullException(nameof(info));

            Device = device;

            CreateDeviceMemory((ulong)info.allocationSize, (uint)info.memoryTypeIndex);

            Size = info.allocationSize;
            MemoryTypeIndex = info.memoryTypeIndex;
        }

        public VkDeviceMemory(VkDevice device, long allocationSize, int memoryTypeIndex) {
            if (device == null) throw new ArgumentNullException(nameof(device));

            Device = device;

            CreateDeviceMemory((ulong)allocationSize, (uint)memoryTypeIndex);

            Size = allocationSize;
            MemoryTypeIndex = memoryTypeIndex;
        }

        void CreateDeviceMemory(ulong allocationSize, uint memoryTypeIndex) {
            var info = new Unmanaged.VkMemoryAllocateInfo();
            info.sType = VkStructureType.MemoryAllocateInfo;
            info.allocationSize = allocationSize;
            info.memoryTypeIndex = memoryTypeIndex;

            var result = Device.Commands.allocateMemory(Device.Native, ref info, Device.Instance.AllocationCallbacks, out deviceMemory);
            if (result != VkResult.Success) throw new DeviceMemoryException(result, string.Format("Error allocating device memory: {0}", result));
        }

        public IntPtr Map(long offset, long size) {
            IntPtr data;
            var result = Device.Commands.mapMemory(Device.Native, deviceMemory, (ulong)offset, (ulong)size, VkMemoryMapFlags.None, out data);
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
            result.offset = (ulong)range.offset;
            result.size = (ulong)range.size;

            return result;
        }

        static unsafe void FlushInternal(VkDevice device, int count, Unmanaged.VkMappedMemoryRange* ranges) {
            var result = device.Commands.flushMemory(device.Native, (uint)count, (IntPtr)ranges);
            if (result != VkResult.Success) throw new DeviceMemoryException(result, string.Format("Error flushing memory: {0}", result));
        }

        public static void Flush(VkDevice device, IList<VkMappedMemoryRange> ranges) {
            if (ranges == null) throw new ArgumentNullException(nameof(ranges));

            unsafe {
                var rangesNative = stackalloc Unmanaged.VkMappedMemoryRange[ranges.Count];

                for (int i = 0; i < ranges.Count; i++) {
                    rangesNative[i] = Marshal(ranges[i]);
                }

                FlushInternal(device, ranges.Count, rangesNative);
            }
        }

        public static void Flush(VkDevice device, VkMappedMemoryRange ranges) {
            if (ranges == null) throw new ArgumentNullException(nameof(ranges));

            var rangeNative = Marshal(ranges);

            unsafe {
                FlushInternal(device, 1, &rangeNative);
            }
        }

        public void Flush(IList<VkMappedMemoryRange> ranges) {
            if (ranges == null) throw new ArgumentNullException(nameof(ranges));

            unsafe {
                var rangesNative = stackalloc Unmanaged.VkMappedMemoryRange[ranges.Count];

                for (int i = 0; i < ranges.Count; i++) {
                    rangesNative[i] = Marshal(ranges[i]);
                    rangesNative[i].memory = deviceMemory;
                }

                FlushInternal(Device, ranges.Count, rangesNative);
            }
        }

        public void Flush(VkMappedMemoryRange ranges) {
            if (ranges == null) throw new ArgumentNullException(nameof(ranges));

            Flush(ranges.offset, ranges.size);
        }

        public void Flush(long offset, long size) {
            var rangeNative = new Unmanaged.VkMappedMemoryRange();
            rangeNative.sType = VkStructureType.MappedMemoryRange;
            rangeNative.memory = deviceMemory;
            rangeNative.offset = (ulong)offset;
            rangeNative.size = (ulong)size;

            unsafe {
                FlushInternal(Device, 1, &rangeNative);
            }
        }

        static unsafe void InvalidateInternal(VkDevice device, int count, Unmanaged.VkMappedMemoryRange* ranges) {
            var result = device.Commands.invalidateMemory(device.Native, (uint)count, (IntPtr)ranges);
            if (result != VkResult.Success) throw new DeviceMemoryException(result, string.Format("Error invalidating memory: {0}", result));
        }

        public static void Invalidate(VkDevice device, IList<VkMappedMemoryRange> ranges) {
            if (ranges == null) throw new ArgumentNullException(nameof(ranges));

            unsafe {
                var rangesNative = stackalloc Unmanaged.VkMappedMemoryRange[ranges.Count];

                for (int i = 0; i < ranges.Count; i++) {
                    rangesNative[i] = Marshal(ranges[i]);
                }

                InvalidateInternal(device, ranges.Count, rangesNative);
            }
        }

        public static void Invalidate(VkDevice device, VkMappedMemoryRange ranges) {
            if (ranges == null) throw new ArgumentNullException(nameof(ranges));

            var rangeNative = Marshal(ranges);

            unsafe {
                InvalidateInternal(device, 1, &rangeNative);
            }
        }

        public void Invalidate(IList<VkMappedMemoryRange> ranges) {
            if (ranges == null) throw new ArgumentNullException(nameof(ranges));

            unsafe {
                var rangesNative = stackalloc Unmanaged.VkMappedMemoryRange[ranges.Count];

                for (int i = 0; i < ranges.Count; i++) {
                    rangesNative[i] = Marshal(ranges[i]);
                    rangesNative[i].memory = deviceMemory;
                }

                InvalidateInternal(Device, ranges.Count, rangesNative);
            }
        }

        public void Invalidate(VkMappedMemoryRange ranges) {
            if (ranges == null) throw new ArgumentNullException(nameof(ranges));

            Invalidate(Device, ranges);
        }

        public void Invalidate(long offset, long size) {
            var rangeNative = new Unmanaged.VkMappedMemoryRange();
            rangeNative.sType = VkStructureType.MappedMemoryRange;
            rangeNative.memory = deviceMemory;
            rangeNative.offset = (ulong)offset;
            rangeNative.size = (ulong)size;

            unsafe {
                InvalidateInternal(Device, 1, &rangeNative);
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
