namespace ALttPRandomizer.Settings {
    using System;

    internal class SettingNameAttribute : Attribute {
        public SettingNameAttribute(string name) {
            this.Name = name;
        }

        public string Name { get; }
    }

    internal class NoSettingNameAttribute : Attribute { }

    internal class AdditionalSettingAttribute : Attribute {
        public AdditionalSettingAttribute(string setting) {
            this.Setting = setting;
        }

        public string Setting { get; }
    }
}
