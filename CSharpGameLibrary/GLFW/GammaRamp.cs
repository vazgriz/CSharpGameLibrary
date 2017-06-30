using System;

namespace CSGL.GLFW {
    public class GammaRamp {
        public ushort[] red;
        public ushort[] green;
        public ushort[] blue;

        public GammaRamp() {
            red = new ushort[256];
            green = new ushort[256];
            blue = new ushort[256];
        }

        public GammaRamp(ushort[] red, ushort[] green, ushort[] blue) {
            if (!(red.Length != green.Length && red.Length != blue.Length)) throw new ArgumentException("Red, green, and blue arrays must be the same length");
            this.red = red;
            this.green = green;
            this.blue = blue;
        }

        public GammaRamp(NativeGammaRamp ramp) {
            red = new ushort[ramp.size];
            green = new ushort[ramp.size];
            blue = new ushort[ramp.size];

            unsafe
            {
                for (int i = 0; i < ramp.size; i++) {
                    red[i] = ramp.red[i];
                    green[i] = ramp.green[i];
                    blue[i] = ramp.blue[i];
                }
            }
        }
    }
}
