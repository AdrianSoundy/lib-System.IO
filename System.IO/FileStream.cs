//
// Copyright (c) 2020 The nanoFramework project contributors
// Portions Copyright (c) Microsoft Corporation.  All rights reserved.
// See LICENSE file in the project root for full license information.
//

using System.Threading;
using nanoFramework.IO;

namespace System.IO
{
    /// <summary>
    /// Provides a Stream for a file, supporting both synchronous and asynchronous read and write operations.
    /// </summary>
    public class FileStream : Stream
    {
        // Driver data

        private NativeFileStream _nativeFileStream;
        private FileSystemManager.FileRecord _fileRecord;
        private String _fileName;
        private bool _canRead;
        private bool _canWrite;
        private bool _canSeek;

        private long _seekLimit;

        private bool _disposed;

        //--//

        /// <summary>
        /// Initializes a new instance of the FileStream class with the specified path and creation mode.
        /// </summary>
        /// <param name="path">A relative or absolute path for the file that the current FileStream object will encapsulate.</param>
        /// <param name="mode">A constant that determines how to open or create the file.</param>
        public FileStream(String path, FileMode mode)
            : this(path, mode, (mode == FileMode.Append ? FileAccess.Write : FileAccess.ReadWrite), FileShare.Read, NativeFileStream.BufferSizeDefault)
        {
        }

        /// <summary>
        /// Initializes a new instance of the FileStream class with the specified path, creation mode, and read/write permission.
        /// </summary>
        /// <param name="path">A relative or absolute path for the file that the current FileStream object will encapsulate.</param>
        /// <param name="mode">A constant that determines how to open or create the file.</param>
        /// <param name="access">A constant that determines how the file can be accessed by the FileStream object. This also determines the values returned by the CanRead and CanWrite properties of the FileStream object. 
        /// CanSeek is true if path specifies a disk file.</param>
        public FileStream(String path, FileMode mode, FileAccess access)
            : this(path, mode, access, FileShare.Read, NativeFileStream.BufferSizeDefault)
        {
        }

        /// <summary>
        /// Initializes a new instance of the FileStream class with the specified path, 
        /// creation mode, read/write and sharing permission.
        /// </summary>
        /// <param name="path">A relative or absolute path for the file that the current FileStream object will encapsulate.</param>
        /// <param name="mode">A constant that determines how to open or create the file.</param>
        /// <param name="access">A constant that determines how the file can be accessed by the FileStream object. This also determines the values returned by the CanRead and CanWrite properties of the FileStream object. 
        /// CanSeek is true if path specifies a disk file.</param>
        /// <param name="share">A constant that determines how the file will be shared by processes.
        /// </param>
        public FileStream(String path, FileMode mode, FileAccess access, FileShare share)
            : this(path, mode, access, share, NativeFileStream.BufferSizeDefault)
        {
        }

