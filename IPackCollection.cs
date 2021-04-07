using System.Collections.Generic;
using TmfLib.Pathable;

namespace TmfLib {
    public interface IPackCollection {

        PathingCategory Categories { get; }

        IList<PointOfInterest> PointsOfInterest { get; }

    }
}
