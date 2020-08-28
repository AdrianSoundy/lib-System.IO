//
// Copyright (c) 2020 The nanoFramework project contributors
// Portions Copyright (c) Microsoft Corporation.  All rights reserved.
// See LICENSE file in the project root for full license information.
//
using System;

namespace System.IO
{
    // Contains constants for specifying how the OS should open a file.
    // These will control whether you overwrite a file, open an existing
    // file, or some combination thereof.
    //
    // To append to a file, use Append (which maps to OpenOrCreate then we seek
    // to the end of the file).  To truncate a file or create it if it doesn't
    // exist, use Create.
    //

    /// <summary>
    /// Specifies how the operating system should open a file.
    /// </summary>
    [Serializable]
    public enum FileMode
    {
        /// <summary>
        /// Specifies that the operating system should create a new file. 
        /// This requires Write permission. If the file already exists, an IOException exception is thrown.
        /// </summary>
        CreateNew = 1,

        /// <summary>
        /// Specifies that the operating system should create a new file. If the file already exists, 
        /// it will be overwritten. This requires Write permission. FileMode.Create is equivalent to requesting 
        /// that if the file does not exist, use CreateNew; otherwise, use Truncate. If the file already exists 
        /// but is a hidden file, an UnauthorizedAccessException exception is thrown.
        /// </summary>
        Create = 2,

        /// <summary>
        /// Specifies that the operating system should open an existing file. The ability to open the 
        /// file is dependent on the value specified by the FileAccess enumeration. 
        /// A FileNotFoundException exception is thrown if the file does not exist.
        /// </summary>
        Open = 3,

        /// <summary>
        /// Specifies that the operating system should open a file if it exists; 
        /// otherwise, a new file should be created. If the file is opened with FileAccess.Read, 
        /// Read permission is required. If the file access is FileAccess.Write, Write permission is required. 
        /// If the file is opened with FileAccess.ReadWrite, both Read and Write permissions are required.
        /// </summary>
        OpenOrCreate = 4,

        /// <summary>
        /// Specifies that the operating system should open an existing file. When the file is opened, 
        /// it should be truncated so that its size is zero bytes. This requires Write permission. 
        /// Attempts to read from a file opened with FileMode.Truncate cause an ArgumentException exception.
        /// </summary>
        Truncate = 5,

        /// <summary>
        /// Opens the file if it exists and seeks to the end of the file, or creates a new file. 
        /// This requires Append permission. FileMode.Append can be used only in conjunction with FileAccess.Write. 
        /// Trying to seek to a position before the end of the file throws an IOException exception, 
        /// and any attempt to read fails and throws a NotSupportedException exception.
        /// </summary>
        Append = 6,
    }
}


