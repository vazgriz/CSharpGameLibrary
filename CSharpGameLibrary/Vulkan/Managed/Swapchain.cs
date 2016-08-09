using System;
using System.Collections.Generic;

using CSGL;
using CSGL.Vulkan.Unmanaged;

namespace CSGL.Vulkan.Managed {
    public class SwapchainCreateInfo {
        public Surface Surface { get; set; }
        public uint MinImageCount { get; set; }
        public VkFormat Format { get; set; }
        public VkColorSpaceKHR ColorSpace { get; set; }
        public VkExtent2D imsageExtent;
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

        public Surface Surface { get; private set; }

        public VkSwapchainKHR Native {
            get {
                return swapchain;
            }
        }

        public Swapchain(SwapchainCreateInfo info) {
            Surface = info.Surface;

            CreateSwapchain(info);
        }

        void CreateSwapchain(SwapchainCreateInfo mInfo) {
            unsafe
            {
                VkSwapchainCreateInfoKHR info = new VkSwapchainCreateInfoKHR();
                info.sType = VkStructureType.StructureTypeSwapchainCreateInfoKhr;
                info.surface = mInfo.Surface.Native;
            }
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        void Dispose(bool disposing) {
            if (disposed) return;

            if (disposing) {
                Surface = null;
            }

            disposed = true;
        }

        ~Swapchain() {
            Dispose(false);
        }
    }
}
