//
// Copyright (c) 2020 The nanoFramework project contributors
// Portions Copyright (c) Microsoft Corporation.  All rights reserved.
// See LICENSE file in the project root for full license information.
//
using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Collections;
using nanoFramework.IO;

namespace System.IO
{
    /// <summary>
    /// Provides static methods for the creation, copying, deletion, moving, and 
    /// opening of a single file, and aids in the creation of FileStream objects.
    /// </summary>
    public static class File
    {
        /// <summary>
        /// Copies an existing file to a new file. Overwriting a file of the same name is not allowed.
        /// </summary>
        /// <param name="sourceFileName">The file to copy.</param>
        /// <param name="destFileName">The name of the destination file. This cannot be a directory or an existing file.</param>
        /// <remarks>
        /// 
        /// </remarks>
        public static void Copy(String sourceFileName, String destFileName)
        {
            Copy(sourceFileName, destFileName, false, false);
        }

        /// <summary>
        /// Copies an existing file to a new file. Overwriting a file of the same name is allowed.
        /// </summary>
        /// <param name="sourceFileName">The file to copy.</param>
        /// <param name="destFileName">The name of the destination file. This cannot be a directory.</param>
        /// <param name="overwrite">true if the destination file can be overwritten; otherwise, false.</param>
        public static void Copy(String sourceFileName, String destFileName, bool overwrite)
        {
            Copy(sourceFileName, destFileName, overwrite, false);
        }

        private const int _defaultCopyBufferSize = 2048; /// Experiment on desktop shows 2k-4k is ideal size perfwise.

        internal static void Copy(String sourceFileName, String destFileName, bool overwrite, bool deleteOriginal)
        {
            // sourceFileName and destFileName validation in Path.GetFullPath()

            sourceFileName = Path.GetFullPath(sourceFileName);
            destFileName = Path.GetFullPath(destFileName);

            FileMode writerMode = (overwrite) ? FileMode.Create : FileMode.CreateNew;

            FileStream reader = new FileStream(sourceFileName, FileMode.Open, FileAccess.Read, FileShare.Read, NativeFileStream.BufferSizeDefault);

            try
            {
                using (FileStream writer = new FileStream(destFileName, writerMode, FileAccess.Write, FileShare.None, NativeFileStream.BufferSizeDefault))
                {
                    long fileLength = reader.Length;
                    writer.SetLength(fileLength);

                    byte[] buffer = new byte[_defaultCopyBufferSize];
                    for (; ; )
                    {
                        int readSize = reader.Read(buffer, 0, _defaultCopyBufferSize);
                        if (readSize <= 0)
                            break;

                        writer.Write(buffer, 0, readSize);
                    }

                    // Copy the attributes too
                    NativeIO.SetAttributes(destFileName, NativeIO.GetAttributes(sourceFileName));
                }
            }
            finally
            {
                if (deleteOriginal)
                {
                    reader.DisposeAndDelete();
                }
                else
                {
                    reader.Dispose();
                }
            }
        }

        /// <summary>
        /// Creates or overwrites a file in the specified path.
        /// </summary>
        /// <param name="path">The path and name of the file to create.</param>
        /// <returns>A FileStream that provides read/write access to the file specified in path.</returns>
        public static FileStream Create(String path)
        {
            return new FileStream(path, FileMode.Create, FileAccess.ReadWrite, FileShare.None, NativeFileStream.BufferSizeDefault);
        }


        /// <summary>
        /// Creates or overwrites the specified file.
        /// </summary>
        /// <param name="path">The name of the file.</param>
        /// <param name="bufferSize">The number of bytes buffered for reads and writes to the file.</param>
        /// <returns>A FileStream with the specified buffer size that provides read/write access to the file specified in path.</returns>
        public static FileStream Create(String path, int bufferSize)
        {
            return new FileStream(path, FileMode.Create, FileAccess.ReadWrite, FileShare.None, bufferSize);
        }

