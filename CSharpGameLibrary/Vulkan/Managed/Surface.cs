using System;
using System.Collections.Generic;

using CSGL.GLFW;
using CSGL.Vulkan.Unmanaged;

namespace CSGL.Vulkan.Managed {
    public class Surface : IDisposable {
        VkSurfaceKHR surface;
        bool disposed = false;

        PhysicalDevice physicalDevice;

        vkDestroySurfaceKHRDelegate destroySurface;
        vkGetPhysicalDeviceSurfaceCapabilitiesKHRDelegate getCapabilities;
        vkGetPhysicalDeviceSurfaceFormatsKHRDelegate getFormats;
        vkGetPhysicalDeviceSurfacePresentModesKHRDelegate getModes;

        public Instance Instance { get; private set; }

        public List<VkSurfaceFormatKHR> Formats { get; private set; }
        public List<VkPresentModeKHR> Modes { get; private set; }

        VkSurfaceCapabilitiesKHR capabilities;
        public VkSurfaceCapabilitiesKHR Capabilities {
            get {
                return capabilities;
            }
        }

        public VkSurfaceKHR Native {
            get {
                return surface;
            }
        }

        public Surface(PhysicalDevice device, WindowPtr window) {
            if (device == null) throw new ArgumentException(string.Format("{0} can not be null", nameof(Instance)));
            if (window == WindowPtr.Null) throw new ArgumentException(string.Format("{0} can not be null", nameof(window)));

            physicalDevice = device;
            Instance = device.Instance;

            Vulkan.Load(ref destroySurface, Instance);
            Vulkan.Load(ref getCapabilities, Instance);
            Vulkan.Load(ref getFormats, Instance);
            Vulkan.Load(ref getModes, Instance);

            CreateSurface(window);

            getCapabilities(physicalDevice.Native, surface, ref capabilities);

            GetFormats();
            GetModes();
        }

        void CreateSurface(WindowPtr window) {
            unsafe
            {
                var result = GLFW.GLFW.CreateWindowSurface(Instance.Native, window, Instance.AllocationCallbacks, ref surface);
                if (result != VkResult.Success) throw new SurfaceException(string.Format("Error creating surface: {0}", result));
            }
        }

        void GetFormats() {
            Formats = new List<VkSurfaceFormatKHR>();
            unsafe {
                uint count = 0;
                VkSurfaceFormatKHR* temp = null;
                getFormats(physicalDevice.Native, surface, ref count, ref *temp);
                var formats = stackalloc VkSurfaceFormatKHR[(int)count];
                getFormats(physicalDevice.Native, surface, ref count, ref formats[0]);

                for (int i = 0; i <count; i++) {
                    Formats.Add(formats[i]);
                }
            }
        }

        void GetModes() {
            Modes = new List<VkPresentModeKHR>();
            unsafe
            {
                uint count = 0;
                VkPresentModeKHR* temp = null;
                getModes(physicalDevice.Native, surface, ref count, ref *temp);
                var modes = stackalloc VkPresentModeKHR[(int)count];
                getModes(physicalDevice.Native, surface, ref count, ref modes[0]);

                for (int i = 0; i < count; i++) {
                    Modes.Add(modes[i]);
                }
            }
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        void Dispose(bool disposing) {
            if (disposed) return;
            unsafe
            {
                destroySurface(Instance.Native, surface, Instance.AllocationCallbacks);
            }

            if (disposing) {
                Instance = null;
                destroySurface = null;
                getCapabilities = null;
                getFormats = null;
                getModes = null;
            }

            disposed = true;
        }

        ~Surface() {
            Dispose(false);
        }
    }

    public class SurfaceException : Exception {
        public SurfaceException(string message) : base(message) { }
    }
}
