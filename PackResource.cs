using System;
using System.IO;
using System.Threading.Tasks;

namespace TmfLib {
    public class PackResource : IPackResource {

        private readonly Func<Task<byte[]>>    _refreshData;
        private readonly WeakReference<byte[]> _dataReference;

        public PackResource(Func<Task<byte[]>> refreshData) {
            _refreshData   = refreshData;
            _dataReference = new WeakReference<byte[]>(null);
        }

        public async Task<byte[]> GetDataAsync() {
            if (!_dataReference.TryGetTarget(out byte[] data)) {
                data = await _refreshData();
                _dataReference.SetTarget(data);
            }

            return data;
        }

    }
}
