using System;

namespace NSD.WSLouch.Internals.Http
{
    /// <summary>
    ///     Адаптер HTTP протокола
    /// </summary>
    internal interface IHttpWorker : IDisposable
    {
        /// <summary>
        ///     Вызвать SOAP-метод
        /// </summary>
        /// <param name="message">
        ///     SOAP-запрос
        /// </param>
        /// <returns>
        ///     Результат HTTP-запроса
        /// </returns>
        HttpResult CallRequest(RequestMessage message);

        /// <summary>
        ///     Вызвать SOAP-метод с вложением
        /// </summary>
        /// <param name="message">
        ///     SOAP-запрос
        /// </param>
        /// <param name="attachment">
        ///     Вложение
        /// </param>
        /// <returns>
        ///     Результат HTTP-запроса
        /// </returns>
        HttpResult CallRequest(RequestMessage message, byte[] attachment);
    }
}
