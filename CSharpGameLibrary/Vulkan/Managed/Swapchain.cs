using System;
using System.Collections.Generic;

using CSGL;
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
        public VkSurfaceTransformFlags PreTransform { get; set; }
        public VkCompositeAlphaFlags CompositeAlpha { get; set; }
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

        vkCreateSwapchainKHRDelegate createSwapchain;
        vkDestroySwapchainKHRDelegate destroySwapchain;

        public Instance Instance { get; private set; }
        public Surface Surface { get; private set; }
        public Device Device { get; private set; }

        public VkSwapchainKHR Native {
            get {
                return swapchain;
            }
        }

        public Swapchain(Device device, SwapchainCreateInfo info) {
            if (info.Surface == null) throw new ArgumentNullException(string.Format("{0} can not be null", nameof(info.Surface)));
            Surface = info.Surface;
            Instance = Surface.Instance;
            Device = device;

            createSwapchain = Instance.Commands.createSwapchain;
            destroySwapchain = Instance.Commands.destroySwapchain;

            CreateSwapchain(info);
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
                    var result = createSwapchain(Device.Native, ref info, Instance.AllocationCallbacks, ref swapchain);
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
                destroySwapchain(Device.Native, swapchain, Instance.AllocationCallbacks);
            }
            if (disposing) {
                Surface = null;
                Device = null;
                Instance = null;

                createSwapchain = null;
                destroySwapchain = null;
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
