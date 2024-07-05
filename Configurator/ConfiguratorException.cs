namespace KebabGGbab.Configurator
{
    public static class ConfiguratorException
    {
        public class EmptyOrNullPathException : Exception 
        { 
            internal EmptyOrNullPathException() : base("Строка, содержащая путь к файлу или директории, не может быть пустой или равна null.") { }
        }
        public class WrongPathException : Exception 
        {
            internal WrongPathException() : base("Строка, содержащая путь к файлу или директории, содержит ошибку") { }
        }
    }
}
