using System;
using System.Collections.Generic;

namespace CSGL.Vulkan {
    public class DescriptorSetAllocateInfo {
        public IList<DescriptorSetLayout> setLayouts;
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

    public class CopyDescriptorSet {
        public DescriptorSet srcSet;
        public uint srcBinding;
        public uint srcArrayElement;
        public DescriptorSet dstSet;
        public uint dstBinding;
        public uint dstArrayElement;
        public uint descriptorCount;
    }

    public class DescriptorSet : IDisposable, INative<VkDescriptorSet> {
        VkDescriptorSet descriptorSet;
        bool disposed;

        public Device Device { get; private set; }
        public DescriptorPool Pool { get; private set; }
        public DescriptorSetLayout Layout { get; private set; }

        public VkDescriptorSet Native {
            get {
                return descriptorSet;
            }
        }

        //set when pool is reset
        //prevents double free
        internal bool CanDispose { get; set; }

        internal DescriptorSet(Device device, DescriptorPool pool, VkDescriptorSet descriptorSet, DescriptorSetLayout setLayout) {
            Device = device;
            Pool = pool;
            this.descriptorSet = descriptorSet;
            Layout = setLayout;
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        void Dispose(bool disposing) {
            if (disposed) return;

            if (CanDispose) {
                Pool.Free(this);
            }

            disposed = true;
        }

        ~DescriptorSet() {
            Dispose(false);
        }

        public static void Update(Device device, IList<WriteDescriptorSet> writes, IList<CopyDescriptorSet> copies) {
            if (device == null) throw new ArgumentNullException(nameof(device));

            int copyCount = 0;
            int writeCount = 0;

            int totalBuffers = 0;
            int totalImages = 0;
            int totalBufferViews = 0;

            if (writes != null) {
                writeCount = writes.Count;
                for (int i = 0; i < writeCount; i++) {
                    var write = writes[i];

                    if (write.bufferInfo != null) totalBuffers += write.bufferInfo.Count;
                    if (write.imageInfo != null) totalImages += write.imageInfo.Count;
                    if (write.texelBufferView != null) totalBufferViews += write.texelBufferView.Count;
                }
            }

            if (copies != null) {
                copyCount = copies.Count;
            }

            unsafe {
                var bufferInfos = stackalloc VkDescriptorBufferInfo[totalBuffers];
                var imageInfos = stackalloc VkDescriptorImageInfo[totalImages];
                var bufferViews = stackalloc VkBufferView[totalBufferViews];

                int bufferIndex = 0;
                int imageIndex = 0;
                int bufferViewIndex = 0;

                var writesNative = stackalloc VkWriteDescriptorSet[writeCount];
                var copiesNative = stackalloc VkCopyDescriptorSet[copyCount];

                for (int i = 0; i < writeCount; i++) {
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
                            imageInfo.sampler = VkSampler.Null;
                            imageInfo.imageView = VkImageView.Null;

                            if (mWrite.imageInfo[j].sampler != null) {
                                imageInfo.sampler = mWrite.imageInfo[j].sampler.Native;
                            }

                            if (mWrite.imageInfo[j].imageView != null) {
                                imageInfo.imageView = mWrite.imageInfo[j].imageView.Native;
                            }

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

                for (int i = 0; i < copyCount; i++) {
                    var mCopy = copies[i];

                    copiesNative[i].sType = VkStructureType.CopyDescriptorSet;
                    copiesNative[i].srcSet = mCopy.srcSet.Native;
                    copiesNative[i].srcBinding = mCopy.srcBinding;
                    copiesNative[i].srcArrayElement = mCopy.srcArrayElement;
                    copiesNative[i].dstSet = mCopy.dstSet.Native;
                    copiesNative[i].dstBinding = mCopy.dstBinding;
                    copiesNative[i].dstArrayElement = mCopy.dstArrayElement;
                    copiesNative[i].descriptorCount = mCopy.descriptorCount;
                }

                device.Commands.updateDescriptorSets(device.Native, (uint)writeCount, (IntPtr)writesNative, (uint)copyCount, (IntPtr)copiesNative);
            }
        }

        public void Update(List<WriteDescriptorSet> writes, List<CopyDescriptorSet> copies) {
            Update(Device, writes, copies);
        }
    }
}
