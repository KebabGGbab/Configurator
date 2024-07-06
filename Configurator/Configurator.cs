using System.Configuration;
using System.Text;
using System.Text.Json;

namespace KebabGGbab.Configurator
{
    public static class Configurator
    {
        /// <summary>
        /// Список имён всех конфигураций по определённому пути, не включая расширение.
        /// </summary>
        private static Configurations? Configurations { get; set; }

        /// <summary>
        /// Инициализировать новый объект, содержащий информацию о конфигурациях
        /// </summary>
        /// <param name="path">Путь для поиска конфигураций</param>
        public static void SetConfigurations(string path)
        {
            Configurations = new Configurations(path);
        }
        public static List<string>? GetConfigsName()
        {
            return Configurations?.GetConfigsName();
        }
        public static List<string>? GetFullPathConfigs()
        {
            return Configurations?.GetFullPathConfigs();
        }

        /// <summary>
        /// Загружает файл конфигурации с расширением .config
        /// </summary>
        /// <param name="path">Путь к файлу конфигурации</param>
        /// <param name="changeUsingConfig">Изменить ли текущую конфигурацию? По умолчанию false</param>
        /// <returns>Объект Dictionary, содержащий в себе данные секции appSettings. Key и Value каждого элемента возвращаемой коллекции представляют собой свойства key и value одной из секций add соответственно.</returns>
        public static Dictionary<string, string> LoadConfiguration(string path, bool changeUsingConfig = false)
        {
            string nameConfig = Path.GetFileNameWithoutExtension(path);
            KeyValueConfigurationCollection settings = ConfigurationManager.OpenMappedExeConfiguration(new() { ExeConfigFilename = path }, ConfigurationUserLevel.None).AppSettings.Settings;
            Dictionary<string, string> settingsDictionary = [];
            foreach (KeyValueConfigurationElement element in settings)
                settingsDictionary.Add(element.Key, element.Value);
            if (changeUsingConfig)
                RefreshUsingConfig(nameConfig);
            return settingsDictionary;
        }

        /// <summary>
        /// Сохранить файл конфигурации или создать новый, если не сущетсвует файла с таким именем. Расширение файла .config
        /// </summary>
        /// <param name="keyValues">Объект Dictionary, элементы которого, содержат Key и Value для сохранения их в файл конфигурации в секцию appSettings как секции со свойствами key и value соответственно</param>
        /// <param name="path">Путь к файлу конфигурации</param>
        /// <param name="changeUsingConfig">Изменить ли текущую конфигурацию? По умолчанию false</param>
        public static void SaveConfiguration(Dictionary<string, string> keyValues, string path, bool changeUsingConfig = false)
        {
            string configName = Path.GetFileNameWithoutExtension(path);
            if (Configurations == null)
                throw new NullReferenceException();
            if (!Configurations.GetConfigsName().Contains(configName))
            {
                File.Copy($"{path.Substring(0, path.LastIndexOf('\\'))}Default.config", path);
                Configurations.Add(path);
            }
            Configuration configuration = ConfigurationManager.OpenMappedExeConfiguration(new() { ExeConfigFilename = path }, ConfigurationUserLevel.None);
            KeyValueConfigurationCollection settings = configuration.AppSettings.Settings;
            foreach (KeyValueConfigurationElement element in settings)
                foreach (KeyValuePair<string, string> keyValuePair in keyValues)
                    if (element.Key == keyValuePair.Key)
                        element.Value = keyValuePair.Value;
            configuration.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection(configuration.AppSettings.SectionInformation.Name);
            if (changeUsingConfig)
                RefreshUsingConfig(configName);
        }

        /// <summary>
        /// Удалить файл конфигурации с расширением .config 
        /// </summary>
        /// <param name="path">Путь к файлу конфигурации</param>
        public static void DeleteConfiguration(string path, bool changeUsingConfig = false)
        {
            if (File.Exists(path))
            { 
             File.Delete(path);
            }
            Configurations?.Remove(path);
            if (changeUsingConfig)
            {
                RefreshUsingConfig("Default");
            }
        }

