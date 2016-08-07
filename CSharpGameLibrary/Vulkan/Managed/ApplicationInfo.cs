using System;

using CSGL.Vulkan.Unmanaged;

namespace CSGL.Vulkan.Managed {
    public class ApplicationInfo {
        public VkVersion APIVersion { get; set; }
        public VkVersion EngineVersion { get; set; }
        public VkVersion ApplicationVersion { get; set; }
        public string EngineName { get; set; }
        public string ApplicationName { get; set; }

        public ApplicationInfo(VkVersion APIVersion, string appName, string engineName, VkVersion appVersion, VkVersion engineVersion) {
            this.APIVersion = APIVersion;
            ApplicationName = appName;
            EngineName = engineName;
            ApplicationVersion = appVersion;
            EngineVersion = engineVersion;
        }
    }
}
