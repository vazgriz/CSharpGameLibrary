using System;
using System.Collections.Generic;

using CSGL.GLFW;
using CSGL.Vulkan.Unmanaged;

namespace CSGL.Vulkan {
    public class Surface : IDisposable, INative<VkSurfaceKHR> {
        VkSurfaceKHR surface;
        bool disposed = false;

        PhysicalDevice physicalDevice;
        
        vkGetPhysicalDeviceSurfaceCapabilitiesKHRDelegate getCapabilities = null;
        vkGetPhysicalDeviceSurfaceFormatsKHRDelegate getFormats = null;
        vkGetPhysicalDeviceSurfacePresentModesKHRDelegate getModes = null;

        public Instance Instance { get; private set; }

        public IList<VkSurfaceFormatKHR> Formats { get; private set; }
        public IList<VkPresentModeKHR> PresentModes { get; private set; }
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
            var result = GLFW_VK.CreateWindowSurface(Instance.Native, window, Instance.AllocationCallbacks, out surface);
            if (result != VkResult.Success) throw new SurfaceException(string.Format("Error creating surface: {0}", result));
        }

        void GetFormats() {
            var formats = new List<VkSurfaceFormatKHR>();

            uint count = 0;
            getFormats(physicalDevice.Native, surface, ref count, IntPtr.Zero);
            var formatsNative = new NativeArray<VkSurfaceFormatKHR>((int)count);
            getFormats(physicalDevice.Native, surface, ref count, formatsNative.Address);

            using (formatsNative) {
                for (int i = 0; i < count; i++) {
                    var format = formatsNative[i];
                    formats.Add(format);
                }
            }

            Formats = formats.AsReadOnly();
        }

        void GetModes() {
            var presentModes = new List<VkPresentModeKHR>();

            uint count = 0;
            getModes(physicalDevice.Native, surface, ref count, IntPtr.Zero);
            var modes = new int[(int)count];    //VkPresentModeKHR is an enum and can't be marshalled directly
            var modesMarshalled = new PinnedArray<int>(modes);
            getModes(physicalDevice.Native, surface, ref count, modesMarshalled.Address);

            for (int i = 0; i < count; i++) {
                var mode = (VkPresentModeKHR)modes[i];
                presentModes.Add(mode);
            }

            PresentModes = presentModes.AsReadOnly();
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
