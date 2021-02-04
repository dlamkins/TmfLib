using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace TmfLib.Pathable {
    public readonly struct StaticTrailSection : ITrailSection {

        private readonly Vector3[] _trailPoints;

        public int                  MapId       { get; }
        public IEnumerable<Vector3> TrailPoints => _trailPoints;

        public StaticTrailSection(int mapId, IEnumerable<Vector3> trailPoints) {
            this.MapId   = mapId;
            _trailPoints = trailPoints.ToArray();
        }

    }
}
