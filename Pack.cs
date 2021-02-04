using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TmfLib.Content;
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

        public async Task<PackCollection> LoadAllAsync() {
            var collection = new PackCollection(this.ResourceManager);

            var markerReader = new PackFileReader(collection);

            var candidates = new List<(Stream fileStream, IDataReader dataReader)>();

            _dataReader.LoadOnFileType((fileStream, dataReader) => {
                candidates.Add((fileStream, dataReader));
            }, ".xml");

            foreach (var candidate in candidates) {
                await markerReader.PopulatePackFromStream(candidate.fileStream);
            }

            return collection;
        }

        private async Task<IPackCollection> LoadMapFromOptimizedMarkerPackAsync(int mapId) {
            var collection = new PackCollection(this.ResourceManager);

            var markerReader = new PackFileReader(collection);

            await markerReader.PopulatePackFromStream(await _dataReader.GetFileStreamAsync(PackConstImpl.FILE_OPTIMIZED_MARKERCATEGORIES));

            if (_dataReader.FileExists(string.Format(PackConstImpl.FILE_OPTIMIZED_MAPPATHABLES, mapId))) {
                await markerReader.PopulatePackFromStream(await _dataReader.GetFileStreamAsync(string.Format(PackConstImpl.FILE_OPTIMIZED_MAPPATHABLES, mapId)));
            }

            return collection;
        }

        private async Task<IPackCollection> LoadMapFromNonOptimizedMarkerPackAsync(int mapId) {
            var completeCollection = await LoadAllAsync();

            return new PackCollection(completeCollection.ResourceManager,
                                            completeCollection.Categories,
                                            completeCollection.PointsOfInterest.Where(p => p.MapId == mapId));
        }

        public async Task<IPackCollection> LoadMapAsync(int mapId) {
            if (this.ManifestedPack) {
                return await LoadMapFromOptimizedMarkerPackAsync(mapId);
            } else {
                return await LoadMapFromNonOptimizedMarkerPackAsync(mapId);
            }   
        }

    }
}
