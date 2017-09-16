using System;

namespace CSGL.Vulkan {
    public class VkImageViewCreateInfo {
        public VkImage image;
        public VkImageViewType viewType;
        public VkFormat format;
        public Unmanaged.VkComponentMapping components;
        public Unmanaged.VkImageSubresourceRange subresourceRange;
    }

    public class VkImageView : IDisposable, INative<Unmanaged.VkImageView> {
        Unmanaged.VkImageView imageView;
        bool disposed = false;

        public Unmanaged.VkImageView Native {
            get {
                return imageView;
            }
        }

        public VkDevice Device { get; private set; }
        public VkImage Image { get; private set; }
        public VkImageViewType ViewType { get; private set; }
        public VkFormat Format { get; private set; }
        public Unmanaged.VkComponentMapping Components { get; private set; }
        public Unmanaged.VkImageSubresourceRange SubresourceRange { get; private set; }

        public VkImageView(VkDevice device, VkImageViewCreateInfo info) {
            if (device == null) throw new ArgumentNullException(nameof(device));
            if (info == null) throw new ArgumentNullException(nameof(info));

            Device = device;

            CreateImageView(info);

            Image = info.image;
            ViewType = info.viewType;
            Format = info.format;
            Components = info.components;
            SubresourceRange = info.subresourceRange;
        }

        void CreateImageView(VkImageViewCreateInfo mInfo) {
            if (mInfo.image == null) throw new ArgumentNullException(nameof(mInfo.image));

            var info = new Unmanaged.VkImageViewCreateInfo();
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

        ~VkImageView() {
            Dispose(true);
        }
    }

    public class ImageViewException : VulkanException {
        public ImageViewException(VkResult result, string message) : base(result, message) { }
    }
}
