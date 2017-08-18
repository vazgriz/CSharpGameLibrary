using System;
using System.Runtime.InteropServices;

namespace CSGL.Vulkan1 {
    public delegate void DebugReportCallbackDelegate(
        VkDebugReportFlagsEXT flags,
        VkDebugReportObjectTypeEXT objectType,
        ulong _object,
        ulong location,
        string layerPrefix,
        string message
    );

    public class DebugReportCallbackCreateInfo {
        public VkDebugReportFlagsEXT flags;
        public DebugReportCallbackDelegate callback;
    }

    public class DebugReportCallback : IDisposable, INative<VkDebugReportCallbackEXT> {
        VkDebugReportCallbackEXT callback;
        bool disposed;

        public Instance Instance { get; private set; }

        public VkDebugReportCallbackEXT Native {
            get {
                return callback;
            }
        }

        public VkDebugReportFlagsEXT Flags { get; private set; }
        public DebugReportCallbackDelegate Callback { get; private set; }

        delegate bool InternalCallbackDelegate(
            VkDebugReportFlagsEXT flags,
            VkDebugReportObjectTypeEXT objectType,
            ulong _object,
            IntPtr location,
            IntPtr layerPrefix,
            IntPtr message,
            IntPtr userData
        );
        InternalCallbackDelegate internalCallback;

        public DebugReportCallback(Instance instance, DebugReportCallbackCreateInfo info) {
            if (instance == null) throw new ArgumentNullException(nameof(instance));
            if (info == null) throw new ArgumentNullException(nameof(info));

            Instance = instance;
            internalCallback = InternalCallback;

            CreateCallback(info);
        }

        void CreateCallback(DebugReportCallbackCreateInfo mInfo) {
            var info = new VkDebugReportCallbackCreateInfoEXT();
            info.sType = VkStructureType.DebugReportCallbackCreateInfoExt;
            info.flags = mInfo.flags;
            info.pfnCallback = Marshal.GetFunctionPointerForDelegate(internalCallback);

            var result = Instance.Commands.createDebugReportCallback(Instance.Native, ref info, Instance.AllocationCallbacks, out callback);
            if (result != VkResult.Success) throw new DebugReportCallbackException(string.Format("Error creating debug report callback: {0}", result));
        }

        bool InternalCallback(
            VkDebugReportFlagsEXT flags, 
            VkDebugReportObjectTypeEXT objectType,
            ulong _object,
            IntPtr location,    //size_t in native code
            IntPtr layerPrefix,
            IntPtr message,
            IntPtr userData)    //ignored
        {
            ulong _location = (ulong)location;
            string _layerPrefix = Interop.GetString(layerPrefix);
            string _message = Interop.GetString(message);

            Callback(flags, objectType, _object, _location, _layerPrefix, _message);

            //specification allows the callback to set this value
            //however C# delegates are multicast, so potentially there is no single value to return.
            //specification also says application *should* return false, so just do that instead
            return false;
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        void Dispose(bool disposing) {
            if (disposed) return;

            Instance.Commands.destroyDebugReportCallback(Instance.Native, callback, Instance.AllocationCallbacks);
            disposed = true;
        }

        ~DebugReportCallback() {
            Dispose(false);
        }
    }

    public class DebugReportCallbackException : Exception {
        public DebugReportCallbackException(string message) : base(message) { }
    }
}