        /// <summary>
        /// Загрузить имя текущей конфигурации
        /// </summary>
        /// <returns></returns>
        public static string LoadUsingConfig()
        {
            KeyValueConfigurationCollection settings = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None).AppSettings.Settings;
            ExistsUsingConfigKey(settings);
            return settings["UsingConfig"].Value;
        }

        private static void RefreshUsingConfig(string configName)
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            KeyValueConfigurationCollection settings = config.AppSettings.Settings;
            ExistsUsingConfigKey(settings);
            settings["UsingConfig"].Value = configName;
            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection(config.AppSettings.SectionInformation.Name);
        }

        private static void ExistsUsingConfigKey(KeyValueConfigurationCollection settings)
        {
            if (settings["UsingConfig"] == null)
                settings.Add("UsingConfig", "");
        }

        /// <summary>
        /// Прочитать файл с расширением и десериализовать его содержимое в объект
        /// </summary>
        /// <typeparam name="T">Тип объекта, в который необходимо десериализировать данные JSON-файла</typeparam>
        /// <param name="path">Путь к JSON-файлу</param>
        /// <returns>Десериализованный объект, либо default значение для типа, если файл по указанному пути не существует, либо он пуст или произошла какая-либо ошибка </returns>
        public static T? LoadJSONConfiguration<T>(string path)
        {
            string? content = ReadFileContent(path);
            if (string.IsNullOrEmpty(content))
            {
                return default;
            }
            try
            {
                return JsonSerializer.Deserialize<T>(content);
            }
            catch (JsonException ex)
            {
                return default;
            }
        }

        /// <summary>
        /// Сохраняет объект в файл в формате JSON в кодировке UTF-8 
        /// </summary>
        /// <typeparam name="T">Тип данных объекта, который будет сериализован в JSON</typeparam>
        /// <param name="path">Путь к файлу, в который будет сохранен сериализованный объект</param>
        /// <param name="obj">Объект, который будет сериализован и сохранен в файл</param>
        /// <param name="fileMode">Необязательный параметр. Указывает, как операционная система должна открывать файл.</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="IOException"></exception>
        public static void SaveJSONConfiguration<T>(string path, T obj, FileMode fileMode = FileMode.OpenOrCreate)
        {
            SaveJSONConfiguration(path, obj, Encoding.UTF8, fileMode);
        }
        /// <summary>
        /// Сохраняет объект в файл в формате JSON в указаной кодировке
        /// </summary>
        /// <typeparam name="T">Тип данных объекта, который будет сериализован в JSON</typeparam>
        /// <param name="path">Путь к файлу, в который будет сохранен сериализованный объект</param>
        /// <param name="obj">Объект, который будет сериализован и сохранен в файл</param>
        /// <param name="encoding">Кодировка, в которой необходимо выполнить сохранение</param>
        /// <param name="fileMode">Необязательный параметр. Указывает, как операционная система должна открывать файл.</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="IOException"></exception>
        public static void SaveJSONConfiguration<T>(string path, T obj, Encoding encoding, FileMode fileMode = FileMode.OpenOrCreate)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentNullException(nameof(path), "Путь к файлу не может быть пустым.");
            }
            if (obj == null)
            {
                throw new ArgumentNullException(nameof(obj), "Оюъект должен быть инициализирован.");
            }
            if (fileMode == FileMode.Create)
            {
                if (File.Exists(path))
                {
                    throw new IOException($"Файл '{path}' уже существует.");
                }
            }
            using StreamWriter writer = new(File.Open(path, fileMode), encoding);
            writer.Write(JsonSerializer.Serialize(obj));
        }

        /// <summary>
        /// Читает всё содержимое файла 
        /// </summary>
        /// <param name="path">Путь к файлу, который необходимо прочитать</param>
        /// <returns>Содержимое файла или пустую строку, если файл не найден</returns>
        private static string? ReadFileContent(string path)
        {
            if (File.Exists(path))
            {
                using StreamReader reader = new(File.Open(path, FileMode.Open));
                return reader.ReadToEnd();
            }
            return null;
        }
    }
}
