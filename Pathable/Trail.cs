using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using TmfLib.Prototype;
using TmfLib.Reader;

namespace TmfLib.Pathable {
    public class Trail : PointOfInterest, ITrail {

        public IEnumerable<ITrailSection> TrailSections { get; set; }

        public IEnumerable<Vector3> TrailPoints => this.TrailSections.SelectMany(s => s.TrailPoints);

        public Trail(IPackResourceManager resourceManager, AttributeCollection explicitAttributes, PathingCategory rootPathingCategory) : base(resourceManager, PointOfInterestType.Trail, explicitAttributes, rootPathingCategory) {
            Preprocess(rootPathingCategory);
        }

        private void Preprocess(PathingCategory rootPathingCategory) {
            // Load trail data
            if (this.TryGetAggregatedAttributeValue(PackConstImpl.XML_KNOWNATTRIBUTE_TRAILDATA, out string trlFilePath)) {
                this.TrailSections = TrlFileReader.GetTrailSegments(this.ResourceManager.LoadResource(trlFilePath));
            }
        }

    }
}
