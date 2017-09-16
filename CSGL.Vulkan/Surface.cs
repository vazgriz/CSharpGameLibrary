using System;
using System.Collections.Generic;

using CSGL.GLFW;
using CSGL.GLFW.Unmanaged;

namespace CSGL.Vulkan {
    public class Surface : IDisposable, INative<Unmanaged.VkSurfaceKHR> {
        Unmanaged.VkSurfaceKHR surface;
        bool disposed = false;

        public Instance Instance { get; private set; }

        public Unmanaged.VkSurfaceKHR Native {
            get {
                return surface;
            }
        }

        public Surface(Instance instance, Window window) {
            if (instance == null) throw new ArgumentNullException(nameof(instance));
            if (window == null) throw new ArgumentNullException(nameof(window));

            Init(instance, window.Native);
        }

        public Surface(Instance instance, WindowPtr window) {
            if (instance == null) throw new ArgumentNullException(nameof(instance));
            if (window == WindowPtr.Null) throw new ArgumentNullException(nameof(window));

            Init(instance, window);
        }

        void Init(Instance instance, WindowPtr window) {
            Instance = instance;

            CreateSurface(window);
        }

        void CreateSurface(WindowPtr window) {
            var result = (VkResult)GLFW.GLFW.CreateWindowSurface(Instance.Native.native, window, Instance.AllocationCallbacks, out surface.native);
            if (result != VkResult.Success) throw new SurfaceException(result, string.Format("Error creating surface: {0}", result));
        }

        public Unmanaged.VkSurfaceCapabilitiesKHR GetCapabilities(PhysicalDevice physicalDevice) {
            unsafe {
                Unmanaged.VkSurfaceCapabilitiesKHR cap;
                Instance.Commands.getCapabilities(physicalDevice.Native, surface, (IntPtr)(&cap));
                return cap;
            }
        }

        public IList<Unmanaged.VkSurfaceFormatKHR> GetFormats(PhysicalDevice physicalDevice) {
            unsafe {
                var formats = new List<Unmanaged.VkSurfaceFormatKHR>();

                uint count = 0;
                Instance.Commands.getFormats(physicalDevice.Native, surface, ref count, IntPtr.Zero);
                var formatsNative = stackalloc Unmanaged.VkSurfaceFormatKHR[(int)count];
                Instance.Commands.getFormats(physicalDevice.Native, surface, ref count, (IntPtr)formatsNative);
                
                for (int i = 0; i < count; i++) {
                    formats.Add(formatsNative[i]);
                }

                return formats;
            }
        }

        public IList<VkPresentModeKHR> GetPresentModes(PhysicalDevice physicalDevice) {
            unsafe {
                var presentModes = new List<VkPresentModeKHR>();

                uint count = 0;
                Instance.Commands.getModes(physicalDevice.Native, surface, ref count, IntPtr.Zero);
                var presentModesNative = stackalloc VkPresentModeKHR[(int)count];
                Instance.Commands.getModes(physicalDevice.Native, surface, ref count, (IntPtr)presentModesNative);
                
                for (int i = 0; i < count; i++) {
                    presentModes.Add(presentModesNative[i]);
                }

                return presentModes;
            }
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        void Dispose(bool disposing) {
            if (disposed) return;

            Instance.Commands.destroySurface(Instance.Native, surface, Instance.AllocationCallbacks);

            disposed = true;
        }

        ~Surface() {
            Dispose(false);
        }
    }

    public class SurfaceException : VulkanException {
        public SurfaceException(VkResult result, string message) : base(result, message) { }
    }
}
