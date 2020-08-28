//
// Copyright (c) 2020 The nanoFramework project contributors
// Portions Copyright (c) Microsoft Corporation.  All rights reserved.
// See LICENSE file in the project root for full license information.
//
using System;
using nanoFramework.IO;

namespace System.IO
{
    /// <summary>
    /// Provides the base class for both FileInfo and DirectoryInfo objects.
    /// </summary>
    public abstract class FileSystemInfo : MarshalByRefObject
    {
        /// <summary>
        /// Represents the fully qualified path of the directory or file.
        /// </summary>
        protected String m_fullPath;  // fully qualified path of the directory

        //--//

        /// <summary>
        /// Gets the full path of the directory or file.
        /// </summary>
        public virtual String FullName
        {
            get
            {
                return m_fullPath;
            }
        }

        /// <summary>
        /// A string containing the FileSystemInfo extension.
        /// </summary>
        public String Extension
        {
            get
            {
                return Path.GetExtension(FullName);
            }
        }

        /// <summary>
        /// For files, gets the name of the file. For directories, gets the name of the last directory in the hierarchy if a 
        /// hierarchy exists. Otherwise, the Name property gets the name of the directory.
        /// </summary>
        public abstract String Name
        {
            get;
        }

        /// <summary>
        /// Gets a value indicating whether the file or directory exists.
        /// </summary>
        public abstract bool Exists
        {
            get;
        }

        /// <summary>
        /// Deletes a file or directory.
        /// </summary>
        public abstract void Delete();

        /// <summary>
        /// Gets or sets the attributes for the current file or directory.
        /// </summary>
        public FileAttributes Attributes
        {
            get
            {
                RefreshIfNull();
                return (FileAttributes)_nativeFileInfo.Attributes;
            }
        }

        /// <summary>
        /// Gets or sets the creation time of the current file or directory.
        /// </summary>
        public DateTime CreationTime
        {
            get
            {
                return CreationTimeUtc;
//                return CreationTimeUtc.ToLocalTime();
            }
        }

        /// <summary>
        /// Gets or sets the creation time, in coordinated universal time (UTC), of the current file or directory.
        /// </summary>
        public DateTime CreationTimeUtc
        {
            get
            {
                RefreshIfNull();
                return new DateTime(_nativeFileInfo.CreationTime);
            }
        }

        /// <summary>
        /// Gets or sets the time the current file or directory was last accessed.
        /// </summary>
        public DateTime LastAccessTime
        {
            get
            {
                return LastAccessTimeUtc;
//                return LastAccessTimeUtc.ToLocalTime();
            }
        }

        /// <summary>
        /// Gets or sets the time, in coordinated universal time (UTC), when the current file or directory was last written to.
        /// </summary>
        public DateTime LastAccessTimeUtc
        {
            get
            {
                RefreshIfNull();
                return new DateTime(_nativeFileInfo.LastAccessTime);
            }
        }

        /// <summary>
        /// Gets or sets the time when the current file or directory was last written to.
        /// </summary>
        public DateTime LastWriteTime
        {
            get
            {
                return LastWriteTimeUtc;
//                return LastWriteTimeUtc.ToLocalTime();
            }
        }

        /// <summary>
        /// Gets or sets the time, in coordinated universal time (UTC), when the current file or directory was last written to.
        /// </summary>
        public DateTime LastWriteTimeUtc
        {
            get
            {
                RefreshIfNull();
                return new DateTime(_nativeFileInfo.LastWriteTime);
            }
        }

        /// <summary>
        /// Refreshes the state of the object.
        /// </summary>
        public void Refresh()
        {
            Object record = FileSystemManager.AddToOpenListForRead(m_fullPath);

            try
            {
                _nativeFileInfo = NativeFindFile.GetFileInfo(m_fullPath);

                if (_nativeFileInfo == null)
                {
                    IOException.IOExceptionErrorCode errorCode = (this is FileInfo) ? IOException.IOExceptionErrorCode.FileNotFound : IOException.IOExceptionErrorCode.DirectoryNotFound;
                    throw new IOException("", (int)errorCode);
                }
            }
            finally
            {
                FileSystemManager.RemoveFromOpenList(record);
            }
        }

        /// <summary>
        /// If information from the native file system has not yet been retrieved, retrieves it.
        /// </summary>
        protected void RefreshIfNull()
        {
            if (_nativeFileInfo == null)
            {
                Refresh();
            }
        }

        internal NativeFileInfo _nativeFileInfo;
    }
}