        /// <summary>
        /// Initializes a new instance of the FileStream class with the specified path, 
        /// creation mode, read/write and sharing permission, and buffer size.
        /// </summary>
        /// <param name="path">A relative or absolute path for the file that the current FileStream object will encapsulate.</param>
        /// <param name="mode">A constant that determines how to open or create the file.</param>
        /// <param name="access">A constant that determines how the file can be accessed by the FileStream object. This also determines the values returned by the CanRead and CanWrite properties of the FileStream object. 
        /// CanSeek is true if path specifies a disk file.</param>
        /// <param name="share">A constant that determines how the file will be shared by processes.
        /// </param>
        /// <param name="bufferSize">A positive Int32 value greater than 0 indicating the buffer size. 
        /// </param>
        public FileStream(String path, FileMode mode, FileAccess access, FileShare share, int bufferSize)
        {
            // This will perform validation on path
            _fileName = Path.GetFullPath(path);

            // make sure mode, access, and share are within range
            if (mode < FileMode.CreateNew || mode > FileMode.Append ||
                access < FileAccess.Read || access > FileAccess.ReadWrite ||
                share < FileShare.None || share > FileShare.ReadWrite)
            {
                throw new ArgumentOutOfRangeException();
            }

            // Get wantsRead and wantsWrite from access, note that they cannot both be false
            bool wantsRead = (access & FileAccess.Read) == FileAccess.Read;
            bool wantsWrite = (access & FileAccess.Write) == FileAccess.Write;

            // You can't open for readonly access (wantsWrite == false) when
            // mode is CreateNew, Create, Truncate or Append (when it's not Open or OpenOrCreate)
            if (mode != FileMode.Open && mode != FileMode.OpenOrCreate && !wantsWrite)
            {
                throw new ArgumentException();
            }

            // We need to register the share information prior to the actual file open call (the NativeFileStream ctor)
            // so subsequent file operation on the same file will behave correctly
            _fileRecord = FileSystemManager.AddToOpenList(_fileName, (int)access, (int)share);

            try
            {
                uint attributes = NativeIO.GetAttributes(_fileName);
                bool exists = (attributes != 0xFFFFFFFF);
                bool isReadOnly = (exists) ? (((FileAttributes)attributes) & FileAttributes.ReadOnly) == FileAttributes.ReadOnly : false;

                // If the path specified is an existing directory, fail
                if (exists && ((((FileAttributes)attributes) & FileAttributes.Directory) == FileAttributes.Directory))
                {
                    throw new IOException("", (int)IOException.IOExceptionErrorCode.UnauthorizedAccess);
                }

                // The seek limit is 0 (the beginning of the file) for all modes except Append
                _seekLimit = 0;

                switch (mode)
                {
                    case FileMode.CreateNew: // if the file exists, IOException is thrown
                        if (exists) throw new IOException("", (int)IOException.IOExceptionErrorCode.PathAlreadyExists);
                        _nativeFileStream = new NativeFileStream(_fileName, bufferSize);
                        break;

                    case FileMode.Create: // if the file exists, it should be overwritten
                        _nativeFileStream = new NativeFileStream(_fileName, bufferSize);
                        if (exists) _nativeFileStream.SetLength(0);
                        break;

                    case FileMode.Open: // if the file does not exist, IOException/FileNotFound is thrown
                        if (!exists) throw new IOException("", (int)IOException.IOExceptionErrorCode.FileNotFound);
                        _nativeFileStream = new NativeFileStream(_fileName, bufferSize);
                        break;

                    case FileMode.OpenOrCreate: // if the file does not exist, it is created
                        _nativeFileStream = new NativeFileStream(_fileName, bufferSize);
                        break;

                    case FileMode.Truncate: // the file would be overwritten. if the file does not exist, IOException/FileNotFound is thrown
                        if (!exists) throw new IOException("", (int)IOException.IOExceptionErrorCode.FileNotFound);
                        _nativeFileStream = new NativeFileStream(_fileName, bufferSize);
                        _nativeFileStream.SetLength(0);
                        break;

                    case FileMode.Append: // Opens the file if it exists and seeks to the end of the file. Append can only be used in conjunction with FileAccess.Write
                        // Attempting to seek to a position before the end of the file will throw an IOException and any attempt to read fails and throws an NotSupportedException
                        if (access != FileAccess.Write) throw new ArgumentException();
                        _nativeFileStream = new NativeFileStream(_fileName, bufferSize);
                        _seekLimit = _nativeFileStream.Seek(0, (uint)SeekOrigin.End);
                        break;

                    // We've already checked the mode value previously, so no need for default
                    //default:
                    //    throw new ArgumentOutOfRangeException();
                }

                // Now that we have a valid NativeFileStream, we add it to the FileRecord, so it could gets clean up
                // in case an eject or force format
                _fileRecord.NativeFileStream = _nativeFileStream;

                // Retrive the filesystem capabilities
                _nativeFileStream.GetStreamProperties(out _canRead, out _canWrite, out _canSeek);

                // If the file is readonly, regardless of the filesystem capability, we'll turn off write
                if (isReadOnly)
                {
                    _canWrite = false;
                }

                // Make sure the requests (wantsRead / wantsWrite) matches the filesystem capabilities (canRead / canWrite)
                if ((wantsRead && !_canRead) || (wantsWrite && !_canWrite))
                {
                    throw new IOException("", (int)IOException.IOExceptionErrorCode.UnauthorizedAccess);
                }

                // finally, adjust the _canRead / _canWrite to match the requests
                if (!wantsWrite)
                {
                    _canWrite = false;
                }
                else if (!wantsRead)
                {
                    _canRead = false;
                }
            }
            catch
            {
                // something went wrong, clean up and re-throw the exception
                if (_nativeFileStream != null)
                {
                    _nativeFileStream.Close();
                }

                FileSystemManager.RemoveFromOpenList(_fileRecord);

                throw;
            }
        }

