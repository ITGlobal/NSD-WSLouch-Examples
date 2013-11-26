namespace NSD.WSLouch.Internals
{
    /// <summary>
    ///     Модель заголовка SOAP-запроса
    /// </summary>
    internal sealed class HeaderModel
    {
        /// <summary>
        ///     Дайджест
        /// </summary>
        public string DigestValue { get; set; }

        /// <summary>
        ///     URL
        /// </summary>
        public string Uri { get; set; }
    }
}
