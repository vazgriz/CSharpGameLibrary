using System;
using System.Collections.Generic;

namespace CSGL.Vulkan {
    public class SamplerCreateInfo {
        public VkFilter magFilter;
        public VkFilter minFilter;
        public VkSamplerMipmapMode mipmapMode;
        public VkSamplerAddressMode addressModeU;
        public VkSamplerAddressMode addressModeV;
        public VkSamplerAddressMode addressModeW;
        public float mipLodBias;
        public bool anisotropyEnable;
        public float maxAnisotropy;
        public bool compareEnable;
        public VkCompareOp compareOp;
        public float minLod;
        public float maxLod;
        public VkBorderColor borderColor;
        public bool unnormalizedCoordinates;
    }

    public class Sampler : IDisposable, INative<VkSampler> {
        VkSampler sampler;
        bool disposed;

        public VkSampler Native {
            get {
                return sampler;
            }
        }

        public Device Device { get; private set; }

        public Sampler(Device device, SamplerCreateInfo info) {
            if (device == null) throw new ArgumentNullException(nameof(device));

            Device = device;

            CreateSampler(info);
        }

        void CreateSampler(SamplerCreateInfo mInfo) {
            var info = new VkSamplerCreateInfo();
            info.sType = VkStructureType.SamplerCreateInfo;
            info.magFilter = mInfo.magFilter;
            info.minFilter = mInfo.minFilter;
            info.mipmapMode = mInfo.mipmapMode;
            info.addressModeU = mInfo.addressModeU;
            info.addressModeV = mInfo.addressModeV;
            info.addressModeW = mInfo.addressModeW;
            info.mipLodBias = mInfo.mipLodBias;
            info.anisotropyEnable = mInfo.anisotropyEnable ? 1u : 0u;
            info.maxAnisotropy = mInfo.maxAnisotropy;
            info.compareEnable = mInfo.compareEnable ? 1u : 0u;
            info.compareOp = mInfo.compareOp;
            info.minLod = mInfo.minLod;
            info.maxLod = mInfo.maxLod;
            info.borderColor = mInfo.borderColor;
            info.unnormalizedCoordinates = mInfo.unnormalizedCoordinates ? 1u : 0u;

            var result = Device.Commands.createSampler(Device.Native, ref info, Device.Instance.AllocationCallbacks, out sampler);
            if (result != VkResult.Success) throw new SamplerException(result, string.Format("Error creating sampler: {0}", result));
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        void Dispose(bool disposing) {
            if (disposed) return;

            Device.Commands.destroySampler(Device.Native, sampler, Device.Instance.AllocationCallbacks);

            disposed = true;
        }

        ~Sampler() {
            Dispose(false);
        }
    }

    public class SamplerException : VulkanException {
        public SamplerException(VkResult result, string message) : base(result, message) { }
    }
}
