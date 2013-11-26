using System;
using System.Runtime.Serialization;

namespace NSD.WSLouch
{
    /// <summary>
    ///     Исключение, возникающее, если параметры командной строки неверны
    /// </summary>
    /// <remarks>
    ///     Стереотипная реализация класса исключения
    /// </remarks>
    [Serializable]
    public class CommandLineException : Exception
    {
        public CommandLineException()
        { }

        public CommandLineException(string message) 
            : base(message)
        { }

        public CommandLineException(string message, Exception inner)
            : base(message, inner)
        { }

        protected CommandLineException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        { } 
    }
}