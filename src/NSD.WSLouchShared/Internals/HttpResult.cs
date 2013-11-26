using System.Net;

namespace NSD.WSLouch.Internals
{
    /// <summary>
    ///     Результат HTTP-запроса
    /// </summary>
    internal sealed class HttpResult
    {
        /// <summary>
        ///     Код статуса HTTP
        /// </summary>
        public HttpStatusCode Code { get; set; }

        /// <summary>
        ///     Тип содержимого
        /// </summary>
        public string ContentType { get; set; }

        /// <summary>
        ///     Содержимое
        /// </summary>
        public byte[] Content { get; set; }
    }
}
