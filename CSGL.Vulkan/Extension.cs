using System;

using CSGL.Vulkan;

namespace CSGL.Vulkan {
    public class Extension {
        public string Name { get; private set; }
        public VkVersion Version { get; private set; }

        internal Extension(Unmanaged.VkExtensionProperties prop) {
            unsafe {
                Name = Interop.GetString(&prop.extensionName);
            }
            Version = prop.specVersion;
        }
    }
}
