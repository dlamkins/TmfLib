using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace TmfLib.Content {
    internal sealed class ZipArchiveReader : IDataReader {

        private readonly AsyncSafeZipArchive _archive;

        private readonly string _archivePath;
        private readonly string _subPath;

        public ZipArchiveReader(string archivePath, string subPath = "") {
            if (!File.Exists((archivePath)))
                throw new FileNotFoundException("Archive path not found.", archivePath);

            _archivePath = archivePath;
            _subPath = subPath;

            _archive = new AsyncSafeZipArchive(archivePath);
        }

        public IDataReader GetSubPath(string subPath) {
            return new ZipArchiveReader(_archivePath, Path.Combine(subPath));
        }
        
        public string GetPathRepresentation(string relativeFilePath = null) {
            return $"{_archivePath}[{Path.GetFileName(Path.Combine(_subPath, relativeFilePath ?? string.Empty))}]";
        }
        
        public async Task LoadOnFileTypeAsync(Func<Stream, IDataReader, Task> loadFileFunc, string fileExtension = "", IProgress<string> progress = null) {
            var validEntries = _archive.Entries.Where(e => e.EndsWith(fileExtension.ToLowerInvariant())).ToList();

            foreach (var entry in validEntries) {
                progress?.Report($"Loading {entry}...");
                await loadFileFunc.Invoke(await this.GetFileStreamAsync(entry), this);
            }
        }

        public bool FileExists(string filePath) {
            return _archive.FileExists(filePath);
        }

        public Stream GetFileStream(string filePath) => _archive.GetFileStream(filePath);

        public Task<Stream> GetFileStreamAsync(string filePath) => _archive.GetFileStreamAsync(filePath);

        public byte[] GetFileBytes(string filePath) => _archive.GetFileBytes(filePath);

        public Task<byte[]> GetFileBytesAsync(string filePath) => _archive.GetFileBytesAsync(filePath);

        public void Dispose() {
            // TODO: Allow dispose on AsyncZipArchive
        }

    }
}
