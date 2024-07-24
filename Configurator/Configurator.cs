using System.Configuration;

namespace KebabGGbab.Configurator
{
    /// <summary>
    /// Содержит некоторые реализовынные методы для работы с файлами .config.
    /// </summary>
    public class Configurator
    {
        /// <summary>
        /// Коллекция конфигураций, с которыми может работать этот объект класса Configurator.
        /// </summary>
        public ConfigCollection ConfigsCollection { get; private set; }
        /// <summary>
        /// Название конфигурации, котороё было последним сохранено в дефолтную конфигурацию приложения.
        /// </summary>
        public string UsingConfig { get; private set; }

        /// <summary>
        /// Инициализировать объект класса Configurator.
        /// </summary>
        /// <param name="path">Путь, файл или файлы по которому будут добавлены в ConfigsCollection.</param>
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

        /// <summary>
        /// Загрузить коллекцию всех пар ключ-значение коллекция секции AppSettings конфигурации.
        /// </summary>
        /// <param name="name">Название конфигурации.</param>
        /// <param name="ifNullLoadUsingConfig">Загрузить значение использующийся конфигурации, в случае, если конфигурация не существует.</param>
        /// <param name="changeUsingConfig">Изменить использующуюся конфигурацию? По умолчанию false.</param>
        /// <returns>Коллекция значений пары ключ-значение.</returns>
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

        /// <summary>
        /// Загрузить коллекцию определённых пар ключ-значение коллекция секции AppSettings конфигурации.
        /// </summary>
        /// <param name="name">Название конфигурации.</param>
        /// <param name="keys">Коллекция типа string ключей, которые необходимо отыскать.</param>
        /// <param name="ifNullLoadUsingConfig">Загрузить значение использующийся конфигурации, в случае, если конфигурация не существует.</param>
        /// <param name="changeUsingConfig">Изменить использующуюся конфигурацию? По умолчанию false.</param>
        /// <returns>Коллекция значений пары ключ-значение.</returns>
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

        /// <summary>
        /// Получить словарь, в который входят все пары ключ-значение коллекции секции AppSettings конфигурации.
        /// </summary>
        /// <param name="config">Конфигурация.</param>
        /// <param name="changeUsingConfig">Изменить использующуюся конфигурацию? По умолчанию false.</param>
        /// <returns>Коллекция значений пары ключ-значение.</returns>
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

        /// <summary>
        /// Получить словарь, в который входят определённые пары ключ-значение коллекции секции AppSettings конфигурации.
        /// </summary>
        /// <param name="config">Конфигурация</param>
        /// <param name="keys">Коллекция типа string ключей, которые необходимо отыскать.</param>
        /// <param name="changeUsingConfig">Изменить использующуюся конфигурацию? По умолчанию false.</param>
        /// <returns>Коллекция значений пары ключ-значение.</returns>
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
        /// Сохранить файл конфигурации. Если файла не существует, то он будет создан. Если файл существует - он будет изменён.В слуае, если файл существует, но не находится в ConfigsCollection, то он будет перезаписан
        /// </summary>
        /// <param name="keyValues">Объект Dictionary, элементы которого, содержат Key и Value для сохранения их в файл конфигурации в секцию appSettings как секции со свойствами key и value соответственно.</param>
        /// <param name="path">Путь к файлу конфигурации.</param>
        /// <param name="changeUsingConfig">Изменить использующуюся конфигурацию? По умолчанию false.</param>
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
        /// Удалить файл конфигурации. Конфигурация должна быть элементом ConfigCollection.
        /// </summary>
        /// <param name="name">Название конфигурации.</param>
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
        /// Загрузить значение определённого ключа из дефолтной конфигурации приложения.
        /// </summary>
        /// <param name="key">Ключ, значение которого будет загруженно.</param>
        /// <returns>Значение ключа</returns>
        public string LoadKeyValueFromAppConfig(string key)
        {
            KeyValueConfigurationCollection settings = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None).AppSettings.Settings;
            if (ExistsKey(settings, key))
            {
                return settings[key].Value;
            }
            return String.Empty;
        }

        /// <summary>
        /// Обновить значение определённого ключа из дефолтной конфигурации приложения.
        /// </summary>
        /// <param name="key">Ключ, значение которого будет обновлено.</param>
        /// <param name="value">Значение, которое необходимо задать ключу.</param>
        public void RefreshValueKeyInAppConfig(string key, string value)
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            KeyValueConfigurationCollection settings = config.AppSettings.Settings;
            SetValueToKey(settings, key, value);
            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection(config.AppSettings.SectionInformation.Name);
        }

        /// <summary>
        /// Добавить ключ в коллекцию секции AppSettings конфигурации. Значением ключа будет пустая строка. 
        /// </summary>
        /// <param name="settings">Коллекция секции AppSettings конфигурации.</param>
        /// <param name="key">Ключ, который нужно добавить.</param>
        public void AddKey(KeyValueConfigurationCollection settings, string key)
        {
            if (!ExistsKey(settings, key))
            {
                settings.Add(key, string.Empty);
            }
        }

        /// <summary>
        /// Добавить ключ в коллекцию секции AppSettings конфигурации.
        /// </summary>
        /// <param name="settings">Коллекция секции AppSettings конфигурации.</param>
        /// <param name="key">Ключ, который нужно добавить.</param>
        /// <param name="value">Значение, которое будет присвоено ключу.</param>
        public void AddKey(KeyValueConfigurationCollection settings, string key, string value)
        {
            if (!ExistsKey(settings, key))
            {
                settings.Add(key, value);
            }
        }

        /// <summary>
        /// Проверить существование ключа в коллекции секции AppSettings конфигурации.</summary>
        /// <param name="settings">Коллекция секции AppSettings конфигурации.</param>
        /// <param name="key">Ключ, который неоходимо найти</param>
        /// <returns>True, если ключ найден. False, если ключ не найден.</returns>
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
        /// Присваивает значение ключу.
        /// </summary>
        /// <param name="settings">Коллекция элементов из секции settings.</param>
        /// <param name="key">Ключ, которому нужно присвоить значение.</param>
        /// <param name="value">Значение, которое нужно присвоить ключу.</param>
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

        /// <summary>
        /// Создать файл конфигурации с базовой структурой.
        /// </summary>
        /// <param name="path">Путь, по которому будет создан файл.</param>
        private void CreateConfigFile(string path)
        {
            using var writer = new StreamWriter(File.Open(path, FileMode.Create));
            writer.Write("<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<configuration>\r\n\t<connectionStrings>\r\n\t</connectionStrings>\r\n\t<appSettings>\r\n\t</appSettings>\r\n</configuration>");
        }
    }
}
