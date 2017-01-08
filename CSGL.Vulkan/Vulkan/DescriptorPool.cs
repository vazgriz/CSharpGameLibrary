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

    public class DescriptorImageInfo {
        public Sampler sampler;
        public ImageView imageView;
        public VkImageLayout imageLayout;
    }

    public class WriteDescriptorSet {
        public DescriptorSet dstSet;
        public uint dstBinding;
        public uint dstArrayElement;
        public uint descriptorCount;
        public VkDescriptorType descriptorType;
        public DescriptorImageInfo imageInfo;
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
            info.sType = VkStructureType.DescriptorPoolCreateInfo;
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
            infoNative.sType = VkStructureType.DescriptorSetAllocateInfo;
            infoNative.descriptorPool = descriptorPool;
            infoNative.descriptorSetCount = info.descriptorSetCount;

            var layoutsMarshalled = new NativeArray<VkDescriptorSetLayout>(info.setLayouts);
            infoNative.pSetLayouts = layoutsMarshalled.Address;

            var descriptorSetsMarshalled = new NativeArray<VkDescriptorSet>((int)info.descriptorSetCount);

            var result = Device.Commands.allocateDescriptorSets(Device.Native, ref infoNative, descriptorSetsMarshalled.Address);
            if (result != VkResult.Success) throw new DescriptorPoolException(string.Format("Error allocating descriptor sets: {0}", result));


            var results = new DescriptorSet[(int)info.descriptorSetCount];

            for (int i = 0; i < info.descriptorSetCount; i++) {
                results[i] = new DescriptorSet(Device, this, descriptorSetsMarshalled[i]);
            }

            layoutsMarshalled.Dispose();
            descriptorSetsMarshalled.Dispose();

            return results;
        }

        public void Update(WriteDescriptorSet[] writes) {
            using (var writesMarshalled = new MarshalledArray<VkWriteDescriptorSet>(writes.Length)) {
                var disposables = new IDisposable[writes.Length * 2];
                
                for (int i = 0; i < writes.Length; i++) {
                    var write = writes[i];

                    var writeNative = new VkWriteDescriptorSet();
                    writeNative.sType = VkStructureType.WriteDescriptorSet;
                    writeNative.dstSet = write.dstSet.Native;
                    writeNative.dstBinding = write.dstBinding;
                    writeNative.dstArrayElement = write.dstArrayElement;
                    writeNative.descriptorCount = write.descriptorCount;
                    writeNative.descriptorType = write.descriptorType;

                    if (write.imageInfo != null) {
                        var imageInfo = new VkDescriptorImageInfo();
                        imageInfo.sampler = write.imageInfo.sampler.Native;
                        imageInfo.imageView = write.imageInfo.imageView.Native;
                        imageInfo.imageLayout = write.imageInfo.imageLayout;

                        var imageInfoMarshalled = new Marshalled<VkDescriptorImageInfo>(imageInfo);
                        disposables[i * 2] = imageInfoMarshalled;
                        writeNative.pImageInfo = imageInfoMarshalled.Address;
                    }

                    if (write.bufferInfo != null) {
                        var bufferInfo = new VkDescriptorBufferInfo();
                        bufferInfo.buffer = write.bufferInfo.buffer.Native;
                        bufferInfo.offset = write.bufferInfo.offset;
                        bufferInfo.range = write.bufferInfo.range;

                        var bufferInfoMarshalled = new Marshalled<VkDescriptorBufferInfo>(bufferInfo);
                        disposables[i * 2 + 1] = bufferInfoMarshalled;
                        writeNative.pBufferInfo = bufferInfoMarshalled.Address;
                    }

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
