using System;
using System.Collections.Generic;

namespace CSGL.Vulkan {
    public class VkDescriptorSetLayoutBinding {
        public int binding;
        public VkDescriptorType descriptorType;
        public int descriptorCount;
        public VkShaderStageFlags stageFlags;
        public IList<VkSampler> immutableSamplers;
    }

    public class VkDescriptorSetLayoutCreateInfo {
        public IList<VkDescriptorSetLayoutBinding> bindings;
    }

    public class VkDescriptorSetLayout : IDisposable, INative<Unmanaged.VkDescriptorSetLayout> {
        Unmanaged.VkDescriptorSetLayout descriptorSetLayout;

        bool disposed;

        public VkDevice Device { get; private set; }
        public IList<VkDescriptorSetLayoutBinding> Bindings { get; private set; }

        public Unmanaged.VkDescriptorSetLayout Native {
            get {
                return descriptorSetLayout;
            }
        }

        public VkDescriptorSetLayout(VkDevice device, VkDescriptorSetLayoutCreateInfo info) {
            if (device == null) throw new ArgumentNullException(nameof(device));
            if (info == null) throw new ArgumentNullException(nameof(info));

            Device = device;

            CreateDescriptorSetLayout(info);

            Bindings = info.bindings.CloneReadOnly();
        }

        void CreateDescriptorSetLayout(VkDescriptorSetLayoutCreateInfo mInfo) {
            if (mInfo.bindings == null) throw new ArgumentNullException(nameof(mInfo.bindings));

            unsafe {
                var info = new Unmanaged.VkDescriptorSetLayoutCreateInfo();
                info.sType = VkStructureType.DescriptorSetLayoutCreateInfo;

                int totalSamplerCount = 0;
                for (int i = 0; i < mInfo.bindings.Count; i++) {
                    if (mInfo.bindings[i].immutableSamplers != null) {
                        totalSamplerCount += mInfo.bindings[i].descriptorCount;
                    }
                }

                var samplersNative = stackalloc Unmanaged.VkSampler[totalSamplerCount];
                int samplerIndex = 0;

                var bindingsNative = stackalloc Unmanaged.VkDescriptorSetLayoutBinding[mInfo.bindings.Count];

                for (int i = 0; i < mInfo.bindings.Count; i++) {
                    var binding = mInfo.bindings[i];
                    var bindingNative = new Unmanaged.VkDescriptorSetLayoutBinding();
                    bindingNative.binding = (uint)binding.binding;
                    bindingNative.descriptorCount = (uint)binding.descriptorCount;
                    bindingNative.descriptorType = binding.descriptorType;
                    bindingNative.stageFlags = binding.stageFlags;

                    if (binding.immutableSamplers != null) {
                        for (int j = 0; j < binding.immutableSamplers.Count; j++) {
                            samplersNative[samplerIndex + j] = binding.immutableSamplers[j].Native;
                        }
                        bindingNative.pImmutableSamplers = (IntPtr)(samplersNative + samplerIndex);
                        samplerIndex += binding.immutableSamplers.Count;
                    }

                    bindingsNative[i] = bindingNative;
                }

                info.bindingCount = (uint)mInfo.bindings.Count;
                info.pBindings = (IntPtr)bindingsNative;
                
                var result = Device.Commands.createDescriptorSetLayout(Device.Native, ref info, Device.Instance.AllocationCallbacks, out descriptorSetLayout);
                if (result != VkResult.Success) throw new DescriptorSetLayoutException(result, string.Format("Error creating description set layout: {0}", result));
            }
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        void Dispose(bool disposing) {
            if (disposed) return;

            Device.Commands.destroyDescriptorSetLayout(Device.Native, descriptorSetLayout, Device.Instance.AllocationCallbacks);

            disposed = true;
        }

        ~VkDescriptorSetLayout() {
            Dispose(false);
        }
    }

    public class DescriptorSetLayoutException : VulkanException {
        public DescriptorSetLayoutException(VkResult result, string message) : base(result, message) { }
    }
}
