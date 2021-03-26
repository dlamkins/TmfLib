using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace TmfLib.Pathable {
    public readonly struct TrailSection : ITrailSection {

        private readonly Vector3[] _trailPoints;

        public int MapId { get; }

        public IEnumerable<Vector3> TrailPoints => _trailPoints;

        public TrailSection(int mapId, IEnumerable<Vector3> trailPoints) {
            this.MapId   = mapId;
            _trailPoints = trailPoints.ToArray();
        }

    }
}
