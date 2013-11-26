using System;
using System.Runtime.InteropServices;

namespace NSD.WSLouch.Internals.Http
{
    /// <summary>
    ///     Утилиты для взаимодействия с нативным кодом
    /// </summary>
    internal static class InteropUtil
    {
        /// <summary>
        ///     Закрепить объект
        /// </summary>
        /// <param name="obj">
        ///     Объект
        /// </param>
        /// <returns>
        ///     RAII-токен закрепленного объекта
        /// </returns>
        public static GCHandleRAII Pin(object obj)
        {
            return new GCHandleRAII(GCHandle.Alloc(obj, GCHandleType.Pinned));
        }

        /// <summary>
        ///     RAII-токен закрепленного объекта
        /// </summary>
        public sealed class GCHandleRAII : IDisposable
        {
            private GCHandle handle;

            /// <summary>
            ///     Конструктор
            /// </summary>
            /// <param name="handle">
            ///     Указатель на закрепленный объект
            /// </param>
            public GCHandleRAII(GCHandle handle)
            {
                this.handle = handle;
            }

            /// <summary>
            ///     Адрес закрепленного объекта
            /// </summary>
            public IntPtr Address
            {
                get { return handle.AddrOfPinnedObject(); }
            }

            /// <summary>
            ///     Выполняет определяемые приложением задачи, связанные с удалением, высвобождением или сбросом неуправляемых ресурсов.
            /// </summary>
            public void Dispose()
            {
                handle.Free();
            }
        }

        /// <summary>
        ///     Выделить неуправляемый буфер
        /// </summary>
        /// <param name="size">
        ///     Размер буфера
        /// </param>
        /// <returns>
        ///     RAII-токен неуправляемого буфера
        /// </returns>
        public static BufferRAII Alloc(uint size)
        {
            return new BufferRAII(Marshal.AllocHGlobal((IntPtr)size));
        }

        /// <summary>
        ///     RAII-токен неуправляемого буфера
        /// </summary>
        public sealed class BufferRAII : IDisposable
        {
            private readonly IntPtr pointer;

            /// <summary>
            ///     Конструктор
            /// </summary>
            /// <param name="pointer">
            ///     Указатель на неуправляемый буфер
            /// </param>
            public BufferRAII(IntPtr pointer)
            {
                this.pointer = pointer;
            }

            /// <summary>
            ///     Указатель на неуправляемый буфер
            /// </summary>
            public IntPtr Address { get { return pointer; } }

            /// <summary>
            ///     Выполняет определяемые приложением задачи, связанные с удалением, высвобождением или сбросом неуправляемых ресурсов.
            /// </summary>
            public void Dispose()
            {
                Marshal.FreeHGlobal(pointer);
            }
        }
    }
}