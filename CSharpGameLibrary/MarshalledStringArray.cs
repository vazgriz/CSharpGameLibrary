using System;
using System.Collections.Generic;

namespace CSGL {
    public class MarshalledStringArray : IDisposable {
        MarshalledArray<IntPtr> marshalled;
        MarshalledArray<byte>[] strings;

        bool disposed = false;

        public MarshalledStringArray(int count) {
            Init(count);
        }

        public MarshalledStringArray(string[] array) {
            Init(array.Length);

            for (int i = 0; i < array.Length; i++) {
                this[i] = array[i];
            }
        }

        public MarshalledStringArray(List<string> list) {
            Init(list.Count);

            for (int i = 0; i < list.Count; i++) {
                this[i] = list[i];
            }
        }

        void Init(int count) {
            marshalled = new MarshalledArray<IntPtr>(count);
            strings = new MarshalledArray<byte>[count];
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
                return marshalled.Address;
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
