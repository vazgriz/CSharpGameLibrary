using System;
using System.Collections.Generic;

namespace CSGL {
    public class NativeStringArray : IDisposable {
        NativeArray<IntPtr> ptrs;
        NativeArray<byte>[] strings;

        int count;
        IntPtr address;

        bool disposed = false;

        public NativeStringArray(int count) {
            Init(count);
        }

        public NativeStringArray(string[] array) {
            if (array != null) {
                Init(array.Length);
                for (int i = 0; i < array.Length; i++) {
                    this[i] = array[i]; //indexer takes care of converting the strings
                }
            }
        }

        public NativeStringArray(List<string> list) {
            if (list != null) {
                Init(list.Count);
                for (int i = 0; i < list.Count; i++) {
                    this[i] = list[i];
                }
            }
        }

        void Init(int count) {
            ptrs = new NativeArray<IntPtr>(count);
            strings = new NativeArray<byte>[count];

            address = ptrs.Address;
            this.count = count;
        }

        public string this[int i] {
            set {
                var native = Interop.GetUTF8(value);
                var nativeString = new NativeArray<byte>(native);
                strings[i] = nativeString;
                ptrs[i] = nativeString.Address;
            }
        }

        public IntPtr Address {
            get {
                return address;
            }
        }

        public int Count {
            get {
                return count;
            }
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        void Dispose(bool disposing) {
            if (disposed) return;

            ptrs.Dispose();
            for (int i = 0; i < strings.Length; i++) {
                strings[i].Dispose();
            }

            if (disposing) {
                ptrs = null;
                strings = null;
            }

            disposed = true;
        }

        ~NativeStringArray() {
            Dispose(false);
        }
    }
}
