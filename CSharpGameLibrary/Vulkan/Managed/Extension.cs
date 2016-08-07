using System;

namespace CSGL.Vulkan.Managed {
    public class Extension {
        public string Name { get; private set; }
        public VkVersion Version { get; private set; }

        internal Extension(string name, uint version) {
            Name = name;
            Version = version;
        }
    }
}