        /// <summary>
        /// Releases the unmanaged resources used by the FileStream and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                try
                {
                    if (disposing)
                    {
                        _canRead = false;
                        _canWrite = false;
                        _canSeek = false;
                    }

                    if (_nativeFileStream != null)
                    {
                        _nativeFileStream.Close();
                    }
                }
                finally
                {
                    if (_fileRecord != null)
                    {
                        FileSystemManager.RemoveFromOpenList(_fileRecord);
                        _fileRecord = null;
                    }

                    _nativeFileStream = null;
                    _disposed = true;
                }
            }
        }

        /// <summary>
        /// Filestream destructor.
        /// </summary>
        ~FileStream()
        {
            Dispose(false);
        }

        // This is for internal use to support proper atomic CopyAndDelete
        internal void DisposeAndDelete()
        {
            _nativeFileStream.Close();
            _nativeFileStream = null; // so Dispose(true) won't close the stream again
            NativeIO.Delete(_fileName);

            Dispose(true);
        }

        /// <summary>
        /// Clears buffers for this stream and causes any buffered data to be written to the file.
        /// </summary>
        public override void Flush()
        {
            if (_disposed) throw new ObjectDisposedException();
            _nativeFileStream.Flush();
        }

        /// <summary>
        /// Sets the length of this stream to the given value.
        /// </summary>
        /// <param name="value">The new length of the stream.</param>
        public override void SetLength(long value)
        {
            if (_disposed) throw new ObjectDisposedException();
            if (!_canWrite || !_canSeek) throw new NotSupportedException();

            // argument validation in interop layer
            _nativeFileStream.SetLength(value);
        }

        /// <summary>
        /// Reads a block of bytes from the stream and writes the data in a given buffer.
        /// </summary>
        /// <param name="buffer">When this method returns, contains the specified byte array with the values between offset and (offset + count - 1) 
        /// replaced by the bytes read from the current source.</param>
        /// <param name="offset">The byte offset in array at which the read bytes will be placed.</param>
        /// <param name="count">The maximum number of bytes to read.</param>
        /// <returns>The total number of bytes read into the buffer. This might be less than the number of bytes requested if that number of bytes are not currently available, 
        /// or zero if the end of the stream is reached.
        /// </returns>
        public override int Read(byte[] buffer, int offset, int count)
        {
            if (_disposed) throw new ObjectDisposedException();
            if (!_canRead) throw new NotSupportedException();

            lock (_nativeFileStream)
            {
                // argument validation in interop layer
                return _nativeFileStream.Read(buffer, offset, count, NativeFileStream.TimeoutDefault);
            }
        }

        /// <summary>
        /// Sets the current position of this stream to the given value.
        /// </summary>
        /// <param name="offset">The point relative to origin from which to begin seeking.</param>
        /// <param name="origin">Specifies the beginning, the end, or the current position as a 
        /// reference point for offset, using a value of type SeekOrigin.</param>
        /// <returns>The new position in the stream.</returns>
        public override long Seek(long offset, SeekOrigin origin)
        {
            if (_disposed) throw new ObjectDisposedException();
            if (!_canSeek) throw new NotSupportedException();

            long oldPosition = this.Position;
            long newPosition = _nativeFileStream.Seek(offset, (uint)origin);

            if (newPosition < _seekLimit)
            {
                this.Position = oldPosition;
                throw new IOException();
            }

            return newPosition;
        }

        /// <summary>
        /// Writes a block of bytes to the file stream.
        /// </summary>
        /// <param name="buffer">The buffer containing data to write to the stream.</param>
        /// <param name="offset">The zero-based byte offset in array from which to begin copying bytes to the stream.</param>
        /// <param name="count">The maximum number of bytes to write.</param>
        public override void Write(byte[] buffer, int offset, int count)
        {
            if (_disposed) throw new ObjectDisposedException();
            if (!_canWrite) throw new NotSupportedException();

            // argument validation in interop layer
            int bytesWritten;

            lock (_nativeFileStream)
            {
                // we check for count being != 0 because we want to handle negative cases
                // as well in the interop layer
                while (count != 0)
                {
                    bytesWritten = _nativeFileStream.Write(buffer, offset, count, NativeFileStream.TimeoutDefault);

                    if (bytesWritten == 0) throw new IOException();

                    offset += bytesWritten;
                    count -= bytesWritten;
                }
            }
        }

        /// <summary>
        /// Gets a value that indicates whether the current stream supports reading.
        /// </summary>
        public override bool CanRead
        {
            get { return _canRead; }
        }

        /// <summary>
        /// Gets a value that indicates whether the current stream supports writing.
        /// </summary>
        public override bool CanWrite
        {
            get { return _canWrite; }
        }

        /// <summary>
        /// Gets a value that indicates whether the current stream supports seeking.
        /// </summary>
        public override bool CanSeek
        {
            get { return _canSeek; }
        }

        /// <summary>
        /// Gets a value that indicates whether the FileStream was opened asynchronously or synchronously.
        /// </summary>
        public virtual bool IsAsync
        {
            get { return false; }
        }

        /// <summary>
        /// Gets the length in bytes of the stream.
        /// </summary>
        public override long Length
        {
            get
            {
                if (_disposed) throw new ObjectDisposedException();
                if (!_canSeek) throw new NotSupportedException();

                return _nativeFileStream.GetLength();
            }
        }

        /// <summary>
        /// Gets the absolute path of the file opened in the FileStream.
        /// </summary>
        public String Name
        {
            get { return _fileName; }
        }

        /// <summary>
        /// Gets or sets the current position of this stream.
        /// </summary>
        public override long Position
        {
            get
            {
                if (_disposed) throw new ObjectDisposedException();
                if (!_canSeek) throw new NotSupportedException();

                // argument validation in interop layer
                return _nativeFileStream.Seek(0, (uint)SeekOrigin.Current);
            }

            set
            {
                if (_disposed) throw new ObjectDisposedException();
                if (!_canSeek) throw new NotSupportedException();
                if (value < _seekLimit) throw new IOException();

                // argument validation in interop layer
                _nativeFileStream.Seek(value, (uint)SeekOrigin.Begin);
            }
        }
    }
}


