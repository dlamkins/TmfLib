using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using TmfLib.Prototype;

namespace TmfLib.Pathable {
    public class PathingCategory : KeyedCollection<string, ICategory>, ICategory {

        private const bool DEFAULT_ISSEPARATOR   = false;
        private const bool DEFAULT_DEFAULTTOGGLE = true;

        public AttributeCollection ExplicitAttributes { get; } = new();

        IAggregatesAttributes IAggregatesAttributes.AttributeParent => Parent;

        public string Name { get; set; }

        private ICategory _parent;
        public ICategory Parent {
            get => _parent;
            set {
                if (_parent == value) return;

                _parent?.Remove(this);

                _parent = value;
                _parent?.Add(this);

                _cachedNamespace = null;
            }
        }

        public bool Root { get; }

        public  string DisplayName { get; set; } = string.Empty;

        public bool IsSeparator { get; set; } = DEFAULT_ISSEPARATOR;

        public bool DefaultToggle { get; set; } = DEFAULT_DEFAULTTOGGLE;

        private readonly SynchronizedCollection<PointOfInterest> _pathables = new();
        public           IReadOnlyCollection<PointOfInterest>    Pathables => new ReadOnlyCollection<PointOfInterest>(_pathables);


        private string _cachedNamespace = null;
        public  string Namespace => GetNamespace();

        public PathingCategory(string name) : base(StringComparer.OrdinalIgnoreCase) {
            this.Name = name;
        }

        public PathingCategory(bool root = false) {
            this.Root = root;
        }

        // TODO: Review cache invalidation - it doesn't bubble down, so can become invalid if an ancestor changes its parent.
        private string GetNamespace() => _cachedNamespace ??= string.Join(".", this.GetParentsDesc().Select(c => c.Name));
        
        public IEnumerable<ICategory> GetParents() {
            ICategory parentCategory = this;

            do {
                yield return parentCategory;
            } while ((parentCategory = parentCategory.Parent) != null && (!parentCategory.Root));
        }

        
        public IEnumerable<ICategory> GetParentsDesc() {
            return this.GetParents().Reverse();
        }

        public void AddPathable(PointOfInterest pathable) {
            _pathables.Add(pathable);
        }

        public void ClearPathables() {
            _pathables.Clear();
        }

        public void SetAttributes(AttributeCollection attributes) {
            this.ExplicitAttributes.AddOrUpdateAttributes(attributes);
        }

        public ICategory GetOrAddCategoryFromNamespace(string @namespace) {
            if (@namespace == null) throw new ArgumentNullException(nameof(@namespace));

            return InternalGetOrAddCategoryFromNamespace(@namespace, true);
        }

        public bool TryGetCategoryFromNamespace(string @namespace, out ICategory category) {
            if (@namespace == null) throw new ArgumentNullException(nameof(@namespace));

            return (category = InternalGetOrAddCategoryFromNamespace(@namespace, false)) != null;
        }

        private PathingCategory InternalGetOrAddCategoryFromNamespace(string @namespace, bool canAdd) {
            return InternalGetOrAddCategoryFromNamespace(new Queue<string>(@namespace.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries)), canAdd);
        }

        private PathingCategory InternalGetOrAddCategoryFromNamespace(Queue<string> namespaceSegments, bool canAdd) {
            string segmentValue = namespaceSegments.Dequeue();

            PathingCategory targetPathingCategory;

            if (!this.Contains(segmentValue)) {
                // Subcategory was not already defined.
                if (canAdd) {
                    targetPathingCategory = new PathingCategory(segmentValue) { Parent = this };
                } else {
                    return null;
                }
            } else {
                // Subcategory was already defined.
                targetPathingCategory = this[segmentValue] as PathingCategory;
            }

            if (targetPathingCategory == null) return null;

            return namespaceSegments.Any()
                       // Not at end of namespace - continue drilling.
                       ? targetPathingCategory.InternalGetOrAddCategoryFromNamespace(namespaceSegments, canAdd)
                       // At end of namespace - return target category.
                       : targetPathingCategory;
        }

        protected override string GetKeyForItem(ICategory category) {
            return category.Name;
        }

    }
}
