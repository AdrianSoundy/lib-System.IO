//
// Copyright (c) 2020 The nanoFramework project contributors
// Portions Copyright (c) Microsoft Corporation.  All rights reserved.
// See LICENSE file in the project root for full license information.
//
using System;

namespace System.IO
{
    /// <summary>
    /// Provides attributes for files and directories.
    /// This enumeration has a FlagsAttribute attribute that allows a bitwise combination of its member values.
    /// </summary>
    [Flags]
    public enum FileAttributes
    {
        /// <summary>
        /// The file is read-only.
        /// </summary>
        ReadOnly = 0x1,
        /// <summary>
        /// The file is hidden, and thus is not included in an ordinary directory listing.
        /// </summary>
        Hidden = 0x2,
        /// <summary>
        /// The file is a system file. 
        /// That is, the file is part of the operating system or is used exclusively by the operating system.
        /// </summary>
        System = 0x4,
        /// <summary>
        /// The file is a directory.
        /// </summary>
        Directory = 0x10,
        /// <summary>
        /// The file is a candidate for backup or removal.
        /// </summary>
        Archive = 0x20,
        /// <summary>
        /// The file is a standard file that has no special attributes. 
        /// This attribute is valid only if it is used alone.
        /// </summary>
        Normal = 0x80,
    }
}


