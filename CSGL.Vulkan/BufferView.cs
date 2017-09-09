using System;
using System.Collections.Generic;

namespace CSGL.Vulkan {
    public class BufferViewCreateInfo {
        public Buffer buffer;
        public VkFormat format;
        public ulong offset;
        public ulong range;
    }

    public class BufferView : INative<VkBufferView>, IDisposable {
        bool disposed;
        VkBufferView bufferView;

        public Device Device { get; private set; }
        public ulong Offset { get; private set; }
        public ulong Range { get; private set; }

        public VkBufferView Native {
            get {
                return bufferView;
            }
        }

        public BufferView(Device device, BufferViewCreateInfo info) {
            if (device == null) throw new ArgumentNullException(nameof(device));
            if (info == null) throw new ArgumentNullException(nameof(info));

            Device = device;
        }

        void CreateBufferView(BufferViewCreateInfo mInfo) {
            VkBufferViewCreateInfo info = new VkBufferViewCreateInfo();
            info.sType = VkStructureType.BufferViewCreateInfo;
            info.buffer = mInfo.buffer.Native;
            info.format = mInfo.format;
            info.offset = mInfo.offset;
            info.range = mInfo.range;

            var result = Device.Commands.createBufferView(Device.Native, ref info, Device.Instance.AllocationCallbacks, out bufferView);
            if (result != VkResult.Success) throw new BufferViewException(result, string.Format("Error creating buffer view: {0}", result));

            Offset = mInfo.offset;
            Range = mInfo.range;
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

        ~BufferView() {
            Dispose(false);
        }
    }

    public class BufferViewException : VulkanException {
        public BufferViewException(VkResult result, string message) : base(result, message) { }
    }
}
