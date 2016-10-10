using System;
using System.Collections.Generic;

namespace CSGL {
    public class MarshalledStringArray : IDisposable {
        MarshalledArray<IntPtr> marshalled;
        MarshalledArray<byte>[] strings;

        int count;
        IntPtr address;

        bool disposed = false;

        public MarshalledStringArray(int count) {
            Init(count);
        }

        public MarshalledStringArray(string[] array) {
            Init(array.Length);

            if (array != null) {
                for (int i = 0; i < array.Length; i++) {
                    this[i] = array[i];
                }
            }
        }

        public MarshalledStringArray(List<string> list) {
            Init(list.Count);

            if (list != null) {
                for (int i = 0; i < list.Count; i++) {
                    this[i] = list[i];
                }
            }
        }

        void Init(int count) {
            marshalled = new MarshalledArray<IntPtr>(count);
            strings = new MarshalledArray<byte>[count];

            address = marshalled.Address;
            this.count = count;
        }

        public string this[int i] {
            set {
                var native = Interop.GetUTF8(value);
                var ms = new MarshalledArray<byte>(native);
                strings[i] = ms;
                marshalled[i] = ms.Address;
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

            marshalled.Dispose();
            for (int i = 0; i < strings.Length; i++) {
                strings[i]?.Dispose();
            }

            if (disposing) {
                marshalled = null;
                strings = null;
            }

            disposed = true;
        }

        ~MarshalledStringArray() {
            Dispose(false);
        }
    }
}
