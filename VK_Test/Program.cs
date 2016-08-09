using System;
using System.Collections.Generic;

using CSGL.GLFW;
using CSGL.Vulkan;
using CSGL.Vulkan.Managed;

namespace VK_Test {
    class Program {
        static void Main(string[] args) {
            GLFW.Init();
            Vulkan.Init();

            List<string> ex = new List<string>(GLFW.GetRequiredInstanceExceptions());
            ex.Add("VK_EXT_debug_report");

            List<string> l = new List<string> {
                "VK_LAYER_LUNARG_standard_validation"
            };
            ApplicationInfo app = new ApplicationInfo(new VkVersion(), "Test", "None", new VkVersion(), new VkVersion());
            InstanceCreateInfo info = new InstanceCreateInfo(app, ex, l);

            WindowPtr window;

            window = GLFW.CreateWindow(800, 600, "Test", MonitorPtr.Null, WindowPtr.Null);

            using (var instance = new Instance(info)) {
                var pDevice = instance.PhysicalDevices[0];
                Surface surface = new Surface(pDevice, window);

                int graphicsIndex = -1;
                int presentIndex = -1;
                for (int i = 0; i < pDevice.QueueFamilies.Count; i++) {
                    var q = pDevice.QueueFamilies[i];
                    if (graphicsIndex == -1 && (q.Flags & VkQueueFlags.QueueGraphicsBit) != 0) {
                        graphicsIndex = i;
                    }
                    if (presentIndex == -1 && (q.SurfaceSupported(surface))) {
                        presentIndex = i;
                    }
                }
                QueueCreateInfo qInfo = new QueueCreateInfo((uint)graphicsIndex, 1, new float[] { 1f });
                List<string> dEx = new List<string> {
                    "VK_KHR_swapchain"
                };
                List<QueueCreateInfo> qInfos = new List<QueueCreateInfo>{
                    qInfo
                };
                DeviceCreateInfo dInfo = new DeviceCreateInfo(dEx, qInfos);

                using (Device device = new Device(pDevice, dInfo)) {
                    Queue q = device.GetQueue((uint)graphicsIndex, 0);
                    Queue presentQ = device.GetQueue((uint)presentIndex, 0);

                    Console.WriteLine("Formats:");
                    foreach (var f in surface.Formats) {
                        Console.WriteLine("  {0}", f.format);
                    }

                    Console.WriteLine("Modes:");
                    foreach (var m in surface.Modes) {
                        Console.WriteLine("  {0}", m);
                    }

                    using (surface) {
                        while (!GLFW.WindowShouldClose(window)) {
                            GLFW.PollEvents();
                        }
                    }
                    GLFW.DestroyWindow(window);
                }
            }
            GLFW.Terminate();
        }
    }
}