        /// <summary>
        /// Deletes the specified file.
        /// </summary>
        /// <param name="path">The name of the file to be deleted. Wildcard characters are not supported.</param>
        public static void Delete(String path)
        {
            // path validation in Path.GetFullPath()

            path = Path.GetFullPath(path);
            string folderPath = Path.GetDirectoryName(path);

            // We have to make sure no one else has the file opened, and no one else can modify it when we're deleting
            Object record = FileSystemManager.AddToOpenList(path);

            try
            {
                uint attributes = NativeIO.GetAttributes(folderPath);
                // If the folder does not exist or invalid we throw DirNotFound Exception (same as desktop).
                if (attributes == 0xFFFFFFFF)
                {
                    throw new IOException("", (int)IOException.IOExceptionErrorCode.DirectoryNotFound);
                }

                // Folder exists, lets verify whether the file itself exists.
                attributes = NativeIO.GetAttributes(path);
                if (attributes == 0xFFFFFFFF)
                {
                    // No-op on file not found
                    return;
                }

                if ((attributes & (uint)(FileAttributes.Directory | FileAttributes.ReadOnly)) != 0)
                {
                    // it's a readonly file or an directory
                    throw new IOException("", (int)IOException.IOExceptionErrorCode.UnauthorizedAccess);
                }

                NativeIO.Delete(path);
            }
            finally
            {
                // regardless of what happened, we need to release the file when we're done
                FileSystemManager.RemoveFromOpenList(record);
            }
        }

        /// <summary>
        /// Determines whether the specified file exists.
        /// </summary>
        /// <param name="path">The file to check.</param>
        /// <returns>
        /// true if the caller has the required permissions and path contains the name of an existing file; otherwise, false. This method also returns false if path is null, an invalid path, or a zero-length string. If the caller does not have sufficient permissions to read the specified file, 
        /// no exception is thrown and the method returns false regardless of the existence of path.
        /// </returns>
        public static bool Exists(String path)
        {
            try
            {
                // path validation in Path.GetFullPath()

                path = Path.GetFullPath(path);

                // Is this the absolute root? this is not a file.
                string root = Path.GetPathRoot(path);
                if (String.Equals(root, path))
                {
                    return false;
                }
                else
                {
                    uint attributes = NativeIO.GetAttributes(path);

                    // This is essentially file not found.
                    if (attributes == 0xFFFFFFFF)
                        return false;

                    if ((attributes & (uint)FileAttributes.Directory) == 0)
                    {
                        // Not a directory, it must be a file.
                        return true;
                    }
                }
            }
            catch (Exception)
            {
                // Like desktop, exists here does not throw exception in
                // a number of cases, instead returns false. For more
                // details see MSDN.
            }

            return false;
        }

        /// <summary>
        /// Opens a FileStream on the specified path with read/write access with no sharing.
        /// </summary>
        /// <param name="path">The file to open.</param>
        /// <param name="mode">A FileMode value that specifies whether a file is created if one does not exist, and determines whether the contents of existing files are retained or overwritten.</param>
        /// <returns></returns>
        public static FileStream Open(String path, FileMode mode)
        {
            return new FileStream(path, mode, (mode == FileMode.Append ? FileAccess.Write : FileAccess.ReadWrite), FileShare.None, NativeFileStream.BufferSizeDefault);
        }

        /// <summary>
        /// Opens a FileStream on the specified path, with the specified mode and access with no sharing.
        /// </summary>
        /// <param name="path">The file to open.</param>
        /// <param name="mode">A FileMode value that specifies whether a file is created if one does not exist, and determines whether the contents of existing files are retained or overwritten.</param>
        /// <param name="access">A FileAccess value that specifies the operations that can be performed on the file.</param>
        /// <returns></returns>
        public static FileStream Open(String path, FileMode mode, FileAccess access)
        {
            return new FileStream(path, mode, access, FileShare.None, NativeFileStream.BufferSizeDefault);
        }

        /// <summary>
        /// Opens a FileStream on the specified path, having the specified mode with read, write, or read/write access and the specified sharing option.
        /// </summary>
        /// <param name="path">The file to open.</param>
        /// <param name="mode">A FileMode value that specifies whether a file is created if one does not exist, and determines whether the contents of existing files are retained or overwritten.</param>
        /// <param name="access">A FileAccess value that specifies the operations that can be performed on the file.</param>
        /// <param name="share">A FileShare value specifying the type of access other threads have to the file.
        /// </param>
        /// <returns>A FileStream on the specified path, having the specified mode with read, write, or read/write access and the specified sharing option.</returns>
        public static FileStream Open(String path, FileMode mode, FileAccess access, FileShare share)
        {
            return new FileStream(path, mode, access, share, NativeFileStream.BufferSizeDefault);
        }

