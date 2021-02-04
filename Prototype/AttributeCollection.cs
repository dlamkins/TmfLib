﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace TmfLib.Prototype {
    public class AttributeCollection : KeyedCollection<string, IAttribute> {

        public AttributeCollection() : base(StringComparer.OrdinalIgnoreCase) { /* NOOP */ }

        public AttributeCollection(IEnumerable<IAttribute> attributeCollection) : base(StringComparer.OrdinalIgnoreCase) {
            foreach (var attribute in attributeCollection.GroupBy(a => a.Name).Select(g => g.Last())) {
                this.Add(attribute);
            }
        }

        public void AddOrUpdateAttribute(IAttribute attribute) {
            this.Remove(attribute.Name); // Prevent duplicates and force updates
            this.Add(attribute);
        }

        public void AddOrUpdateAttributes(IEnumerable<IAttribute> attributes) {
            foreach (var attribute in attributes) {
                AddOrUpdateAttribute(attribute);
            }
        }

        public bool TryGetAttribute(string attributeName, out IAttribute attribute) {
            attribute = this.Contains(attributeName)
                ? this[attributeName]
                : null;

            return attribute != null;
        }

        protected override string GetKeyForItem(IAttribute item) => item.Name;

        public override string ToString() {
            return string.Join(", ", this);
        }

    }
}
