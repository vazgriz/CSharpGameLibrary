using System;

namespace CSGL.Vulkan.Managed {
    public struct VkVersion {
        uint version;

        public VkVersion(uint major, uint minor, uint revision) {
            version = 0;
            Major = major;
            Minor = minor;
            Revision = revision;
        }

        public uint Version {
            get {
                return version;
            }
            set {
                version = value;
            }
        }

        public uint Major {
            get {
                return (version >> 22) & 0x3ff;
            }
            set {
                version &= ~0xffc00000;
                version |= (value << 22);
            }
        }

        public uint Minor {
            get {
                return (version >> 12) & 0x3ff;
            }
            set {
                version &= ~((uint)0x3ff000);
                version |= ((value << 12) & 0x3ff000);
            }
        }

        public uint Revision {
            get {
                return (version) & 0xfff;
            }
            set {
                version &= ~((uint)0xfff);
                version |= (value & 0xfff);
            }
        }
    }
}
