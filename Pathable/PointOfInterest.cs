using TmfLib.Prototype;

namespace TmfLib.Pathable {
    public class PointOfInterest : IPointOfInterest {

        public AttributeCollection ExplicitAttributes { get; }

        IAggregatesAttributes IAggregatesAttributes.AttributeParent => this.ParentPathingCategory;

        public PointOfInterestType Type { get; }

        public PathingCategory ParentPathingCategory { get; private set; }

        private int _mapId = -1;
        public  int MapId => _mapId;

        public IPackResourceManager ResourceManager { get; }

        public PointOfInterest(IPackResourceManager resourceManager, PointOfInterestType type, AttributeCollection explicitAttributes, PathingCategory rootPathingCategory) {
            this.ResourceManager = resourceManager;
            this.Type = type;
            this.ExplicitAttributes = explicitAttributes;

            Preprocess(rootPathingCategory);
        }

        private void Preprocess(PathingCategory rootPathingCategory) {
            // Assign ParentCategory
            if (this.TryGetAggregatedAttributeValue(PackConstImpl.XML_KNOWNATTRIBUTE_TYPE, out string categoryNamespace)) {
                this.ParentPathingCategory = rootPathingCategory.GetOrAddCategoryFromNamespace(categoryNamespace);
                this.ParentPathingCategory.AddPathable(this);

                this.ExplicitAttributes.AddOrUpdateAttribute(new DynamicAttribute(PackConstImpl.XML_KNOWNATTRIBUTE_TYPE, this.ParentPathingCategory.GetNamespace));
            }

            // Assign MapId
            if (this.TryGetAggregatedAttributeValue(PackConstImpl.XML_KNOWNATTRIBUTE_MAPID, out string mapId)) {
                InvariantParseUtil.TryParseInt(mapId, out _mapId);
            }
        }

    }
}
