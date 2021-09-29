using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using TmfLib.Content;

namespace TmfLib {
    public class PackResourceManager : IPackResourceManager {

        private IDataReader DataReader { get; }

        internal PackResourceManager(IDataReader dataReader) {
            this.DataReader = dataReader;
        }

        public bool ResourceExists(string resourcePath) {
            return this.DataReader.FileExists(resourcePath);
        }

        public async Task<byte[]> LoadResourceAsync(string resourcePath) {
            return await this.DataReader.GetFileBytesAsync(resourcePath);
        }

    }
}
