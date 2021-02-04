using System.Collections.Generic;
using System.Numerics;

namespace TmfLib.Pathable {
    public interface ITrailSection {

        int                  MapId       { get; }
        IEnumerable<Vector3> TrailPoints { get; }

    }
}
