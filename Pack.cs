using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using TmfLib.Content;
using TmfLib.Reader;

namespace TmfLib {
    public class Pack {

        private readonly IDataReader _dataReader;

        public string Name => Path.GetFileNameWithoutExtension(_dataReader.GetPathRepresentation());

        /// <summary>
        /// Indicates that the marker pack has been optimized for loading by this library.
        /// </summary>
        public bool ManifestedPack { get; }

        public IPackResourceManager ResourceManager { get; }

        private Pack(IDataReader dataReader) {
            _dataReader = dataReader;

            this.ManifestedPack = dataReader.FileExists(PackConstImpl.FILE_OPTIMIZED_MARKERCATEGORIES);

            this.ResourceManager = new PackResourceManager(dataReader);
        }

        public static Pack FromIDataReader(IDataReader dataReader) => new Pack(dataReader);

        public static Pack FromArchivedMarkerPack(string archivePath) => new Pack(new ZipArchiveReader(archivePath));

        public static Pack FromDirectoryMarkerPack(string directoryPath) => new Pack(new DirectoryReader(directoryPath));

        public void ReleaseLocks() {
            _dataReader.AttemptReleaseLocks();
        }

        public async Task<IPackCollection> LoadAllAsync(IPackCollection packCollection = null, PackReaderSettings packReaderSettings = null) {
            var collection = packCollection ?? new PackCollection();

            var reader = new PackReader(collection, this.ResourceManager, packReaderSettings);

            var candidates = new List<(Stream fileStream, IDataReader dataReader)>();

            await _dataReader.LoadOnFileTypeAsync((fileStream, dataReader) => {
                candidates.Add((fileStream, dataReader));
                return Task.CompletedTask;
            }, ".xml");

            foreach (var candidate in candidates) {
                await reader.PopulatePackFromStream(candidate.fileStream);
            }

            return collection;
        }

        private async Task<IPackCollection> LoadMapFromOptimizedMarkerPackAsync(int mapId, IPackCollection packCollection, PackReaderSettings packReaderSettings = null) {
            var collection = packCollection ?? new PackCollection();

            var reader = new PackReader(collection, this.ResourceManager, packReaderSettings);

            await reader.PopulatePackFromStream(await _dataReader.GetFileStreamAsync(PackConstImpl.FILE_OPTIMIZED_MARKERCATEGORIES));

            if (_dataReader.FileExists(string.Format(PackConstImpl.FILE_OPTIMIZED_MAPPATHABLES, mapId))) {
                await reader.PopulatePackFromStream(await _dataReader.GetFileStreamAsync(string.Format(PackConstImpl.FILE_OPTIMIZED_MAPPATHABLES, mapId)));
            }

            return collection;
        }

        private async Task<IPackCollection> LoadMapFromNonOptimizedMarkerPackAsync(int mapId, IPackCollection packCollection, PackReaderSettings packReaderSettings = null) {
            await LoadAllAsync(packCollection == null
                                   ? null
                                   : new FilteredPackCollection(packCollection,
                                                                poi => poi.MapId == mapId),
                               packReaderSettings);

            return packCollection;
        }

        public async Task<IPackCollection> LoadMapAsync(int mapId, IPackCollection packCollection = null, PackReaderSettings packReaderSettings = null) {
            if (this.ManifestedPack) {
                return await LoadMapFromOptimizedMarkerPackAsync(mapId, packCollection, packReaderSettings);
            } else {
                return await LoadMapFromNonOptimizedMarkerPackAsync(mapId, packCollection, packReaderSettings);
            }   
        }

    }
}
