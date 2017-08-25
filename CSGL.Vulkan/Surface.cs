using System;
using System.Collections.Generic;

using CSGL.GLFW;
using CSGL.GLFW.Unmanaged;
using CSGL.Vulkan.Unmanaged;

namespace CSGL.Vulkan {
    public class Surface : IDisposable, INative<VkSurfaceKHR> {
        VkSurfaceKHR surface;
        bool disposed = false;

        public Instance Instance { get; private set; }

        public VkSurfaceKHR Native {
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
            if (result != VkResult.Success) throw new SurfaceException(string.Format("Error creating surface: {0}", result));
        }

        public VkSurfaceCapabilitiesKHR GetCapabilities(PhysicalDevice physicalDevice) {
            unsafe
            {
                VkSurfaceCapabilitiesKHR cap;
                Instance.Commands.getCapabilities(physicalDevice.Native, surface, (IntPtr)(&cap));
                return cap;
            }
        }

        public List<VkSurfaceFormatKHR> GetFormats(PhysicalDevice physicalDevice) {
            var formats = new List<VkSurfaceFormatKHR>();

            uint count = 0;
            Instance.Commands.getFormats(physicalDevice.Native, surface, ref count, IntPtr.Zero);
            var formatsNative = new NativeArray<VkSurfaceFormatKHR>((int)count);
            Instance.Commands.getFormats(physicalDevice.Native, surface, ref count, formatsNative.Address);

            using (formatsNative) {
                for (int i = 0; i < count; i++) {
                    var format = formatsNative[i];
                    formats.Add(format);
                }
            }

            return formats;
        }

        public List<VkPresentModeKHR> GetModes(PhysicalDevice physicalDevice) {
            var presentModes = new List<VkPresentModeKHR>();

            uint count = 0;
            Instance.Commands.getModes(physicalDevice.Native, surface, ref count, IntPtr.Zero);
            var modes = new int[(int)count];    //VkPresentModeKHR is an enum and can't be marshalled directly
            var modesMarshalled = new PinnedArray<int>(modes);
            Instance.Commands.getModes(physicalDevice.Native, surface, ref count, modesMarshalled.Address);

            using (modesMarshalled) {
                for (int i = 0; i < count; i++) {
                    var mode = (VkPresentModeKHR)modes[i];
                    presentModes.Add(mode);
                }
            }

            return presentModes;
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

    public class SurfaceException : Exception {
        public SurfaceException(string message) : base(message) { }
    }
}
