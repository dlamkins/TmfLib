using System.Collections.Generic;
using System.Linq;
using TmfLib.Pathable;

namespace TmfLib {
    public class PackCollection : IPackCollection {

        public PathingCategory Categories { get; }

        public IList<PointOfInterest> PointsOfInterest { get; }

        internal PackCollection(PathingCategory categories = null, IEnumerable<PointOfInterest> pointsOfInterest = null) {
            this.Categories       = categories                 ?? new PathingCategory(true);
            this.PointsOfInterest = pointsOfInterest?.ToList() ?? new List<PointOfInterest>();
        }

    }
}
