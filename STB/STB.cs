using System;
using System.IO;
using System.Runtime.InteropServices;

using CSGL.Graphics;
using static CSGL.STB.Unmanaged.STB_native;

namespace CSGL.STB {
    public static class STB {
        public static IntPtr LoadPtr(byte[] buffer, out int x, out int y, out int comp, int req_comp) {
            GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            IntPtr ptr = handle.AddrOfPinnedObject();
            IntPtr error;
            IntPtr resultPtr = stbi_load_from_memory(ptr, buffer.Length, out x, out y, out comp, req_comp, out error);

            try {
                if (resultPtr == IntPtr.Zero && error != IntPtr.Zero) {
                    throw new ImageException(Interop.GetString(error));
                }
                
                return resultPtr;
            }
            finally {
                handle.Free();
            }
        }

        public static IntPtr LoadPtr(Stream stream, out int x, out int y, out int comp, int req_comp) {
            byte[] buffer = new byte[stream.Length - stream.Position];
            stream.Read(buffer, 0, buffer.Length);
            return LoadPtr(buffer, out x, out y, out comp, req_comp);
        }

        public static byte[] Load(byte[] buffer, out int x, out int y, out int comp, int req_comp) {
            IntPtr resultPtr = LoadPtr(buffer, out x, out y, out comp, req_comp);
            int realComponents;
            if (req_comp == 0) realComponents = comp;
            else realComponents = req_comp;
            byte[] result = new byte[x * y * realComponents];

            Interop.Copy(resultPtr, result);
            stbi_image_free(resultPtr);

            return result;
        }

        public static byte[] Load(Stream stream, out int x, out int y, out int comp, int req_comp) {
            byte[] buffer = new byte[stream.Length - stream.Position];
            stream.Read(buffer, 0, buffer.Length);
            return Load(buffer, out x, out y, out comp, req_comp);
        }

        public static Bitmap<Color4b> Load(Stream stream) {
            byte[] buffer = new byte[stream.Length - stream.Position];
            stream.Read(buffer, 0, buffer.Length);

            int x;
            int y;
            int components;

            IntPtr data = LoadPtr(buffer, out x, out y, out components, 4);
            Bitmap<Color4b> result = new Bitmap<Color4b>(x, y);
            Interop.Copy(data, result.Data);
            FreeImage(data);

            return result;
        }

        public static IntPtr LoadHDRPtr(byte[] buffer, out int x, out int y, out int comp, int req_comp) {
            GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            IntPtr ptr = handle.AddrOfPinnedObject();
            IntPtr error;
            IntPtr resultPtr = stbi_loadf_from_memory(ptr, buffer.Length, out x, out y, out comp, req_comp, out error);

            try {
                if (error != IntPtr.Zero) {
                    throw new ImageException(Interop.GetString(error));
                }

                return resultPtr;
            }
            finally {
                handle.Free();
            }
        }

        public static IntPtr LoadHDRPtr(Stream stream, out int x, out int y, out int comp, int req_comp) {
            byte[] buffer = new byte[stream.Length - stream.Position];
            stream.Read(buffer, 0, buffer.Length);
            return LoadHDRPtr(buffer, out x, out y, out comp, req_comp);
        }

        public static float[] LoadHDR(byte[] buffer, out int x, out int y, out int comp, int req_comp) {
            IntPtr resultPtr = LoadHDRPtr(buffer, out x, out y, out comp, req_comp);
            int realComponents;
            if (req_comp == 0) realComponents = comp;
            else realComponents = req_comp;
            float[] result = new float[x * y * realComponents];

            Interop.Copy(resultPtr, result);
            stbi_image_free(resultPtr);

            return result;
        }

        public static float[] LoadHDR(Stream stream, out int x, out int y, out int comp, int req_comp) {
            byte[] buffer = new byte[stream.Length - stream.Position];
            stream.Read(buffer, 0, buffer.Length);
            return LoadHDR(buffer, out x, out y, out comp, req_comp);
        }

        public static Bitmap<Color4> LoadHDR(Stream stream) {
            byte[] buffer = new byte[stream.Length - stream.Position];
            stream.Read(buffer, 0, buffer.Length);

            int x;
            int y;
            int components;

            IntPtr data = LoadHDRPtr(buffer, out x, out y, out components, 4);
            Bitmap<Color4> result = new Bitmap<Color4>(x, y);
            Interop.Copy(data, result.Data);
            FreeImage(data);

            return result;
        }

        public static bool IsHDR(byte[] buffer) {
            GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            int result = stbi_is_hdr_from_memory(handle.AddrOfPinnedObject(), buffer.Length);
            handle.Free();

            return result != 0;
        }

        public static void FreeImage(IntPtr ptr) {
            stbi_image_free(ptr);
        }
    }

    public class ImageException : Exception {
        public ImageException(string message) : base(message) { }
    }
}
