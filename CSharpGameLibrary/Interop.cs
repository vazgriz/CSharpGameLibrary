using System;
using System.Text;
using System.Runtime.InteropServices;

using SMarshal = System.Runtime.InteropServices.Marshal;

namespace CSGL {
    public static class Interop {
        static UTF8Encoding utf8;

        static Interop() {
            utf8 = new UTF8Encoding(false, true);
        }

        public static unsafe string GetString(byte* ptr) {
            int length = 0;
            while (ptr[length] != 0) {
                length++;
            }
            byte[] array = new byte[length];
            for (int i = 0; i < length; i++) {
                array[i] = ptr[i];
            }
            return utf8.GetString(array);
        }

        public static string GetString(byte[] array) {
            int count = 0;
            while (array[count] != 0) count++;
            return utf8.GetString(array, 0, count);
        }

        public static byte[] GetUTF8(string s) {
            if (s == null) return null;
            int length = utf8.GetByteCount(s);
            byte[] result = new byte[length + 1];   //need room for null terminator
            utf8.GetBytes(s, 0, s.Length, result, 0);
            return result;
        }

        public static unsafe void Copy(void* source, void* dest, int size) {
            byte* _source = (byte*)source;
            byte* _dest = (byte*)dest;
            for (int i = 0; i < size; i++) {
                _dest[i] = _source[i];
            }
        }

        public static unsafe void Copy<T>(T[] source, void* dest, int count) where T : struct {
            GCHandle handle = GCHandle.Alloc(source, GCHandleType.Pinned);
            Copy((void*)handle.AddrOfPinnedObject(), dest, count * SMarshal.SizeOf<T>());
            handle.Free();
        }

        public static unsafe void Copy<T>(T[] source, void* dest) where T : struct {
            Copy(source, dest, source.Length);
        }

        public static unsafe void Marshal<T>(void* dest, T[] array) where T : struct {
            byte* ptr = (byte*)dest;
            int size = SMarshal.SizeOf<T>();
            for (int i = 0; i < array.Length; i++) {
                SMarshal.StructureToPtr(array[i], (IntPtr)ptr, false);
                ptr += size;
            }
        }

        public static unsafe void Marshal<T>(void* dest, T value) where T : struct {
            SMarshal.StructureToPtr(value, (IntPtr)dest, false);
        }

        public static unsafe T Unmarshal<T>(void* source) where T : struct {
            return SMarshal.PtrToStructure<T>((IntPtr)source);
        }

        public static unsafe T Unmarshal<T>(void* source, int index) where T : struct {
            return Unmarshal<T>((byte*)source + (index * SizeOf<T>()));
        }

        public static int SizeOf<T>() {
            return SMarshal.SizeOf<T>();
        }

        public static int SizeOf<T>(T[] array) {
            return SMarshal.SizeOf<T>() * array.Length;
        }
    }
}
