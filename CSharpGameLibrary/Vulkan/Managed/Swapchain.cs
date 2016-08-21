using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

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
                getImages(Device.Native, swapchain, ref count, IntPtr.Zero);
                var images = stackalloc byte[Marshal.SizeOf<VkImage>() * (int)count];
                getImages(Device.Native, swapchain, ref count, (IntPtr)images);

                for (int i = 0; i < count; i++) {
                    VkImage image = Marshal.PtrToStructure<VkImage>((IntPtr)images + Marshal.SizeOf<VkImage>() * i);
                    Images.Add(new Image(Device, image));
                }
            }
        }

        void CreateSwapchain(SwapchainCreateInfo mInfo) {
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
            if (mInfo.QueueFamilyIndices != null) {
                info.pQueueFamilyIndices = mInfo.QueueFamilyIndices.ToArray();
                info.queueFamilyIndexCount = (uint)mInfo.QueueFamilyIndices.Count;
            }

            if (mInfo.OldSwapchain != null) {
                info.oldSwapchain = mInfo.OldSwapchain.Native;
            }

            IntPtr infoPtr = Marshal.AllocHGlobal(Marshal.SizeOf<VkSwapchainCreateInfoKHR>());
            Marshal.StructureToPtr(info, infoPtr, false);

            IntPtr swapchainPtr = Marshal.AllocHGlobal(Marshal.SizeOf<VkSwapchainKHR>());

            try {
                var result = Device.Commands.createSwapchain(Device.Native, infoPtr, Instance.AllocationCallbacks, swapchainPtr);
                if (result != VkResult.Success) throw new SwapchainException(string.Format("Error creating swapchain: {0}", result));

                swapchain = Marshal.PtrToStructure<VkSwapchainKHR>(swapchainPtr);
            }
            finally {
                Marshal.DestroyStructure<VkSwapchainCreateInfoKHR>(infoPtr);

                Marshal.FreeHGlobal(infoPtr);
                Marshal.FreeHGlobal(swapchainPtr);
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
