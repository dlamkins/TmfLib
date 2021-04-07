using System;

namespace TmfLib {
    public class PackResource : IPackResource {

        private readonly Func<byte[]>          _refreshData;
        private readonly WeakReference<byte[]> _dataReference;

        public byte[] Data => GetData();

        public PackResource(Func<byte[]> refreshData, byte[] data) {
            _refreshData   = refreshData;
            _dataReference = new WeakReference<byte[]>(data);
        }

        private byte[] GetData() {
            if (!_dataReference.TryGetTarget(out byte[] data)) {
                data = _refreshData();
                _dataReference.SetTarget(data);
            }

            return data;
        }

    }
}
