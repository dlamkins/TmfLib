using System.Collections.Generic;
using System.Linq;
using TmfLib.Pathable;

namespace TmfLib {
    public class PackCollection : IPackCollection {

        public IPackResourceManager ResourceManager { get; }

        public PathingCategory Categories { get; private set; }

        public List<PointOfInterest> PointsOfInterest { get; private set; }

        internal PackCollection(IPackResourceManager resourceManager, PathingCategory categories = null, IEnumerable<PointOfInterest> pointsOfInterest = null) {
            this.ResourceManager  = resourceManager;
            this.Categories       = categories ?? new PathingCategory(true);
            this.PointsOfInterest = pointsOfInterest?.ToList() ?? new List<PointOfInterest>();
        }

    }
}
