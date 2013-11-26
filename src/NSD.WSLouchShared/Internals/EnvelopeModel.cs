namespace NSD.WSLouch.Internals
{
    /// <summary>
    ///     Модель конверта SOAP-запроса
    /// </summary>
    internal sealed class EnvelopeModel
    {
        /// <summary>
        ///     SOAP-метод
        /// </summary>
        public string MethodName { get; set; }

        /// <summary>
        ///     Идентификатор запроса
        /// </summary>
        public string RequestId { get; set; }
    }
}
