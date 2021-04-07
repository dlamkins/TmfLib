using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TmfLib.Content;
using TmfLib.Pathable;
using TmfLib.Reader;

namespace TmfLib {
    public class Pack {

        private readonly IDataReader _dataReader;

        public string Name => _dataReader.GetPathRepresentation();

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

        public static Pack FromArchivedMarkerPack(string archivePath) => new Pack(new ZipArchiveReader(archivePath));

        public static Pack FromDirectoryMarkerPack(string directoryPath) => new Pack(new DirectoryReader(directoryPath));

        public async Task<IPackCollection> LoadAllAsync(IPackCollection packCollection = null) {
            var collection = packCollection ?? new PackCollection();

            var markerReader = new PackFileReader(collection, this.ResourceManager);

            var candidates = new List<(Stream fileStream, IDataReader dataReader)>();

            _dataReader.LoadOnFileType((fileStream, dataReader) => {
                candidates.Add((fileStream, dataReader));
            }, ".xml");

            foreach (var candidate in candidates) {
                await markerReader.PopulatePackFromStream(candidate.fileStream);
            }

            return collection;
        }

        private async Task<IPackCollection> LoadMapFromOptimizedMarkerPackAsync(int mapId, IPackCollection packCollection) {
            var collection = packCollection ?? new PackCollection();

            var markerReader = new PackFileReader(collection, this.ResourceManager);

            await markerReader.PopulatePackFromStream(await _dataReader.GetFileStreamAsync(PackConstImpl.FILE_OPTIMIZED_MARKERCATEGORIES));

            if (_dataReader.FileExists(string.Format(PackConstImpl.FILE_OPTIMIZED_MAPPATHABLES, mapId))) {
                await markerReader.PopulatePackFromStream(await _dataReader.GetFileStreamAsync(string.Format(PackConstImpl.FILE_OPTIMIZED_MAPPATHABLES, mapId)));
            }

            return collection;
        }

        private async Task<IPackCollection> LoadMapFromNonOptimizedMarkerPackAsync(int mapId, IPackCollection packCollection) {
            await LoadAllAsync(packCollection == null
                                   ? null
                                   : new FilteredPackCollection(packCollection,
                                                                poi => poi.MapId == mapId));

            return packCollection;
        }

        public async Task<IPackCollection> LoadMapAsync(int mapId, IPackCollection packCollection = null) {
            if (this.ManifestedPack) {
                return await LoadMapFromOptimizedMarkerPackAsync(mapId, packCollection);
            } else {
                return await LoadMapFromNonOptimizedMarkerPackAsync(mapId, packCollection);
            }   
        }

    }
}
