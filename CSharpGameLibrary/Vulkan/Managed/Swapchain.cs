using System;
using System.Collections.Generic;

using CSGL;
using CSGL.Vulkan;
using CSGL.Vulkan.Unmanaged;

namespace CSGL.Vulkan.Managed {
    public class SwapchainCreateInfo {
        public Surface Surface { get; set; }
        public uint MinImageCount { get; set; }
        public VkFormat ImageFormat { get; set; }
        public VkColorSpaceKHR ColorSpace { get; set; }
        public VkExtent2D ImageExtent { get; set; }
        public uint ImageArrayLayers { get; set; }
        public VkImageUsageFlags ImageUsageFlags { get; set; }
        public VkSharingMode ImageSharingMode { get; set; }
        public List<uint> QueueFamilyIndices { get; set; }
        public VkSurfaceTransformFlagsKHR PreTransform { get; set; }
        public VkCompositeAlphaFlagsKHR CompositeAlpha { get; set; }
        public VkPresentModeKHR PresentMode { get; set; }
        public bool Clipped { get; set; }
        public Swapchain OldSwapchain { get; set; }

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
                getImages(Device.Native, swapchain, &count, null);
                VkImage* images = stackalloc VkImage[(int)count];
                getImages(Device.Native, swapchain, &count, images);

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

                fixed (VkSwapchainKHR* temp = &swapchain) {
                    var result = Device.Commands.createSwapchain(Device.Native, ref info, Instance.AllocationCallbacks, temp);
                    if (result != VkResult.Success) throw new SwapchainException(string.Format("Error creating swapchain: {0}", result));
                }
            }
        }

        public void Dispose() {
            if (disposed) return;

            unsafe {
                Device.Commands.destroySwapchain(Device.Native, swapchain, Instance.AllocationCallbacks);
            }

            disposed = true;
        }
    }

    public class SwapchainException : Exception {
        public SwapchainException(string message) : base(message) { }
    }
}
