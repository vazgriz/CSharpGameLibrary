using System;
using System.Collections.Generic;

using CSGL;
using CSGL.Vulkan;
using CSGL.Vulkan.Unmanaged;

namespace CSGL.Vulkan {
    public class SwapchainCreateInfo {
        public Surface surface;
        public uint minImageCount;
        public VkFormat imageFormat;
        public VkColorSpaceKHR imageColorSpace;
        public VkExtent2D imageExtent;
        public uint imageArrayLayers;
        public VkImageUsageFlags imageUsage;
        public VkSharingMode imageSharingMode;
        public List<uint> queueFamilyIndices;
        public VkSurfaceTransformFlagsKHR preTransform;
        public VkCompositeAlphaFlagsKHR compositeAlpha;
        public VkPresentModeKHR presentMode;
        public bool clipped;
        public Swapchain oldSwapchain;
    }

    public class Swapchain : IDisposable, INative<VkSwapchainKHR> {
        VkSwapchainKHR swapchain;
        bool disposed;

        vkGetSwapchainImagesKHRDelegate getImages;

        public Device Device { get; private set; }
        public Surface Surface { get; private set; }
        public IList<Image> Images { get; private set; }
        public VkFormat Format { get; private set; }
        public VkColorSpaceKHR ColorSpace { get; private set; }
        public VkExtent2D Extent { get; private set; }
        public uint ArrayLayers { get; private set; }
        public VkImageUsageFlags Usage { get; private set; }
        public VkSharingMode SharingMode { get; private set; }
        public VkPresentModeKHR PresentMode { get; private set; }

        public VkSwapchainKHR Native {
            get {
                return swapchain;
            }
        }

        public Swapchain(Device device, SwapchainCreateInfo info) {
            if (device == null) throw new ArgumentNullException(nameof(device));
            if (info == null) throw new ArgumentNullException(nameof(info));
            if (info.surface == null) throw new ArgumentNullException(nameof(info.surface));

            Surface = info.surface;
            Device = device;

            getImages = Device.Commands.getSwapchainImages;

            CreateSwapchain(info);

            GetImages();
        }

        void GetImages() {
            List<Image> images = new List<Image>();

            uint count = 0;
            getImages(Device.Native, swapchain, ref count, IntPtr.Zero);
            var imagesNative = new NativeArray<VkImage>((int)count);
            getImages(Device.Native, swapchain, ref count, imagesNative.Address);

            using (imagesNative) {
                for (int i = 0; i < count; i++) {
                    var image = imagesNative[i];
                    images.Add(new Image(Device, image, Format));
                }
            }

            Images = images.AsReadOnly();
        }

        void CreateSwapchain(SwapchainCreateInfo mInfo) {
            var info = new VkSwapchainCreateInfoKHR();
            info.sType = VkStructureType.SwapchainCreateInfoKhr;
            info.surface = mInfo.surface.Native;
            info.minImageCount = mInfo.minImageCount;
            info.imageFormat = mInfo.imageFormat;
            info.imageColorSpace = mInfo.imageColorSpace;
            info.imageExtent = mInfo.imageExtent;
            info.imageArrayLayers = mInfo.imageArrayLayers;
            info.imageUsage = mInfo.imageUsage;
            info.imageSharingMode = mInfo.imageSharingMode;

            var indicesMarshalled = new NativeArray<uint>(mInfo.queueFamilyIndices);
            info.queueFamilyIndexCount = (uint)indicesMarshalled.Count;
            info.pQueueFamilyIndices = indicesMarshalled.Address;

            info.preTransform = mInfo.preTransform;
            info.compositeAlpha = mInfo.compositeAlpha;
            info.presentMode = mInfo.presentMode;
            info.clipped = mInfo.clipped ? 1u : 0u;

            if (mInfo.oldSwapchain != null) {
                info.oldSwapchain = mInfo.oldSwapchain.Native;
            }

            using (indicesMarshalled) {
                var result = Device.Commands.createSwapchain(Device.Native, ref info, Device.Instance.AllocationCallbacks, out swapchain);
                if (result != VkResult.Success) throw new SwapchainException(result, string.Format("Error creating swapchain: {0}", result));
            }

            Format = info.imageFormat;
            ColorSpace = info.imageColorSpace;
            Extent = info.imageExtent;
            ArrayLayers = info.imageArrayLayers;
            PresentMode = info.presentMode;
            Usage = info.imageUsage;
            SharingMode = info.imageSharingMode;
        }

        public VkResult AcquireNextImage(ulong timeout, Semaphore semaphore, Fence fence, out uint index) {
            VkSemaphore sTemp = VkSemaphore.Null;
            VkFence fTemp = VkFence.Null;
            if (semaphore != null) sTemp = semaphore.Native;
            if (fence != null) fTemp = fence.Native;

            var result = Device.Commands.acquireNextImage(Device.Native, swapchain, timeout, sTemp, fTemp, out index);
            return result;
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        void Dispose(bool disposing) {
            if (disposed) return;

            Device.Commands.destroySwapchain(Device.Native, swapchain, Device.Instance.AllocationCallbacks);

            foreach (var image in Images) {
                GC.SuppressFinalize(image);
            }

            disposed = true;
        }

        ~Swapchain() {
            Dispose(false);
        }
    }

    public class SwapchainException : VulkanException {
        public SwapchainException(VkResult result, string message) : base(result, message) { }
    }
}
