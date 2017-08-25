using System;

namespace CSGL.Vulkan {
    public class Layer {
        public string Name { get; private set; }
        public string Description { get; private set; }
        public VkVersion SpecVersion { get; private set; }
        public uint ImplementationVersion { get; private set; }

        internal Layer(VkLayerProperties prop) {
            unsafe
            {
                Name = Interop.GetString(&prop.layerName);
                Description = Interop.GetString(&prop.description);
            }
            SpecVersion = prop.specVersion;
            ImplementationVersion = prop.implementationVersion;
        }
    }
}
