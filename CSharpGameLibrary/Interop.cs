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
            Buffer.MemoryCopy(source, dest, size, size);
        }

        public static void Copy(IntPtr source, IntPtr dest, long size) {
            unsafe {
                Copy((void*)source, (void*)dest, size);
            }
        }

        public static void Copy<T>(IList<T> source, IntPtr dest, int count) where T : struct {
            long size = SizeOf<T>();
            for (int i = 0; i < count; i++) {
                IntPtr ptr = (IntPtr)((long)dest + size * i);
                Write(source[i], ptr);
            }
        }

        public static void Copy<T>(IList<T> source, IntPtr dest) where T : struct {
            Copy(source, dest, source.Count);
        }

        public static void Copy<T>(IntPtr source, IList<T> dest, int count) where T : struct {
            long size = SizeOf<T>();
            for (int i = 0; i < count; i++) {
                IntPtr ptr = (IntPtr)((long)source + size * i);
                dest[i] = Read<T>(ptr);
            }
        }

        public static void Copy<T>(IntPtr source, IList<T> dest) where T : struct {
            Copy(source, dest, dest.Count);
        }

        public static void Write<T>(T source, IntPtr dest) where T : struct {
            unsafe {
                Unsafe.Write((void*)dest, source);
            }
        }

        public static void Write<T>(T data, byte[] dest, int offset) where T : struct {
            unsafe {
                fixed (byte* ptr = dest) {
                    Write(data, (IntPtr)(ptr + offset));
                }
            }
        }

        public static T Read<T>(IntPtr source) where T : struct {
            unsafe {
                return Unsafe.Read<T>((byte*)source);
            }
        }

        public static long SizeOf<T>() where T : struct {
            return Unsafe.SizeOf<T>();
        }

        public static long SizeOf<T>(IList<T> list) where T : struct {
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
        
        public static void Marshal<T, U>(IList<U> list, IntPtr dest) where T : struct where U : INative<T> {
            if (list == null || list.Count == 0) return;

            unsafe {
                Marshal<T, U>(list, (void*)dest, list.Count);
            }
        }

        public static IntPtr AddressOf<T>(ref T data) where T : struct {
            unsafe {
                return (IntPtr)Unsafe.AsPointer(ref data);
            }
        }
    }
}
