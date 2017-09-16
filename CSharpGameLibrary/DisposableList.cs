using System;
using System.Collections;
using System.Collections.Generic;

namespace CSGL {
    public class DisposableList<T> : IDisposable, IList<T> where T : IDisposable {
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

        public bool IsReadOnly {
            get {
                return ((IList<T>)items).IsReadOnly;
            }
        }

        public DisposableList() {
            items = new List<T>();
        }

        public DisposableList(IList<T> items) {
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

        public int IndexOf(T item) {
            return items.IndexOf(item);
        }

        public void Insert(int index, T item) {
            items.Insert(index, item);
        }

        public void RemoveAt(int index) {
            items.RemoveAt(index);
        }

        public void Clear() {
            items.Clear();
        }

        public bool Contains(T item) {
            return items.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex) {
            items.CopyTo(array, arrayIndex);
        }

        public bool Remove(T item) {
            return items.Remove(item);
        }

        public IEnumerator<T> GetEnumerator() {
            return items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return items.GetEnumerator();
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
