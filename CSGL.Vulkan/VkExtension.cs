using System;

using CSGL.Vulkan;

namespace CSGL.Vulkan {
    public class VkExtension {
        public string Name { get; private set; }
        public VkVersion Version { get; private set; }

        internal VkExtension(Unmanaged.VkExtensionProperties prop) {
            unsafe {
                Name = Interop.GetString(&prop.extensionName);
            }
            Version = prop.specVersion;
        }
    }
}
