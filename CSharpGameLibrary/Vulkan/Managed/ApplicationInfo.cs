using System;

using CSGL.Vulkan.Unmanaged;

namespace CSGL.Vulkan.Managed {
    public class ApplicationInfo : IDisposable {
        VkVersion apiVersion;
        VkVersion engineVersion;
        VkVersion applicationVersion;
        string engineName;
        string applicationName;

        bool disposed = false;
        bool dirty = false;

        MarshalledArray<byte> engineNameMarshalled;
        MarshalledArray<byte> applicationNameMarshalled;
        Marshalled<VkApplicationInfo> marshalled;

        public VkVersion APIVersion {
            get {
                return apiVersion;
            }
            set {
                apiVersion = value;
                dirty = true;
            }
        }

        public VkVersion EngineVersion {
            get {
                return engineVersion;
            }
            set {
                engineVersion = value;
                dirty = true;
            }
        }
        public VkVersion ApplicationVersion {
            get {
                return applicationVersion;
            }
            set {
                applicationVersion = value;
                dirty = true;
            }
        }

        public string EngineName {
            get {
                return engineName;
            }
            set {
                engineName = value;
                dirty = true;
            }
        }

        public string ApplicationName {
            get {
                return applicationName;
            }
            set {
                applicationName = value;
                dirty = true;
            }
        }

        public Marshalled<VkApplicationInfo> Marshalled {
            get {
                if (dirty) {
                    Apply();
                }
                return marshalled;
            }
        }

        public ApplicationInfo(VkVersion APIVersion, string appName, string engineName, VkVersion appVersion, VkVersion engineVersion) {
            this.APIVersion = APIVersion;
            ApplicationName = appName;
            EngineName = engineName;
            ApplicationVersion = appVersion;
            EngineVersion = engineVersion;

            marshalled = new Marshalled<VkApplicationInfo>();

            Apply();
        }

        public void Apply() {
            if (engineNameMarshalled != null) engineNameMarshalled.Dispose();
            if (applicationNameMarshalled != null) applicationNameMarshalled.Dispose();
            engineNameMarshalled = new MarshalledArray<byte>(Interop.GetUTF8(EngineName));
            applicationNameMarshalled = new MarshalledArray<byte>(Interop.GetUTF8(ApplicationName));
            marshalled.Value = GetNative();

            dirty = false;
        }

        VkApplicationInfo GetNative() {
            VkApplicationInfo info = new VkApplicationInfo();
            info.sType = VkStructureType.StructureTypeApplicationInfo;
            info.apiVersion = APIVersion;
            info.engineVersion = EngineVersion;
            info.applicationVersion = ApplicationVersion;
            info.pEngineName = engineNameMarshalled.Address;
            info.pApplicationName = applicationNameMarshalled.Address;

            return info;
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        void Dispose(bool disposing) {
            if (disposed) return;

            engineNameMarshalled.Dispose();
            applicationNameMarshalled.Dispose();
            marshalled.Dispose();

            if (disposing) {
                engineName = null;
                applicationName = null;
                engineNameMarshalled = null;
                applicationNameMarshalled = null;
                marshalled = null;
            }
        }

        ~ApplicationInfo() {
            Dispose(false);
        }
    }
}
