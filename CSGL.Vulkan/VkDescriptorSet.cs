using System;
using System.Collections.Generic;

namespace CSGL.Vulkan {
    public class DescriptorSetAllocateInfo {
        public IList<VkDescriptorSetLayout> setLayouts;
    }

    public class DescriptorBufferInfo {
        public VkBuffer buffer;
        public ulong offset;
        public ulong range;
    }

    public class DescriptorImageInfo {
        public VkSampler sampler;
        public VkImageView imageView;
        public VkImageLayout imageLayout;
    }

    public class WriteDescriptorSet {
        public VkDescriptorSet dstSet;
        public uint dstBinding;
        public uint dstArrayElement;
        public VkDescriptorType descriptorType;
        public IList<DescriptorImageInfo> imageInfo;
        public IList<DescriptorBufferInfo> bufferInfo;
        public IList<VkBufferView> texelBufferView;
    }

    public class CopyDescriptorSet {
        public VkDescriptorSet srcSet;
        public uint srcBinding;
        public uint srcArrayElement;
        public VkDescriptorSet dstSet;
        public uint dstBinding;
        public uint dstArrayElement;
        public uint descriptorCount;
    }

    public class VkDescriptorSet : IDisposable, INative<Unmanaged.VkDescriptorSet> {
        Unmanaged.VkDescriptorSet descriptorSet;
        bool disposed;

        public VkDevice Device { get; private set; }
        public VkDescriptorPool Pool { get; private set; }
        public VkDescriptorSetLayout Layout { get; private set; }

        public Unmanaged.VkDescriptorSet Native {
            get {
                return descriptorSet;
            }
        }

        //set when pool is reset
        //prevents double free
        internal bool CanDispose { get; set; }

        internal VkDescriptorSet(VkDevice device, VkDescriptorPool pool, Unmanaged.VkDescriptorSet descriptorSet, VkDescriptorSetLayout setLayout) {
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

        ~VkDescriptorSet() {
            Dispose(false);
        }

        public static void Update(VkDevice device, IList<WriteDescriptorSet> writes, IList<CopyDescriptorSet> copies) {
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
                var bufferInfos = stackalloc Unmanaged.VkDescriptorBufferInfo[totalBuffers];
                var imageInfos = stackalloc Unmanaged.VkDescriptorImageInfo[totalImages];
                var bufferViews = stackalloc Unmanaged.VkBufferView[totalBufferViews];

                int bufferIndex = 0;
                int imageIndex = 0;
                int bufferViewIndex = 0;

                var writesNative = stackalloc Unmanaged.VkWriteDescriptorSet[writeCount];
                var copiesNative = stackalloc Unmanaged.VkCopyDescriptorSet[copyCount];

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
                            var bufferInfo = new Unmanaged.VkDescriptorBufferInfo();
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
                            var imageInfo = new Unmanaged.VkDescriptorImageInfo();
                            imageInfo.sampler = Unmanaged.VkSampler.Null;
                            imageInfo.imageView = Unmanaged.VkImageView.Null;

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

        public void Update(IList<WriteDescriptorSet> writes, IList<CopyDescriptorSet> copies) {
            Update(Device, writes, copies);
        }
    }
}
