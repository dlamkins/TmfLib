using System.Collections.Generic;

namespace TmfLib.Pathable {
    public interface ITrail : IPointOfInterest, ITrailSection {

        IEnumerable<ITrailSection> TrailSections { get; }

    }

}
