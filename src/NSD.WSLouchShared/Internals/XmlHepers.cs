using System.IO;
using System.Security.Cryptography.Xml;
using System.Xml;

namespace NSD.WSLouch.Internals
{
    /// <summary>
    ///     Вспомогательные методы для работы с XML
    /// </summary>
    internal static class XmlHepers
    {
        /// <summary>
        ///     Найти узел по его локальному имени
        /// </summary>
        /// <param name="xml">
        ///     Родительский узел
        /// </param>
        /// <param name="localName">
        ///     Локальное имя целевого узла
        /// </param>
        /// <returns>
        ///     Узел, если он найден. null - в противном случае
        /// </returns>
        public static XmlNode GetNodeByLocalName(this XmlNode xml, string localName)
        {
            return xml.SelectSingleNode(string.Format(".//*[local-name()='{0}']", localName));
        }
        
        /// <summary>
        ///     Выполнить каноникализацию XML-узла
        /// </summary>
        /// <param name="node">
        ///     Узел
        /// </param>
        /// <returns>
        ///     Каноникализированный XM-текст
        /// </returns>
        public static string Canonicalize(this XmlNode node)
        {
            var transform = new XmlDsigExcC14NTransform();
            var document = new XmlDocument();
            
            document.LoadXml(node.OuterXml);
            transform.LoadInput(document);

            using (var stream = (Stream) transform.GetOutput())
            using (var reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }
    }
}
