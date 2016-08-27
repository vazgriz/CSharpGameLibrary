using System;
using System.Collections.Generic;

namespace CSGL.Vulkan.Managed {
    public class InstanceCreateInfo : IDisposable {
        ApplicationInfo applicationInfo;
        List<string> extensions;
        List<string> layers;

        bool dirty = false;
        bool disposed = false;

        Marshalled<VkInstanceCreateInfo> marshalled;
        MarshalledArray<IntPtr> extensionsMarshalled;
        MarshalledArray<IntPtr> layersMarshalled;

        List<MarshalledArray<byte>> extensionStringsMarshalled;
        List<MarshalledArray<byte>> layerStringsMarshalled;

        public ApplicationInfo ApplicationInfo {
            get {
                return applicationInfo;
            }
            set {
                applicationInfo = value;
                dirty = true;
            }
        }

        public List<string> Extensions {
            get {
                return extensions;
            }
            set {
                extensions = value;
                dirty = true;
            }
        }

        public List<string> Layers {
            get {
                return layers;
            }
            set {
                layers = value;
                dirty = true;
            }
        }

        public Marshalled<VkInstanceCreateInfo> Marshalled {
            get {
                if (dirty) {
                    Apply();
                }
                return marshalled;
            }
        }

        public InstanceCreateInfo(ApplicationInfo appInfo, List<string> extensions, List<string> layers) {
            ApplicationInfo = appInfo;
            Extensions = extensions;
            Layers = layers;

            marshalled = new Marshalled<VkInstanceCreateInfo>();
            extensionStringsMarshalled = new List<MarshalledArray<byte>>();
            layerStringsMarshalled = new List<MarshalledArray<byte>>();

            Apply();
        }

        public void Apply() {
            extensionsMarshalled?.Dispose();
            layersMarshalled?.Dispose();
            foreach (var s in extensionStringsMarshalled) s.Dispose();
            foreach (var s in layerStringsMarshalled) s.Dispose();
            extensionStringsMarshalled.Clear();
            layerStringsMarshalled.Clear();

            if (extensions == null) extensions = new List<string>();
            if (layers == null) layers = new List<string>();

            extensionsMarshalled = new MarshalledArray<IntPtr>(Extensions.Count);

            for (int i = 0; i < Extensions.Count; i++) {
                var s = Interop.GetUTF8(Extensions[i]);
                var ms = new MarshalledArray<byte>(s);
                extensionStringsMarshalled.Add(ms);
                extensionsMarshalled[i] = ms.Address;
            }

            layersMarshalled = new MarshalledArray<IntPtr>(Layers.Count);

            for (int i = 0; i < Layers.Count; i++) {
                var s = Interop.GetUTF8(Layers[i]);
                var ms = new MarshalledArray<byte>(s);
                layerStringsMarshalled.Add(ms);
                layersMarshalled[i] = ms.Address;
            }

            marshalled.Value = GetNative();
        }

        VkInstanceCreateInfo GetNative() {
            VkInstanceCreateInfo info = new VkInstanceCreateInfo();
            info.sType = VkStructureType.StructureTypeInstanceCreateInfo;
            info.pApplicationInfo = applicationInfo.Marshalled.Address;
            info.enabledExtensionCount = (uint)Extensions.Count;
            info.ppEnabledExtensionNames = extensionsMarshalled.Address;
            info.enabledLayerCount = (uint)Layers.Count;
            info.ppEnabledLayerNames = layersMarshalled.Address;

            return info;
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        void Dispose(bool disposing) {
            if (disposed) return;
            
            marshalled.Dispose();
            extensionsMarshalled.Dispose();
            layersMarshalled.Dispose();
            foreach (var s in extensionStringsMarshalled) s.Dispose();
            foreach (var s in layerStringsMarshalled) s.Dispose();

            if (disposing) {
                applicationInfo = null;
                extensions = null;
                layers = null;

                marshalled = null;
                extensionsMarshalled = null;
                layersMarshalled = null;

                extensionStringsMarshalled = null;
                layerStringsMarshalled = null;
            }
        }

        ~InstanceCreateInfo() {
            Dispose(false);
        }
    }
}
