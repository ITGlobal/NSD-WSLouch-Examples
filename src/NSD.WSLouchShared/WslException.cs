using System;
using System.Runtime.Serialization;

namespace NSD.WSLouch
{
    /// <summary>
    ///     Исключение в адаптере службы WSL
    /// </summary>
    /// <remarks>
    ///     Стереотипная реализация класса исключения
    /// </remarks>
    [Serializable]
    public class WslException : Exception
    {
        public WslException()
        { }

        public WslException(string message) 
            : base(message)
        { }

        public WslException(string message, Exception inner) 
            : base(message, inner)
        { }

        protected WslException(
            SerializationInfo info,
            StreamingContext context) 
            : base(info, context)
        { }
    }
}