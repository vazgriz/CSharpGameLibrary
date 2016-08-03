using System;
using System.Runtime.InteropServices;

namespace CSGL.GLFW {
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct VideoMode {
        public readonly int width;
        public readonly int height;
        public readonly int redBits;
        public readonly int greenBits;
        public readonly int blueBits;
        public readonly int refreshRate;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct NativeGammaRamp {
        public short* red;
        public short* green;
        public short* blue;
        public uint size;

        public NativeGammaRamp(short* red, short* green, short* blue, uint size) {
            this.red = red;
            this.green = green;
            this.blue = blue;
            this.size = size;
        }
    }

    public struct GammaRamp {
        public short[] red;
        public short[] green;
        public short[] blue;
        public uint size;

        public GammaRamp(short[] red, short[] green, short[] blue, uint size) {
            this.red = red;
            this.green = green;
            this.blue = blue;
            this.size = size;
        }

        public GammaRamp(NativeGammaRamp ramp) {
            size = ramp.size;
            red = new short[size];
            green = new short[size];
            blue = new short[size];

            unsafe
            {
                for (int i = 0; i < size; i++) {
                    short r = ramp.red[i];
                    short g = ramp.green[i];
                    short b = ramp.blue[i];
                    red[i] = r;
                    green[i] = g;
                    blue[i] = b;
                }
            }
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
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

    public struct Image {
        public int width;
        public int height;
        public byte[] data;

        public Image(int width, int height, byte[] data) {
            this.width = width;
            this.height = height;
            this.data = data;
        }
    }
}
