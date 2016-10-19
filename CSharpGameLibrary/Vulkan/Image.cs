using System;
using System.Collections.Generic;

namespace CSGL.Vulkan {
    public class ImageCreateInfo {

    }

    public class Image : IDisposable {
        VkImage image;
        bool disposed = false;

        public Device Device { get; private set; }

        public VkImage Native {
            get {
                return image;
            }
        }

        internal Image(Device device, VkImage image) { //for images that are implicitly created, eg a swapchains's images
            Device = device;
            this.image = image;
        }

        //public Image(Device device, ImageCreateInfo info) {
        //    Device = device;
        //}

        public void Dispose() {
            if (disposed) return;

            disposed = true;
        }
    }
}
