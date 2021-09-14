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

        /// <summary>
        /// The name of the category.
        /// </summary>
        public string Name { get; set; }

        private PathingCategory _parent;
        /// <summary>
        /// The category's parent category.
        /// </summary>
        public PathingCategory Parent {
            get => _parent;
            set {
                if (_parent == value) return;

                _parent?.Remove(this);

                _parent = value;
                _parent?.Add(this);

                SpoilNamespaceCache();
            }
        }
        
        /// <summary>
        /// Indicates that the category is the root node (which contains all categories loaded for a pack).
        /// </summary>
        public bool Root { get; }
        
        /// <summary>
        /// The display name of the category displayed within the UI.
        /// </summary>
        public string DisplayName { get; set; } = string.Empty;
        
        /// <summary>
        /// If the category is used to display a header for other categories.
        /// </summary>
        public bool IsSeparator { get; set; } = DEFAULT_ISSEPARATOR;

        /// <summary>
        /// If the category should be enabled by default.
        /// </summary>
        public bool DefaultToggle { get; set; } = DEFAULT_DEFAULTTOGGLE;

        private string _cachedNamespace = null;
        /// <summary>
        /// The full namespace that this category is within.
        /// </summary>
        public string Namespace => GetNamespace();

        public PathingCategory(string name) : base(StringComparer.OrdinalIgnoreCase) {
            this.Name = name;
        }

        public PathingCategory(bool root = false) {
            this.Root = root;
        }
        
        private string GetNamespace() {
            return _cachedNamespace ??= string.Join(".", GetParentsDesc().Select(c => c.Name));
        }

        private void SpoilNamespaceCache() {
            _cachedNamespace = null;

            foreach (var category in this) {
                category.SpoilNamespaceCache();
            } 
        }

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
            return GetParents().Reverse();
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
