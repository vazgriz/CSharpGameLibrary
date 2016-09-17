using System;

using CSGL.Vulkan;

namespace CSGL.Vulkan.Managed {
    public class Extension {
        public string Name { get; private set; }
        public VkVersion Version { get; private set; }

        internal Extension(VkExtensionProperties prop) {
            unsafe
            {
                Name = Interop.GetString(&prop.extensionName);
            }
            Version = prop.specVersion;
        }
    }
}
