using System;
using System.Runtime.InteropServices;

using CSGL;

namespace STB.Unmanaged {
    class STB_native {
        const string lib = "stb";

        [DllImport(lib)]
        public static extern IntPtr stbi_load_from_memory(byte[] buffer, int length,
            out int x, out int y,
            out int comp, int req_comp,
            out IntPtr error);

        [DllImport(lib)]
        public static extern IntPtr stbi_loadf_from_memory(byte[] buffer, int length,
            out int x, out int y,
            out int comp, int req_comp,
            out IntPtr error);

        [DllImport(lib)]
        public static extern void stbi_hdr_to_ldr_gamma(float gamma);

        [DllImport(lib)]
        public static extern void stbi_hdr_to_ldr_scale(float scale);

        [DllImport(lib)]
        public static extern void stbi_ldr_to_hdr_gamma(float gamma);

        [DllImport(lib)]
        public static extern void stbi_ldr_to_hdr_scale(float scale);

        [DllImport(lib)]
        public static extern bool stbi_is_hdr_from_memory(IntPtr buffer, int length);

        [DllImport(lib)]
        public static extern void stbi_image_free(IntPtr image);

        [DllImport(lib)]
        public static extern int stbi_info_from_memory(byte[] buffer, int len,
            out int x, out int y,
            out int comp,
            out IntPtr error);

        [DllImport(lib)]
        public static extern void stbi_set_unpremultiply_on_load(int flag);

        [DllImport(lib)]
        public static extern void stbi_convert_iphone_png_to_rgb(int flag);

        [DllImport(lib)]
        public static extern void stbi_set_flip_vertically_on_load(int flag);
    }
}
