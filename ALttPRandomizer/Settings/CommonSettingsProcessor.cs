namespace ALttPRandomizer.Settings {
    using ALttPRandomizer.Model;
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    public class CommonSettingsProcessor {
        public KeyValuePair<string, string> GetSettingPair<T>(T value) where T : Enum {
            var name = this.GetValueName(value);

            Type type = typeof(T);

            var settingName = type.GetCustomAttribute<CommonValueAttribute>()?.Name;
            if (settingName == null) {
                settingName = type.Name.ToLower();
            }

            return new(settingName, name);
        }

        public KeyValuePair<string, string> GetSettingPair<T>(string fieldName, T value) where T : Enum {
            var name = this.GetValueName(value);

            var fi = typeof(SeedSettings).GetProperty(fieldName, BindingFlags.Instance | BindingFlags.Public);

            var settingName = fi?.GetCustomAttribute<CommonValueAttribute>()?.Name ?? fieldName.ToLower();

            return new(settingName, name);
        }

        private string GetValueName<T>(T value) where T : Enum {
            Type type = typeof(T);

            var fi = type.GetField(value.ToString(), BindingFlags.Static | BindingFlags.Public);

            var name = fi?.GetCustomAttribute<CommonValueAttribute>()?.Name;

            if (name != null) {
                return name;
            } else {
                return value.ToString().ToLower();
            }
        }
    }
}
