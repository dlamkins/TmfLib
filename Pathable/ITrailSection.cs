using System.Collections.Generic;
using System.Numerics;

namespace TmfLib.Pathable {
    public interface ITrailSection {

        /// <summary>
        /// The map ID this <see cref="ITrailSection"/> is intended to show on.
        /// </summary>
        int MapId { get; }

        IEnumerable<Vector3> TrailPoints { get; }

    }
}
