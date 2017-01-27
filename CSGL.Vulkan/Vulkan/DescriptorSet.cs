using System;
using System.Collections.Generic;

namespace CSGL.Vulkan {
    public class DescriptorSetAllocateInfo {
        public uint descriptorSetCount;
        public List<DescriptorSetLayout> setLayouts;
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
        public VkDescriptorType descriptorType;
        public List<DescriptorImageInfo> imageInfo;
        public List<DescriptorBufferInfo> bufferInfo;
        public List<BufferView> texelBufferView;
    }

    public class DescriptorSet : INative<VkDescriptorSet> {
        VkDescriptorSet descriptorSet;

        public Device Device { get; private set; }
        public DescriptorPool Pool { get; private set; }

        public VkDescriptorSet Native {
            get {
                return descriptorSet;
            }
        }

        internal DescriptorSet(Device device, DescriptorPool pool, VkDescriptorSet descriptorSet) {
            Device = device;
            Pool = pool;
            this.descriptorSet = descriptorSet;
        }

        public static void Update(Device device, List<WriteDescriptorSet> writes) {
            if (device == null) throw new ArgumentNullException(nameof(device));

            int totalBuffers = 0;
            int totalImages = 0;
            int totalBufferViews = 0;

            for (int i = 0; i < writes.Count; i++) {
                var write = writes[i];

                if (write.bufferInfo != null) totalBuffers += write.bufferInfo.Count;
                if (write.imageInfo != null) totalImages += write.imageInfo.Count;
                if (write.texelBufferView != null) totalBufferViews += write.texelBufferView.Count;
            }

            unsafe
            {
                var bufferInfos = stackalloc VkDescriptorBufferInfo[totalBuffers];
                var imageInfos = stackalloc VkDescriptorImageInfo[totalImages];
                var bufferViews = stackalloc VkBufferView[totalBufferViews];

                int bufferIndex = 0;
                int imageIndex = 0;
                int bufferViewIndex = 0;

                var writesNative = stackalloc VkWriteDescriptorSet[writes.Count];

                for (int i = 0; i < writes.Count; i++) {
                    var mWrite = writes[i];

                    writesNative[i].sType = VkStructureType.WriteDescriptorSet;
                    writesNative[i].dstSet = mWrite.dstSet.Native;
                    writesNative[i].dstBinding = mWrite.dstBinding;
                    writesNative[i].dstArrayElement = mWrite.dstArrayElement;
                    writesNative[i].descriptorType = mWrite.descriptorType;

                    if (mWrite.bufferInfo != null) {
                        writesNative[i].descriptorCount = (uint)mWrite.bufferInfo.Count;
                        writesNative[i].pBufferInfo = (IntPtr)(&bufferInfos[bufferViewIndex]);

                        for (int j = 0; j < writesNative[i].descriptorCount; j++) {
                            VkDescriptorBufferInfo bufferInfo = new VkDescriptorBufferInfo();
                            bufferInfo.buffer = mWrite.bufferInfo[j].buffer.Native;
                            bufferInfo.offset = mWrite.bufferInfo[j].offset;
                            bufferInfo.range = mWrite.bufferInfo[j].range;

                            bufferInfos[bufferIndex] = bufferInfo;
                            bufferIndex++;
                        }
                    } else if (mWrite.imageInfo != null) {
                        writesNative[i].descriptorCount = (uint)mWrite.imageInfo.Count;
                        writesNative[i].pImageInfo = (IntPtr)(&imageInfos[imageIndex]);

                        for (int j = 0; j < writesNative[i].descriptorCount; j++) {
                            VkDescriptorImageInfo imageInfo = new VkDescriptorImageInfo();
                            imageInfo.sampler = mWrite.imageInfo[j].sampler.Native;
                            imageInfo.imageView = mWrite.imageInfo[j].imageView.Native;
                            imageInfo.imageLayout = mWrite.imageInfo[j].imageLayout;

                            imageInfos[imageIndex] = imageInfo;
                            imageIndex++;
                        }
                    } else if (mWrite.texelBufferView != null) {
                        writesNative[i].descriptorCount = (uint)mWrite.texelBufferView.Count;
                        writesNative[i].pTexelBufferView = (IntPtr)(&bufferViews[bufferViewIndex]);

                        for (int j = 0; j < writesNative[i].descriptorCount; j++) {
                            bufferViews[j] = mWrite.texelBufferView[j].Native;
                            bufferViewIndex++;
                        }
                    }
                }

                device.Commands.updateDescriptorSets(device.Native, (uint)writes.Count, (IntPtr)writesNative, 0, IntPtr.Zero);
            }
        }
    }
}
