﻿using Moxel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace AddIn
{
    [ComVisible(true)]
    [Guid("AB634001-F13D-11d0-A459-004095E1DAEA")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IInitDone
    {
        /// <summary>
        /// Инициализация компонента
        /// </summary>
        /// <param name="connection">reference to IDispatch</param>
        [PreserveSig]
        HRESULT Init(
            [MarshalAs(UnmanagedType.IDispatch)]
            object connection);

        /// <summary>
        /// Вызывается перед уничтожением компонента
        /// </summary>
        [PreserveSig]
        HRESULT Done();

        /// <summary>
        /// Возвращается инициализационная информация
        /// </summary>
        /// <param name="info">Component information</param>
        [PreserveSig]
        HRESULT GetInfo(
            [MarshalAs(UnmanagedType.SafeArray, SafeArraySubType=VarEnum.VT_VARIANT)]
            ref object[] info);
    }
}
