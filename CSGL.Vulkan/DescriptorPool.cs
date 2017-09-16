using System;
using System.Collections.Generic;

namespace CSGL.Vulkan {
    public class DescriptorPoolCreateInfo {
        public VkDescriptorPoolCreateFlags flags;
        public uint maxSets;
        public IList<Unmanaged.VkDescriptorPoolSize> poolSizes;
    }

    public class DescriptorPool : IDisposable, INative<Unmanaged.VkDescriptorPool> {
        Unmanaged.VkDescriptorPool descriptorPool;

        bool disposed;

        public Device Device { get; private set; }

        public Unmanaged.VkDescriptorPool Native {
            get {
                return descriptorPool;
            }
        }

        public VkDescriptorPoolCreateFlags Flags { get; private set; }
        public uint MaxSets { get; private set; }
        public IList<Unmanaged.VkDescriptorPoolSize> PoolSizes { get; private set; }

        List<DescriptorSet> descriptorSets;

        public DescriptorPool(Device device, DescriptorPoolCreateInfo info) {
            if (device == null) throw new ArgumentNullException(nameof(device));
            if (info == null) throw new ArgumentNullException(nameof(info));

            Device = device;

            CreateDescriptorPool(info);

            Flags = info.flags;
            MaxSets = info.maxSets;
            PoolSizes = info.poolSizes.CloneReadOnly();

            descriptorSets = new List<DescriptorSet>();
        }

        void CreateDescriptorPool(DescriptorPoolCreateInfo mInfo) {
            if (mInfo.poolSizes == null) throw new ArgumentNullException(nameof(mInfo.poolSizes));

            var info = new Unmanaged.VkDescriptorPoolCreateInfo();
            info.sType = VkStructureType.DescriptorPoolCreateInfo;
            info.flags = mInfo.flags;
            info.maxSets = mInfo.maxSets;

            var poolSizesMarshalled = new MarshalledArray<Unmanaged.VkDescriptorPoolSize>(mInfo.poolSizes);
            info.poolSizeCount = (uint)poolSizesMarshalled.Count;
            info.pPoolSizes = poolSizesMarshalled.Address;

            using (poolSizesMarshalled) {
                var result = Device.Commands.createDescriptorPool(Device.Native, ref info, Device.Instance.AllocationCallbacks, out descriptorPool);
                if (result != VkResult.Success) throw new DescriptorPoolException(result, string.Format("Error creating descriptor pool: {0}", result));
            }
        }

        public IList<DescriptorSet> Allocate(DescriptorSetAllocateInfo info) {
            if (info == null) throw new ArgumentNullException(nameof(info));
            if (info.setLayouts == null) throw new ArgumentNullException(nameof(info.setLayouts));

            unsafe {
                var infoNative = new Unmanaged.VkDescriptorSetAllocateInfo();
                infoNative.sType = VkStructureType.DescriptorSetAllocateInfo;
                infoNative.descriptorPool = descriptorPool;
                infoNative.descriptorSetCount = (uint)info.setLayouts.Count;

                var layoutsNative = stackalloc Unmanaged.VkDescriptorSetLayout[info.setLayouts.Count];
                Interop.Marshal<Unmanaged.VkDescriptorSetLayout, DescriptorSetLayout>(info.setLayouts, layoutsNative);
                infoNative.pSetLayouts = (IntPtr)layoutsNative;

                var resultsNative = stackalloc Unmanaged.VkDescriptorSet[info.setLayouts.Count];

                var result = Device.Commands.allocateDescriptorSets(Device.Native, ref infoNative, (IntPtr)resultsNative);
                if (result != VkResult.Success) throw new DescriptorPoolException(result, string.Format("Error allocating descriptor sets: {0}", result));

                var results = new List<DescriptorSet>(info.setLayouts.Count);

                for (int i = 0; i < info.setLayouts.Count; i++) {
                    results.Add(new DescriptorSet(Device, this, resultsNative[i], info.setLayouts[i]));
                    descriptorSets.Add(results[i]);
                }

                return results;
            }
        }

        public DescriptorSet Allocate(DescriptorSetLayout layout) {
            if (layout == null) throw new ArgumentNullException(nameof(layout));

            unsafe {
                var info = new Unmanaged.VkDescriptorSetAllocateInfo();
                info.sType = VkStructureType.DescriptorSetAllocateInfo;
                info.descriptorPool = descriptorPool;
                info.descriptorSetCount = 1;

                var layoutNative = layout.Native;
                info.pSetLayouts = (IntPtr)(&layoutNative);

                Unmanaged.VkDescriptorSet setNative;
                var result = Device.Commands.allocateDescriptorSets(Device.Native, ref info, (IntPtr)(&setNative));
                if (result != VkResult.Success) throw new DescriptorPoolException(result, string.Format("Error allocating descriptor set: {0}", result));

                var set =  new DescriptorSet(Device, this, setNative, layout);

                descriptorSets.Add(set);

                return set;
            }
        }

        public void Reset(VkDescriptorPoolResetFlags flags) {
            var result = Device.Commands.resetDescriptorPool(Device.Native, descriptorPool, flags);
            if (result != VkResult.Success) throw new DescriptorPoolException(result, string.Format("Error resetting descriptor pool: {0}", result));

            foreach (var descriptorSet in descriptorSets) {
                descriptorSet.CanDispose = false;
            }

            descriptorSets.Clear();
        }

        public void Free(IList<DescriptorSet> descriptorSets) {
            unsafe {
                var descriptorSetsNative = stackalloc Unmanaged.VkDescriptorSet[descriptorSets.Count];
                Interop.Marshal<Unmanaged.VkDescriptorSet, DescriptorSet>(descriptorSets, descriptorSetsNative);
                Device.Commands.freeDescriptorSets(Device.Native, descriptorPool, (uint)descriptorSets.Count, (IntPtr)descriptorSetsNative);
            }
        }

        public void Free(DescriptorSet descriptorSets) {
            unsafe {
                var descriptorSetNative = descriptorSets.Native;
                Device.Commands.freeDescriptorSets(Device.Native, descriptorPool, 1, (IntPtr)(&descriptorSetNative));
            }
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        void Dispose(bool disposing) {
            if (disposed) return;

            Device.Commands.destroyDescriptorPool(Device.Native, descriptorPool, Device.Instance.AllocationCallbacks);

            foreach (var descriptorSet in descriptorSets) {
                descriptorSet.CanDispose = false;
            }

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
