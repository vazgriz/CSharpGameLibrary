using System;
using System.Text;

namespace CSGL.Vulkan {
    public struct VkVersion {
        uint version;

        public VkVersion(int major, int minor, int revision) {
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

        public int Major {
            get {
                return (int)(version >> 22);
            }
            set {
                version &= ~0xffc00000;
                version |= ((uint)value << 22);
            }
        }

        public int Minor {
            get {
                return (int)(version >> 12) & 0x3ff;
            }
            set {
                version &= ~((uint)0x3ff000);
                version |= (((uint)value << 12) & 0x3ff000);
            }
        }

        public int Revision {
            get {
                return (int)(version) & 0xfff;
            }
            set {
                version &= ~((uint)0xfff);
                version |= ((uint)value & 0xfff);
            }
        }

        public override string ToString() {
            return string.Format("{0}.{1}.{2}", Major, Minor, Revision);
        }
    }
}
