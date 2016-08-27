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
            Console.ReadKey();
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
        List<Framebuffer> framebuffers;
        CommandPool pool;
        List<CommandBuffer> commandBuffers;
        Semaphore imageAvailable;
        Semaphore renderFinished;

        public void Dispose() {
            imageAvailable.Dispose();
            renderFinished.Dispose();
            pool.Dispose();
            foreach (var fb in framebuffers) fb.Dispose();
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
            GLFW.WindowHint(WindowHint.Visible, 0);
            window = GLFW.CreateWindow(width, height, "Test", MonitorPtr.Null, WindowPtr.Null);

            List<string> extensions = new List<string>(GLFW.GetRequiredInstanceExceptions());
            extensions.Add("VK_EXT_debug_report");

            List<string> layers = new List<string> {
                "VK_LAYER_LUNARG_standard_validation",
                //"VK_LAYER_LUNARG_api_dump"
            };

            foreach (var s in Instance.AvailableExtensions) {
                Console.WriteLine(s.Name);
            }

            foreach (var s in Instance.AvailableLayers) {
                Console.WriteLine(s.Name);
            }

            var app = new ApplicationInfo(new VkVersion(1, 0, 17), "Test", "None", new VkVersion(0, 0, 1), new VkVersion());
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

            CreateImageViews();
            CreatePipeline();
            CreateFramebuffers();
            CreateCommandPool((uint)graphicsIndex);
            CreateCommandBuffers();
            CreateSemaphores();

            var waitSemaphores = new Semaphore[] { imageAvailable };
            var waitStages = new VkPipelineStageFlags[] { VkPipelineStageFlags.PipelineStageColorAttachmentOutputBit };
            var submitCommandBuffers = new CommandBuffer[1];
            var signalSemaphores = new Semaphore[] { renderFinished };

            var submitInfo = new SubmitInfo();
            submitInfo.WaitSemaphores = waitSemaphores;
            submitInfo.WaitDstStageMask = waitStages;
            submitInfo.CommandBuffers = submitCommandBuffers;
            submitInfo.SignalSemaphores = signalSemaphores;

            var submitInfoArray = new SubmitInfo[] { submitInfo };
            uint[] imageIndices = new uint[1];

            var presentInfo = new PresentInfo();
            presentInfo.WaitSemaphores = signalSemaphores;
            presentInfo.Swapchains = new Swapchain[] { swapchain };
            presentInfo.ImageIndices = imageIndices;

            GLFW.ShowWindow(window);

            while (!GLFW.WindowShouldClose(window)) {
                GLFW.PollEvents();
                if (GLFW.GetKey(window, CSGL.Input.KeyCode.Enter) == CSGL.Input.KeyAction.Press) {
                    GLFW.SetWindowShouldClose(window, true);
                }
                
                uint index;
                var result = swapchain.AcquireNextImage(imageAvailable, out index);

                submitCommandBuffers[0] = commandBuffers[(int)index];
                graphicsQueue.Submit(submitInfoArray, null);

                imageIndices[0] = index;
                presentQueue.Present(presentInfo);
            }

            device.WaitIdle();

            GLFW.DestroyWindow(window);
            GLFW.Terminate();
        }

        void CreateSemaphores() {
            imageAvailable = new Semaphore(device);
            renderFinished = new Semaphore(device);
        }

        void CreateCommandPool(uint index) {
            CommandPoolCreateInfo info = new CommandPoolCreateInfo();
            info.QueueFamilyIndex = index;

            pool = new CommandPool(device, info);
        }

        void CreateCommandBuffers() {
            CommandBufferAllocateInfo info = new CommandBufferAllocateInfo();
            info.CommandPool = pool;
            info.Level = VkCommandBufferLevel.CommandBufferLevelPrimary;
            info.Count = (uint)framebuffers.Count;

            commandBuffers = new List<CommandBuffer>(pool.Allocate(info));

            for (int i = 0; i < commandBuffers.Count; i++) {
                var buffer = commandBuffers[i];

                var beginInfo = new CommandBeginInfo();
                beginInfo.Flags = VkCommandBufferUsageFlags.CommandBufferUsageSimultaneousUseBit;

                buffer.Begin(beginInfo);

                var renderPassInfo = new RenderPassBeginInfo();
                renderPassInfo.RenderPass = renderPass;
                renderPassInfo.Framebuffer = framebuffers[i];
                var renderArea = new VkRect2D();
                renderArea.extent = swapchainExtent;
                renderPassInfo.RenderArea = renderArea;
                var clearColor = new VkClearValue();
                var clearColorValue = new VkClearColorValue();
                clearColorValue.color = new CSGL.Graphics.Color32(1, 0, 1, 1);
                clearColor.color = clearColorValue;
                renderPassInfo.ClearValues = new VkClearValue[] { clearColor };

                buffer.BeginRenderPass(renderPassInfo, VkSubpassContents.SubpassContentsInline);

                buffer.BindPipeline(VkPipelineBindPoint.PipelineBindPointGraphics, pipeline);

                buffer.Draw(3, 1, 0, 0);

                buffer.EndRenderPass();

                var result = buffer.End();
                if (result != VkResult.Success) throw new Exception(string.Format("Error recording framebuffer: {0}", result));
            }
        }

        void CreateFramebuffers() {
            framebuffers = new List<Framebuffer>(swapchain.Images.Count);

            for (int i = 0; i < swapchain.Images.Count; i++) {
                ImageView imageView = swapchainImageViews[i];

                FramebufferCreateInfo info = new FramebufferCreateInfo();
                info.RenderPass = renderPass;
                info.Attachments = new ImageView[] { imageView };
                info.Width = swapchainExtent.width;
                info.Height = swapchainExtent.height;
                info.Layers = 1;

                framebuffers.Add(new Framebuffer(device, info));
            }
        }

        void CreateImageViews() {
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
                info.QueueFamilyIndices = new uint[] { graphicsIndex, presentIndex };
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

        void CreatePipeline() {
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

            var dependency = new VkSubpassDependency();
            dependency.srcSubpass = ~(uint)0;   //VK_SUBPASS_EXTERNAL == ~0U
            dependency.srcStageMask = VkPipelineStageFlags.PipelineStageBottomOfPipeBit;
            dependency.srcAccessMask = VkAccessFlags.AccessMemoryReadBit;
            dependency.dstStageMask = VkPipelineStageFlags.PipelineStageColorAttachmentOutputBit;
            dependency.dstAccessMask = VkAccessFlags.AccessColorAttachmentReadBit | VkAccessFlags.AccessColorAttachmentWriteBit;

            var renderpassCreate = new RenderPassCreateInfo();
            renderpassCreate.Attachments = new VkAttachmentDescription[] { colorAttachment };
            renderpassCreate.Subpasses = new SubpassDescription[] { subpass };
            renderpassCreate.Dependencies = new VkSubpassDependency[] { dependency };

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
        }
    }
}
