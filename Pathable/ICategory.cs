using System.Collections.Generic;
using TmfLib.Prototype;

namespace TmfLib.Pathable {
    public interface ICategory : IAggregatesAttributes, ICollection<ICategory> {

        /// <summary>
        /// The name of the category.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// The display name of the category displayed within the UI.
        /// </summary>
        string DisplayName { get; }

        /// <summary>
        /// Indicates that the category is the root node (which contains all categories loaded for a pack).
        /// </summary>
        bool Root { get; }

        /// <summary>
        /// The category's parent category.
        /// </summary>
        ICategory Parent { get; }

        /// <summary>
        /// If the category is used to display a header for other categories.
        /// </summary>
        bool IsSeparator { get; }

        /// <summary>
        /// If the category should be enabled by default.
        /// </summary>
        bool DefaultToggle { get; }

        /// <summary>
        /// The full namespace that this category is within.
        /// </summary>
        string Namespace { get; }

        /// <summary>
        /// Enumerates the category in ascending order up to its highest level parent.
        /// </summary>
        IEnumerable<ICategory> GetParents();
        
        /// <summary>
        /// Enumerates the category in descending order up to its highest level parent.
        /// </summary>
        IEnumerable<ICategory> GetParentsDesc();

        ICategory GetOrAddCategoryFromNamespace(string @namespace);

        bool TryGetCategoryFromNamespace(string @namespace, out ICategory category);

    }
}
