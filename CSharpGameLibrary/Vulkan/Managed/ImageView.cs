using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace CSGL.Vulkan.Managed {
    public class ImageViewCreateInfo {
        public Image Image { get; set; }
        public VkImageViewType ViewType { get; set; }
        public VkFormat Format { get; set; }
        public VkComponentMapping Components { get; set; }
        public VkImageSubresourceRange SubresourceRange { get; set; }

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

                IntPtr infoPtr = Marshal.AllocHGlobal(Marshal.SizeOf<VkImageViewCreateInfo>());
                Marshal.StructureToPtr(info, infoPtr, false);

                IntPtr imageViewPtr = Marshal.AllocHGlobal(Marshal.SizeOf<VkImageView>());

                try {
                    var result = Device.Commands.createImageView(Device.Native, infoPtr, Device.Instance.AllocationCallbacks, imageViewPtr);
                    if (result != VkResult.Success) throw new ImageViewException(string.Format("Error creating image view: {0}", result));

                    imageView = Marshal.PtrToStructure<VkImageView>(imageViewPtr);
                }
                finally {
                    Marshal.DestroyStructure<VkImageViewCreateInfo>(infoPtr);
                    Marshal.DestroyStructure<VkImageView>(imageViewPtr);

                    Marshal.FreeHGlobal(infoPtr);
                    Marshal.FreeHGlobal(imageViewPtr);
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
