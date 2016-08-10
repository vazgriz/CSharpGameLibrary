using System;
using System.Collections.Generic;

using CSGL.GLFW;
using CSGL.Vulkan.Unmanaged;

namespace CSGL.Vulkan.Managed {
    public class Surface : IDisposable {
        VkSurfaceKHR surface;
        bool disposed = false;

        PhysicalDevice physicalDevice;
        
        vkGetPhysicalDeviceSurfaceCapabilitiesKHRDelegate getCapabilities = null;
        vkGetPhysicalDeviceSurfaceFormatsKHRDelegate getFormats = null;
        vkGetPhysicalDeviceSurfacePresentModesKHRDelegate getModes = null;

        public Instance Instance { get; private set; }

        public List<VkSurfaceFormatKHR> Formats { get; private set; }
        public List<VkPresentModeKHR> Modes { get; private set; }
        public VkSurfaceCapabilitiesKHR Capabilities { get; private set; }

        public VkSurfaceKHR Native {
            get {
                return surface;
            }
        }

        public Surface(PhysicalDevice device, WindowPtr window) {
            if (device == null) throw new ArgumentException(string.Format("{0} can not be null", nameof(device)));
            if (window == WindowPtr.Null) throw new ArgumentException(string.Format("{0} can not be null", nameof(window)));

            physicalDevice = device;
            Instance = device.Instance;
            
            getCapabilities = Instance.Commands.getCapabilities;
            getFormats = Instance.Commands.getFormats;
            getModes = Instance.Commands.getModes;

            CreateSurface(window);

            VkSurfaceCapabilitiesKHR temp = new VkSurfaceCapabilitiesKHR();
            getCapabilities(physicalDevice.Native, surface, ref temp);
            Capabilities = temp;

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
                Instance.Commands.destroySurface(Instance.Native, surface, Instance.AllocationCallbacks);
            }

            if (disposing) {
                Instance = null;
                
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
