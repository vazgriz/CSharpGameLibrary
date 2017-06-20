using System;

using CSGL.Vulkan1;

namespace CSGL.Vulkan1 {
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
