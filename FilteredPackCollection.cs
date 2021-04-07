using System;
using System.Collections;
using System.Collections.Generic;
using TmfLib.Pathable;

namespace TmfLib {
    public class FilteredPackCollection : IPackCollection {

        private class FilteredPoiList : IList<PointOfInterest> {

            private readonly IList<PointOfInterest>      _backingList;
            private readonly Func<PointOfInterest, bool> _addFilterFunc;

            public FilteredPoiList(IList<PointOfInterest> backingList, Func<PointOfInterest, bool> addFilterFuncFunc) {
                _backingList   = backingList;
                _addFilterFunc = addFilterFuncFunc;
            }

            public IEnumerator<PointOfInterest> GetEnumerator() => _backingList.GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => _backingList.GetEnumerator();

            public void Add(PointOfInterest item) {
                if (_addFilterFunc(item)) {
                    _backingList.Add(item);
                }
            }

            public void Clear() => _backingList.Clear();

            public bool Contains(PointOfInterest item) => _backingList.Contains(item);

            public void CopyTo(PointOfInterest[] array, int arrayIndex) => _backingList.CopyTo(array, arrayIndex);

            public bool Remove(PointOfInterest item) => _backingList.Remove(item);

            public int  Count      => _backingList.Count;
            public bool IsReadOnly => _backingList.IsReadOnly;

            public int IndexOf(PointOfInterest item) => _backingList.IndexOf(item);

            public void Insert(int index, PointOfInterest item) => _backingList.Insert(index, item);

            public void RemoveAt(int index) => _backingList.RemoveAt(index);

            public PointOfInterest this[int index] {
                get => _backingList[index];
                set => _backingList[index] = value;
            }

        }

        private readonly IPackCollection _backingCollection;
        private readonly FilteredPoiList _filteredPoiList;

        public PathingCategory        Categories       => _backingCollection.Categories;
        public IList<PointOfInterest> PointsOfInterest => _filteredPoiList;

        public FilteredPackCollection(IPackCollection backingCollection, Func<PointOfInterest, bool> poiFilterFunc) {
            _backingCollection = backingCollection;

            _filteredPoiList = new FilteredPoiList(_backingCollection.PointsOfInterest, poiFilterFunc);
        }

    }
}
