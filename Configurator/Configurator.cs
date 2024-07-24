using System.Configuration;

namespace KebabGGbab.Configurator
{
    public class Configurator
    {
        /// <summary>
        /// Список имён всех конфигураций по определённому пути, не включая расширение.
        /// </summary>
        public ConfigsCollection ConfigsCollection { get; private set; }
        public string UsingConfig { get; private set; }

        public Configurator(string path)
        {
            ConfigsCollection = new(path);
            KeyValueConfigurationCollection settings = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None).AppSettings.Settings;
            if (ExistsKey(settings, "UsingConfig"))
            {
                AddKey(settings, "UsingConfig");
            }
            UsingConfig = LoadKeyValueFromAppConfig("UsingConfig");
        }

        public Dictionary<string, string>? LoadConfiguration(string name, bool ifNullLoadUsingConfig = false, bool changeUsingConfig = false)
        {
            Config? config = ConfigsCollection[name];
            if (config == null)
            {
                if (ifNullLoadUsingConfig)
                {
                    return LoadConfiguration(UsingConfig, false, changeUsingConfig);
                }
                return null;
            }
            return GetSettingsDictionary(config, changeUsingConfig);
        }

        public Dictionary<string, string>? LoadConfiguration(string name, IEnumerable<string> keys, bool ifNullLoadUsingConfig = false, bool changeUsingConfig = false)
        {
            Config? config = ConfigsCollection[name];
            if (config == null)
            {
                if (ifNullLoadUsingConfig)
                {
                    return LoadConfiguration(UsingConfig, keys, false, changeUsingConfig);
                }
                return null;
            }
            return GetSettingsDictionary(config, keys, changeUsingConfig);
        }

        private Dictionary<string, string> GetSettingsDictionary(Config config, bool changeUsingConfig = false)
        {
            KeyValueConfigurationCollection settings = config.Configuration.AppSettings.Settings;
            Dictionary<string, string> settingsDictionary = settings.AllKeys.ToDictionary(key => key, key => settings[key].Value);
            if (changeUsingConfig)
            {
                RefreshValueKeyInAppConfig("UsingConfig", config.Name);
            }
            return settingsDictionary;
        }

        private Dictionary<string, string> GetSettingsDictionary(Config config, IEnumerable<string> keys, bool changeUsingConfig = false)
        {
            KeyValueConfigurationCollection settings = config.Configuration.AppSettings.Settings;
            Dictionary<string, string> settingsDictionary = settings.AllKeys
                                                                    .Where(keys.Contains)
                                                                    .ToDictionary(key => key, key => settings[key].Value);
            if (changeUsingConfig)
            {
                RefreshValueKeyInAppConfig("UsingConfig", config.Name);
            }
            return settingsDictionary;
        }

        /// <summary>
        /// Сохранить файл конфигурации или создать новый, если не сущетсвует файла с таким именем. Расширение файла .config
        /// </summary>
        /// <param name="keyValues">Объект Dictionary, элементы которого, содержат Key и Value для сохранения их в файл конфигурации в секцию appSettings как секции со свойствами key и value соответственно</param>
        /// <param name="path">Путь к файлу конфигурации</param>
        /// <param name="changeUsingConfig">Изменить ли текущую конфигурацию? По умолчанию false</param>
        public void SaveConfiguration(Dictionary<string, string> keyValues, string path, bool changeUsingConfig = false)
        {
            if (!string.IsNullOrEmpty(path))
            {
                throw new ArgumentException($"Путь не может быть null или равен нулю", nameof(path));
            }
            string configName = Path.GetFileNameWithoutExtension(path);
            Config? config = ConfigsCollection[configName];
            if (config == null)
            {
                CreateConfigFile(path);
                ConfigsCollection.Add(path);
                config = ConfigsCollection[configName];
            }
            KeyValueConfigurationCollection settings = config.Configuration.AppSettings.Settings;
            foreach (KeyValuePair<string, string> keyValuePair in keyValues)
            {
                if (ExistsKey(settings, keyValuePair.Key))
                {
                    SetValueToKey(settings, keyValuePair.Key, keyValuePair.Value);
                }
                else
                {
                    AddKey(settings, keyValuePair.Key, keyValuePair.Value);
                }
            }
            config.Configuration.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection(config.Configuration.AppSettings.SectionInformation.Name);
            if (changeUsingConfig)
            {
                RefreshValueKeyInAppConfig("UsingConfig", configName);
            }
        }

        /// <summary>
        /// Удалить файл конфигурации с расширением .config 
        /// </summary>
        /// <param name="path">Путь к файлу конфигурации</param>
        public void DeleteConfiguration(string name)
        {
            Config? config = ConfigsCollection[name];
            if (config != null)
            {
                string path = config.FileInfo.FullName;
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
                ConfigsCollection.Remove(name);
                if (LoadKeyValueFromAppConfig("UsingConfig") == name)
                {
                    RefreshValueKeyInAppConfig("UsingConfig", "");
                }
            }
        }

        /// <summary>
        /// Загрузить значение определённого ключа из конфигурации приложения
        /// </summary>
        /// <returns>Значения ключа</returns>
        public string LoadKeyValueFromAppConfig(string key)
        {
            KeyValueConfigurationCollection settings = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None).AppSettings.Settings;
            if (ExistsKey(settings, key))
            {
                return settings[key].Value;
            }
            return String.Empty;
        }

        public void RefreshValueKeyInAppConfig(string key, string value)
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            KeyValueConfigurationCollection settings = config.AppSettings.Settings;
            SetValueToKey(settings, key, value);
            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection(config.AppSettings.SectionInformation.Name);
        }
        public void AddKey(KeyValueConfigurationCollection settings, string key)
        {
            if (!ExistsKey(settings, key))
            {
                settings.Add(key, "");
            }
        }

        public void AddKey(KeyValueConfigurationCollection settings, string key, string value)
        {
            if (!ExistsKey(settings, key))
            {
                settings.Add(key, value);
            }
        }

        public bool ExistsKey(KeyValueConfigurationCollection settings, string key)
        {
            if (settings[key] != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Присваивает значение ключу
        /// </summary>
        /// <param name="settings">Коллекция элементов из секции settings</param>
        /// <param name="key">Ключ, которому нужно присвоить значение</param>
        /// <param name="value">Значение, которое нужно присвоить ключу</param>
        public void SetValueToKey(KeyValueConfigurationCollection settings, string key, string value)
        {
            if (!ExistsKey(settings, key))
            {
                AddKey(settings, key, value);
            }
            else
            {
                settings[key].Value = value;
            }
        }

        private void CreateConfigFile(string path)
        {
            using var writer = new StreamWriter(File.Open(path, FileMode.Create));
            writer.Write("<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<configuration>\r\n\t<connectionStrings>\r\n\t</connectionStrings>\r\n\t<appSettings>\r\n\t</appSettings>\r\n</configuration>");
        }
    }
}
