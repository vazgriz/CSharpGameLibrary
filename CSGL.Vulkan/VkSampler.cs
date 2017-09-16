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

    public class VkSampler : IDisposable, INative<Unmanaged.VkSampler> {
        Unmanaged.VkSampler sampler;
        bool disposed;

        public Unmanaged.VkSampler Native {
            get {
                return sampler;
            }
        }

        public VkDevice Device { get; private set; }
        public VkFilter MagFilter { get; private set; }
        public VkFilter MinFilter { get; private set; }
        public VkSamplerMipmapMode MipmapMode { get; private set; }
        public VkSamplerAddressMode AddressModeU { get; private set; }
        public VkSamplerAddressMode AddressModeV { get; private set; }
        public VkSamplerAddressMode AddressModeW { get; private set; }
        public float MipLodBias { get; private set; }
        public bool AnisotropyEnable { get; private set; }
        public float MaxAnisotropy { get; private set; }
        public bool CompareEnable { get; private set; }
        public VkCompareOp CompareOp { get; private set; }
        public float MinLod { get; private set; }
        public float MaxLod { get; private set; }
        public VkBorderColor BorderColor { get; private set; }
        public bool UnnormalizedCoordinates { get; private set; }

        public VkSampler(VkDevice device, SamplerCreateInfo info) {
            if (device == null) throw new ArgumentNullException(nameof(device));
            if (info == null) throw new ArgumentNullException(nameof(info));

            Device = device;

            CreateSampler(info);

            MagFilter = info.magFilter;
            MinFilter = info.minFilter;
            MipmapMode = info.mipmapMode;
            AddressModeU = info.addressModeU;
            AddressModeV = info.addressModeV;
            AddressModeW = info.addressModeW;
            MipLodBias = info.mipLodBias;
            AnisotropyEnable = info.anisotropyEnable;
            MaxAnisotropy = info.maxAnisotropy;
            CompareEnable = info.compareEnable;
            CompareOp = info.compareOp;
            MinLod = info.minLod;
            MaxLod = info.maxLod;
            BorderColor = info.borderColor;
            UnnormalizedCoordinates = info.unnormalizedCoordinates;
        }

        void CreateSampler(SamplerCreateInfo mInfo) {
            var info = new Unmanaged.VkSamplerCreateInfo();
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

        ~VkSampler() {
            Dispose(false);
        }
    }

    public class SamplerException : VulkanException {
        public SamplerException(VkResult result, string message) : base(result, message) { }
    }
}
