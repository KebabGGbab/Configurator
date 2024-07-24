using System.Configuration;

namespace KebabGGbab.Configurator
{
    /// <summary>
    /// Класс, объекты которого содержат информацию о конфигурациях.
    /// </summary>
    public class Config
    {
        /// <summary>
        /// Название конфигурации.
        /// </summary>
        public readonly string Name;
        /// <summary>
        /// Информация о файле конфигурации.
        /// </summary>
        public readonly FileInfo FileInfo;
        /// <summary>
        /// Объект конфигурации.
        /// </summary>
        public readonly Configuration Configuration;

        /// <summary>
        /// Иницифализация объекта Config.
        /// </summary>
        /// <param name="fileInfo">Объект, содержащий информацию о файле конфигурации.</param>
        /// <exception cref="ArgumentException">Возникает в случае, если расширение файла не ".config".</exception>
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