using System;

using CSGL.GLFW;
using CSGL.Vulkan;
using CSGL.Vulkan.Managed;
using CSGL.Vulkan.Unmanaged;

namespace VK_Test2 {
    class Program : IDisposable {
        static void Main(string[] args) {
            using (var p = new Program()) {
                p.Run();
            }
        }

        int width = 800;
        int height = 600;

        IntPtr alloc = IntPtr.Zero;

        VkInstance instance;

        vkCreateInstanceDelegate createInstance;
        vkDestroyInstanceDelegate destroyInstance;

        public void Dispose() {
            destroyInstance(instance, alloc);
        }

        public void Run() {
            GLFW.Init();
            GLFW.WindowHint(WindowHint.ClientAPI, (int)ContextAPI.NoAPI);
            WindowPtr window = GLFW.CreateWindow(width, height, "Test2", MonitorPtr.Null, WindowPtr.Null);

            LoadVulkan();

            CreateInstance();

            GLFW.Terminate();
        }

        void CreateInstance() {
            VkInstanceCreateInfo createInfo = new VkInstanceCreateInfo();
            createInfo.sType = VkStructureType.StructureTypeInstanceCreateInfo;

            var result = createInstance(ref createInfo, alloc, out instance);
            Console.WriteLine("Instance creation: {0}", result);
        }

        void LoadVulkan() {
            Vulkan.Load(ref createInstance);
            Vulkan.Load(ref destroyInstance);
        }
    }
}
