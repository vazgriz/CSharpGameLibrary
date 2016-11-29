using System;
using System.Runtime.InteropServices;

using CSGL;
using static STB.Unmanaged.STB_native;

namespace STB {
    public static class STB {
        public static byte[] Load(byte[] buffer, out int x, out int y, out int comp, int req_comp) {
            GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            IntPtr ptr = handle.AddrOfPinnedObject();
            IntPtr error;
            IntPtr resultPtr = stbi_load_from_memory(ptr, buffer.Length, out x, out y, out comp, req_comp, out error);

            try {
                if (error != IntPtr.Zero) {
                    throw new ImageException(Interop.GetString(error));
                }

                int realComponents;
                if (req_comp == 0) realComponents = comp;
                else realComponents = req_comp;
                byte[] result = new byte[x * y * realComponents];

                Interop.Copy(resultPtr, result);
                stbi_image_free(resultPtr);

                return result;
            }
            finally {
                handle.Free();
            }
        }

        public static float[] LoadHDR(byte[] buffer, out int x, out int y, out int comp, int req_comp) {
            GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            IntPtr ptr = handle.AddrOfPinnedObject();
            IntPtr error;
            IntPtr resultPtr = stbi_loadf_from_memory(ptr, buffer.Length, out x, out y, out comp, req_comp, out error);

            try {
                if (error != IntPtr.Zero) {
                    throw new ImageException(Interop.GetString(error));
                }

                int realComponents;
                if (req_comp == 0) realComponents = comp;
                else realComponents = req_comp;
                float[] result = new float[x * y * realComponents];

                Interop.Copy(resultPtr, result);
                stbi_image_free(resultPtr);

                return result;
            }
            finally {
                handle.Free();
            }
        }

        public static bool IsHDR(byte[] buffer) {
            GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            int result = stbi_is_hdr_from_memory(handle.AddrOfPinnedObject(), buffer.Length);
            handle.Free();

            return result != 0;
        }
    }
}
