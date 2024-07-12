using System.Text.Json;
using System.Text;

namespace Configurator
{
    public static class ConfiguratorJSON
    {
        /// <summary>
        /// Прочитать файл JSON и десериализовать его
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
            catch
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
        public static void SaveJSONConfiguration<T>(string path, T obj, FileMode fileMode = FileMode.Open)
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
        public static void SaveJSONConfiguration<T>(string path, T obj, Encoding encoding, FileMode fileMode = FileMode.Open)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentNullException(nameof(path), "Путь к файлу не может быть пустым.");
            }
            if (obj == null)
            {
                throw new ArgumentNullException(nameof(obj), "Оюъект должен быть инициализирован.");
            }
            if (fileMode == FileMode.CreateNew)
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
        public static string? ReadFileContent(string path)
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
