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

            using (var capMarshalled = new Marshalled<VkSurfaceCapabilitiesKHR>()) {
                getCapabilities(physicalDevice.Native, surface, capMarshalled.Address);
                Capabilities = capMarshalled.Value;
            }

            GetFormats();
            GetModes();
        }

        void CreateSurface(WindowPtr window) {
            var result = GLFW.GLFW.CreateWindowSurface(Instance.Native, window, Instance.AllocationCallbacks, out surface);
            if (result != VkResult.Success) throw new SurfaceException(string.Format("Error creating surface: {0}", result));
        }

        void GetFormats() {
            Formats = new List<VkSurfaceFormatKHR>();

            uint count = 0;
            getFormats(physicalDevice.Native, surface, ref count, IntPtr.Zero);
            var formats = new MarshalledArray<VkSurfaceFormatKHR>((int)count);
            getFormats(physicalDevice.Native, surface, ref count, formats.Address);

            for (int i = 0; i <count; i++) {
                var format = formats[i];
                Formats.Add(format);
            }

            formats.Dispose();
        }

        void GetModes() {
            Modes = new List<VkPresentModeKHR>();

            uint count = 0;
            getModes(physicalDevice.Native, surface, ref count, IntPtr.Zero);
            var modes = new int[(int)count];    //VkPresentModeKHR is an enum and can't be marshalled directly
            var modesMarshalled = new PinnedArray<int>(modes);
            getModes(physicalDevice.Native, surface, ref count, modesMarshalled.Address);

            for (int i = 0; i < count; i++) {
                var mode = (VkPresentModeKHR)modes[i];
                Modes.Add(mode);
            }
        }

        public void Dispose() {
            if (disposed) return;
            
            Instance.Commands.destroySurface(Instance.Native, surface, Instance.AllocationCallbacks);

            disposed = true;
        }
    }

    public class SurfaceException : Exception {
        public SurfaceException(string message) : base(message) { }
    }
}
