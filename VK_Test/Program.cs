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
            using (var instance = new Instance(info)) {
                Console.WriteLine("Activated extensions:");
                foreach (var s in instance.Extensions) {
                    Console.WriteLine("  {0}", s);
                }
                Console.WriteLine("Activated layers:");
                foreach (var s in instance.Layers) {
                    Console.WriteLine("  {0}", s);
                }
                Console.WriteLine("Physical devices");
                foreach (var p in instance.PhysicalDevices) {
                    Console.WriteLine("  {0}", p.Name);
                    Console.WriteLine("    API Version: {0}", p.Properties.APIVersion);
                    Console.WriteLine("    Device Type: {0}", p.Properties.Type);
                    Console.WriteLine("    Driver Version: {0}", p.Properties.DriverVersion);
                    Console.WriteLine("    UUID: {0}", p.Properties.PipelineCache.ToString());
                    Console.WriteLine("    Available extensions:");
                    foreach (var e in p.AvailableExtensions) {
                        Console.WriteLine("      {0}", e.Name);
                    }
                }

                var pDevice = instance.PhysicalDevices[0];
                QueueCreateInfo qInfo = new QueueCreateInfo((uint)pDevice.GraphicsIndex, 1, new float[] { 1f });
                List<string> dEx = new List<string> {
                    "VK_KHR_swapchain"
                };
                List<QueueCreateInfo> qInfos = new List<QueueCreateInfo>{
                    qInfo
                };
                DeviceCreateInfo dInfo = new DeviceCreateInfo(dEx, qInfos);

                using (Device device = new Device(pDevice, dInfo)) {

                }
            }
            GLFW.Terminate();
        }
    }
}
