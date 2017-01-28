﻿using System;
using System.Text;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using SMarshal = System.Runtime.InteropServices.Marshal;

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
            unsafe
            {
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

        public static unsafe void Copy(void* source, void* dest, long size) {
            //if source and dest are not congruent modulo
            if ((ulong)source % 8 != (ulong)dest % 8) {
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

        public static unsafe void Copy<T>(T[] source, void* dest, int count) where T : struct {
            GCHandle handle = GCHandle.Alloc(source, GCHandleType.Pinned);
            Copy((void*)handle.AddrOfPinnedObject(), dest, count * Unsafe.SizeOf<T>());
            handle.Free();
        }

        public static unsafe void Copy<T>(T[] source, void* dest) where T : struct {
            Copy(source, dest, source.Length);
        }

        public static void Copy(IntPtr source, IntPtr dest, long size) {
            unsafe
            {
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
            unsafe
            {
                Unsafe.Write((void*)dest, source);
            }
        }

        public static void Copy<T>(List<T> source, IntPtr dest) where T : struct {
            unsafe
            {
                int size = SizeOf<T>();
                for (int i = 0; i < source.Count; i++) {
                    Unsafe.Write((void*)dest, source[i]);
                    dest += size;
                }
            }
        }

        public static unsafe void Copy<T>(T source, void* dest) where T : struct {
            Unsafe.Write(dest, source);
        }

        public static int SizeOf<T>() where T : struct {
            return Unsafe.SizeOf<T>();
        }

        public static int SizeOf<T>(T[] array) where T : struct {
            return Unsafe.SizeOf<T>() * array.Length;
        }

        public static int SizeOf<T>(List<T> list) where T : struct {
            return Unsafe.SizeOf<T>() * list.Count;
        }

        public static long Offset<T1, T2>(ref T1 type, ref T2 field)
            where T1 : struct
            where T2 : struct {
            unsafe
            {
                return (byte*)Unsafe.AsPointer(ref field) - (byte*)Unsafe.AsPointer(ref type);
            }
        }

        public static int MarshalledSizeOf<T>() where T : struct {
            return SMarshal.SizeOf<T>();
        }

        public static int MarshalledSizeOf<T>(T[] array) where T : struct {
            return MarshalledSizeOf<T>() * array.Length;
        }

        public static int MSizeOf<T>(INative<T> obj) where T : struct {
            return SMarshal.SizeOf<T>();
        }

        public static int MSizeOf<T>(INative<T>[] array) where T : struct {
            if (array == null) {
                return 0;
            }

            return MarshalledSizeOf<T>() * array.Length;
        }

        public static unsafe void Marshal<T>(T obj, void* dest) {
            Unsafe.Write(dest, obj);
        }

        public static unsafe void Marshal<T>(T[] array, void* dest) where T : struct {
            if (array == null || array.Length == 0) return;

            int size = MarshalledSizeOf<T>();
            byte* curDest = (byte*)dest;

            for (int i = 0; i < array.Length; i++) {
                Unsafe.Write(curDest, array[i]);
                curDest += size;
            }
        }

        public static unsafe void Marshal<T>(INative<T> obj, void* dest) where T : struct {
            Unsafe.Write(dest, obj.Native);
        }

        public static unsafe void Marshal<T>(INative<T>[] array, void* dest, int count) where T : struct {
            if (array == null || array.Length == 0) return;

            int size = MarshalledSizeOf<T>();
            byte* curDest = (byte*)dest;

            for (int i = 0; i < count; i++) {
                Unsafe.Write(curDest, array[i].Native);
                curDest += size;
            }
        }

        public static unsafe void Marshal<T>(INative<T>[] array, void* dest) where T : struct {
            if (array == null || array.Length == 0) return;
            Marshal(array, dest, array.Length);
        }

        public static unsafe void Marshal<T, U>(List<U> list, void* dest, int count) where T : struct where U : INative<T> {
            if (list == null || list.Count == 0) return;

            int size = MarshalledSizeOf<T>();
            byte* curDest = (byte*)dest;

            for (int i = 0; i < count; i++) {
                Unsafe.Write(curDest, list[i].Native);
                curDest += size;
            }
        }

        public static unsafe void Marshal<T, U>(List<U> list, void* dest) where T : struct where U : INative<T> {
            if (list == null || list.Count == 0) return;
            Marshal<T, U>(list, dest, list.Count);
        }
    }
}
