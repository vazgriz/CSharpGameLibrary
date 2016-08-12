using System;
using System.Collections.Generic;

namespace CSGL.Vulkan.Managed {
    public class ImageViewCreateInfo {
        public Image Image;
        public VkImageViewType ViewType;
        public VkFormat Format;
        public VkComponentMapping Components;
        public VkImageSubresourceRange SubresourceRange;

        public ImageViewCreateInfo(Image image) {
            Image = image;
        }
    }

    public class ImageView : IDisposable {
        VkImageView imageView;
        bool disposed = false;

        public Device Device { get; private set; }

        public ImageView(Device device, ImageViewCreateInfo info) {
            if (device == null) throw new ArgumentNullException(nameof(device));
            if (info == null) throw new ArgumentNullException(nameof(info));
            if (info.Image == null) throw new ArgumentNullException(nameof(info.Image));

            Device = device;

            CreateImageView(info);
        }

        void CreateImageView(ImageViewCreateInfo mInfo) {
            unsafe
            {
                VkImageViewCreateInfo info = new VkImageViewCreateInfo();
                info.sType = VkStructureType.StructureTypeImageViewCreateInfo;
                info.image = mInfo.Image.Native;
                info.viewType = mInfo.ViewType;
                info.format = mInfo.Format;
                info.components = mInfo.Components;
                info.subresourceRange = mInfo.SubresourceRange;

                fixed (VkImageView* temp = &imageView) {
                    var result = Device.Commands.createImageView(Device.Native, &info, Device.Instance.AllocationCallbacks, temp);
                    if (result != VkResult.Success) throw new ImageViewException(string.Format("Error creating image view: {0}", result));
                }
            }
        }

        public void Dispose() {
            if (disposed) return;

            unsafe
            {
                Device.Commands.destroyImageView(Device.Native, imageView, Device.Instance.AllocationCallbacks);
            }

            disposed = true;
        }
    }

    public class ImageViewException : Exception {
        public ImageViewException(string message) : base(message) { }
    }
}
