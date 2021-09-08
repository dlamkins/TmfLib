using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
                _cachedResources.TryAdd(resourcePath, new PackResource(async () => await this.DataReader.GetFileBytesAsync(resourcePath)));
            }
            
            return await _cachedResources[resourcePath].GetDataAsync();
        }

        public async Task PreloadResourcesAsync(IEnumerable<string> resourcePaths) {
            foreach (string resourcePath in resourcePaths) {
                _ = await LoadResourceAsync(resourcePath);
            }
        }

    }
}
