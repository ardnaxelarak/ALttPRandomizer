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

        public NoSettingNameAttribute(params RandomizerInstance[] randomizers) : base(randomizers) { }
    }

    internal class IgnoreSettingAttribute : RandomizerSpecificAttribute {
        public IgnoreSettingAttribute(params RandomizerInstance[] randomizers) : base(randomizers) { }
    }

    [AttributeUsage(AttributeTargets.Field, AllowMultiple=true)]
    internal class AdditionalSettingAttribute : RandomizerSpecificAttribute {
        public AdditionalSettingAttribute(params string[] settings) : base(null) {
            this.Settings = settings;
        }

        public AdditionalSettingAttribute(RandomizerInstance[] randomizers, params string[] settings) : base(randomizers) {
            this.Settings = settings;
        }

        public string[] Settings { get; }
    }

    [AttributeUsage(AttributeTargets.Field, AllowMultiple=true)]
    internal class AddStartingItemsAttribute : RandomizerSpecificAttribute {
        public AddStartingItemsAttribute(params string[] items) : base(null) {
            this.Items = items;
        }

        public AddStartingItemsAttribute(RandomizerInstance[] randomizers, params string[] items) : base(randomizers) {
            this.Items = items;
        }

        public string[] Items { get; }
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple=true)]
    internal class RequiredSettingAttribute : RandomizerSpecificAttribute {
        public RequiredSettingAttribute(params object[] values) : base(null) {
            this.Values = values;
        }

        public RequiredSettingAttribute(RandomizerInstance[] randomizers, params object[] values) : base(randomizers) {
            this.Values = values;
        }

        public object[] Values { get; }
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple=true)]
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
