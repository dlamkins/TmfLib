using System.IO;
using System.Threading.Tasks;
using TmfLib.Content;

namespace TmfLib {
    public class PackResourceManager : IPackResourceManager {

        public IDataReader DataReader { get; }

        internal PackResourceManager(IDataReader dataReader) {
            this.DataReader = dataReader;
        }

        public bool ResourceExists(string resourcePath) {
            return this.DataReader.FileExists(resourcePath);
        }

        public async Task<byte[]> LoadResourceAsync(string resourcePath) {
            return await this.DataReader.GetFileBytesAsync(resourcePath);
        }

        public async Task<Stream> LoadResourceStreamAsync(string resourcePath) {
            return await this.DataReader.GetFileStreamAsync(resourcePath);
        }

    }
}
