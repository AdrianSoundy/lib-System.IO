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
    /// Exposes instance methods for creating, moving, and enumerating through directories and subdirectories. 
    /// This class cannot be inherited.
    /// </summary>
    public sealed class DirectoryInfo : FileSystemInfo
    {
        private DirectoryInfo()
        {
        }

        /// <summary>
        /// Initializes a new instance of the DirectoryInfo class on the specified path.
        /// </summary>
        /// <param name="path">A string specifying the path on which to create the DirectoryInfo.</param>
        public DirectoryInfo(string path)
        {
            // path validation in Path.GetFullPath()

            m_fullPath = Path.GetFullPath(path);
        }

        /// <summary>
        /// Gets the name of this DirectoryInfo instance.
        /// </summary>
        public override string Name
        {
            get
            {
                return Path.GetFileName(m_fullPath);
            }
        }

        /// <summary>
        /// Gets the parent directory of a specified subdirectory.
        /// </summary>
        public DirectoryInfo Parent
        {
            get
            {
                string parentDirPath = Path.GetDirectoryName(m_fullPath);
                if (parentDirPath == null)
                    return null;

                return new DirectoryInfo(parentDirPath);
            }
        }

        /// <summary>
        /// Creates a subdirectory or subdirectories on the specified path. 
        /// The specified path can be relative to this instance of the DirectoryInfo class.
        /// </summary>
        /// <param name="path">The specified path. This cannot be a different disk volume or Universal Naming Convention (UNC) name.</param>
        /// <returns>The last directory specified in path.</returns>
        public DirectoryInfo CreateSubdirectory(string path)
        {
            // path validatation in Path.Combine()

            string subDirPath = Path.Combine(m_fullPath, path);

            // This will also ensure "path" is valid.
            subDirPath = Path.GetFullPath(subDirPath);

            return Directory.CreateDirectory(subDirPath);
        }

        /// <summary>
        /// Creates a directory.
        /// </summary>
        public void Create()
        {
            Directory.CreateDirectory(m_fullPath);
        }

        /// <summary>
        /// Gets a value indicating whether the directory exists.
        /// </summary>
        public override bool Exists
        {
            get
            {
                return Directory.Exists(m_fullPath);
            }
        }

        /// <summary>
        /// Returns a file list from the current directory.
        /// </summary>
        /// <returns></returns>
        public FileInfo[] GetFiles()
        {
            string[] fileNames = Directory.GetFiles(m_fullPath);

            FileInfo[] files = new FileInfo[fileNames.Length];

            for (int i = 0; i < fileNames.Length; i++)
            {
                files[i] = new FileInfo(fileNames[i]);
            }

            return files;
        }

        /// <summary>
        /// Returns the subdirectories of the current directory.
        /// </summary>
        /// <returns>An array of DirectoryInfo objects.</returns>
        public DirectoryInfo[] GetDirectories()
        {
            // searchPattern validation in Directory.GetDirectories()

            string[] dirNames = Directory.GetDirectories(m_fullPath);

            DirectoryInfo[] dirs = new DirectoryInfo[dirNames.Length];

            for (int i = 0; i < dirNames.Length; i++)
            {
                dirs[i] = new DirectoryInfo(dirNames[i]);
            }

            return dirs;
        }

        /// <summary>
        /// Returns the root directory.
        /// </summary>
        public DirectoryInfo Root
        {
            get
            {
                return new DirectoryInfo(Path.GetPathRoot(m_fullPath));
            }
        }

        /// <summary>
        /// Moves a DirectoryInfo instance and its contents to a new path.
        /// </summary>
        /// <param name="destDirName">The name and path to which to move this directory. The destination cannot be another disk volume or a directory with the identical name. 
        /// It can be an existing directory to which you want to add this directory as a subdirectory.
        /// </param>
        public void MoveTo(string destDirName)
        {
            // destDirName validation in Directory.Move()

            Directory.Move(m_fullPath, destDirName);
        }

        /// <summary>
        /// Deletes this DirectoryInfo if it is empty.
        /// </summary>
        public override void Delete()
        {
            Directory.Delete(m_fullPath);
        }

        /// <summary>
        /// Deletes this instance of a DirectoryInfo, specifying whether to delete subdirectories and files.
        /// </summary>
        /// <param name="recursive">true to delete this directory, its subdirectories, and all files; otherwise, false.</param>
        public void Delete(bool recursive)
        {
            Directory.Delete(m_fullPath, recursive);
        }

        /// <summary>
        /// Returns the original path that was passed by the user.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return m_fullPath;
        }
    }
}


