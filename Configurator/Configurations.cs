namespace KebabGGbab.Configurator
{
    public class Configurations
    {
        /// <summary>
        /// Содержит имена файлов .config без расширения
        /// </summary>
        private List<string> ConfigsName = [];
        /// <summary>
        /// Содержит полный путь к файлам .config
        /// </summary>
        private List<string> FullPathConfigs = [];
        public string RootPathConfigs { get; private set; }

        public Configurations(string path) 
        {
            ArgumentException.ThrowIfNullOrEmpty(path);
            if (Directory.Exists(path))
            {
                RootPathConfigs = path;
                FileInfo[] userConfigs = new DirectoryInfo(path).GetFiles();
                foreach (FileInfo config in userConfigs)
                {
                    if (config.Extension == ".config")
                    {
                        ConfigsName.Add(Path.GetFileNameWithoutExtension(config.Name));
                        FullPathConfigs.Add(config.FullName);
                    }
                }
            }
            else
            {
                throw new DirectoryNotFoundException($"Каталог '{path}' не найден.");
            }
        }
        public List<string> GetConfigsName()
        {
            return ConfigsName;
        }
        public List<string> GetFullPathConfigs()
        {
            return FullPathConfigs;
        }
        public string GetRootPathConfigs()
        {
            return RootPathConfigs;
        }
        public void Add(string path)
        {
            if (path == null)
                return;
            if (!FullPathConfigs.Contains(path))
            {
                FileInfo fileInfo = new(path);
                string configName = fileInfo.Name;
                if (configName.Contains(".config"))
                {
                    ConfigsName.Add(configName.Remove(configName.Length - 7));
                    FullPathConfigs.Add(fileInfo.FullName);
                }
            }
        }
        public void Add(IEnumerable<string> paths)
        {
            foreach (string path in paths)
            {
                Add(path);
            }
        }
        public void Remove(string path)
        {
            if (path == null)
                return;
            FullPathConfigs.Remove(path);
            ConfigsName.Remove(Path.GetFileNameWithoutExtension(path));
        }
        public void Remove(IEnumerable<string> paths)
        {
            foreach(string path in paths)
            {
                Remove(path);
            }
        }
    }
}
