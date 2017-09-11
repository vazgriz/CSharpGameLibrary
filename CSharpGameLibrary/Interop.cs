using System;
using System.Text;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using SMarshal = System.Runtime.InteropServices.Marshal;
using System.Reflection;
using System.Reflection.Emit;

namespace CSGL {
    public static class Interop {
        static UTF8Encoding utf8;

        static Interop() {
            utf8 = new UTF8Encoding(false, true);
        }

        public static unsafe string GetString(byte* ptr) {
            if (ptr == null) return null;
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

        public static string GetString(IntPtr ptr) {
            unsafe {
                return GetString((byte*)ptr);
            }
        }

        public static string GetString(byte[] array) {
            int count = 0;
            while (count < array.Length && array[count] != 0) count++;
            return utf8.GetString(array, 0, count);
        }

        public static byte[] GetUTF8(string s) {
            if (s == null) return null;
            int length = utf8.GetByteCount(s);
            byte[] result = new byte[length + 1];   //need room for null terminator
            utf8.GetBytes(s, 0, s.Length, result, 0);
            return result;
        }

        static class ListAccessor<T> {
            //http://stackoverflow.com/a/17308019
            //stores a dynamically created delegate to access the List's internal array, for each T
            public static Func<List<T>, T[]> accessor;

            static ListAccessor() {
                var dm = new DynamicMethod("get", MethodAttributes.Static | MethodAttributes.Public, CallingConventions.Standard,
                    typeof(T[]), new Type[] { typeof(List<T>) }, typeof(ListAccessor<T>), true);
                var il = dm.GetILGenerator();
                il.Emit(OpCodes.Ldarg_0); // Load List<T> argument
                il.Emit(OpCodes.Ldfld, typeof(List<T>).GetField("_items", BindingFlags.NonPublic | BindingFlags.Instance)); // Replace argument by field
                il.Emit(OpCodes.Ret); // Return field
                accessor = (Func<List<T>, T[]>)dm.CreateDelegate(typeof(Func<List<T>, T[]>));
            }
        }

        public static T[] GetInternalArray<T>(List<T> list) {
            if (list == null) return null;
            //returns the internal backing array
            return ListAccessor<T>.accessor(list);
        }

        public static unsafe void Copy(void* source, void* dest, long size) {
            //if source and dest are not congruent modulo
            if ((long)source % 8 != (long)dest % 8) {
                byte* _source = (byte*)source;
                byte* _dest = (byte*)dest;

                for (long i = 0; i < size; i++) {
                    _dest[i] = _source[i];
                }

                return;
            }

            //copies start, middle end sections seperately so that the middle section can be copied by boundary aligned double words
            long s = (long)source;

            long startMod = s % 8;
            long startOffset = (8 - startMod) % 8;

            long endMod = (s + size) % 8;
            long endOffset = ((s + size) - endMod) - s;

            long wordCount = (endOffset - startOffset) / 8;

            {
                byte* _dest = (byte*)dest + endOffset;
                byte* _source = (byte*)source + endOffset;
                for (long i = endMod - 1; i >= 0; i--) {
                    _dest[i] = _source[i];
                }
            }

            {
                long* _dest = (long*)dest + startOffset;
                long* _source = (long*)source + startOffset;

                for (long i = wordCount - 1; i >= 0; i--) {
                    _dest[i] = _source[i];
                }
            }

            {
                byte* _dest = (byte*)dest;
                byte* _source = (byte*)source;

                for (long i = startMod - 1; i >= 0; i--) {
                    _dest[i] = _source[i];
                }
            }
        }

        public static void Copy(IntPtr source, IntPtr dest, long size) {
            unsafe {
                Copy((void*)source, (void*)dest, size);
            }
        }

        public static void Copy<T>(T[] source, IntPtr dest, int count) where T : struct {
            GCHandle handle = GCHandle.Alloc(source, GCHandleType.Pinned);
            Copy(handle.AddrOfPinnedObject(), dest, count * Unsafe.SizeOf<T>());
            handle.Free();
        }

        public static void Copy<T>(T[] source, IntPtr dest) where T : struct {
            Copy(source, dest, source.Length);
        }

        public static void Copy<T>(IntPtr source, T[] dest, int count) where T : struct {
            GCHandle handle = GCHandle.Alloc(dest, GCHandleType.Pinned);
            Copy(source, handle.AddrOfPinnedObject(), count);
            handle.Free();
        }

        public static void Copy<T>(IntPtr source, T[] dest) where T : struct {
            GCHandle handle = GCHandle.Alloc(dest, GCHandleType.Pinned);
            Copy(source, handle.AddrOfPinnedObject(), dest.Length);
            handle.Free();
        }

        public static void Copy<T>(T source, IntPtr dest) where T : struct {
            unsafe {
                Unsafe.Write((void*)dest, source);
            }
        }

        public static void Copy<T>(T data, byte[] dest, int offset) where T : struct {
            unsafe {
                fixed (byte* ptr = dest) {
                    Copy(data, (IntPtr)(ptr + offset));
                }
            }
        }

        public static long SizeOf<T>() where T : struct {
            return Unsafe.SizeOf<T>();
        }

        public static long SizeOf<T>(T[] array) where T : struct {
            return Unsafe.SizeOf<T>() * array.Length;
        }

        public static long SizeOf<T>(List<T> list) where T : struct {
            return Unsafe.SizeOf<T>() * list.Count;
        }

        public static long Offset<T1, T2>(ref T1 type, ref T2 field)
            where T1 : struct
            where T2 : struct {
            unsafe {
                return (byte*)Unsafe.AsPointer(ref field) - (byte*)Unsafe.AsPointer(ref type);
            }
        }

        public static unsafe void Marshal<T, U>(IList<U> list, void* dest, int count) where T : struct where U : INative<T> {
            if (list == null || list.Count == 0) return;

            int size = (int)SizeOf<T>();
            byte* curDest = (byte*)dest;

            for (int i = 0; i < count; i++) {
                Unsafe.Write(curDest, list[i].Native);
                curDest += size;
            }
        }

        public static unsafe void Marshal<T, U>(IList<U> list, void* dest) where T : struct where U : INative<T> {
            if (list == null || list.Count == 0) return;
            Marshal<T, U>(list, dest, list.Count);
        }

        public static IntPtr AddressOf<T>(ref T data) where T : struct {
            unsafe {
                return (IntPtr)Unsafe.AsPointer(ref data);
            }
        }
    }
}
