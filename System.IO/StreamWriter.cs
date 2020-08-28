//
// Copyright (c) 2020 The nanoFramework project contributors
// Portions Copyright (c) Microsoft Corporation.  All rights reserved.
// See LICENSE file in the project root for full license information.
//
using System;
using System.Text;
using System.IO;

namespace System.IO
{
    /// <summary>
    /// Implements a TextWriter for writing characters to a stream in a particular encoding.
    /// </summary>
    public class StreamWriter : TextWriter
    {
        private Stream m_stream;
        private bool m_disposed;
        private byte[] m_buffer;

        private int m_curBufPos;

        private const string c_NewLine = "\r\n";
        private const int c_BufferSize = 0xFFF;

        //--//

        /// <summary>
        /// Initializes a new instance of the StreamWriter class for the specified stream 
        /// by using UTF-8 encoding and the default buffer size.
        /// </summary>
        /// <param name="stream">The stream to write to.</param>
        public StreamWriter(Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException();
            }

            if (!stream.CanWrite)
            {
                throw new ArgumentException();
            }

            m_stream = stream;
            m_buffer = new byte[c_BufferSize];
            m_curBufPos = 0;
            m_disposed = false;
        }

        /// <summary>
        /// Initializes a new instance of the StreamWriter class for the specified file by using the default encoding and buffer size.
        /// </summary>
        /// <param name="path">The complete file path to write to. path can be a file name.</param>
        public StreamWriter(String path)
            : this(path, false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the StreamWriter class for the specified file by using the default encoding and buffer size. If the file exists, 
        /// it can be either overwritten or appended to. If the file does not exist, this constructor creates a new file.
        /// </summary>
        /// <param name="path">The complete file path to write to.</param>
        /// <param name="append">true to append data to the file; false to overwrite the file. If the specified file does not exist, this parameter has no effect, 
        /// and the constructor creates a new file.
        /// </param>
        public StreamWriter(String path, bool append)
            : this(new FileStream(path, append ? FileMode.Append : FileMode.Create, FileAccess.Write, FileShare.Read))
        {
        }

        /// <summary>
        /// Closes the current StreamWriter object and the underlying stream.
        /// </summary>
        public override void Close()
        {
            Dispose();
        }

        /// <summary>
        /// Releases the unmanaged resources used by the StreamWriter and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (m_stream != null)
            {
                if (disposing)
                {
                    try
                    {
                        if (m_stream.CanWrite)
                        {
                            Flush();
                        }
                    }
                    catch { }

                    try
                    {
                        m_stream.Close();
                    }
                    catch {}
                }

                m_stream = null;
                m_buffer = null;
                m_curBufPos = 0;
            }

            m_disposed = true;
        }

        /// <summary>
        /// Clears all buffers for the current writer and causes any buffered data to be written to the underlying stream.
        /// </summary>
        public override void Flush()
        {
            if (m_disposed) throw new ObjectDisposedException();

            if (m_curBufPos > 0)
            {
                try
                {
                    m_stream.Write(m_buffer, 0, m_curBufPos);
                }
                catch (Exception e)
                {
                    throw new IOException("StreamWriter Flush. ", e);
                }

                m_curBufPos = 0;
            }
        }

        /// <summary>
        /// Writes a character to the stream.
        /// </summary>
        /// <param name="value">The character to write to the stream.</param>
        public override void Write(char value)
        {
            byte[] buffer = this.Encoding.GetBytes(value.ToString());

            WriteBytes(buffer, 0, buffer.Length);
        }

        /// <summary>
        /// Writes a line terminator to the stream.
        /// </summary>
        public override void WriteLine()
        {
            byte[] tempBuf = this.Encoding.GetBytes(c_NewLine);
            WriteBytes(tempBuf, 0, tempBuf.Length);
            return;
        }

        /// <summary>
        /// Writes a string followed by a line terminator to the stream.
        /// </summary>
        /// <param name="value">The string to write. If the value is null, only a line terminator is written.</param>
        public override void WriteLine(string value)
        {
            byte[] tempBuf = this.Encoding.GetBytes(value + c_NewLine);
            WriteBytes(tempBuf, 0, tempBuf.Length);
            return;
        }

        /// <summary>
        /// Gets the underlying stream that interfaces with a backing store.
        /// </summary>
        public virtual Stream BaseStream
        {
            get
            {
                return m_stream;
            }
        }

        /// <summary>
        /// Gets the Encoding in which the output is written.
        /// </summary>
        public override Encoding Encoding
        {
            get { return System.Text.Encoding.UTF8; }
        }

        //--//

        internal void WriteBytes(byte[] buffer, int index, int count)
        {
            if (m_disposed) throw new ObjectDisposedException();

            // If this write will overrun the buffer flush the current buffer to stream and
            // write remaining bytes directly to stream.
            if (m_curBufPos + count >= c_BufferSize)
            {
                // Flush the current buffer to the stream and write new bytes
                // directly to stream.
                try
                {
                    m_stream.Write(m_buffer, 0, m_curBufPos);
                    m_curBufPos = 0;

                    m_stream.Write(buffer, index, count);
                    return;
                }
                catch (Exception e)
                {
                    throw new IOException("StreamWriter WriteBytes. ", e);
                }
            }

            // Else add bytes to the internal buffer
            Array.Copy(buffer, index, m_buffer, m_curBufPos, count);

            m_curBufPos += count;

            return;
        }
    }
}


