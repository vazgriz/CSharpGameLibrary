using System;

namespace CSGL.Vulkan {
    public class DescriptorPoolCreateInfo {
        public VkDescriptorPoolCreateFlags flags;
        public uint maxSets;
        public VkDescriptorPoolSize[] poolSizes;
    }

    public class DescriptorBufferInfo {
        public Buffer buffer;
        public ulong offset;
        public ulong range;
    }

    public class WriteDescriptorSet {
        public DescriptorSet dstSet;
        public uint dstBinding;
        public uint dstArrayElement;
        public uint descriptorCount;
        public VkDescriptorType descriptorType;
        public IntPtr pImageInfo;
        public DescriptorBufferInfo bufferInfo;
        public IntPtr pTexelBufferView;
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

        public DescriptorPool(Device device, DescriptorPoolCreateInfo info) {
            if (device == null) throw new ArgumentNullException(nameof(device));
            if (info == null) throw new ArgumentNullException(nameof(info));

            Device = device;

            CreateDescriptorPool(info);
        }

        void CreateDescriptorPool(DescriptorPoolCreateInfo mInfo) {
            var info = new VkDescriptorPoolCreateInfo();
            info.sType = VkStructureType.StructureTypeDescriptorPoolCreateInfo;
            info.flags = mInfo.flags;
            info.maxSets = mInfo.maxSets;

            var poolSizesMarshalled = new PinnedArray<VkDescriptorPoolSize>(mInfo.poolSizes);
            info.poolSizeCount = (uint)poolSizesMarshalled.Length;
            info.pPoolSizes = poolSizesMarshalled.Address;

            try {
                var result = Device.Commands.createDescriptorPool(Device.Native, ref info, Device.Instance.AllocationCallbacks, out descriptorPool);
                if (result != VkResult.Success) throw new DescriptorPoolException(string.Format("Error creating descriptor pool: {0}", result));
            }
            finally {
                poolSizesMarshalled.Dispose();
            }
        }

        public DescriptorSet[] Allocate(DescriptorSetAllocateInfo info) {
            var infoNative = new VkDescriptorSetAllocateInfo();
            infoNative.sType = VkStructureType.StructureTypeDescriptorSetAllocateInfo;
            infoNative.descriptorPool = descriptorPool;
            infoNative.descriptorSetCount = info.descriptorSetCount;

            var layoutsMarshalled = new MarshalledArray<VkDescriptorSetLayout>(info.setLayouts);
            infoNative.pSetLayouts = layoutsMarshalled.Address;

            var descriptorSetsMarshalled = new MarshalledArray<VkDescriptorSet>((int)info.descriptorSetCount);

            var result = Device.Commands.allocateDescriptorSets(Device.Native, ref infoNative, descriptorSetsMarshalled.Address);
            if (result != VkResult.Success) throw new DescriptorPoolException(string.Format("Error allocating descriptor sets: {0}", result));

            layoutsMarshalled.Dispose();
            descriptorSetsMarshalled.Dispose();

            var results = new DescriptorSet[(int)info.descriptorSetCount];

            for (int i = 0; i < info.descriptorSetCount; i++) {
                results[i] = new DescriptorSet(Device, this, descriptorSetsMarshalled[i]);
            }

            return results;
        }

        public void Update(WriteDescriptorSet[] writes) {
            using (var writesMarshalled = new MarshalledArray<VkWriteDescriptorSet>(writes.Length)) {
                var disposables = new IDisposable[writes.Length];
                
                for (int i = 0; i < writes.Length; i++) {
                    var write = writes[i];

                    var writeNative = new VkWriteDescriptorSet();
                    writeNative.sType = VkStructureType.StructureTypeWriteDescriptorSet;
                    writeNative.dstSet = write.dstSet.Native;
                    writeNative.dstBinding = write.dstBinding;
                    writeNative.dstArrayElement = write.dstArrayElement;
                    writeNative.descriptorCount = write.descriptorCount;
                    writeNative.descriptorType = write.descriptorType;
                    writeNative.pImageInfo = write.pImageInfo;

                    var bufferInfo = new VkDescriptorBufferInfo();
                    bufferInfo.buffer = write.bufferInfo.buffer.Native;
                    bufferInfo.offset = write.bufferInfo.offset;
                    bufferInfo.range = write.bufferInfo.range;

                    var bufferInfoMarshalled = new Marshalled<VkDescriptorBufferInfo>(bufferInfo);
                    disposables[i] = bufferInfoMarshalled;
                    writeNative.pBufferInfo = bufferInfoMarshalled.Address;

                    writeNative.pTexelBufferView = write.pTexelBufferView;

                    writesMarshalled[i] = writeNative;
                }

                Device.Commands.updateDescriptorSets(Device.Native, (uint)writes.Length, writesMarshalled.Address, 0, IntPtr.Zero);

                for (int i = 0; i < disposables.Length; i++) {
                    disposables[i]?.Dispose();
                }
            }
        }

        public void Dispose() {
            if (disposed) return;

            Device.Commands.destroyDescriptorPool(Device.Native, descriptorPool, Device.Instance.AllocationCallbacks);

            disposed = true;
        }
    }

    public class DescriptorPoolException : Exception {
        public DescriptorPoolException(string message) : base(message) { }
    }
}
