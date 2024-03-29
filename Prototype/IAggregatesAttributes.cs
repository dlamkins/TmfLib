﻿namespace TmfLib.Prototype {
    public interface IAggregatesAttributes {

        /// <summary>
        /// The attributes explicitly defined for the object (not ones inherited).
        /// </summary>
        AttributeCollection ExplicitAttributes { get; }

        /// <summary>
        /// The parent of this object.
        /// </summary>
        IAggregatesAttributes AttributeParent { get; }

    }

    public static class AggregatesAttributesImpl {

        public static string GetAggregatedAttributeValue(this IAggregatesAttributes owner, string attributeName) {
            if (owner.ExplicitAttributes.Contains(attributeName)) {
                return owner.ExplicitAttributes[attributeName].Value;
            } else if (owner.AttributeParent != null) {
                return owner.AttributeParent.GetAggregatedAttributeValue(attributeName);
            }

            return null;
        }

        public static bool TryGetAggregatedAttributeValue(this IAggregatesAttributes owner, string attributeName, out string value) {
            value = GetAggregatedAttributeValue(owner, attributeName);

            return value != null;
        }

        public static AttributeCollection GetAggregatedAttributes(this IAggregatesAttributes owner) {
            var aggregateAttributes = new AttributeCollection();

            if (owner.AttributeParent != null) {
                aggregateAttributes.AddOrUpdateAttributes(owner.AttributeParent.GetAggregatedAttributes());
            }

            aggregateAttributes.AddOrUpdateAttributes(owner.ExplicitAttributes);

            return aggregateAttributes;
        }

    }
}
