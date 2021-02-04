namespace TmfLib.Prototype {
    /// <summary>
    /// Represents a content-type agnostic name/value pairing.
    /// </summary>
    public class Attribute : IAttribute {

        public string Name  { get; }
        public string Value { get; set; }

        public Attribute(string name, string value) {
            this.Name  = name;
            this.Value = value;
        }

        public override string ToString() {
            return $"{this.Name}={this.Value}";
        }

    }
}
