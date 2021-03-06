﻿using System;
using System.Runtime.InteropServices;

namespace CSGL.Vulkan {
    public delegate void VkDebugReportCallbackDelegate(
        VkDebugReportFlagsEXT flags,
        VkDebugReportObjectTypeEXT objectType,
        long _object,
        long location,
        int messageCode,
        string layerPrefix,
        string message
    );

    public class VkDebugReportCallbackCreateInfo {
        public VkDebugReportFlagsEXT flags;
        public VkDebugReportCallbackDelegate callback;
    }

    public class VkDebugReportCallback : IDisposable, INative<Unmanaged.VkDebugReportCallbackEXT> {
        Unmanaged.VkDebugReportCallbackEXT callback;
        bool disposed;

        public VkInstance Instance { get; private set; }

        public Unmanaged.VkDebugReportCallbackEXT Native {
            get {
                return callback;
            }
        }

        public VkDebugReportFlagsEXT Flags { get; private set; }
        public event VkDebugReportCallbackDelegate Callback = delegate { };

        delegate uint InternalCallbackDelegate(
            VkDebugReportFlagsEXT flags,
            VkDebugReportObjectTypeEXT objectType,
            ulong _object,
            IntPtr location,
            int messageCode,
            IntPtr layerPrefix,
            IntPtr message,
            IntPtr userData
        );
        InternalCallbackDelegate internalCallback;

        public VkDebugReportCallback(VkInstance instance, VkDebugReportCallbackCreateInfo info) {
            if (instance == null) throw new ArgumentNullException(nameof(instance));
            if (info == null) throw new ArgumentNullException(nameof(info));

            Instance = instance;
            internalCallback = InternalCallback;

            CreateCallback(info);

            Callback = info.callback;
            Flags = info.flags;
        }

        void CreateCallback(VkDebugReportCallbackCreateInfo mInfo) {
            var info = new Unmanaged.VkDebugReportCallbackCreateInfoEXT();
            info.sType = VkStructureType.DebugReportCallbackCreateInfoExt;
            info.flags = mInfo.flags;
            info.pfnCallback = Marshal.GetFunctionPointerForDelegate(internalCallback);

            var result = Instance.Commands.createDebugReportCallback(Instance.Native, ref info, Instance.AllocationCallbacks, out callback);
            if (result != VkResult.Success) throw new DebugReportCallbackException(result, string.Format("Error creating debug report callback: {0}", result));
        }

        uint InternalCallback(
            VkDebugReportFlagsEXT flags,
            VkDebugReportObjectTypeEXT objectType,
            ulong _object,
            IntPtr location,    //size_t in native code
            int messageCode,
            IntPtr layerPrefix,
            IntPtr message,
            IntPtr userData)    //ignored
        {
            ulong _location = (ulong)location;
            string _layerPrefix = Interop.GetString(layerPrefix);
            string _message = Interop.GetString(message);

            Callback?.Invoke(flags, objectType, (long)_object, (long)_location, messageCode, _layerPrefix, _message);

            //specification allows the callback to set this value
            //however C# delegates are multicast, so potentially there is no single value to return.
            //specification also says application *should* return false, so just do that instead
            return 0;
        }

        public static void ReportMessage(
            VkInstance instance,
            VkDebugReportFlagsEXT flags,
            VkDebugReportObjectTypeEXT objectType,
            ulong _object,
            ulong location,
            int messageCode,
            string layerPrefix,
            string message)
        {
            if (instance == null) throw new ArgumentNullException(nameof(instance));

            IntPtr _location = (IntPtr)location;
            byte[] _layerPrefix = Interop.GetUTF8(layerPrefix);
            byte[] _message = Interop.GetUTF8(message);

            unsafe {
                fixed (byte* layerPtr = _layerPrefix)
                fixed (byte* messagePtr = _message) {
                    instance.Commands.debugReportMessage(instance.Native, flags, objectType, _object, _location, messageCode, (IntPtr)layerPtr, (IntPtr)messagePtr);
                }
            }
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

        ~VkDebugReportCallback() {
            Dispose(false);
        }
    }

    public class DebugReportCallbackException : VulkanException {
        public DebugReportCallbackException(VkResult result, string message) : base(result, message) { }
    }
}
