namespace ALttPRandomizer.Settings {
    using System;

    internal class CommonValueAttribute : Attribute {
        public CommonValueAttribute(string name) {
            Name = name;
        }

        public string Name { get; }
    }
}
