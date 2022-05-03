using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using TmfLib.Pathable;
using TmfLib.Prototype;

namespace TmfLib.Writer {
    public class PackWriter {

        public PackWriterSettings PackWriterSettings { get; set; }

        private static readonly string[] _manualMarkerCategories = {
            PackConstImpl.XML_KNOWNATTRIBUTE_NAME,
            //"displayname",
            //"isseparator",
            //"defaulttoggle"
        };

        public PackWriter(PackWriterSettings settings = null) {
            this.PackWriterSettings = settings ?? PackWriterSettings.DefaultPackWriterSettings;
        }

        public static async Task<Stream> GetPackArchiveStreamAsync(Pack pack, IPackCollection packCollection, PackWriterSettings settings = null) {
            return await new PackWriter(settings).GetArchiveStreamAsync(pack, packCollection);
        }

        public static async Task WritePackAsync(Pack pack, IPackCollection packCollection, string directoryPath, string packName, PackWriterSettings settings = null) {
            await new PackWriter(settings).WriteAsync(pack, packCollection, directoryPath, packName);
        }

        private async IAsyncEnumerable<(string filename, Stream stream)> GetFileStreams(Pack pack, IPackCollection packCollection) {
            // Export categories
            yield return (PackConstImpl.FILE_OPTIMIZED_MARKERCATEGORIES, WriteMarkerCategories(packCollection));

            // Get pathables by map ID
            var groupedPathables = packCollection.PointsOfInterest
                                       .Where(poi => poi.ParentPathingCategory != null && !ShouldIgnorePathableType(poi))
                                       .OrderBy(poi => poi.ParentPathingCategory.Namespace)
                                       .GroupBy(poi => poi.MapId);

            // Export each set of pathables per map
            foreach (var pathablesGroup in groupedPathables) {
                yield return (string.Format(PackConstImpl.FILE_OPTIMIZED_MAPPATHABLES, pathablesGroup.Key), WriteMapPois(pathablesGroup.Key, pathablesGroup));
            }

            // Export resources (textures, icons, and trl files) - dirty
            foreach (string resourceAttribute in new[] {PackConstImpl.XML_KNOWNATTRIBUTE_TEXTURE,
                                                        PackConstImpl.XML_KNOWNATTRIBUTE_ICONFILE,
                                                        PackConstImpl.XML_KNOWNATTRIBUTE_TRAILDATA}) {
                await foreach (var resource in GetReferencedResources(pack, packCollection, resourceAttribute)) {
                    yield return resource;
                }
            }
        }

        private bool ShouldIgnorePathableType(IPointOfInterest pathable) {
            switch (pathable.Type) {
                case PointOfInterestType.Marker when !this.PackWriterSettings.IncludeMarkers:
                case PointOfInterestType.Trail when !this.PackWriterSettings.IncludeTrails:
                case PointOfInterestType.Route when !this.PackWriterSettings.IncludeRoutes:
                    return true;
                default:
                    return false;
            }
        }

        private async IAsyncEnumerable<(string filename, Stream stream)> GetReferencedResources(Pack pack, IPackCollection packCollection, string resourceAttribute) {
            IEnumerable<string> resources = packCollection.PointsOfInterest
                                                .Where(p => !ShouldIgnorePathableType(p))
                                                .Select(p => p.GetAggregatedAttributeValue(resourceAttribute))
                                                .Distinct()
                                                .Where(s => s != null && pack.ResourceManager.ResourceExists(s)); // seems cursed

            foreach (string resourcePath in resources) {
                yield return (resourcePath.ToLowerInvariant(), new MemoryStream(await pack.ResourceManager.LoadResourceAsync(resourcePath)));
            }
        }

        public async Task WriteAsync(Pack pack, IPackCollection packCollection, string directoryPath, string packName) {
            if (!Directory.Exists(directoryPath)) throw new DirectoryNotFoundException($"Directory path '{directoryPath}' does not exist.");

            switch (this.PackWriterSettings.PackOutputMethod) {
                case PackWriterSettings.OutputMethod.Archive:
                    var archive = await GetArchiveStreamAsync(pack, packCollection);

                    string packPath = Path.Combine(directoryPath, packName);

                    using (var fileOut = new FileStream(packPath, FileMode.Create)) {
                        await archive.CopyToAsync(fileOut);
                    }
                    break;
                case PackWriterSettings.OutputMethod.Directory:
                    string rootPath = Path.Combine(directoryPath, packName);

                    Directory.CreateDirectory(rootPath);

                    await foreach (var file in GetFileStreams(pack, packCollection)) {
                        string filePath = Path.Combine(rootPath, file.filename);
                        string fileDir  = Path.GetDirectoryName(filePath);

                        Directory.CreateDirectory(fileDir);

                        using (var fileOut = new FileStream(filePath, FileMode.Create)) {
                            await file.stream.CopyToAsync(fileOut);
                        }
                    }
                    break;
            }
        }

