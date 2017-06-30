using System;
using System.Runtime.InteropServices;

namespace CSGL.GLFW {
    [StructLayout(LayoutKind.Sequential)]
    public struct VideoMode {
        public readonly int width;
        public readonly int height;
        public readonly int redBits;
        public readonly int greenBits;
        public readonly int blueBits;
        public readonly int refreshRate;
    }

    [StructLayout(LayoutKind.Sequential)]
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

    public struct GammaRamp {
        public ushort[] red;
        public ushort[] green;
        public ushort[] blue;
        public uint size;

        public GammaRamp(ushort[] red, ushort[] green, ushort[] blue, uint size) {
            this.red = red;
            this.green = green;
            this.blue = blue;
            this.size = size;
        }

        public GammaRamp(NativeGammaRamp ramp) {
            size = ramp.size;
            red = new ushort[size];
            green = new ushort[size];
            blue = new ushort[size];

            unsafe
            {
                for (int i = 0; i < size; i++) {
                    red[i] = ramp.red[i];
                    green[i] = ramp.green[i];
                    blue[i] = ramp.blue[i];
                }
            }
        }
    }

    [StructLayout(LayoutKind.Sequential)]
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
