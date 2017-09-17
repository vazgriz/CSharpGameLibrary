using System;
using System.Collections.Generic;

using CSGL;
using CSGL.Vulkan;

namespace CSGL.Vulkan {
    public class VkSwapchainCreateInfo {
        public VkSurface surface;
        public int minImageCount;
        public VkFormat imageFormat;
        public VkColorSpaceKHR imageColorSpace;
        public VkExtent2D imageExtent;
        public int imageArrayLayers;
        public VkImageUsageFlags imageUsage;
        public VkSharingMode imageSharingMode;
        public IList<int> queueFamilyIndices;
        public VkSurfaceTransformFlagsKHR preTransform;
        public VkCompositeAlphaFlagsKHR compositeAlpha;
        public VkPresentModeKHR presentMode;
        public bool clipped;
        public VkSwapchain oldSwapchain;
    }

    public class VkSwapchain : IDisposable, INative<Unmanaged.VkSwapchainKHR> {
        Unmanaged.VkSwapchainKHR swapchain;
        bool disposed;

        public VkDevice Device { get; private set; }
        public VkSurface Surface { get; private set; }
        public IList<VkImage> Images { get; private set; }
        public VkFormat Format { get; private set; }
        public VkColorSpaceKHR ColorSpace { get; private set; }
        public VkExtent2D Extent { get; private set; }
        public int ArrayLayers { get; private set; }
        public VkImageUsageFlags Usage { get; private set; }
        public VkSharingMode SharingMode { get; private set; }
        public IList<int> QueueFamilyIndices { get; private set; }
        public VkSurfaceTransformFlagsKHR PreTransform { get; private set; }
        public VkCompositeAlphaFlagsKHR CompositeAlpha { get; private set; }
        public VkPresentModeKHR PresentMode { get; private set; }
        public bool Clipped { get; private set; }

        public Unmanaged.VkSwapchainKHR Native {
            get {
                return swapchain;
            }
        }

        public VkSwapchain(VkDevice device, VkSwapchainCreateInfo info) {
            if (device == null) throw new ArgumentNullException(nameof(device));
            if (info == null) throw new ArgumentNullException(nameof(info));

            Surface = info.surface;
            Device = device;

            CreateSwapchain(info);

            GetImages();

            Format = info.imageFormat;
            ColorSpace = info.imageColorSpace;
            Extent = info.imageExtent;
            ArrayLayers = info.imageArrayLayers;
            PresentMode = info.presentMode;
            Usage = info.imageUsage;
            SharingMode = info.imageSharingMode;
            QueueFamilyIndices = info.queueFamilyIndices.CloneReadOnly();
            PreTransform = info.preTransform;
            CompositeAlpha = info.compositeAlpha;
            PresentMode = info.presentMode;
            Clipped = info.clipped;
        }

        void GetImages() {
            List<VkImage> images = new List<VkImage>();

            uint count = 0;
            Device.Commands.getSwapchainImages(Device.Native, swapchain, ref count, IntPtr.Zero);
            var imagesNative = new NativeArray<Unmanaged.VkImage>((int)count);
            Device.Commands.getSwapchainImages(Device.Native, swapchain, ref count, imagesNative.Address);

            using (imagesNative) {
                for (int i = 0; i < count; i++) {
                    var image = imagesNative[i];
                    images.Add(new VkImage(Device, image, Format));
                }
            }

            Images = images.AsReadOnly();
        }

        void CreateSwapchain(VkSwapchainCreateInfo mInfo) {
            if (mInfo.surface == null) throw new ArgumentNullException(nameof(mInfo.surface));

            unsafe {
                int indicesCount = 0;
                if (mInfo.queueFamilyIndices != null) indicesCount = mInfo.queueFamilyIndices.Count;

                var info = new Unmanaged.VkSwapchainCreateInfoKHR();
                info.sType = VkStructureType.SwapchainCreateInfoKhr;
                info.surface = mInfo.surface.Native;
                info.minImageCount = (uint)mInfo.minImageCount;
                info.imageFormat = mInfo.imageFormat;
                info.imageColorSpace = mInfo.imageColorSpace;
                info.imageExtent = mInfo.imageExtent.GetNative();
                info.imageArrayLayers = (uint)mInfo.imageArrayLayers;
                info.imageUsage = mInfo.imageUsage;
                info.imageSharingMode = mInfo.imageSharingMode;

                var indicesNative = stackalloc uint[indicesCount];
                if (mInfo.queueFamilyIndices != null) Interop.Copy(mInfo.queueFamilyIndices, (IntPtr)indicesNative);

                info.queueFamilyIndexCount = (uint)indicesCount;
                info.pQueueFamilyIndices = (IntPtr)indicesNative;

                info.preTransform = mInfo.preTransform;
                info.compositeAlpha = mInfo.compositeAlpha;
                info.presentMode = mInfo.presentMode;
                info.clipped = mInfo.clipped ? 1u : 0u;

                if (mInfo.oldSwapchain != null) {
                    info.oldSwapchain = mInfo.oldSwapchain.Native;
                }
                
                var result = Device.Commands.createSwapchain(Device.Native, ref info, Device.Instance.AllocationCallbacks, out swapchain);
                if (result != VkResult.Success) throw new SwapchainException(result, string.Format("Error creating swapchain: {0}", result));
            }
        }

        public VkResult AcquireNextImage(ulong timeout, VkSemaphore semaphore, VkFence fence, out uint index) {
            var semaphoreNative = Unmanaged.VkSemaphore.Null;
            var fenceNative = Unmanaged.VkFence.Null;
            if (semaphore != null) semaphoreNative = semaphore.Native;
            if (fence != null) fenceNative = fence.Native;

            var result = Device.Commands.acquireNextImage(Device.Native, swapchain, timeout, semaphoreNative, fenceNative, out index);
            if (!(result == VkResult.Success || result == VkResult.SuboptimalKhr || result == VkResult.NotReady || result == VkResult.Timeout)) {
                throw new SwapchainException(result, string.Format("Error acquiring image: {0}", result));
            }
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

        ~VkSwapchain() {
            Dispose(false);
        }
    }

    public class SwapchainException : VulkanException {
        public SwapchainException(VkResult result, string message) : base(result, message) { }
    }
}
