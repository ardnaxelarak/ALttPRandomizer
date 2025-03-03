using System;

namespace ALttPRandomizer.Settings {
    using ALttPRandomizer.Model;
    using System.Collections.Generic;
    using System.Reflection;

    public class CommonSettingsProcessor {
        public IList<string> GetSettings(SeedSettings settings) {
            var args = new List<string>();

            var props = typeof(SeedSettings).GetProperties(BindingFlags.Instance | BindingFlags.Public);
            foreach (var prop in props) {
                var value = prop.GetValue(settings) ?? throw new SettingsLookupException("settings.{} not found", prop.Name);
                var valueFieldName = value.ToString() ?? throw new SettingsLookupException("settings.{}.ToString() returned null", prop.Name);
                var fi = prop.PropertyType.GetField(valueFieldName, BindingFlags.Static | BindingFlags.Public)
                    ?? throw new SettingsLookupException("Could not get field info for value {}.{}", prop.PropertyType, valueFieldName);

                if (prop.GetCustomAttribute<NoSettingNameAttribute>() == null) {
                    var settingName = prop.GetCustomAttribute<SettingNameAttribute>()?.Name ?? prop.Name.ToLower();
                    var valueName = fi.GetCustomAttribute<SettingNameAttribute>()?.Name ?? valueFieldName.ToLower();

                    args.Add(string.Format("--{0}={1}", settingName, valueName));
                }

                foreach (var att in fi.GetCustomAttributes<AdditionalSettingAttribute>()) {
                    args.Add(att.Setting);
                }
            }

            return args;
        }
    }

    public class SettingsLookupException : Exception {
        public SettingsLookupException(string message, params object?[] args) : base(string.Format(message, args)) { }
    }
}
