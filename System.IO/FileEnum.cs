//
// Copyright (c) 2020 The nanoFramework project contributors
// Portions Copyright (c) Microsoft Corporation.  All rights reserved.
// See LICENSE file in the project root for full license information.
//
using System;
using System.Collections;
using NativeIO = nanoFramework.IO.NativeIO;
using nanoFramework.IO;

namespace System.IO
{
    /// <summary>
    /// Contains values to represent files and directories.
    /// </summary>
    public enum FileEnumFlags
    {
        /// <summary>
        /// Files
        /// </summary>
        Files = 0x0001,
        /// <summary>
        /// Directories
        /// </summary>
        Directories = 0x0002,
        /// <summary>
        /// Files and directories
        /// </summary>
        FilesAndDirectories = Files | Directories,
    }

    /// <summary>
    /// Contains members for working with files.
    /// </summary>
    public class FileEnum : IEnumerator, IDisposable
    {
        private NativeFindFile  m_findFile;
        private NativeFileInfo  m_currentFile;
        private FileEnumFlags   m_flags;
        private string          m_path;
        private bool            m_disposed;
        private object          m_openForReadHandle;

        /// <summary>
        /// nitializes a new instances of the FileEnum class.
        /// </summary>
        /// <param name="path">The path for reading.</param>
        /// <param name="flags">The type of objects to read.</param>
        public FileEnum(string path, FileEnumFlags flags)
        {
            m_flags = flags;
            m_path  = path;

            m_openForReadHandle = FileSystemManager.AddToOpenListForRead(m_path);
            m_findFile          = new NativeFindFile(m_path, "*");
        }

        #region IEnumerator Members

        /// <summary>
        /// 
        /// </summary>
        public object Current
        {
            get
            {
                if (m_disposed) throw new ObjectDisposedException();

                return m_currentFile.FileName;
            }
        }

        /// <summary>
        /// Advances to the next element in the collection.
        /// </summary>
        /// <returns></returns>
        public bool MoveNext()
        {
            if (m_disposed) throw new ObjectDisposedException();

            NativeFileInfo fileinfo = m_findFile.GetNext();

            while (fileinfo != null)
            {
                if (m_flags != FileEnumFlags.FilesAndDirectories)
                {
                    uint targetAttribute = (0 != (m_flags & FileEnumFlags.Directories) ? (uint)FileAttributes.Directory : 0);

                    if ((fileinfo.Attributes & (uint)FileAttributes.Directory) == targetAttribute)
                    {
                        m_currentFile = fileinfo;
                        break;
                    }
                }
                else
                {
                    m_currentFile = fileinfo;
                    break;
                }

                fileinfo = m_findFile.GetNext();
            }

            if (fileinfo == null)
            {
                m_findFile.Close();
                m_findFile = null;

                FileSystemManager.RemoveFromOpenList(m_openForReadHandle);
                m_openForReadHandle = null;
            }

            return fileinfo != null;
        }

        /// <summary>
        /// Sets the enumerator to its initial position, which is before the first element in the collection.
        /// </summary>
        public void Reset()
        {
            if (m_disposed) throw new ObjectDisposedException();

            if (m_findFile != null)
            {
                m_findFile.Close();
            }

            if(m_openForReadHandle == null)
            {
                m_openForReadHandle = FileSystemManager.AddToOpenListForRead(m_path);
            }

            m_findFile = new NativeFindFile(m_path, "*");
        }

        #endregion

        /// <summary>
        /// Releases all resources used by the FileEnum.
        /// </summary>
        /// <param name="disposing">true to release managed resources; otherwise, false.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (m_findFile != null)
            {
                m_findFile.Close();
                m_findFile = null;
            }

            if (m_openForReadHandle != null)
            {
                FileSystemManager.RemoveFromOpenList(m_openForReadHandle);
                m_openForReadHandle = null;
            }

            m_disposed = true;
        }

        /// <summary>
        /// Filenum destructor.
        /// </summary>
        ~FileEnum()
        {
            Dispose(false);
        }

        #region IDisposable Members

        /// <summary>
        /// Releases all resources used by the FileEnum.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }

    /// <summary>
    /// Exposes the file enumerator, which supports a simple iteration over a collection.
    /// </summary>
    public class FileEnumerator : IEnumerable
    {
        private string m_path;
        private FileEnumFlags m_flags;

        /// <summary>
        /// Initializes a new instance of the FileEnumerator class.
        /// </summary>
        /// <param name="path">The file path.</param>
        /// <param name="flags">The type of objects to enumerate.</param>
        public FileEnumerator(string path, FileEnumFlags flags)
        {
            m_path  = Path.GetFullPath(path);
            m_flags = flags;

            if (!Directory.Exists(m_path)) throw new IOException("", (int)IOException.IOExceptionErrorCode.DirectoryNotFound);
        }

        #region IEnumerable Members

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns></returns>
        public IEnumerator GetEnumerator()
        {
            return new FileEnum(m_path, m_flags);
        }

        #endregion
    }
}