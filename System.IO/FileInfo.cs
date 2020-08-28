//
// Copyright (c) 2020 The nanoFramework project contributors
// Portions Copyright (c) Microsoft Corporation.  All rights reserved.
// See LICENSE file in the project root for full license information.
//
using System;

using NativeIO = nanoFramework.IO.NativeIO;

namespace System.IO
{
    /// <summary>
    /// Provides properties and instance methods for the creation, copying, deletion, moving, 
    /// and opening of files, and aids in the creation of FileStream objects. 
    /// This class cannot be inherited.
    /// </summary>
    [Serializable]
    public sealed class FileInfo : FileSystemInfo
    {
        /// <summary>
        /// Initializes a new instance of the FileInfo class, which acts as a wrapper for a file path.
        /// </summary>
        /// <param name="fileName">The fully qualified name of the new file, or the relative file name. 
        /// Do not end the path with the directory separator character.</param>
        public FileInfo(String fileName)
        {
            // path validation in Path.GetFullPath()

            m_fullPath = Path.GetFullPath(fileName);
        }

        /// <summary>
        /// Gets the name of the file.
        /// </summary>
        public override String Name
        {
            get
            {
                return Path.GetFileName(m_fullPath);
            }
        }

        /// <summary>
        /// Gets the size, in bytes, of the current file.
        /// </summary>
        public long Length
        {
            get
            {
                RefreshIfNull();
                return (long)_nativeFileInfo.Size;
            }
        }

        /// <summary>
        /// Gets a string representing the directory's full path.
        /// </summary>
        public String DirectoryName
        {
            get
            {
                return Path.GetDirectoryName(m_fullPath);
            }
        }

        /// <summary>
        /// Gets an instance of the parent directory.
        /// </summary>
        public DirectoryInfo Directory
        {
            get
            {
                String dirName = DirectoryName;

                if (dirName == null)
                {
                    return null;
                }

                return new DirectoryInfo(dirName);
            }
        }

        /// <summary>
        /// Creates a file.
        /// </summary>
        /// <returns></returns>
        public FileStream Create()
        {
            return File.Create(m_fullPath);
        }

        /// <summary>
        /// Permanently deletes a file.
        /// </summary>
        public override void Delete()
        {
            File.Delete(m_fullPath);
        }

        /// <summary>
        /// Gets a value indicating whether a file exists.
        /// </summary>
        public override bool Exists
        {
            get
            {
                return File.Exists(m_fullPath);
            }
        }

        /// <summary>
        /// Returns the path as a string.
        /// </summary>
        /// <returns>A string representing the path.</returns>
        public override String ToString()
        {
            return m_fullPath;
        }
    }
}


