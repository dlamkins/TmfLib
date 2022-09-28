using System;
using System.Linq;
using NanoXml;
using TmfLib.Pathable;

namespace TmfLib.Builder {
    public static class PathableCategoryBuilder {

        private const string ELEMENT_CATEGORY = "markercategory";

        private const string MARKERCATEGORY_NAME_ATTR          = "name";
        private const string MARKERCATEGORY_DISPLAYNAME_ATTR   = "displayname";
        private const string MARKERCATEGORY_ISSEPARATOR_ATTR   = "isseparator";
        private const string MARKERCATEGORY_DEFAULTTOGGLE_ATTR = "defaulttoggle";
        private const string MARKERCATEGORY_ISHIDDEN_ATTR      = "ishidden";

        public static void UnpackCategory(NanoXmlNode categoryNode, PathingCategory pathingCategoryParent) {
            if (!string.Equals(categoryNode.Name, ELEMENT_CATEGORY, StringComparison.OrdinalIgnoreCase)) {
                // TODO: Log attempted to unpack wrong element type as markercategory
                return;
            }

            var loadedCategory = FromNanoXmlNode(categoryNode, pathingCategoryParent);

            if (loadedCategory == null) return;

            foreach (var childCategoryNode in categoryNode.SubNodes) {
                UnpackCategory(childCategoryNode, loadedCategory);
            }
        }

        public static PathingCategory FromNanoXmlNode(NanoXmlNode categoryNode, PathingCategory parent) {
            string categoryName = categoryNode.GetAttribute(MARKERCATEGORY_NAME_ATTR)?.Value;

            // Can't define a marker category without a name.
            if (string.IsNullOrEmpty(categoryName)) {
                // TODO: Log markercategory has no name.
                return null;
            }

            var subjCategory = parent.Contains(categoryName)
                                   // We're extending an existing category.
                                   ? parent[categoryName]
                                   // We're adding a new category.
                                   : parent.GetOrAddCategoryFromNamespace(categoryName);

            subjCategory.LoadedFromPack = true;

            // Have to ensure we only override if the attributes exist in case we're overwriting an existing category.
            if (categoryNode.TryGetAttribute(MARKERCATEGORY_DISPLAYNAME_ATTR, out var displayNameAttr))
                subjCategory.DisplayName = displayNameAttr.Value;

            if (categoryNode.TryGetAttribute(MARKERCATEGORY_ISSEPARATOR_ATTR, out var isSeparatorAttr))
                subjCategory.IsSeparator = isSeparatorAttr.Value == "1";

            if (categoryNode.TryGetAttribute(MARKERCATEGORY_DEFAULTTOGGLE_ATTR, out var defaultToggleAttr))
                subjCategory.DefaultToggle = defaultToggleAttr.Value != "0";

            if (categoryNode.TryGetAttribute(MARKERCATEGORY_ISHIDDEN_ATTR, out var isHiddenAttr))
                subjCategory.IsHidden = isHiddenAttr.Value != "0";

            // Remove redundant attributes now kept track of by the pathable itself.
            categoryNode.Attributes.RemoveAll(attr => new[] {
                                                  MARKERCATEGORY_NAME_ATTR,
                                                  //MARKERCATEGORY_DISPLAYNAME_ATTR,
                                                  MARKERCATEGORY_ISSEPARATOR_ATTR,
                                                  MARKERCATEGORY_DEFAULTTOGGLE_ATTR,
                                                  MARKERCATEGORY_ISHIDDEN_ATTR
                                              }.Contains(attr.Name));

            subjCategory.SetAttributes(PathablePrototypeAttributeBuilder.FromNanoXmlNode(categoryNode));

            return subjCategory;
        }

    }
}
