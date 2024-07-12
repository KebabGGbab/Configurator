using System.Configuration;

namespace KebabGGbab.Configurator
{
    public class Config
    {
        public readonly string Name;

        public readonly FileInfo FileInfo;

        public readonly Configuration Configuration;

        public Config(FileInfo fileInfo) 
        {
            if (fileInfo.Extension == ".config")
            {
                Name = Path.GetFileNameWithoutExtension(fileInfo.Name);
                FileInfo = fileInfo;
                Configuration = ConfigurationManager.OpenExeConfiguration(FileInfo.FullName);
            }
            else
            {
                throw new ArgumentException("Файл должен иметь расширение '.config'", nameof(fileInfo));
            }
        }
    }
}