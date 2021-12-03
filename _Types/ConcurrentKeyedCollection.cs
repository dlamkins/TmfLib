using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;

namespace TmfLib {
    public abstract class ConcurrentKeyedCollection<TKey, TItem> : Collection<TItem> {

        private readonly ConcurrentDictionary<TKey, TItem> _dictionary;
        private readonly IEqualityComparer<TKey>           _comparer;

        protected ConcurrentKeyedCollection() : this(null) { /* NOOP */ }

        protected ConcurrentKeyedCollection(IEqualityComparer<TKey> comparer) {
            _comparer = comparer ?? EqualityComparer<TKey>.Default;

            _dictionary = new ConcurrentDictionary<TKey, TItem>(_comparer);
        }

        public TItem this[TKey key] {
            get {
                if (key == null) {
                    throw new ArgumentNullException(nameof(key));
                }

                return _dictionary[key];
            }
        }

        public bool Contains(TKey key) {
            if (key == null) {
                throw new ArgumentNullException(nameof(key));
            }

            return _dictionary.ContainsKey(key);
        }

        private bool ContainsItem(TItem item) {
            TKey key;

            if ((key = GetKeyForItem(item)) == null) {
                return this.Items.Contains(item);
            }

            return _dictionary.TryGetValue(key, out var itemInDictionary)
                && EqualityComparer<TItem>.Default.Equals(itemInDictionary, item);
        }

        public bool Remove(TKey key) {
            if (key == null) {
                throw new ArgumentNullException(nameof(key));
            }

            return _dictionary.TryRemove(key, out _);
        }

        protected IDictionary<TKey, TItem> Dictionary => _dictionary;

        protected override void ClearItems() {
            base.ClearItems();
            _dictionary.Clear();
        }
        
        protected abstract TKey GetKeyForItem(TItem item);

        protected override void InsertItem(int index, TItem item) {
            var key = GetKeyForItem(item);
            if (key != null) {
                AddKey(key, item);
            }
            base.InsertItem(index, item);
        }

        protected override void RemoveItem(int index) {
            var key = GetKeyForItem(this.Items[index]);
            if (key != null) {
                RemoveKey(key);
            }
            base.RemoveItem(index);
        }

        protected override void SetItem(int index, TItem item) {
            var newKey = GetKeyForItem(item);
            var oldKey = GetKeyForItem(this.Items[index]);

            if (_comparer.Equals(oldKey, newKey)) {
                if (newKey != null && _dictionary != null) {
                    _dictionary[newKey] = item;
                }
            } else {
                if (newKey != null) {
                    AddKey(newKey, item);
                }

                if (oldKey != null) {
                    RemoveKey(oldKey);
                }
            }
            base.SetItem(index, item);
        }

        private void AddKey(TKey key, TItem item) {
            _dictionary.AddOrUpdate(key, item, (_, _) => item);
        }

        private void RemoveKey(TKey key) {
            _dictionary.TryRemove(key, out _);
        }

    }
}
