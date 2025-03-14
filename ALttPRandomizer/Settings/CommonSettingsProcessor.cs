namespace ALttPRandomizer.Settings {
    using ALttPRandomizer.Model;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    public class CommonSettingsProcessor {
        public IEnumerable<string> GetSettings(RandomizerInstance randomizer, SeedSettings settings) {
            var props = typeof(SeedSettings).GetProperties(BindingFlags.Instance | BindingFlags.Public);
            var starting = new List<string>();
            foreach (var prop in props) {
                if (prop.Name == nameof(SeedSettings.PlayerName)) {
                    continue;
                }
                var value = prop.GetValue(settings) ?? throw new SettingsLookupException("settings.{0} not found", prop.Name);
                var valueFieldName = value.ToString() ?? throw new SettingsLookupException("settings.{0}.ToString() returned null", prop.Name);
                var fi = prop.PropertyType.GetField(valueFieldName, BindingFlags.Static | BindingFlags.Public)
                    ?? throw new SettingsLookupException("Could not get field info for value {0}.{1}", prop.PropertyType, valueFieldName);

                if (!prop.GetCustomAttributes<NoSettingNameAttribute>().Any(att => att.HasRandomizer(randomizer))) {
                    var settingName =
                        prop.GetCustomAttributes<SettingNameAttribute>()
                            .FirstOrDefault(att => att.HasRandomizer(randomizer))?.Name ?? prop.Name.ToLower();
                    var valueName =
                        fi.GetCustomAttributes<SettingNameAttribute>()
                            .FirstOrDefault(att => att.HasRandomizer(randomizer))?.Name ?? valueFieldName.ToLower();

                    yield return string.Format("--{0}={1}", settingName, valueName);
                }

                foreach (var att in fi.GetCustomAttributes<AdditionalSettingAttribute>().Where(att => att.HasRandomizer(randomizer))) {
                    yield return att.Setting;
                }

                foreach (var att in fi.GetCustomAttributes<AddStartingItemsAttribute>().Where(att => att.HasRandomizer(randomizer))) {
                    foreach (var item in att.Items) {
                        starting.Add(item);
                    }
                }
            }

            if (starting.Count > 0) {
                yield return "--usestartinventory=true";
                yield return string.Format("--startinventory={0}", string.Join(",", starting));
            }
        }

        public void ValidateSettings(RandomizerInstance randomizer, SeedSettings settings) {
            var props = typeof(SeedSettings).GetProperties(BindingFlags.Instance | BindingFlags.Public);
            foreach (var prop in props) {
                var value = prop.GetValue(settings) ?? throw new SettingsLookupException("settings.{0} not found", prop.Name);

                foreach (var att in prop.GetCustomAttributes<RequiredSettingAttribute>().Where(att => att.HasRandomizer(randomizer))) {
                    if (!att.Values.Contains(value)) {
                        throw new InvalidSettingsException("{0} contains value {1} not in required set [{2}]", prop.Name, value, string.Join(", ", att.Values));
                    }
                }

                foreach (var att in prop.GetCustomAttributes<ForbiddenSettingAttribute>().Where(att => att.HasRandomizer(randomizer))) {
                    if (att.Values.Contains(value)) {
                        throw new InvalidSettingsException("{0} contains forbidden value {1}", prop.Name, value);
                    }
                }
            }
        }
    }

    public class SettingsLookupException : Exception {
        public SettingsLookupException(string message, params object?[] args) : base(string.Format(message, args)) { }
    }

    public class InvalidSettingsException : Exception {
        public InvalidSettingsException(string message, params object?[] args) : base(string.Format(message, args)) { }
    }
}
