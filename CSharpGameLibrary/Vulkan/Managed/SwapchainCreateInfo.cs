using System;

namespace CSGL.Vulkan.Managed {
    public class SwapchainCreateInfo : IDisposable {
        Surface surface;
        uint minImageCount;
        VkFormat imageFormat;
        VkColorSpaceKHR colorSpace;
        VkExtent2D imageExtent;
        uint imageArrayLayers;
        VkImageUsageFlags imageUsageFlags;
        VkSharingMode imageSharingMode;
        uint[] queueFamilyIndices;
        VkSurfaceTransformFlagsKHR preTransform;
        VkCompositeAlphaFlagsKHR compositeAlpha;
        VkPresentModeKHR presentMode;
        bool clipped;
        Swapchain oldSwapchain;

        bool disposed = false;
        bool dirty;

        Marshalled<VkSwapchainCreateInfoKHR> marshalled;
        MarshalledArray<uint> indicesMarshalled;

        public Surface Surface {
            get {
                return surface;
            }
            set {
                surface = value;
                dirty = true;
            }
        }

        public uint MinImageCount {
            get {
                return minImageCount;
            }
            set {
                minImageCount = value;
                dirty = true;
            }
        }

        public VkFormat ImageFormat {
            get {
                return imageFormat;
            }
            set {
                imageFormat = value;
                dirty = true;
            }
        }

        public VkColorSpaceKHR ColorSpace {
            get {
                return colorSpace;
            }
            set {
                colorSpace = value;
                dirty = true;
            }
        }

        public VkExtent2D ImageExtent {
            get {
                return imageExtent;
            }
            set {
                imageExtent = value;
                dirty = true;
            }
        }

        public uint ImageArrayLayers {
            get {
                return ImageArrayLayers;
            }
            set {
                imageArrayLayers = value;
                dirty = true;
            }
        }

        public VkImageUsageFlags ImageUsageFlags {
            get {
                return ImageUsageFlags;
            }
            set {
                imageUsageFlags = value;
                dirty = true;
            }
        }

        public VkSharingMode ImageSharingMode {
            get {
                return imageSharingMode;
            }
            set {
                imageSharingMode = value;
                dirty = true;
            }
        }

        public uint[] QueueFamilyIndices {
            get {
                return queueFamilyIndices;
            }
            set {
                queueFamilyIndices = value;
                dirty = true;
            }
        }

        public VkSurfaceTransformFlagsKHR PreTransform {
            get {
                return preTransform;
            }
            set {
                preTransform = value;
                dirty = true;
            }
        }

        public VkCompositeAlphaFlagsKHR CompositeAlpha {
            get {
                return compositeAlpha;
            }
            set {
                compositeAlpha = value;
                dirty = true;
            }
        }

        public VkPresentModeKHR PresentMode {
            get {
                return presentMode;
            }
            set {
                presentMode = value;
                dirty = true;
            }
        }

        public bool Clipped {
            get {
                return clipped;
            }
            set {
                clipped = value;
                dirty = true;
            }
        }

        public Swapchain OldSwapchain {
            get {
                return oldSwapchain;
            }
            set {
                oldSwapchain = value;
                dirty = true;
            }
        }

        public Marshalled<VkSwapchainCreateInfoKHR> Marshalled {
            get {
                if (dirty) {
                    Apply();
                }
                return marshalled;
            }
        }


        public SwapchainCreateInfo(Surface surface, Swapchain old) {
            Surface = surface;
            OldSwapchain = old;

            marshalled = new Marshalled<VkSwapchainCreateInfoKHR>();
            Apply();
        }

        public void Apply() {
            indicesMarshalled?.Dispose();
            indicesMarshalled = new MarshalledArray<uint>(queueFamilyIndices);

            marshalled.Value = GetNative();
        }

        VkSwapchainCreateInfoKHR GetNative() {
            VkSwapchainCreateInfoKHR info = new VkSwapchainCreateInfoKHR();
            info.sType = VkStructureType.StructureTypeSwapchainCreateInfoKhr;
            info.surface = surface.Native;
            info.minImageCount = minImageCount;
            info.imageFormat = imageFormat;
            info.imageColorSpace = colorSpace;
            info.imageExtent = imageExtent;
            info.imageArrayLayers = imageArrayLayers;
            info.imageUsage = imageUsageFlags;
            info.imageSharingMode = imageSharingMode;
            info.queueFamilyIndexCount = (uint)indicesMarshalled.Count;
            info.pQueueFamilyIndices = indicesMarshalled.Address;
            info.preTransform = preTransform;
            info.compositeAlpha = compositeAlpha;
            info.presentMode= presentMode;
            info.clipped = clipped ? (uint)1 : (uint)0;
            if (oldSwapchain != null) {
                info.oldSwapchain = oldSwapchain.Native;
            }

            return info;
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        void Dispose(bool disposing) {
            if (disposed) return;

            marshalled.Dispose();
            indicesMarshalled?.Dispose();

            if (disposing) {
                marshalled = null;
                indicesMarshalled = null;

                surface = null;
                queueFamilyIndices = null;
            }

            disposed = true;
        }

        ~SwapchainCreateInfo() {
            Dispose(false);
        }
    }
}
