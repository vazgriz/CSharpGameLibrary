using System;

namespace CSGL.Vulkan.Managed {
    public struct Version {
        uint version;

        public Version(int major, int minor, int revision) {
            version = 0;
        }

        public int Major {
            get {
                return (int)(version >> 22) & 0b11111111;
            }
        }
    }
}