        /// <summary>
        /// Gets the FileAttributes of the file on the path.
        /// </summary>
        /// <param name="path">The path to the file.</param>
        /// <returns>The FileAttributes of the file on the path.</returns>
        public static FileAttributes GetAttributes(String path)
        {
            // path validation in Path.GetFullPath()

            String fullPath = Path.GetFullPath(path);

            uint attributes = NativeIO.GetAttributes(fullPath);
            if (attributes == 0xFFFFFFFF)
                throw new IOException("", (int)IOException.IOExceptionErrorCode.FileNotFound);
            else if (attributes == 0x0)
                return FileAttributes.Normal;
            else
                return (FileAttributes)attributes;
        }

        /// <summary>
        /// Sets the specified FileAttributes of the file on the specified path.
        /// </summary>
        /// <param name="path">The path to the file.</param>
        /// <param name="fileAttributes">A bitwise combination of the enumeration values.</param>
        public static void SetAttributes(String path, FileAttributes fileAttributes)
        {
            // path validation in Path.GetFullPath()

            String fullPath = Path.GetFullPath(path);

            NativeIO.SetAttributes(fullPath, (uint)fileAttributes);
        }

        /// <summary>
        /// Opens an existing file for reading.
        /// </summary>
        /// <param name="path">The file to be opened for reading.</param>
        /// <returns>A read-only FileStream on the specified path.</returns>
        public static FileStream OpenRead(String path)
        {
            return new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, NativeFileStream.BufferSizeDefault);
        }

        /// <summary>
        /// Opens an existing file or creates a new file for writing.
        /// </summary>
        /// <param name="path">The file to be opened for writing.</param>
        /// <returns>An unshared FileStream object on the specified path with Write access.</returns>
        public static FileStream OpenWrite(String path)
        {
            return new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None, NativeFileStream.BufferSizeDefault);
        }

        /// <summary>
        /// Opens a binary file, reads the contents of the file into a byte array, and then closes the file.
        /// </summary>
        /// <param name="path">The file to open for reading.</param>
        /// <returns>A byte array containing the contents of the file.</returns>
        public static byte[] ReadAllBytes(String path)
        {
            byte[] bytes;
            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, NativeFileStream.BufferSizeDefault))
            {
                // Do a blocking read
                int index = 0;
                long fileLength = fs.Length;
                if (fileLength > Int32.MaxValue)
                    throw new IOException();
                int count = (int)fileLength;
                bytes = new byte[count];
                while (count > 0)
                {
                    int n = fs.Read(bytes, index, count);
                    index += n;
                    count -= n;
                }
            }

            return bytes;
        }

        /// <summary>
        /// Creates a new file, writes the specified byte array to the file, and then closes the file. If the target file already exists, it is overwritten.
        /// </summary>
        /// <param name="path">The file to write to.</param>
        /// <param name="bytes">The bytes to write to the file.</param>
        public static void WriteAllBytes(String path, byte[] bytes)
        {
            if (bytes == null)
                throw new ArgumentNullException("bytes");

            using (FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Read, NativeFileStream.BufferSizeDefault))
                fs.Write(bytes, 0, bytes.Length);
        }

        /// <summary>
        /// Moves a specified file to a new location, providing the option to specify a new file name.
        /// </summary>
        /// <param name="sourceFileName">The name of the file to move. Can include a relative or absolute path.</param>
        /// <param name="destFileName">The new path and name for the file.</param>
        public static void Move(String sourceFileName, String destFileName)
        {
            // sourceFileName and destFileName validation in Path.GetFullPath()

            sourceFileName = Path.GetFullPath(sourceFileName);
            destFileName = Path.GetFullPath(destFileName);

            bool tryCopyAndDelete = false;

            // We only need to lock the source, not the dest because if dest is taken
            // Move() will failed at the driver's level anyway. (there will be no conflict even if
            // another thread is creating dest, as only one of the operations will succeed --
            // the native calls are atomic)
            Object srcRecord = FileSystemManager.AddToOpenList(sourceFileName);

            try
            {
                if (!Exists(sourceFileName))
                {
                    throw new IOException("", (int)IOException.IOExceptionErrorCode.FileNotFound);
                }

                //We'll try copy and deleting if Move returns false
                tryCopyAndDelete = !NativeIO.Move(sourceFileName, destFileName);
            }
            finally
            {
                FileSystemManager.RemoveFromOpenList(srcRecord);
            }

            if (tryCopyAndDelete)
            {
                Copy(sourceFileName, destFileName, false, true);
            }
        }
    }
}


