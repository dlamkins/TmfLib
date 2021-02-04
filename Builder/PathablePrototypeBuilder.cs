using System;
using System.IO;
using System.Linq;
using NanoXml;
using TmfLib.Pathable;
using TmfLib.Reader;

namespace TmfLib.Builder {
    public static class PathablePrototypeBuilder {

        public static PointOfInterest UnpackPathable(NanoXmlNode pathableNode, IPackResourceManager pathableResourceManager, PathingCategory rootPathingCategory) {
            return pathableNode.Name.ToLowerInvariant() switch {
                PackConstImpl.XML_ELEMENT_POI => UnpackMarkerPoi(pathableNode, pathableResourceManager, rootPathingCategory),
                PackConstImpl.XML_ELEMENT_TRAIL => UnpackTrailPoi(pathableNode, pathableResourceManager, rootPathingCategory),
                PackConstImpl.XML_ELEMENT_ROUTE => UnpackRoutePoi(pathableNode, pathableResourceManager, rootPathingCategory),
                _ => UnpackOtherPoi(pathableNode, pathableResourceManager, rootPathingCategory)
            };
        }

        private static PointOfInterest UnpackMarkerPoi(NanoXmlNode pathableNode, IPackResourceManager pathableResourceManager, PathingCategory rootPathingCategory) {
            var poiAttributes = PathablePrototypeAttributeBuilder.FromNanoXmlNode(pathableNode);

            return new PointOfInterest(pathableResourceManager, PointOfInterestType.Marker, poiAttributes, rootPathingCategory);
        }

        private static PointOfInterest UnpackTrailPoi(NanoXmlNode pathableNode, IPackResourceManager pathableResourceManager, PathingCategory rootPathingCategory) {
            var trailAttributes = PathablePrototypeAttributeBuilder.FromNanoXmlNode(pathableNode);

            if (trailAttributes.Contains(PackConstImpl.XML_KNOWNATTRIBUTE_TRAILDATA)) {
                // Map data is stored in the trl file itself instead of the node which means we have to load the trl file for a moment.
                string trlFile = trailAttributes[PackConstImpl.XML_KNOWNATTRIBUTE_TRAILDATA].Value;

                if (pathableResourceManager.ResourceExists(trlFile)) {
                    var trlStream = pathableResourceManager.LoadResource(trlFile);

                    var firstSegment = TrlFileReader.GetTrailsFromStream(new MemoryStream(trlStream)).First();

                    trailAttributes.AddOrUpdateAttribute(new Prototype.Attribute(PackConstImpl.XML_KNOWNATTRIBUTE_MAPID, firstSegment.MapId.ToString()));

                    return new PointOfInterest(pathableResourceManager, PointOfInterestType.Trail, trailAttributes, rootPathingCategory);
                } else {
                    // TODO: Log referenced trail file does not exist
                    Console.WriteLine("Log referenced trail file does not exist");
                }
            } else {
                // TODO: Log trail failed to get trl data
                Console.WriteLine("Log trail failed to get trl data");
            }

            return null;
        }

        private static PointOfInterest UnpackRoutePoi(NanoXmlNode pathableNode, IPackResourceManager pathableResourceManager, PathingCategory rootPathingCategory) {
            // TODO: Log deprecated route type
            // TODO: Implement route POI type
            return null;
        }

        private static PointOfInterest UnpackOtherPoi(NanoXmlNode pathableNode, IPackResourceManager pathableResourceManager, PathingCategory rootPathingCategory) {
            // TODO: Log unexpected element type passed to pathable builder
            return null;
        }

    }
}
