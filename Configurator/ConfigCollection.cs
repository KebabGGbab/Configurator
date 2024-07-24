using System.Collections;
using System.Configuration;

namespace KebabGGbab.Configurator
{
    /// <summary>
    /// Коллекция объектов Config
    /// </summary>
    public class ConfigCollection : IEnumerable<Config>
    {
        /// <summary>
        /// Индексатор по названию конфигурации.
        /// </summary>
        /// <param name="name">Название конфигурации, объект Config которой необходимо вернуть</param>
        /// <returns>Объект Config.</returns>
        public Config? this[string name] => _configs.FirstOrDefault(config => config.Name == name);


        private Config[] _configs = [];
        public Config[] Configs => _configs;
        public int Count => _configs.Length;

        /// <summary>
        /// Инициализировать объект класса, добавив в коллекцию все конфигурации по одному пути, либо конфигурацию по указанному пути..
        /// </summary>
        /// <param name="path">Путь к файлу или директории</param>
        public ConfigCollection(string path) 
        {
            Add(path);
        }

        /// <summary>
        /// Инициализировать объект класса, добавив в коллекцию конфигураций все конфигурации из коллекции, содержащей пути к файлам или директориям.
        /// </summary>
        /// <param name="paths">Коллекция, в которой содержаться пути к конфигурациям или директориям.</param>
        public ConfigCollection(IEnumerable<string> paths) 
        {
            foreach (string path in paths)
            {
                Add(path);
            }
        }
        
        /// <summary>
        /// Добавить конфигурацию по указанному пути в коллекцию, если указан путь файлу, либо все конфигурации из директории, если указан путь к директории.
        /// </summary>
        /// <param name="path">Путь к файлу или директории.</param>
        /// <param name="subdirectories">Используется в случае, если в path указан путь к директории. Если требуется добавить конфигурации из поддиректорий - True, иначе False.</param>
        public void Add(string path, bool subdirectories = false)
        {
            try
            {
                if (File.Exists(path))
                {
                    if (Path.GetExtension(path) == ".config")
                    {
                        Array.Resize(ref _configs, _configs.Length + 1);
                        _configs[^1] = new Config(new FileInfo(path));
                    }
                }
                else if (Directory.Exists(path))
                {
                    foreach (FileInfo file in new DirectoryInfo(path).GetFiles("*.config"))
                    {
                        Add(file.FullName);
                    }
                    if (subdirectories)
                    {
                        foreach (DirectoryInfo directory in new DirectoryInfo(path).GetDirectories())
                        {
                            Add(directory.FullName);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding path '{path}': {ex.Message}");
            }
        }

        /// <summary>
        /// добавить в коллекцию все конфигурации из коллекции, содержащей пути к файлам или директориям.
        /// </summary>
        /// <param name="paths">>Коллекция, в которой содержаться пути к файлу или директориям.</param>
        /// <param name="subdirectories">Используется в случае, если в path указан путь к директории. Если требуется добавить конфигурации из поддиректорий - True, иначе False.</param>
        public void Add(IEnumerable<string> paths, bool subdirectories = false)
        {
            foreach (string path in paths)
            {
                Add(path, subdirectories);
            }
        }

        /// <summary>
        /// Удалить конфигурацию из коллекции. Будет создана новая коллекция конфигураций, в которой не будет кофигурации с заданным названием.
        /// </summary>
        /// <param name="name">Название конфигурации без расширения</param>
        public void Remove(string name)
        {
            _configs = _configs.Where(config => config.Name != name).ToArray();
        }

        /// <summary>
        /// Удалить все конфигурации, названия которых перечислены в переданной коллекции, из коллекции. Будет создана новая коллекция конфигураций, в которой не будет кофигураций с заданными названиями.
        /// </summary>
        /// <param name="names">Коллекция, в которой перечислены названия конфигураций.</param>
        public void Remove(IEnumerable<string> names)
        {
            foreach(string name in names)
            {
                Remove(name);
            }
        }

        /// <summary>
        /// Получить Enumerator.
        /// </summary>
        /// <returns>Enumerator.</returns>
        public IEnumerator<Config> GetEnumerator()
        {
            for (int i = 0; i < Count; i++)
            {
                yield return _configs[i];
            }
        }

        /// <summary>
        /// Получить Enumerator.
        /// </summary>
        /// <returns>Enumerator.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Плучить перечисление названий конфигураций, содержащихя в коллекции.
        /// </summary>
        /// <returns>Перечисление названий конфигураций.</returns>
        public IEnumerable<string> GetNames()
        {
            for (int i = 0; i < Count; i++)
            {
                yield return _configs[i].Name;
            }
        }

        /// <summary>
        /// Плучить перечисление объектов FileInfo конфигураций, содержащихя в коллекции.
        /// </summary>
        /// <returns>Перечисление объектов FileInfo конфигураций.</returns>
        public IEnumerable<FileInfo> GetFiles()
        {
            for (int i = 0; i < Count; i++)
            {
                yield return _configs[i].FileInfo;
            }
        }

        /// <summary>
        /// Плучить перечисление объектов Configuration конфигураций, содержащихя в коллекции.
        /// </summary>
        /// <returns>Перечисление объектов Configuration конфигураций.</returns>
        public IEnumerable<Configuration> GetConfigurators()
        {
            for (int i = 0; i < Count; i++)
            {
                yield return _configs[i].Configuration;
            }
        }
    }
}
