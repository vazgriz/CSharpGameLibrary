using System;
using System.Collections.Generic;
using System.IO;

using CSGL.GLFW;
using CSGL.Vulkan;
using CSGL.Vulkan.Managed;

namespace VK_Test {
    class Program : IDisposable {
        static void Main(string[] args) {
            using (var p = new Program()) {
                p.Run();
            }
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
        Pipeline pipeline;
        VkExtent2D swapchainExtent;
        VkFormat swapchainImageFormat;
        PipelineLayout pipelineLayout;
        RenderPass renderPass;

        public void Dispose() {
            pipeline.Dispose();
            renderPass.Dispose();
            pipelineLayout.Dispose();
            foreach (var iv in swapchainImageViews) iv.Dispose();
            swapchain.Dispose();
            surface.Dispose();
            device.Dispose();
            instance.Dispose();
        }

        void Run() {
            GLFW.Init();
            Vulkan.Init();
            window = GLFW.CreateWindow(width, height, "Test", MonitorPtr.Null, WindowPtr.Null);

            List<string> extensions = new List<string>(GLFW.GetRequiredInstanceExceptions());
            extensions.Add("VK_EXT_debug_report");

            List<string> layers = new List<string> {
                "VK_LAYER_LUNARG_standard_validation"
            };

            foreach (var s in Instance.AvailableExtensions) {
                Console.WriteLine(s.Name);
            }

            foreach (var s in Instance.AvailableLayers) {
                Console.WriteLine(s.Name);
            }

            var app = new ApplicationInfo(new VkVersion(), "Test", "None", new VkVersion(), new VkVersion());
            var info = new InstanceCreateInfo(app, extensions, layers);

            instance = new Instance(info);
            physicalDevice = instance.PhysicalDevices[0];
            surface = new Surface(physicalDevice, window);

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
            var queueInfo = new QueueCreateInfo((uint)graphicsIndex, 1, new float[] { 1f });
            List<string> deviceExtensions = new List<string> {
                "VK_KHR_swapchain"
            };
            List<QueueCreateInfo> queueInfos = new List<QueueCreateInfo>{
                queueInfo
            };
            var deviceInfo = new DeviceCreateInfo(deviceExtensions, queueInfos);

            device = new Device(physicalDevice, deviceInfo);
            graphicsQueue = device.GetQueue((uint)graphicsIndex, 0);
            presentQueue = device.GetQueue((uint)presentIndex, 0);

            var swapchainInfo = GetCreateInfo((uint)graphicsIndex, (uint)presentIndex);

            swapchain = new Swapchain(device, swapchainInfo);
            
            swapchainImageViews = new List<ImageView>(swapchain.Images.Count);
            for (int i = 0; i < swapchain.Images.Count; i++) {
                ImageViewCreateInfo imageViewInfo = new ImageViewCreateInfo(swapchain.Images[i]);
                imageViewInfo.Format = swapchainFormat;
                var comp = imageViewInfo.Components;
                comp.r = VkComponentSwizzle.ComponentSwizzleIdentity;
                comp.g = VkComponentSwizzle.ComponentSwizzleIdentity;
                comp.b = VkComponentSwizzle.ComponentSwizzleIdentity;
                comp.a = VkComponentSwizzle.ComponentSwizzleIdentity;
                imageViewInfo.Components = comp;
                var sub = imageViewInfo.SubresourceRange;
                sub.aspectMask = VkImageAspectFlags.ImageAspectColorBit;
                sub.baseMipLevel = 0;
                sub.levelCount = 1;
                sub.baseArrayLayer = 0;
                sub.layerCount = 1;
                imageViewInfo.SubresourceRange = sub;

                swapchainImageViews.Add(new ImageView(device, imageViewInfo));
            }

            CreatePipeline();
            
            while (!GLFW.WindowShouldClose(window)) {
                GLFW.PollEvents();
            }

            GLFW.DestroyWindow(window);
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
            info.CompositeAlpha = VkCompositeAlphaFlagsKHR.CompositeAlphaOpaqueBitKhr;
            info.PresentMode = presentMode;
            info.Clipped = true;
            info.OldSwapchain = swapchain;

            swapchainExtent = extent;
            swapchainImageFormat = surfaceFormat.format;

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

        Pipeline CreatePipeline() {
            ShaderModule vert;
            ShaderModuleCreateInfo vertCreate;

            ShaderModule frag;
            ShaderModuleCreateInfo fragCreate;

            using (var reader = File.OpenRead("vert.spv")) {
                vertCreate = new ShaderModuleCreateInfo(reader);
            }
            vert = new ShaderModule(device, vertCreate);

            using (var reader = File.OpenRead("frag.spv")) {
                fragCreate = new ShaderModuleCreateInfo(reader);
            }
            frag = new ShaderModule(device, fragCreate);

            var vertInfo = new PipelineShaderStageCreateInfo();
            vertInfo.Module = vert;
            vertInfo.Name = "main";
            vertInfo.Stage = VkShaderStageFlags.ShaderStageVertexBit;

            var fragInfo = new PipelineShaderStageCreateInfo();
            fragInfo.Module = frag;
            fragInfo.Name = "main";
            fragInfo.Stage = VkShaderStageFlags.ShaderStageFragmentBit;

            var vertexInput = new PipelineVertexInputStateCreateInfo();
            var inputAssembly = new PipelineInputAssemblyStateCreateInfo();
            inputAssembly.Topology = VkPrimitiveTopology.PrimitiveTopologyTriangleList;

            var viewport = new VkViewport();
            viewport.x = 0;
            viewport.y = 0;
            viewport.width = swapchainExtent.width;
            viewport.height = swapchainExtent.height;
            viewport.minDepth = 0;
            viewport.maxDepth = 1f;

            var scissor = new VkRect2D();
            scissor.offset = new VkOffset2D();
            scissor.extent = swapchainExtent;

            var viewportState = new PipelineViewportStateCreateInfo();
            viewportState.Viewports = new VkViewport[] { viewport };
            viewportState.Scissors = new VkRect2D[] { scissor };

            var rasterizer = new PipelineRasterizationStateCreateInfo();
            rasterizer.PolygonMode = VkPolygonMode.PolygonModeFill;
            rasterizer.LineWidth = 1f;
            rasterizer.CullMode = VkCullModeFlags.CullModeBackBit;
            rasterizer.FrontFace = VkFrontFace.FrontFaceClockwise;

            var multisample = new PipelineMultisampleStateCreateInfo();
            multisample.RasterizationSamples = VkSampleCountFlags.SampleCount1Bit;
            multisample.MinSampleShading = 1;

            var colorAttach = new PipelineColorBlendAttachmentState();
            colorAttach.ColorWriteMask = VkColorComponentFlags.ColorComponentRBit | VkColorComponentFlags.ColorComponentGBit
                | VkColorComponentFlags.ColorComponentBBit | VkColorComponentFlags.ColorComponentABit;
            colorAttach.SrcColorBlendFactor = VkBlendFactor.BlendFactorOne;
            colorAttach.DstColorBlendFactor = VkBlendFactor.BlendFactorOne;
            colorAttach.ColorBlendOp = VkBlendOp.BlendOpAdd;
            colorAttach.SrcAlphaBlendFactor = VkBlendFactor.BlendFactorOne;
            colorAttach.DstColorBlendFactor = VkBlendFactor.BlendFactorZero;
            colorAttach.AlphaBlendOp = VkBlendOp.BlendOpAdd;

            var color = new PipelineColorBlendStateCreateInfo();
            color.Attachments = new PipelineColorBlendAttachmentState[] { colorAttach };
            color.BlendConstants = new float[4];    //the unmanaged version is a fixed size array

            var dynamic = new PipelineDynamicStateCreateInfo();
            dynamic.DynamicStates = new VkDynamicState[] { VkDynamicState.DynamicStateViewport };

            var pipelineLayoutCreate = new PipelineLayoutCreateInfo();
            pipelineLayout = new PipelineLayout(device, pipelineLayoutCreate);

            var colorAttachment = new VkAttachmentDescription();
            colorAttachment.format = swapchainImageFormat;
            colorAttachment.samples = VkSampleCountFlags.SampleCount1Bit;
            colorAttachment.loadOp = VkAttachmentLoadOp.AttachmentLoadOpClear;
            colorAttachment.storeOp = VkAttachmentStoreOp.AttachmentStoreOpStore;
            colorAttachment.stencilLoadOp = VkAttachmentLoadOp.AttachmentLoadOpDontCare;
            colorAttachment.stencilStoreOp = VkAttachmentStoreOp.AttachmentStoreOpDontCare;
            colorAttachment.initialLayout = VkImageLayout.ImageLayoutUndefined;
            colorAttachment.finalLayout = VkImageLayout.ImageLayoutPresentSrcKhr;

            var colorAttachmentRef = new VkAttachmentReference();
            colorAttachmentRef.attachment = 0;
            colorAttachmentRef.layout = VkImageLayout.ImageLayoutColorAttachmentOptimal;

            var subpass = new SubpassDescription();
            subpass.PipelineBindPoint = VkPipelineBindPoint.PipelineBindPointGraphics;
            
            subpass.ColorAttachments = new VkAttachmentReference[] { colorAttachmentRef };

            var renderpassCreate = new RenderPassCreateInfo();
            renderpassCreate.Attachments = new VkAttachmentDescription[] { colorAttachment };
            renderpassCreate.Subpasses = new SubpassDescription[] { subpass };

            renderPass = new RenderPass(device, renderpassCreate);

            var pipelineInfo = new GraphicsPipelineCreateInfo();
            pipelineInfo.Stages = new PipelineShaderStageCreateInfo[] { vertInfo, fragInfo };
            pipelineInfo.VertexInputState = vertexInput;
            pipelineInfo.InputAssemblyState = inputAssembly;
            pipelineInfo.ViewportState = viewportState;
            pipelineInfo.RasterizationState = rasterizer;
            pipelineInfo.MultisampleState = multisample;
            pipelineInfo.ColorBlendState = color;
            pipelineInfo.Layout = pipelineLayout;
            pipelineInfo.RenderPass = renderPass;
            pipelineInfo.Subpass = 0;
            pipelineInfo.BasePipeline = null;
            pipelineInfo.BasePipelineIndex = -1;

            pipeline = new Pipeline(device, pipelineInfo, null);
            
            vert.Dispose();
            frag.Dispose();

            return null;
        }
    }
}
