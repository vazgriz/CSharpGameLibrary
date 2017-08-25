using System;
using System.Text;

namespace CSGL.Vulkan {
    public struct VkVersion {
        uint version;

        public VkVersion(uint major, uint minor, uint revision) {
            version = 0;
            Major = major;
            Minor = minor;
            Revision = revision;
        }

        public VkVersion(uint version) {
            this.version = version;
        }

        public static implicit operator uint(VkVersion v) {
            return v.version;
        }

        public static implicit operator VkVersion(uint v) {
            return new VkVersion(v);
        }

        public uint Major {
            get {
                return (version >> 22);
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

        public override string ToString() {
            return string.Format("{0}.{1}.{2}", Major, Minor, Revision);
        }
    }
}
