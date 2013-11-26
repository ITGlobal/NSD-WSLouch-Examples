using System.Net;
using System.Xml;

namespace NSD.WSLouch.Internals
{
    /// <summary>
    ///     Ответ службы WSL
    /// </summary>
    internal sealed class Response
    {
        private readonly XmlDocument xml = new XmlDocument();

        /// <summary>
        ///     Код статуса HTTP
        /// </summary>
        public HttpStatusCode StatusCode { get; set; }

        /// <summary>
        ///     XML-документ
        /// </summary>
        public XmlDocument Xml { get { return xml; } }

        /// <summary>
        ///     Бинарные данные
        /// </summary>
        public byte[] Data { get; set; }
    }
}