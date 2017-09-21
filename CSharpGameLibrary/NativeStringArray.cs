using System;
using System.Collections.Generic;

namespace CSGL {
    public class NativeStringArray : IDisposable {
        NativeArray<IntPtr> ptrs;
        NativeArray<byte>[] strings;
        bool _null = true;

        int count;
        IntPtr address;

        bool disposed = false;

        public NativeStringArray(int count) {
            Init(count);
        }

        public NativeStringArray(IList<string> list) {
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
            _null = false;
        }

        public string this[int i] {
            get {
                var native = strings[i];
                return Interop.GetString(native.Address);
            }
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

            if (!_null) {
                ptrs.Dispose();
                for (int i = 0; i < strings.Length; i++) {
                    strings[i].Dispose();
                }

                if (disposing) {
                    ptrs = null;
                    strings = null;
                }
            }

            disposed = true;
        }

        ~NativeStringArray() {
            Dispose(false);
        }
    }
}
