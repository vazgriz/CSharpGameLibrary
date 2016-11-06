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
        public uint anisotropyEnable;
        public float maxAnisotropy;
        public uint compareEnable;
        public VkCompareOp compareOp;
        public float minLod;
        public float maxLod;
        public VkBorderColor borderColor;
        public uint unnormalizedCoordinates;
    }

    public class Sampler : INative<VkSampler> {
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
        }

        void CreateSampler(SamplerCreateInfo mInfo) {
            var info = new VkSamplerCreateInfo();
            info.sType = VkStructureType.StructureTypeSamplerCreateInfo;
            info.magFilter = mInfo.magFilter;
            info.minFilter = mInfo.minFilter;
            info.mipmapMode = mInfo.mipmapMode;
            info.addressModeU = mInfo.addressModeU;
            info.addressModeV = mInfo.addressModeV;
            info.addressModeW = mInfo.addressModeW;
            info.mipLodBias = mInfo.mipLodBias;
            info.anisotropyEnable = mInfo.anisotropyEnable;
            info.compareEnable = mInfo.compareEnable;
            info.compareOp = mInfo.compareOp;
            info.minLod = mInfo.minLod;
            info.maxLod = mInfo.maxLod;
            info.borderColor = mInfo.borderColor;
            info.unnormalizedCoordinates = mInfo.unnormalizedCoordinates;

            var result = Device.Commands.createSampler(Device.Native, ref info, Device.Instance.AllocationCallbacks, out sampler);
            if (result != VkResult.Success) throw new SamplerException(string.Format("Error creating sampler: {0}", result));
        }
    }

    public class SamplerException : Exception {
        public SamplerException(string message) : base(message) { }
    }
}
