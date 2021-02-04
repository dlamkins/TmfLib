using System.Collections.Generic;
using TmfLib.Pathable;

namespace TmfLib {
    public interface IPackCollection {

        IPackResourceManager ResourceManager { get; }

        PathingCategory Categories { get; }

        List<PointOfInterest> PointsOfInterest { get; }

    }
}
