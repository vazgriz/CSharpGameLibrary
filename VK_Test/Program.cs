using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Reflection;

using CSGL.GLFW;
using CSGL.Vulkan;
using CSGL.Vulkan.Managed;

namespace VK_Test {
    class Program {
        static void Main(string[] args) {
            new Program().Run();
        }

        int width = 800;
        int height = 600;

        VkFormat swapchainFormat;

        WindowPtr window;
        Instance instance;
        PhysicalDevice physicalDevice;
        Surface surface;
        Device device;
        Queue graphicsQueue;
        Queue presentQueue;
        Swapchain swapchain;
        List<ImageView> swapchainImageViews;

        void Run() {
            GLFW.Init();
            Vulkan.Init();

            List<string> ex = new List<string>(GLFW.GetRequiredInstanceExceptions());
            ex.Add("VK_EXT_debug_report");

            List<string> l = new List<string> {
                "VK_LAYER_LUNARG_standard_validation"
            };
            ApplicationInfo app = new ApplicationInfo(new VkVersion(), "Test", "None", new VkVersion(), new VkVersion());
            InstanceCreateInfo info = new InstanceCreateInfo(app, ex, l);

            window = GLFW.CreateWindow(width, height, "Test", MonitorPtr.Null, WindowPtr.Null);
            instance = new Instance(info);
            physicalDevice = instance.PhysicalDevices[0];
            surface = new Surface(physicalDevice, window);

            Type t = typeof(VkImageViewCreateInfo);
            Console.WriteLine("{0} size: {1}", t.Name, Marshal.SizeOf(t));
            FieldInfo[] fields = t.GetFields();
            foreach (var f in fields) {
                Console.WriteLine("{0}: {1}", f.Name, Marshal.OffsetOf(t, f.Name));
            }

            int graphicsIndex = -1;
            int presentIndex = -1;
            for (int i = 0; i < physicalDevice.QueueFamilies.Count; i++) {
                var q = physicalDevice.QueueFamilies[i];
                if (graphicsIndex == -1 && (q.Flags & VkQueueFlags.QueueGraphicsBit) != 0) {
                    graphicsIndex = i;
                }
                if (presentIndex == -1 && (q.SurfaceSupported(surface))) {
                    presentIndex = i;
                }
            }
            QueueCreateInfo queueInfo = new QueueCreateInfo((uint)graphicsIndex, 1, new float[] { 1f });
            List<string> dEx = new List<string> {
                "VK_KHR_swapchain"
            };
            List<QueueCreateInfo> queueInfos = new List<QueueCreateInfo>{
                queueInfo
            };
            DeviceCreateInfo deviceInfo = new DeviceCreateInfo(dEx, queueInfos);

            device = new Device(physicalDevice, deviceInfo);
            graphicsQueue = device.GetQueue((uint)graphicsIndex, 0);
            presentQueue = device.GetQueue((uint)presentIndex, 0);

            var swapchainInfo = GetCreateInfo((uint)graphicsIndex, (uint)presentIndex);

            swapchain = new Swapchain(device, swapchainInfo);

            using (instance)
            using (device)
            using (surface)
            using (swapchain) {
                swapchainImageViews = new List<ImageView>(swapchain.Images.Count);
                for (int i = 0; i < swapchain.Images.Count; i++) {
                    ImageViewCreateInfo imageViewInfo = new ImageViewCreateInfo(swapchain.Images[i]);
                    imageViewInfo.Format = swapchainFormat;
                    imageViewInfo.Components.r = VkComponentSwizzle.ComponentSwizzleIdentity;
                    imageViewInfo.Components.g = VkComponentSwizzle.ComponentSwizzleIdentity;
                    imageViewInfo.Components.b = VkComponentSwizzle.ComponentSwizzleIdentity;
                    imageViewInfo.Components.a = VkComponentSwizzle.ComponentSwizzleIdentity;
                    imageViewInfo.SubresourceRange.aspectMask = VkImageAspectFlags.ImageAspectColorBit;
                    imageViewInfo.SubresourceRange.baseMipLevel = 0;
                    imageViewInfo.SubresourceRange.levelCount = 1;
                    imageViewInfo.SubresourceRange.baseArrayLayer = 0;
                    imageViewInfo.SubresourceRange.layerCount = 1;

                    swapchainImageViews.Add(new ImageView(device, imageViewInfo));
                }

                while (!GLFW.WindowShouldClose(window)) {
                    GLFW.PollEvents();
                }
                GLFW.DestroyWindow(window);

                foreach (var iv in swapchainImageViews) {
                    iv.Dispose();
                }
            }
            GLFW.Terminate();
        }

