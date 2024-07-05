namespace KebabGGbab.Configurator
{
    public class Configurations
    {
        private List<string> ConfigsName = [];
        private List<string> FullPathConfigs = [];
        internal Configurations(string path) 
        {
            Directory.Exists(path);
            FileInfo[] userConfigs = new DirectoryInfo(path).GetFiles();
            foreach (FileInfo config in userConfigs)
            {
                string configName = config.Name;
                if (configName.Contains(".config"))
                {
                    ConfigsName.Add(configName.Remove(configName.Length - 7));
                    FullPathConfigs.Add(config.FullName);
                }
            }
        }
        internal List<string> GetConfigsName()
        {
            return ConfigsName;
        }
        internal List<string> GetFullPathConfigs()
        {
            return FullPathConfigs;
        }
        internal void Add(string path)
        {
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

        internal void Remove(string path)
        {
            FullPathConfigs.Remove(path);
            ConfigsName.Remove(path.Substring(path.LastIndexOf('/') + 1).Substring(0, path.LastIndexOf('.')));
        }
    }
}
