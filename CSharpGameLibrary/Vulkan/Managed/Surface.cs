using System;

using CSGL.GLFW;
using CSGL.Vulkan.Unmanaged;

namespace CSGL.Vulkan.Managed {
    public class Surface : IDisposable {
        VkSurfaceKHR surface;
        bool disposed = false;

        vkDestroySurfaceKHRDelegate destroySurface;
        Instance instance;

        public Surface(Instance instance, WindowPtr window) {
            if (instance == null) throw new ArgumentException(string.Format("{0} can not be null", nameof(instance)));
            if (window == WindowPtr.Null) throw new ArgumentException(string.Format("{0} can not be null", nameof(window)));

            this.instance = instance;

            Vulkan.Load(ref destroySurface, instance);

            unsafe
            {
                var result = GLFW.GLFW.CreateWindowSurface(instance.Native, window, instance.AllocationCallbacks, ref surface);
                if (result != VkResult.Success) throw new SurfaceException(string.Format("Error creating surface: {0}", result));
            }
        }

        public VkSurfaceKHR Native {
            get {
                return surface;
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
                destroySurface(instance.Native, surface, instance.AllocationCallbacks);
            }

            if (disposing) {
                instance = null;
                destroySurface = null;
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
