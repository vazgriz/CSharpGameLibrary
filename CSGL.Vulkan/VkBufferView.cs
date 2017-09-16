using System;
using System.Collections.Generic;

namespace CSGL.Vulkan {
    public class VkBufferViewCreateInfo {
        public VkBuffer buffer;
        public VkFormat format;
        public ulong offset;
        public ulong range;
    }

    public class VkBufferView : INative<Unmanaged.VkBufferView>, IDisposable {
        bool disposed;
        Unmanaged.VkBufferView bufferView;

        public VkDevice Device { get; private set; }
        public VkBuffer Buffer { get; private set; }
        public VkFormat Format { get; private set; }
        public ulong Offset { get; private set; }
        public ulong Range { get; private set; }

        public Unmanaged.VkBufferView Native {
            get {
                return bufferView;
            }
        }

        public VkBufferView(VkDevice device, VkBufferViewCreateInfo info) {
            if (device == null) throw new ArgumentNullException(nameof(device));
            if (info == null) throw new ArgumentNullException(nameof(info));

            Device = device;

            CreateBufferView(info);

            Buffer = info.buffer;
            Format = info.format;
            Offset = info.offset;
            Range = info.range;
        }

        void CreateBufferView(VkBufferViewCreateInfo mInfo) {
            if (mInfo.buffer == null) throw new ArgumentNullException(nameof(mInfo.buffer));

            var info = new Unmanaged.VkBufferViewCreateInfo();
            info.sType = VkStructureType.BufferViewCreateInfo;
            info.buffer = mInfo.buffer.Native;
            info.format = mInfo.format;
            info.offset = mInfo.offset;
            info.range = mInfo.range;

            var result = Device.Commands.createBufferView(Device.Native, ref info, Device.Instance.AllocationCallbacks, out bufferView);
            if (result != VkResult.Success) throw new BufferViewException(result, string.Format("Error creating buffer view: {0}", result));
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        void Dispose(bool disposing) {
            if (disposed) return;

            Device.Commands.destroyBufferView(Device.Native, bufferView, Device.Instance.AllocationCallbacks);

            disposed = true;
        }

        ~VkBufferView() {
            Dispose(false);
        }
    }

    public class BufferViewException : VulkanException {
        public BufferViewException(VkResult result, string message) : base(result, message) { }
    }
}
