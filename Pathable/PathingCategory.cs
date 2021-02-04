using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using TmfLib.Prototype;

namespace TmfLib.Pathable {
    public class PathingCategory : KeyedCollection<string, PathingCategory>, IAggregatesAttributes {

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
            }
        }

        /// <summary>
        /// Indicates that the category is the root node (which contains all categories loaded for a pack).
        /// </summary>
        public bool Root { get; }

        private string _displayName;
        public string DisplayName {
            get => _displayName ?? this.Name;
            set => _displayName = value;
        }

        public bool IsSeparator { get; set; }

        public bool DefaultToggle { get; set; }

        public SynchronizedCollection<PointOfInterest> Pathables { get; } = new();

        public PathingCategory(string name) : base(StringComparer.OrdinalIgnoreCase) {
            this.Name = name;
        }

        public PathingCategory(bool root = false) {
            this.Root = root;
        }

        public string GetNamespace() => string.Join(".", this.GetParentsDesc().Select(c => c.Name));

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

            return this.GetOrAddCategoryFromNamespace(@namespace.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries));
        }

        public PathingCategory GetOrAddCategoryFromNamespace(IEnumerable<string> splitNamespace) {
            List<string> namespaceSegments = splitNamespace.ToList();

            string segmentValue = namespaceSegments[0];

            // Remove this namespace segment so that we can process this recursively
            namespaceSegments.RemoveAt(0);

            PathingCategory targetPathingCategory;

            if (!this.Contains(segmentValue)) {
                // Subcategory was not already defined
                targetPathingCategory = new PathingCategory(segmentValue) { Parent = this };
            } else {
                // Subcategory was already defined
                targetPathingCategory = this[segmentValue];
            }

            return namespaceSegments.Any()
                       // Not at end of namespace - continue drilling
                       ? targetPathingCategory.GetOrAddCategoryFromNamespace(namespaceSegments)
                       // At end of namespace - return target category
                       : targetPathingCategory;
        }

        protected override string GetKeyForItem(PathingCategory item) {
            return item.Name;
        }

    }
}
