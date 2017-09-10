using System;

namespace CSGL.Vulkan {
    public class ImageViewCreateInfo {
        public Image image;
        public VkImageViewType viewType;
        public VkFormat format;
        public VkComponentMapping components;
        public VkImageSubresourceRange subresourceRange;
    }

    public class ImageView : IDisposable, INative<VkImageView> {
        VkImageView imageView;
        bool disposed = false;

        public VkImageView Native {
            get {
                return imageView;
            }
        }

        public Device Device { get; private set; }
        public Image Image { get; private set; }
        public VkImageViewType ViewType { get; private set; }
        public VkFormat Format { get; private set; }
        public VkComponentMapping Components { get; private set; }
        public VkImageSubresourceRange SubresourceRange { get; private set; }

        public ImageView(Device device, ImageViewCreateInfo info) {
            if (device == null) throw new ArgumentNullException(nameof(device));
            if (info == null) throw new ArgumentNullException(nameof(info));
            if (info.image == null) throw new ArgumentNullException(nameof(info.image));

            Device = device;

            CreateImageView(info);

            Image = info.image;
            ViewType = info.viewType;
            Format = info.format;
            Components = info.components;
            SubresourceRange = info.subresourceRange;
        }

        void CreateImageView(ImageViewCreateInfo mInfo) {
            VkImageViewCreateInfo info = new VkImageViewCreateInfo();
            info.sType = VkStructureType.ImageViewCreateInfo;
            info.image = mInfo.image.Native;
            info.viewType = mInfo.viewType;
            info.format = mInfo.format;
            info.components = mInfo.components;
            info.subresourceRange = mInfo.subresourceRange;

            var result = Device.Commands.createImageView(Device.Native, ref info, Device.Instance.AllocationCallbacks, out imageView);
            if (result != VkResult.Success) throw new ImageViewException(result, string.Format("Error creating image view: {0}", result));
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        void Dispose(bool disposing) {
            if (disposed) return;

            Device.Commands.destroyImageView(Device.Native, imageView, Device.Instance.AllocationCallbacks);

            disposed = true;
        }

        ~ImageView() {
            Dispose(true);
        }
    }

    public class ImageViewException : VulkanException {
        public ImageViewException(VkResult result, string message) : base(result, message) { }
    }
}
