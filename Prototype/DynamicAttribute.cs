using System;

namespace TmfLib.Prototype {
    public class DynamicAttribute : IAttribute {
        
        private readonly Func<string> _valueFunc;

        public string Name  { get; }
        public string Value => _valueFunc();

        public DynamicAttribute(string name, Func<string> valueFunc) {
            this.Name  = name;
            _valueFunc = valueFunc;
        }

    }
}