        SwapchainCreateInfo GetCreateInfo(uint graphicsIndex, uint presentIndex) {
            SwapchainCreateInfo info = new SwapchainCreateInfo(surface, null);

            var cap = surface.Capabilities;

            var surfaceFormat = ChooseSwapSurfaceFormat(surface.Formats);
            var presentMode = ChoosePresentMode(surface.Modes);
            var extent = ChooseSwapExtent(cap);

            uint imageCount = cap.minImageCount + 1;
            if (cap.maxImageCount > 0 && imageCount > cap.maxImageCount) {
                imageCount = cap.maxImageCount;
            }

            info.MinImageCount = imageCount;
            info.ImageFormat = surfaceFormat.format;
            swapchainFormat = surfaceFormat.format;
            info.ColorSpace = surfaceFormat.colorSpace;
            info.ImageExtent = extent;
            info.ImageArrayLayers = 1;
            info.ImageUsageFlags = VkImageUsageFlags.ImageUsageColorAttachmentBit;

            if (graphicsIndex != presentIndex) {
                info.ImageSharingMode = VkSharingMode.SharingModeConcurrent;
                info.QueueFamilyIndices = new List<uint> { graphicsIndex, presentIndex };
            } else {
                info.ImageSharingMode = VkSharingMode.SharingModeExclusive;
            }

            info.PreTransform = cap.currentTransform;
            info.CompositeAlpha = VkCompositeAlphaFlags.CompositeAlphaOpaqueBitKhr;
            info.PresentMode = presentMode;
            info.Clipped = true;
            info.OldSwapchain = swapchain;

            return info;
        }

        VkPresentModeKHR ChoosePresentMode(List<VkPresentModeKHR> list) {
            foreach (var mode in list) {
                if (mode == VkPresentModeKHR.PresentModeMailboxKhr) {
                    return mode;
                }
            }
            return VkPresentModeKHR.PresentModeFifoKhr;
        }

        VkExtent2D ChooseSwapExtent(VkSurfaceCapabilitiesKHR capabilities) {
            if (capabilities.currentExtent.width != int.MaxValue) {
                return capabilities.currentExtent;
            } else {
                VkExtent2D actualExtent = new VkExtent2D();
                actualExtent.width = (uint)width;
                actualExtent.height = (uint)height;

                actualExtent.width = Math.Max(capabilities.minImageExtent.width,
                    Math.Min(capabilities.maxImageExtent.width, actualExtent.width));
                actualExtent.height = Math.Max(capabilities.minImageExtent.height,
                    Math.Min(capabilities.maxImageExtent.height, actualExtent.height));

                return actualExtent;
            }
        }

        VkSurfaceFormatKHR ChooseSwapSurfaceFormat(List<VkSurfaceFormatKHR> list) {
            if (list.Count == 1 && list[0].format == VkFormat.FormatUndefined) {
                var result = new VkSurfaceFormatKHR();
                result.format = VkFormat.FormatB8g8r8a8Unorm;
                result.colorSpace = VkColorSpaceKHR.ColorSpaceSrgbNonlinearKhr;
                return result;
            }
            foreach (var format in list) {
                if (format.format == VkFormat.FormatB8g8r8a8Unorm
                    && format.colorSpace == VkColorSpaceKHR.ColorSpaceSrgbNonlinearKhr) {
                    return format;
                }
            }
            return list[0];
        }
    }
}
