namespace ALttPRandomizer.Settings {
    using ALttPRandomizer.Model;
    using System;
    using System.Linq;

    internal class RandomizerNameAttribute : Attribute {
        public RandomizerNameAttribute(string name) {
            this.Name = name;
        }

        public string Name { get; }
    }

    internal abstract class RandomizerSpecificAttribute : Attribute {
        public RandomizerSpecificAttribute(RandomizerInstance[]? randomizers) {
            this.Randomizers = randomizers;
        }

        protected RandomizerInstance[]? Randomizers { get; }

        public bool HasRandomizer(RandomizerInstance name) {
            if (this.Randomizers == null) {
                return true;
            }

            return this.Randomizers.Contains(name);
        }
    }

    internal class SettingNameAttribute : RandomizerSpecificAttribute {
        public SettingNameAttribute(string name) : base(null) {
            this.Name = name;
        }

        public SettingNameAttribute(RandomizerInstance[] randomizers, string name) : base(randomizers) {
            this.Name = name;
        }

        public string Name { get; }
    }

    internal class NoSettingNameAttribute : RandomizerSpecificAttribute {
        public NoSettingNameAttribute() : base(null) { }

        public NoSettingNameAttribute(RandomizerInstance[] randomizers) : base(randomizers) { }
    }

    internal class AdditionalSettingAttribute : RandomizerSpecificAttribute {
        public AdditionalSettingAttribute(string setting) : base(null) {
            this.Setting = setting;
        }

        public AdditionalSettingAttribute(RandomizerInstance[] randomizers, string setting) : base(randomizers) {
            this.Setting = setting;
        }

        public string Setting { get; }
    }

    internal class RequiredSettingAttribute : RandomizerSpecificAttribute {
        public RequiredSettingAttribute(params object[] values) : base(null) {
            this.Values = values;
        }

        public RequiredSettingAttribute(RandomizerInstance[] randomizers, params object[] values) : base(randomizers) {
            this.Values = values;
        }

        public object[] Values { get; }
    }

    internal class ForbiddenSettingAttribute : RandomizerSpecificAttribute {
        public ForbiddenSettingAttribute(params object[] values) : base(null) {
            this.Values = values;
        }

        public ForbiddenSettingAttribute(RandomizerInstance[] randomizers, params object[] values) : base(randomizers) {
            this.Values = values;
        }

        public object[] Values { get; }
    }
}
