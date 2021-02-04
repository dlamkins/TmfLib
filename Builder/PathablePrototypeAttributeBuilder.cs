using NanoXml;
using TmfLib.Prototype;

namespace TmfLib.Builder {
    public static class PathablePrototypeAttributeBuilder {

        public static AttributeCollection FromNanoXmlNode(NanoXmlNode node) {
            return new(node.Attributes);
        }

    }
}
