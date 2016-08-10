using System;
using System.Collections.Generic;

using CSGL;
using CSGL.Vulkan.Unmanaged;

namespace CSGL.Vulkan.Managed {
    public class SwapchainCreateInfo {
        public Surface Surface;
        public uint MinImageCount;
        public VkFormat ImageFormat;
        public VkColorSpaceKHR ColorSpace;
        public VkExtent2D ImageExtent;
        public uint ImageArrayLayers;
        public VkImageUsageFlags ImageUsageFlags;
        public VkSharingMode ImageSharingMode;
        public List<uint> QueueFamilyIndices;
        public VkSurfaceTransformFlags PreTransform;
        public VkCompositeAlphaFlags CompositeAlpha;
        public VkPresentModeKHR PresentMode;
        public bool Clipped;
        public Swapchain OldSwapchain;

        public SwapchainCreateInfo(Surface surface, Swapchain old) {
            Surface = surface;
            OldSwapchain = old;
        }
    }

    public class Swapchain : IDisposable {
        VkSwapchainKHR swapchain;
        bool disposed;
        
        vkGetSwapchainImagesKHRDelegate getImages;

        public Instance Instance { get; private set; }
        public Surface Surface { get; private set; }
        public Device Device { get; private set; }
        public List<Image> Images { get; private set; }

        public VkSwapchainKHR Native {
            get {
                return swapchain;
            }
        }

        public Swapchain(Device device, SwapchainCreateInfo info) {
            if (device == null) throw new ArgumentNullException(nameof(device));
            if (info == null) throw new ArgumentNullException(nameof(info));
            if (info.Surface == null) throw new ArgumentNullException(nameof(info.Surface));
            Surface = info.Surface;
            Instance = Surface.Instance;
            Device = device;
            
            getImages = Device.Commands.getSwapchainImages;

            CreateSwapchain(info);

            GetImages();
        }

        void GetImages() {
            Images = new List<Image>();
            unsafe {
                uint count = 0;
                VkImage* temp = null;
                getImages(Device.Native, swapchain, ref count, ref *temp);
                VkImage* images = stackalloc VkImage[(int)count];
                getImages(Device.Native, swapchain, ref count, ref images[0]);

                for (int i = 0; i < count; i++) {
                    Images.Add(new Image(Device, images[i]));
                }
            }
        }

        void CreateSwapchain(SwapchainCreateInfo mInfo) {
            unsafe
            {
                VkSwapchainCreateInfoKHR info = new VkSwapchainCreateInfoKHR();
                info.sType = VkStructureType.StructureTypeSwapchainCreateInfoKhr;
                info.surface = mInfo.Surface.Native;
                info.minImageCount = mInfo.MinImageCount;
                info.imageFormat = mInfo.ImageFormat;
                info.imageColorSpace = mInfo.ColorSpace;
                info.imageExtent = mInfo.ImageExtent;
                info.imageArrayLayers = mInfo.ImageArrayLayers;
                info.imageUsage = mInfo.ImageUsageFlags;
                info.imageSharingMode = mInfo.ImageSharingMode;
                info.preTransform = mInfo.PreTransform;
                info.compositeAlpha = mInfo.CompositeAlpha;
                info.clipped = mInfo.Clipped;

                uint indCount = 0;
                if (mInfo.QueueFamilyIndices != null) {
                    indCount = (uint)mInfo.QueueFamilyIndices.Count;
                }
                var indices = stackalloc uint[(int)indCount];
                for (int i = 0; i < indCount; i++) {
                    indices[i] = mInfo.QueueFamilyIndices[i];
                }
                info.pQueueFamilyIndices = indices;
                info.queueFamilyIndexCount = indCount;

                if (mInfo.OldSwapchain != null) {
                    info.oldSwapchain = mInfo.OldSwapchain.Native;
                }

                unsafe
                {
                    var result = Device.Commands.createSwapchain(Device.Native, ref info, Instance.AllocationCallbacks, ref swapchain);
                    if (result != VkResult.Success) throw new SwapchainException(string.Format("Error creating swapchain: {0}", result));
                }
            }
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        void Dispose(bool disposing) {
            if (disposed) return;

            unsafe {
                Device.Commands.destroySwapchain(Device.Native, swapchain, Instance.AllocationCallbacks);
            }

            if (disposing) {
                Surface = null;
                Device = null;
                Instance = null;
                Images = null;  //do not Dispose() the images, they are implicitly destroyed by vkDestroySwapchainKHR
            }

            disposed = true;
        }

        ~Swapchain() {
            Dispose(false);
        }
    }

    public class SwapchainException : Exception {
        public SwapchainException(string message) : base(message) { }
    }
}
