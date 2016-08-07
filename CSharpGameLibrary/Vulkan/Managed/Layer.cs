using System;

namespace CSGL.Vulkan.Managed {
    public class Layer {
        public string Name { get; private set; }
        public string Description { get; private set; }
        public VkVersion SpecVersion { get; private set; }
        public uint ImplementationVersion { get; private set; }

        internal Layer(string name, string desc, uint sVersion, uint iVersion) {
            Name = name;
            Description = desc;
            SpecVersion = sVersion;
            ImplementationVersion = iVersion;
        }
    }
}
