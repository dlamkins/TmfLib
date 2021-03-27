using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace TmfLib.Content {
    internal sealed class ZipArchiveReader : IDataReader {

        private readonly ZipArchive _archive;

        private readonly string _archivePath;
        private readonly string _subPath;

        private readonly Mutex _exclusiveStreamAccessMutex;

        private readonly HashSet<string> _entryList;

        public ZipArchiveReader(string archivePath, string subPath = "") {
            if (!File.Exists((archivePath)))
                throw new FileNotFoundException("Archive path not found.", archivePath);

            _archivePath = archivePath;
            _subPath = subPath;

            _exclusiveStreamAccessMutex = new Mutex(false);

            _archive = ZipFile.OpenRead(archivePath);

            _entryList = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
            
            foreach (var entry in _archive.Entries) {
                _entryList.Add(GetUniformFileName(entry.FullName).ToLowerInvariant());
            }

            _entryList.TrimExcess();
        }

        public IDataReader GetSubPath(string subPath) {
            return new ZipArchiveReader(_archivePath, Path.Combine(subPath));
        }
        
        public string GetPathRepresentation(string relativeFilePath = null) {
            return $"{_archivePath}[{Path.GetFileName(Path.Combine(_subPath, relativeFilePath ?? string.Empty))}]";
        }
        
        public void LoadOnFileType(Action<Stream, IDataReader> loadFileFunc, string fileExtension = "", IProgress<string> progress = null) {
            var validEntries = _archive.Entries.Where(e => e.Name.EndsWith($"{fileExtension}", StringComparison.OrdinalIgnoreCase)).ToList();

            foreach (var entry in validEntries) {
                progress?.Report($"Loading {entry.Name}...");
                var entryStream = GetFileStream(entry.FullName);

                loadFileFunc.Invoke(entryStream, this);
            }
        }

        public bool FileExists(string filePath) {
            return _entryList.Contains(GetUniformFileName(filePath));
        }

        private string GetUniformFileName(string filePath) {
            return filePath.Replace(@"\", "/").Replace("//", "/").Trim();
        }

        private ZipArchiveEntry GetArchiveEntry(string filePath) {
            var cleanFilePath = GetUniformFileName(Path.Combine(_subPath, filePath));

            foreach (var zipEntry in _archive.Entries) {
                string cleanZipEntry = GetUniformFileName(zipEntry.FullName);

                if (string.Equals(cleanFilePath, cleanZipEntry, StringComparison.OrdinalIgnoreCase)) {
                    return zipEntry;
                }
            }

            return null;
        }

        public Stream GetFileStream(string filePath) {
            ZipArchiveEntry fileEntry;

            if ((fileEntry = this.GetArchiveEntry(filePath)) != null) {
                _exclusiveStreamAccessMutex.WaitOne();

                var memStream = new MemoryStream();
                using (var entryStream = fileEntry.Open()) {
                    entryStream.CopyTo(memStream);
                }

                memStream.Position = 0;

                _exclusiveStreamAccessMutex.ReleaseMutex();
                return memStream;
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

        public int GetFileBytes(string filePath, out byte[] fileBuffer) {
            fileBuffer = null;

            // We know GetFileStream returns a MemoryStream, so we don't check
            using (var fileStream = GetFileStream(filePath) as MemoryStream) {
                if (fileStream != null) {
                    fileBuffer = fileStream.GetBuffer();
                    return (int)fileStream.Length;
                }
            }

            return 0;
        }

        public async Task<Stream> GetFileStreamAsync(string filePath) {
            return await Task.FromResult(GetFileStream(filePath));
        }

        public async Task<byte[]> GetFileBytesAsync(string filePath) {
            return await Task.FromResult(GetFileBytes(filePath));
        }

        public void Dispose() {
            _archive?.Dispose();
        }

    }
}
