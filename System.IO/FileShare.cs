//
// Copyright (c) 2020 The nanoFramework project contributors
// Portions Copyright (c) Microsoft Corporation.  All rights reserved.
// See LICENSE file in the project root for full license information.
//
using System;

namespace System.IO
{
    /// <summary>
    /// Contains constants for controlling the kind of access other FileStream objects can have to the same file.
    /// This enumeration has a FlagsAttribute attribute that allows a bitwise combination of its member values
    /// </summary>
    [Serializable, Flags]
    public enum FileShare
    {
        /// <summary>
        /// Declines sharing of the current file. 
        /// Any request to open the file (by this process or another process) will fail until the file is closed.
        /// </summary>
        None = 0,

        /// <summary>
        /// Allows subsequent opening of the file for reading. If this flag is not specified, 
        /// any request to open the file for reading (by this process or another process) will 
        /// fail until the file is closed. However, even if this flag is specified, 
        /// additional permissions might still be needed to access the file.
        /// </summary>
        Read = 1,

        /// <summary>
        /// Allows subsequent opening of the file for writing. If this flag is not specified, 
        /// any request to open the file for writing (by this process or another process) will fail until the file is closed. 
        /// However, even if this flag is specified, additional permissions might still be needed to access the file.
        /// </summary>
        Write = 2,

        /// <summary>
        /// Allows subsequent opening of the file for reading or writing. 
        /// If this flag is not specified, any request to open the file for reading or 
        /// writing (by this process or another process) will fail until the file is closed. However, 
        /// even if this flag is specified, additional permissions might still be needed to access the file.
        /// </summary>
        ReadWrite = 3,
    }
}


