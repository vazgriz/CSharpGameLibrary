using System;

using CSGL;
using CSGL.GLFW;
using CSGL.Vulkan;
using CSGL.Vulkan.Managed;
using CSGL.Vulkan.Unmanaged;

namespace VK_Test2 {
    unsafe class Program : IDisposable {
        static void Main(string[] args) {
            using (var p = new Program()) {
                p.Run();
            }
        }

        int width = 800;
        int height = 600;

        string[] extensions = { };
        string[] layers = {
            "VK_LAYER_LUNARG_standard_validation",
            "VK_LAYER_LUNARG_api_dump"
        };
        string[] deviceExtensions = { };

        IntPtr alloc = IntPtr.Zero;

        uint graphicsIndex;
        uint presentIndex;

        WindowPtr window;

        VkInstance instance;
        VkSurfaceKHR surface;
        VkPhysicalDevice physicalDevice;

        vkCreateInstanceDelegate createInstance;
        vkDestroyInstanceDelegate destroyInstance;
        vkEnumeratePhysicalDevicesDelegate enumeratePhysicalDevices;
        vkGetPhysicalDeviceQueueFamilyPropertiesDelegate getQueueFamilyProperties;
        vkGetPhysicalDeviceSurfaceSupportKHRDelegate getSurfaceSupported;
        vkDestroySurfaceKHRDelegate destroySurface;

        public Program() {
            GLFW.Init();
        }

        public void Dispose() {
            destroySurface(instance, surface, alloc);
            destroyInstance(instance, alloc);
            GLFW.Terminate();
        }

        public void Run() {
            GLFW.WindowHint(WindowHint.ClientAPI, (int)ContextAPI.NoAPI);
            window = GLFW.CreateWindow(width, height, "Test2", MonitorPtr.Null, WindowPtr.Null);

            LoadVulkan();
            CreateInstance();
            PickPhysicalDevice();
            CreateSurface();
            GetIndices();
            CreateDevice();
        }

        void CreateInstance() {
            VkInstanceCreateInfo createInfo = new VkInstanceCreateInfo();
            createInfo.sType = VkStructureType.StructureTypeInstanceCreateInfo;

            string[] requiredExtensions = GLFW.GetRequiredInstanceExceptions();

            var extensionsMarshalled = new MarshalledStringArray(requiredExtensions);
            var layersMarshalled = new MarshalledStringArray(layers);

            createInfo.enabledExtensionCount = (uint)requiredExtensions.Length;
            createInfo.ppEnabledExtensionNames = extensionsMarshalled.Address;
            createInfo.enabledLayerCount = (uint)layers.Length;
            createInfo.ppEnabledLayerNames = layersMarshalled.Address;

            byte* info = stackalloc byte[Interop.SizeOf<VkInstanceCreateInfo>()];
            Interop.Marshal(info, createInfo);
            VkInstance temp;

            var result = createInstance((IntPtr)info, alloc, (IntPtr)(&temp));
            instance = temp;

            extensionsMarshalled.Dispose();
            layersMarshalled.Dispose();
        }

        void PickPhysicalDevice() {
            uint count = 0;
            enumeratePhysicalDevices(instance, ref count, IntPtr.Zero);
            byte* marshalled = stackalloc byte[(int)count * Interop.SizeOf<VkPhysicalDevice>()];
            enumeratePhysicalDevices(instance, ref count, (IntPtr)marshalled);

            physicalDevice = Interop.Unmarshal<VkPhysicalDevice>(marshalled);
        }

        void CreateSurface() {
            VkSurfaceKHR temp;
            GLFW.CreateWindowSurface(instance, window, alloc, (IntPtr)(&temp));
            surface = temp;
        }

        void GetIndices() {
            int gIndex = -1;
            int pIndex = -1;

            uint count = 0;
            getQueueFamilyProperties(physicalDevice, ref count, IntPtr.Zero);
            byte* marshalled = stackalloc byte[(int)count * Interop.SizeOf<VkQueueFamilyProperties>()];
            getQueueFamilyProperties(physicalDevice, ref count, (IntPtr)marshalled);

            for (int i = 0; i < count; i++) {
                var queue = Interop.Unmarshal<VkQueueFamilyProperties>(marshalled, i);
                if (gIndex == -1 && (queue.queueFlags & VkQueueFlags.QueueGraphicsBit) != 0) {
                    gIndex = i;
                }
                if (pIndex == -1) {
                    uint supported = 0;
                    getSurfaceSupported(physicalDevice, (uint)i, surface, ref supported);
                    if (supported != 0) {
                        pIndex = i;
                    }
                }
            }

            graphicsIndex = (uint)gIndex;
            presentIndex = (uint)pIndex;
        }

        void CreateDevice() {

        }

        void LoadVulkan() {
            Vulkan.Load(ref createInstance);
            Vulkan.Load(ref destroyInstance);
            Vulkan.Load(ref enumeratePhysicalDevices);
            Vulkan.Load(ref getQueueFamilyProperties);
            Vulkan.Load(ref getSurfaceSupported);
            Vulkan.Load(ref destroySurface);
        }
    }
}
