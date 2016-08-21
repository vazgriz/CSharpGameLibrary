using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

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

            IntPtr ptr = Marshal.AllocHGlobal(Marshal.SizeOf<VkSurfaceCapabilitiesKHR>());
            getCapabilities(physicalDevice.Native, surface, ptr);
            Capabilities = Marshal.PtrToStructure<VkSurfaceCapabilitiesKHR>(ptr);
            Marshal.FreeHGlobal(ptr);

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
                getFormats(physicalDevice.Native, surface, ref count, IntPtr.Zero);
                var formats = stackalloc byte[Marshal.SizeOf<VkSurfaceFormatKHR>() * (int)count];
                getFormats(physicalDevice.Native, surface, ref count, (IntPtr)formats);

                for (int i = 0; i <count; i++) {
                    var format = Marshal.PtrToStructure<VkSurfaceFormatKHR>((IntPtr)formats + Marshal.SizeOf<VkSurfaceFormatKHR>() * i);
                    Formats.Add(format);
                }
            }
        }

        void GetModes() {
            Modes = new List<VkPresentModeKHR>();
            unsafe
            {
                uint count = 0;
                getModes(physicalDevice.Native, surface, ref count, IntPtr.Zero);
                var modes = stackalloc byte[sizeof(int) * (int)count];  //VkPresentModeKHR is an enum and can't be marshalled directly
                getModes(physicalDevice.Native, surface, ref count, (IntPtr)modes);

                for (int i = 0; i < count; i++) {
                    var mode = (VkPresentModeKHR)Marshal.PtrToStructure<int>((IntPtr)modes + sizeof(int) * i);
                    Modes.Add(mode);
                }
            }
        }

        public void Dispose() {
            if (disposed) return;

            unsafe
            {
                Instance.Commands.destroySurface(Instance.Native, surface, Instance.AllocationCallbacks);
            }

            disposed = true;
        }
    }

    public class SurfaceException : Exception {
        public SurfaceException(string message) : base(message) { }
    }
}
