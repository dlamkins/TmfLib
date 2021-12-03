using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;

namespace TmfLib.Content {
    public sealed class AsyncSafeZipArchive {

        private readonly ConcurrentBag<ZipArchive> _availableArchives = new();
        private readonly Dictionary<string, int>   _entryLookup       = new();

        private int _generation = 0;

        private readonly string _archivePath;

        public IEnumerable<string> Entries => _entryLookup.Keys;

        public AsyncSafeZipArchive(string filePath) {
            _archivePath = filePath;

            InitMetadata();
        }

        private void InitMetadata() {
            var (archive, generation) = GetArchive();

            for (int i = 0; i < archive.Entries.Count; i++) {
                string filePath = GetUniformFilePath(archive.Entries[i].FullName);

                // We have to check because zips are case sensitive and technically
                // can have duplicate entries that vary only by capitalization.
                if (!_entryLookup.ContainsKey(filePath)) {
                    _entryLookup.Add(filePath, i);
                }
            }

            ReturnArchive(archive, generation);
        }

        private string GetUniformFilePath(string filePath) {
            return filePath.Replace(@"\", "/").Replace("//", "/").ToLowerInvariant().Trim();
        }

        public bool FileExists(string filePath) {
            return _entryLookup.ContainsKey(GetUniformFilePath(filePath));
        }

        private ZipArchiveEntry GetArchiveEntry(ZipArchive archive, string filePath) {
            return _entryLookup.TryGetValue(GetUniformFilePath(filePath), out int index)
                       ? archive.Entries[index]
                       : null;
        }

        private (ZipArchive ZipArchive, int Generation) GetArchive() {
            return _availableArchives.TryTake(out var archive)
                       ? (archive, _generation)
                       : (ZipFile.OpenRead(_archivePath), _generation);
        }

        private void ReturnArchive(ZipArchive archive, int generation) {
            if (generation == _generation) {
                _availableArchives.Add(archive);
            } else {
                archive.Dispose();
            }
        }

        public async Task<Stream> GetFileStreamAsync(string filePath) {
            var (archive, generation) = GetArchive();

            try {
                ZipArchiveEntry fileEntry;

                if ((fileEntry = this.GetArchiveEntry(archive, filePath)) != null) {
                    var memStream = new MemoryStream();

                    using (var entryStream = fileEntry.Open()) {
                        await entryStream.CopyToAsync(memStream);
                    }

                    memStream.Position = 0;

                    return memStream;
                }
            } finally {
                ReturnArchive(archive, generation);
            }

            return null;
        }

        public Stream GetFileStream(string filePath) {
            var (archive, generation) = GetArchive();

            try {
                ZipArchiveEntry fileEntry;

                if ((fileEntry = this.GetArchiveEntry(archive, filePath)) != null) {
                    var memStream = new MemoryStream();

                    using (var entryStream = fileEntry.Open()) {
                        entryStream.CopyTo(memStream);
                    }

                    memStream.Position = 0;

                    return memStream;
                }
            } finally {
                ReturnArchive(archive, generation);
            }

            return null;
        }

        public async Task<byte[]> GetFileBytesAsync(string filePath) {
            // We know GetFileStream returns a MemoryStream, so we don't check
            using (var fileStream = await GetFileStreamAsync(filePath) as MemoryStream) {
                if (fileStream != null) {
                    return fileStream.ToArray();
                }
            }

            return null;
        }

        public byte[] GetFileBytes(string filePath) {
            // We know GetFileStream returns a MemoryStream, so we don't check
            using (var fileStream = GetFileStream(filePath) as MemoryStream) {
                if (fileStream != null) {
                    return fileStream.ToArray();
                }
            }

            return null;
        }

        public void AttemptReleaseLocks() {
            Interlocked.Increment(ref _generation);

            while (_availableArchives.TryTake(out var archive)) {
                archive.Dispose();
            }
        }

    }
}
