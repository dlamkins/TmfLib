using System;
using TmfLib.Pathable;

namespace TmfLib.Writer.Modify {
    public class ShortenNamespace : IPackModifier {

        private string GetUniqueShortNameFromIndex(int columnNumber) {
            int    dividend  = columnNumber + 1;
            string shortName = string.Empty;

            while (dividend > 0) {
                int modulo = (dividend        - 1) % 26;
                shortName = Convert.ToChar(65 + modulo) + shortName;
                dividend  = (int)((dividend   - modulo) / 26);
            }

            return shortName;
        }

        private void CompressPathableCategoryNamespaces(PathingCategory category) {
            for (int i = 0; i < category.Count; i++) {
                category[i].Name = GetUniqueShortNameFromIndex(i);

                CompressPathableCategoryNamespaces(category[i]);
            }
        }

        public void Apply(IPackCollection pack) {
            CompressPathableCategoryNamespaces(pack.Categories);
        }

    }
}
