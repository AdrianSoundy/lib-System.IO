//
// Copyright (c) 2020 The nanoFramework project contributors
// Portions Copyright (c) Microsoft Corporation.  All rights reserved.
// See LICENSE file in the project root for full license information.
//
using System;

namespace System.IO
{
    // Contains constants for specifying the access you want for a file.
    // You can have Read, Write or ReadWrite access.
    //
    /// <summary>
    /// Defines constants for read, write, or read/write access to a file.
    /// This enumeration has a FlagsAttribute attribute that allows a bitwise combination of its member values.
    /// </summary>
    [Serializable, Flags]
    public enum FileAccess
    {
        /// <summary>
        /// Specifies read access to the file. Data can be read from the file and
        /// the file pointer can be moved. Combine with WRITE for read-write access.
        /// </summary>
        Read = 1,

        /// <summary>
        /// Specifies write access to the file. Data can be written to the file and
        /// the file pointer can be moved. Combine with READ for read-write access.
        /// </summary>
        Write = 2,

        /// <summary>
        /// Specifies read and write access to the file. Data can be written to the
        /// file and the file pointer can be moved. Data can also be read from the file.
        /// </summary>
        ReadWrite = 3,
    }
}


