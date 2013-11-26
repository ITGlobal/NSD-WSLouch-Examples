using System;

namespace NSD.WSLouch
{
    /// <summary>
    ///     Обертка над VCERT API
    /// </summary>
    public interface IVcertAPI : IDisposable
    {
        /// <summary>
        ///     Вычислить хеш блока текста
        /// </summary>
        /// <param name="input">
        ///     Блок текста
        /// </param>
        /// <returns>
        ///     Хеш
        /// </returns>
        string Hash(string input);
        
        /// <summary>
        ///     Зашифровать блок текста
        /// </summary>
        /// <param name="input">
        ///     Блок текста
        /// </param>
        /// <returns>
        ///     Зашифрованный блок
        /// </returns>
        string Sign(string input);
    }
}