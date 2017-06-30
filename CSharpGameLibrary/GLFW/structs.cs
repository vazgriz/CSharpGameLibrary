using System;
using System.Runtime.InteropServices;

namespace CSGL.GLFW {
    public struct VideoMode {
        public readonly int width;
        public readonly int height;
        public readonly int redBits;
        public readonly int greenBits;
        public readonly int blueBits;
        public readonly int refreshRate;
    }
    
    public unsafe struct NativeGammaRamp {
        public ushort* red;
        public ushort* green;
        public ushort* blue;
        public uint size;

        public NativeGammaRamp(ushort* red, ushort* green, ushort* blue, uint size) {
            this.red = red;
            this.green = green;
            this.blue = blue;
            this.size = size;
        }
    }
    
    public unsafe struct NativeImage {
        public int width;
        public int height;
        public byte* data;

        public NativeImage(int width, int height, byte* data) {
            this.width = width;
            this.height = height;
            this.data = data;
        }
    }
}
