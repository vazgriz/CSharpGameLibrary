﻿using System;
using System.Collections.Generic;

namespace CSGL.Vulkan {
    public class ImageCreateInfo {
        public VkImageCreateFlags flags;
        public VkImageType imageType;
        public VkFormat format;
        public VkExtent3D extent;
        public uint mipLevels;
        public uint arrayLayers;
        public VkSampleCountFlags samples;
        public VkImageTiling tiling;
        public VkImageUsageFlags usage;
        public VkSharingMode sharingMode;
        public List<uint> queueFamilyIndices;
        public VkImageLayout initialLayout;
    }

    public class Image : IDisposable, INative<VkImage> {
        VkImage image;
        bool disposed = false;

        VkMemoryRequirements requirements;

        public Device Device { get; private set; }

        public VkImage Native {
            get {
                return image;
            }
        }

        public VkMemoryRequirements Requirements {
            get {
                return requirements;
            }
        }

        public VkImageCreateFlags Flags { get; private set; }
        public VkImageType ImageType { get; private set; }
        public VkFormat Format { get; private set; }
        public VkExtent3D Extent { get; private set; }
        public uint MipLevels { get; private set; }
        public uint ArrayLayers { get; private set; }
        public VkSampleCountFlags Samples { get; private set; }
        public VkImageTiling Tiling { get; private set; }
        public VkImageUsageFlags Usage { get; private set; }

        internal Image(Device device, VkImage image, VkFormat format) { //for images that are implicitly created, eg a swapchains's images
            Device = device;
            this.image = image;
            Format = format;
        }

        public Image(Device device, ImageCreateInfo info) {
            if (device == null) throw new ArgumentNullException(nameof(device));
            Device = device;

            CreateImage(info);

            Device.Commands.getImageMemoryRequirements(Device.Native, image, out requirements);
        }

        void CreateImage(ImageCreateInfo mInfo) {
            var info = new VkImageCreateInfo();
            info.sType = VkStructureType.ImageCreateInfo;
            info.flags = mInfo.flags;
            info.imageType = mInfo.imageType;
            info.format = mInfo.format;
            info.extent = mInfo.extent;
            info.mipLevels = mInfo.mipLevels;
            info.arrayLayers = mInfo.arrayLayers;
            info.samples = mInfo.samples;
            info.tiling = mInfo.tiling;
            info.usage = mInfo.usage;
            info.sharingMode = mInfo.sharingMode;

            var indicesMarshalled = new NativeArray<uint>(mInfo.queueFamilyIndices);
            info.queueFamilyIndexCount = (uint)indicesMarshalled.Count;
            info.pQueueFamilyIndices = indicesMarshalled.Address;
            info.initialLayout = mInfo.initialLayout;

            using (indicesMarshalled) {
                var result = Device.Commands.createImage(Device.Native, ref info, Device.Instance.AllocationCallbacks, out image);
                if (result != VkResult.Success) throw new ImageException(string.Format("Error creating image: {0}", result));
            }

            Flags = mInfo.flags;
            ImageType = mInfo.imageType;
            Format = mInfo.format;
            Extent = mInfo.extent;
            MipLevels = mInfo.mipLevels;
            ArrayLayers = mInfo.arrayLayers;
            Samples = mInfo.samples;
            Tiling = mInfo.tiling;
            Usage = mInfo.usage;
        }

        public void Bind(DeviceMemory memory, ulong offset) {
            Device.Commands.bindImageMemory(Device.Native, image, memory.Native, offset);
        }

        public void Dispose() {
            if (disposed) return;

            Device.Commands.destroyImage(Device.Native, image, Device.Instance.AllocationCallbacks);

            disposed = true;
        }
    }

    public class ImageException : Exception {
        public ImageException(string message) : base(message) { }
    }
}
