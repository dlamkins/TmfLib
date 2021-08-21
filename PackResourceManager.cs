using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using TmfLib.Content;

namespace TmfLib {
    public class PackResourceManager : IPackResourceManager {

        private IDataReader DataReader { get; }

        private readonly ConcurrentDictionary<string, IPackResource> _cachedResources = new(StringComparer.InvariantCultureIgnoreCase);

        internal PackResourceManager(IDataReader dataReader) {
            this.DataReader = dataReader;
        }

        public bool ResourceExists(string resourcePath) {
            return this.DataReader.FileExists(resourcePath);
        }

        public async Task<byte[]> LoadResourceAsync(string resourcePath) {
            if (!_cachedResources.ContainsKey(resourcePath)) {
                //var tempDataReader = this.DataReader.GetSubPath("");

                var resource = this.DataReader.GetFileBytes(resourcePath);

                _cachedResources.TryAdd(resourcePath, new PackResource(() => this.DataReader.GetFileBytes(resourcePath), resource));
            }

            return _cachedResources[resourcePath].Data;
        }

        public byte[] LoadResource(string resourcePath) {
            if (!_cachedResources.ContainsKey(resourcePath)) {
                var resource = this.DataReader.GetFileBytes(resourcePath);

                _cachedResources.TryAdd(resourcePath, new PackResource(() => this.DataReader.GetFileBytes(resourcePath), resource));
            }

            return _cachedResources[resourcePath].Data;
        }

    }
}
