namespace NSD.WSLouch.Internals
{
    /// <summary>
    ///     Ответ службы WSL
    /// </summary>
    internal sealed class Reply
    {
        /// <summary>
        ///     XML-документ
        /// </summary>
        public string Xml { get; set; }

        /// <summary>
        ///     Код ошибки
        /// </summary>
        public int ErrorCode { get; set; }

        /// <summary>
        ///     Сообщение об ошибке
        /// </summary>
        public string ErrorDescription { get; set; }

        /// <summary>
        ///     Бинарные данные
        /// </summary>
        public byte[] Data { get; set; }
    }
}
