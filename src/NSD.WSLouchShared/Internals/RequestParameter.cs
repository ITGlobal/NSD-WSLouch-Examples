namespace NSD.WSLouch.Internals
{
    /// <summary>
    ///     Параметр SOAP-запроса
    /// </summary>
    internal sealed class RequestParameter
    {
        private readonly string name;
        private readonly string value;

        /// <summary>
        ///     Конструктор
        /// </summary>
        /// <param name="name">
        ///     Название параметра
        /// </param>
        /// <param name="value">
        ///     Значение параметра
        /// </param>
        public RequestParameter(string name, string value)
        {
            this.name = name;
            this.value = value;
        }

        /// <summary>
        ///     Название параметра
        /// </summary>
        public string Name { get { return name; } }

        /// <summary>
        ///     Значение параметра
        /// </summary>
        public string Value { get { return value; } }

        /// <summary>
        ///     Возвращает объект <see cref="T:System.String"/>, который представляет текущий объект <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        ///     Объект <see cref="T:System.String"/>, представляющий текущий объект <see cref="T:System.Object"/>.
        /// </returns>
        public override string ToString()
        {
            return string.Format("{0} = \"{1}\"", name, value);
        }
    }
}