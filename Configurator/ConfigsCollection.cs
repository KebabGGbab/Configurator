using System.Collections;
using System.Configuration;

namespace KebabGGbab.Configurator
{
    public class ConfigsCollection : IEnumerable<Config>
    {
        public Config? this[string name] => _configs.FirstOrDefault(config => config.Name == name);

        private Config[] _configs = [];
        public Config[] Configs => _configs;
        public int Count => _configs.Length;

        public ConfigsCollection(string path) 
        {
            Add(path);
        }

        public ConfigsCollection(IEnumerable<string> paths) 
        {
            foreach (string path in paths)
            {
                Add(path);
            }
        }

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

        public void Add(IEnumerable<string> paths, bool subdirectories = false)
        {
            foreach (string path in paths)
            {
                Add(path, subdirectories);
            }
        }

        public void Remove(string name)
        {
            _configs = _configs.Where(config => config.Name != name).ToArray();
        }

        public void Remove(IEnumerable<string> names)
        {
            foreach(string name in names)
            {
                Remove(name);
            }
        }

        public IEnumerator<Config> GetEnumerator()
        {
            for (int i = 0; i < Count; i++)
            {
                yield return _configs[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerable<string> GetNames()
        {
            for (int i = 0; i < Count; i++)
            {
                yield return _configs[i].Name;
            }
        }

        public IEnumerable<FileInfo> GetFiles()
        {
            for (int i = 0; i < Count; i++)
            {
                yield return _configs[i].FileInfo;
            }
        }

        public IEnumerable<Configuration> GetConfigurators()
        {
            for (int i = 0; i < Count; i++)
            {
                yield return _configs[i].Configuration;
            }
        }
    }
}