        public async Task<Stream> GetArchiveStreamAsync(Pack pack, IPackCollection packCollection) {
            var archiveStream = new MemoryStream();

            using (var outputZip = new ZipArchive(archiveStream, ZipArchiveMode.Create, true)) { 
                await foreach (var file in GetFileStreams(pack, packCollection)) {
                    using (var fileEntryStream = outputZip.CreateEntry(file.filename).Open()) {
                        await file.stream.CopyToAsync(fileEntryStream);
                    }
                }
            }

            archiveStream.Seek(0, SeekOrigin.Begin);

            return archiveStream;
        }

        private Stream WriteMarkerCategories(IPackCollection pack) {
            var markerCategoryStream = new MemoryStream();
            var categoryWriter       = XmlWriter.Create(markerCategoryStream);

            categoryWriter.WriteStartDocument();
            categoryWriter.WriteStartElement(PackConstImpl.XML_ELEMENT_OVERLAYDATA);

            foreach (var category in pack.Categories) {
                WriteMarkerCategory(categoryWriter, category);
            }

            categoryWriter.WriteEndElement();

            categoryWriter.WriteEndDocument();
            categoryWriter.Close();

            markerCategoryStream.Seek(0, SeekOrigin.Begin);

            return markerCategoryStream;
        }

        private void WriteMarkerCategory(XmlWriter categoryWriter, PathingCategory pathingCategory) {
            categoryWriter.WriteStartElement(PackConstImpl.XML_ELEMENT_MARKERCATEGORY);

            categoryWriter.WriteAttributeString(PackConstImpl.XML_KNOWNATTRIBUTE_NAME, pathingCategory.Name);

            foreach (var attribute in pathingCategory.ExplicitAttributes.Where(attribute => !_manualMarkerCategories.Contains(attribute.Name))) {
                categoryWriter.WriteAttributeString(attribute.Name, attribute.Value);
            }

            foreach (var subcategory in pathingCategory) {
                WriteMarkerCategory(categoryWriter, subcategory);
            }

            categoryWriter.WriteEndElement();
        }

        private Stream WriteMapPois(int mapId, IEnumerable<IPointOfInterest> pathables) {
            var mapPoiStream = new MemoryStream();
            var poisWriter   = XmlWriter.Create(mapPoiStream);

            poisWriter.WriteStartDocument();
            poisWriter.WriteStartElement(PackConstImpl.XML_ELEMENT_OVERLAYDATA);
            poisWriter.WriteStartElement(PackConstImpl.XML_ELEMENT_POIS);

            foreach (var pathable in pathables) {
                WriteMapPoi(poisWriter, pathable);
            }

            poisWriter.WriteEndElement();
            poisWriter.WriteEndElement();

            poisWriter.WriteEndDocument();
            poisWriter.Close();

            mapPoiStream.Seek(0, SeekOrigin.Begin);

            return mapPoiStream;
        }

        private void WriteMapPoi(XmlWriter pathableWriter, IPointOfInterest pathable) {
            if (pathable.Type == PointOfInterestType.Route) {
                // TODO: Implement route writing
                throw new InvalidOperationException("Exporting routes is not currently supported.");
            }

            pathableWriter.WriteStartElement(pathable.Type == PointOfInterestType.Marker ? PackConstImpl.XML_ELEMENT_POI : PackConstImpl.XML_ELEMENT_TRAIL);

            pathableWriter.WriteAttributeString(PackConstImpl.XML_KNOWNATTRIBUTE_TYPE, pathable.ParentPathingCategory?.Namespace ?? "unspecified");

            foreach (var attribute in pathable.ExplicitAttributes) {
                if (this.PackWriterSettings.SkipMapId && attribute.Name == PackConstImpl.XML_KNOWNATTRIBUTE_MAPID) continue;
                if (attribute.Name == PackConstImpl.XML_KNOWNATTRIBUTE_TYPE) continue;
                pathableWriter.WriteAttributeString(attribute.Name, attribute.Value);
            }

            pathableWriter.WriteEndElement();
        }

    }
}
