using System;
using System.Collections.Generic;

using CSGL.GLFW;
using CSGL.Vulkan.Managed;

namespace VK_Test {
    class Program {
        static void Main(string[] args) {
            GLFW.Init();
            Vulkan.Init();

            Console.WriteLine("Available extenions:");
            foreach (var s in Instance.AvailableExtensions) {
                Console.WriteLine("  {0}", s);
            }

            Console.WriteLine("Available layers:");
            foreach (var s in Instance.AvailableLayers) {
                Console.WriteLine("  {0}", s);
            }

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
            }
            GLFW.Terminate();
        }
    }
}
