using System;
using System.Collections.Generic;

using CSGL;

namespace CSGL.Vulkan {
    public class VkBufferCreateInfo {
        public VkBufferCreateFlags flags;
        public long size;
        public VkBufferUsageFlags usage;
        public VkSharingMode sharingMode;
        public IList<int> queueFamilyIndices;
    }

    public class VkBuffer : IDisposable, INative<Unmanaged.VkBuffer> {
        Unmanaged.VkBuffer buffer;
        bool disposed;

        public VkDevice Device { get; private set; }

        public Unmanaged.VkBuffer Native {
            get {
                return buffer;
            }
        }

        public VkMemoryRequirements Requirements { get; private set; }

        public VkBufferCreateFlags Flags { get; private set; }
        public VkBufferUsageFlags Usage { get; private set; }
        public VkSharingMode SharingMode { get; private set; }
        public IList<int> QueueFamilyIndices { get; private set; }
        public long Size { get; private set; }
        public long Offset { get; private set; }
        public VkDeviceMemory Memory { get; private set; }

        public VkBuffer(VkDevice device, VkBufferCreateInfo info) {
            if (device == null) throw new ArgumentNullException(nameof(device));
            if (info == null) throw new ArgumentNullException(nameof(info));

            Device = device;

            CreateBuffer(info);

            Unmanaged.VkMemoryRequirements requirementsNative;
            Device.Commands.getMemoryRequirements(Device.Native, buffer, out requirementsNative);
            Requirements = new VkMemoryRequirements(requirementsNative);

            Flags = info.flags;
            Usage = info.usage;
            Size = info.size;
            QueueFamilyIndices = info.queueFamilyIndices.CloneReadOnly();
        }

        void CreateBuffer(VkBufferCreateInfo mInfo) {
            unsafe {
                int indicesCount = 0;
                if (mInfo.queueFamilyIndices != null) indicesCount = mInfo.queueFamilyIndices.Count;

                var info = new Unmanaged.VkBufferCreateInfo();
                info.sType = VkStructureType.BufferCreateInfo;
                info.flags = mInfo.flags;
                info.size = (ulong)mInfo.size;
                info.usage = mInfo.usage;
                info.sharingMode = mInfo.sharingMode;

                var queueFamilyIndicesNative = stackalloc uint[indicesCount];
                if (mInfo.queueFamilyIndices != null) Interop.Copy(mInfo.queueFamilyIndices, (IntPtr)queueFamilyIndicesNative);
                
                info.queueFamilyIndexCount = (uint)indicesCount;
                info.pQueueFamilyIndices = (IntPtr)queueFamilyIndicesNative;
                
                var result = Device.Commands.createBuffer(Device.Native, ref info, Device.Instance.AllocationCallbacks, out buffer);
                if (result != VkResult.Success) throw new BufferException(result, string.Format("Error creating Buffer: {0}", result));
            }
        }

        public void Bind(VkDeviceMemory deviceMemory, long offset) {
            if (deviceMemory == null) throw new ArgumentNullException(nameof(deviceMemory));

            var result = Device.Commands.bindBuffer(Device.Native, buffer, deviceMemory.Native, (ulong)offset);
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

        ~VkBuffer() {
            Dispose(false);
        }
    }

    public class BufferException : VulkanException {
        public BufferException(VkResult result, string message) : base(result, message) { }
    }
}
