using System;
using System.Collections.Generic;

namespace CSGL {
    public class DisposableList<T> : IDisposable where T : IDisposable {
        List<T> items;
        bool disposed;

        public T this[int i] {
            get {
                return items[i];
            }
            set {
                items[i] = value;
            }
        }

        public int Count {
            get {
                return items.Count;
            }
        }

        public DisposableList() {
            items = new List<T>();
        }

        public DisposableList(List<T> items) {
            if (items == null) {
                this.items = new List<T>();
            } else {
                this.items = new List<T>(items);
            }
        }

        public DisposableList(T[] items) {
            if (items == null) {
                this.items = new List<T>();
            } else {
                this.items = new List<T>(items);
            }
        }

        public DisposableList(int count) {
            items = new List<T>(count);
        }

        public void Add(T item) {
            items.Add(item);
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        void Dispose(bool disposing) {
            if (disposed) return;

            if (disposing) {
                for (int i = 0; i < items.Count; i++) {
                    items[i]?.Dispose();
                }
            }

            items = null;

            disposed = true;
        }

        ~DisposableList() {
            Dispose(false);
        }
    }
}
