using System;
using System.Collections.Generic;

namespace CSGL.Vulkan {
    public class DescriptorPoolCreateInfo {
        public VkDescriptorPoolCreateFlags flags;
        public uint maxSets;
        public IList<VkDescriptorPoolSize> poolSizes;
    }

    public class DescriptorPool : IDisposable, INative<VkDescriptorPool> {
        VkDescriptorPool descriptorPool;

        bool disposed;

        public Device Device { get; private set; }

        public VkDescriptorPool Native {
            get {
                return descriptorPool;
            }
        }

        public VkDescriptorPoolCreateFlags Flags { get; private set; }
        public uint MaxSets { get; private set; }
        public IList<VkDescriptorPoolSize> PoolSizes { get; private set; }

        public DescriptorPool(Device device, DescriptorPoolCreateInfo info) {
            if (device == null) throw new ArgumentNullException(nameof(device));
            if (info == null) throw new ArgumentNullException(nameof(info));

            Device = device;

            CreateDescriptorPool(info);

            Flags = info.flags;
            MaxSets = info.maxSets;
            if (info.poolSizes != null) PoolSizes = new List<VkDescriptorPoolSize>(info.poolSizes).AsReadOnly();
        }

        void CreateDescriptorPool(DescriptorPoolCreateInfo mInfo) {
            var info = new VkDescriptorPoolCreateInfo();
            info.sType = VkStructureType.DescriptorPoolCreateInfo;
            info.flags = mInfo.flags;
            info.maxSets = mInfo.maxSets;

            var poolSizesMarshalled = new MarshalledArray<VkDescriptorPoolSize>(mInfo.poolSizes);
            info.poolSizeCount = (uint)poolSizesMarshalled.Count;
            info.pPoolSizes = poolSizesMarshalled.Address;

            using (poolSizesMarshalled) {
                var result = Device.Commands.createDescriptorPool(Device.Native, ref info, Device.Instance.AllocationCallbacks, out descriptorPool);
                if (result != VkResult.Success) throw new DescriptorPoolException(result, string.Format("Error creating descriptor pool: {0}", result));
            }
        }

        public DescriptorSet[] Allocate(DescriptorSetAllocateInfo info) {
            var infoNative = new VkDescriptorSetAllocateInfo();
            infoNative.sType = VkStructureType.DescriptorSetAllocateInfo;
            infoNative.descriptorPool = descriptorPool;
            infoNative.descriptorSetCount = info.descriptorSetCount;

            var layoutsMarshalled = new NativeArray<VkDescriptorSetLayout>(info.setLayouts.Count);
            for (int i = 0; i < info.setLayouts.Count; i++) {
                layoutsMarshalled[i] = info.setLayouts[i].Native;
            }
            infoNative.pSetLayouts = layoutsMarshalled.Address;

            var descriptorSetsMarshalled = new NativeArray<VkDescriptorSet>((int)info.descriptorSetCount);

            using (layoutsMarshalled)
            using (descriptorSetsMarshalled) {
                var result = Device.Commands.allocateDescriptorSets(Device.Native, ref infoNative, descriptorSetsMarshalled.Address);
                if (result != VkResult.Success) throw new DescriptorPoolException(result, string.Format("Error allocating descriptor sets: {0}", result));

                var results = new DescriptorSet[(int)info.descriptorSetCount];

                for (int i = 0; i < info.descriptorSetCount; i++) {
                    results[i] = new DescriptorSet(Device, this, descriptorSetsMarshalled[i], info);
                }

                return results;
            }
        }

        public void Reset(VkDescriptorPoolResetFlags flags) {
            var result = Device.Commands.resetDescriptorPool(Device.Native, descriptorPool, flags);
            if (result != VkResult.Success) throw new DescriptorPoolException(result, string.Format("Error resetting descriptor pool: {0}", result));
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        void Dispose(bool disposing) {
            if (disposed) return;

            Device.Commands.destroyDescriptorPool(Device.Native, descriptorPool, Device.Instance.AllocationCallbacks);

            disposed = true;
        }

        ~DescriptorPool() {
            Dispose(false);
        }
    }

    public class DescriptorPoolException : VulkanException {
        public DescriptorPoolException(VkResult result, string message) : base(result, message) { }
    }
}
