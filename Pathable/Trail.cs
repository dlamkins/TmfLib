using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using TmfLib.Prototype;
using TmfLib.Reader;

namespace TmfLib.Pathable {
    public class Trail : PointOfInterest, ITrail {

        public IEnumerable<ITrailSection> TrailSections { get; set; }

        public IEnumerable<Vector3> TrailPoints => this.TrailSections.SelectMany(s => s.TrailPoints);

        private Trail(IPackResourceManager resourceManager, AttributeCollection explicitAttributes, PathingCategory rootPathingCategory)
            : base(resourceManager, PointOfInterestType.Trail, explicitAttributes, rootPathingCategory) { /* NOOP */ }

        public static async Task<Trail> Build(IPackResourceManager resourceManager, AttributeCollection explicitAttributes, PathingCategory rootPathingCategory) {
            var newTrail = new Trail(resourceManager, explicitAttributes, rootPathingCategory);
            await newTrail.Preprocess(rootPathingCategory);

            return newTrail;
        }

        private async Task Preprocess(PathingCategory rootPathingCategory) {
            // Load trail data
            if (this.TryGetAggregatedAttributeValue(PackConstImpl.XML_KNOWNATTRIBUTE_TRAILDATA, out string trlFilePath)) {
                this.TrailSections = TrlFileReader.GetTrailSegments(await this.ResourceManager.LoadResourceAsync(trlFilePath));
            }
        }

    }
}
