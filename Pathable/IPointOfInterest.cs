using TmfLib.Prototype;

namespace TmfLib.Pathable {
    public interface IPointOfInterest : IAggregatesAttributes {

        IPackResourceManager ResourceManager { get; }

        /// <summary>
        /// The type of pathing element this point of interest represents.
        /// </summary>
        PointOfInterestType Type { get; }

        /// <summary>
        /// The <see cref="PathingCategory"/> which contains this <see cref="IPointOfInterest"/>.
        /// </summary>
        PathingCategory ParentPathingCategory { get; }

        /// <summary>
        /// The map ID this <see cref="IPointOfInterest"/> is intended to show on.
        /// </summary>
        int MapId { get; }

    }
}
