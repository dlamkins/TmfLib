using System;
using System.Linq;
using System.Threading.Tasks;
using NanoXml;
using TmfLib.Pathable;
using TmfLib.Reader;

namespace TmfLib.Builder {
    public static class PathablePrototypeBuilder {

        public static async Task<PointOfInterest> UnpackPathableAsync(NanoXmlNode pathableNode, IPackResourceManager pathableResourceManager, PathingCategory rootPathingCategory) {
            return pathableNode.Name.ToLowerInvariant() switch {
                PackConstImpl.XML_ELEMENT_POI => await UnpackMarkerPoi(pathableNode, pathableResourceManager, rootPathingCategory),
                PackConstImpl.XML_ELEMENT_TRAIL => await UnpackTrailPoi(pathableNode, pathableResourceManager, rootPathingCategory),
                PackConstImpl.XML_ELEMENT_ROUTE => await UnpackRoutePoi(pathableNode, pathableResourceManager, rootPathingCategory),
                _ => await UnpackOtherPoi(pathableNode, pathableResourceManager, rootPathingCategory)
            };
        }

        private static Task<PointOfInterest> UnpackMarkerPoi(NanoXmlNode pathableNode, IPackResourceManager pathableResourceManager, PathingCategory rootPathingCategory) {
            var poiAttributes = PathablePrototypeAttributeBuilder.FromNanoXmlNode(pathableNode);

            return Task.FromResult(new PointOfInterest(pathableResourceManager, PointOfInterestType.Marker, poiAttributes, rootPathingCategory));
        }

        private static async Task<PointOfInterest> UnpackTrailPoi(NanoXmlNode pathableNode, IPackResourceManager pathableResourceManager, PathingCategory rootPathingCategory) {
            var trailAttributes = PathablePrototypeAttributeBuilder.FromNanoXmlNode(pathableNode);

            if (trailAttributes.Contains(PackConstImpl.XML_KNOWNATTRIBUTE_TRAILDATA)) {
                // Map data is stored in the trl file itself instead of the node which means we have to load the trl file for a moment.
                string trlFile = trailAttributes[PackConstImpl.XML_KNOWNATTRIBUTE_TRAILDATA].Value;

                if (pathableResourceManager.ResourceExists(trlFile)) {
                    var trlStream = await pathableResourceManager.LoadResourceAsync(trlFile);

                    int mapId = TrlFileReader.GetTrailMap(trlStream);
                    
                    if (mapId < 0) {
                        // Format parse failure.
                        return null;
                    }

                    trailAttributes.AddOrUpdateAttribute(new Prototype.Attribute(PackConstImpl.XML_KNOWNATTRIBUTE_MAPID, mapId.ToString()));

                    return await Trail.Build(pathableResourceManager, trailAttributes, rootPathingCategory);
                } else {
                    // TODO: Log referenced trail file does not exist
                }
            } else {
                // TODO: Log trail failed to get trl data
            }

            return null;
        }

        private static async Task<PointOfInterest> UnpackRoutePoi(NanoXmlNode pathableNode, IPackResourceManager pathableResourceManager, PathingCategory rootPathingCategory) {
            // TODO: Log deprecated route type
            // TODO: Implement route POI type
            return null;
        }

        private static async Task<PointOfInterest> UnpackOtherPoi(NanoXmlNode pathableNode, IPackResourceManager pathableResourceManager, PathingCategory rootPathingCategory) {
            // TODO: Log unexpected element type passed to pathable builder
            return null;
        }

    }
}
