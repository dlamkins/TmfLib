using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using TmfLib.Prototype;

namespace TmfLib.Pathable {
    public class PathingCategory : KeyedCollection<string, PathingCategory>, IAggregatesAttributes {

        private const bool DEFAULT_ISSEPARATOR   = false;
        private const bool DEFAULT_DEFAULTTOGGLE = true;

        public AttributeCollection ExplicitAttributes { get; } = new();

        IAggregatesAttributes IAggregatesAttributes.AttributeParent => Parent;

        public string Name { get; set; }

        private PathingCategory _parent;
        public PathingCategory Parent {
            get => _parent;
            set {
                if (_parent == value) return;

                _parent?.Remove(this);

                _parent = value;
                _parent?.Add(this);

                _cachedNamespace = null;
            }
        }

        private string _cachedNamespace = null;

        /// <summary>
        /// Indicates that the category is the root node (which contains all categories loaded for a pack).
        /// </summary>
        public bool Root { get; }

        public  string DisplayName { get; set; } = string.Empty;

        public bool IsSeparator { get; set; } = DEFAULT_ISSEPARATOR;

        public bool DefaultToggle { get; set; } = DEFAULT_DEFAULTTOGGLE;

        public SynchronizedCollection<PointOfInterest> Pathables { get; } = new();

        public PathingCategory(string name) : base(StringComparer.OrdinalIgnoreCase) {
            this.Name = name;
        }

        public PathingCategory(bool root = false) {
            this.Root = root;
        }

        // TODO: Review cache invalidation - it doesn't bubble down, so can become invalid if an ancestor changes its parent.
        public string GetNamespace() => _cachedNamespace ??= string.Join(".", this.GetParentsDesc().Select(c => c.Name));

        /// <summary>
        /// Enumerates the category in ascending order up to its highest level parent.
        /// </summary>
        public IEnumerable<PathingCategory> GetParents() {
            var parentCategory = this;

            do {
                yield return parentCategory;
            } while ((parentCategory = parentCategory.Parent) != null && (!parentCategory.Root));
        }

        /// <summary>
        /// Enumerates the category in descending order up to its highest level parent.
        /// </summary>
        public IEnumerable<PathingCategory> GetParentsDesc() {
            return this.GetParents().Reverse();
        }

        public void AddPathable(PointOfInterest pathable) {
            this.Pathables.Add(pathable);
        }

        public void SetAttributes(AttributeCollection attributes) {
            this.ExplicitAttributes.AddOrUpdateAttributes(attributes);
        }

        public PathingCategory GetOrAddCategoryFromNamespace(string @namespace) {
            if (@namespace == null) throw new ArgumentNullException(nameof(@namespace));

            return InternalGetOrAddCategoryFromNamespace(@namespace, true);
        }

        public bool TryGetCategoryFromNamespace(string @namespace, out PathingCategory category) {
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
                targetPathingCategory = this[segmentValue];
            }

            return namespaceSegments.Any()
                       // Not at end of namespace - continue drilling.
                       ? targetPathingCategory.InternalGetOrAddCategoryFromNamespace(namespaceSegments, canAdd)
                       // At end of namespace - return target category.
                       : targetPathingCategory;
        }

        protected override string GetKeyForItem(PathingCategory category) {
            return category.Name;
        }

    }
}
