//
// Copyright (c) 2020 The nanoFramework project contributors
// Portions Copyright (c) Microsoft Corporation.  All rights reserved.
// See LICENSE file in the project root for full license information.
//
using System;

namespace System.IO
{
    /// <summary>
    /// Creates a stream whose backing store is memory.
    /// </summary>
    public class MemoryStream : Stream
    {
        private byte[] _buffer;    // Either allocated internally or externally.
        private int _origin;       // For user-provided arrays, start at this origin
        private int _position;     // read/write head.
        private int _length;       // Number of bytes within the memory stream
        private int _capacity;     // length of usable portion of buffer for stream
        private bool _expandable;  // User-provided buffers aren't expandable.
        private bool _isOpen;      // Is this stream open or closed?

        private const int MemStreamMaxLength = 0xFFFF;

        /// <summary>
        /// Initializes a new instance of the MemoryStream class with an expandable capacity initialized to zero.
        /// </summary>
        public MemoryStream()
        {
            _buffer = new byte[256];
            _capacity = 256;
            _expandable = true;
            _origin = 0;      // Must be 0 for byte[]'s created by MemoryStream
            _isOpen = true;
        }

        /// <summary>
        /// Initializes a new non-resizable instance of the MemoryStream class based on the specified byte array.
        /// </summary>
        /// <param name="buffer">The array of unsigned bytes from which to create the current stream.</param>
        public MemoryStream(byte[] buffer)
        {
            if (buffer == null) throw new ArgumentNullException(/*"buffer", Environment.GetResourceString("ArgumentNull_Buffer")*/);
            _buffer = buffer;
            _length = _capacity = buffer.Length;
            _expandable = false;
            _origin = 0;
            _isOpen = true;
        }

        /// <summary>
        /// Gets a value indicating whether the current stream supports reading.
        /// </summary>
        public override bool CanRead
        {
            get { return _isOpen; }
        }

        /// <summary>
        /// Gets a value indicating whether the current stream supports seeking.
        /// </summary>
        public override bool CanSeek
        {
            get { return _isOpen; }
        }

        /// <summary>
        /// Gets a value indicating whether the current stream supports writing.
        /// </summary>
        public override bool CanWrite
        {
            get { return _isOpen; }
        }

        /// <summary>
        /// Releases the unmanaged resources used by the MemoryStream class and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _isOpen = false;
            }
        }

        // returns a bool saying whether we allocated a new array.
        private bool EnsureCapacity(int value)
        {
            if (value > _capacity)
            {
                int newCapacity = value;
                if (newCapacity < 256)
                    newCapacity = 256;
                if (newCapacity < _capacity * 2)
                    newCapacity = _capacity * 2;

                if (!_expandable && newCapacity > _capacity) throw new NotSupportedException();
                if (newCapacity > 0)
                {
                    byte[] newBuffer = new byte[newCapacity];
                    if (_length > 0) Array.Copy(_buffer, 0, newBuffer, 0, _length);
                    _buffer = newBuffer;
                }
                else
                {
                    _buffer = null;
                }

                _capacity = newCapacity;

                return true;
            }

            return false;
        }

        /// <summary>
        /// Overrides the Flush() method so that no action is performed.
        /// </summary>
        public override void Flush()
        {
        }

        /// <summary>
        /// Gets the length of the stream in bytes.
        /// </summary>
        public override long Length
        {
            get
            {
                if (!_isOpen) throw new ObjectDisposedException();
                return _length - _origin;
            }
        }

        /// <summary>
        /// Gets or sets the current position within the stream.
        /// </summary>
        public override long Position
        {
            get
            {
                if (!_isOpen) throw new ObjectDisposedException();
                return _position - _origin;
            }

            set
            {
                if (!_isOpen) throw new ObjectDisposedException();
                if (value < 0 || value > MemStreamMaxLength)
                    throw new ArgumentOutOfRangeException(/*"value", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum")*/);
                _position = _origin + (int)value;
            }
        }

        /// <summary>
        /// Reads a block of bytes from the current stream and writes the data to a buffer.
        /// </summary>
        /// <param name="buffer">When this method returns, contains the specified byte array with the values between offset and (offset + count - 1) 
        /// replaced by the characters read from the current stream.</param>
        /// <param name="offset">The zero-based byte offset in buffer at which to begin storing data from the current stream.</param>
        /// <param name="count">The maximum number of bytes to read.</param>
        /// <returns>The total number of bytes written into the buffer. This can be less than the number of bytes requested if that number of bytes are not currently available, 
        /// or zero if the end of the stream is reached before any bytes are read.</returns>
        public override int Read(byte[] buffer, int offset, int count)
        {
            if (!_isOpen) throw new ObjectDisposedException();

            if (buffer == null)
                throw new ArgumentNullException(/*"buffer", Environment.GetResourceString("ArgumentNull_Buffer")*/);
            if (offset < 0 || count < 0)
                throw new ArgumentOutOfRangeException(/*"offset", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum")*/);
            if (buffer.Length - offset < count)
                throw new ArgumentException(/*Environment.GetResourceString("Argument_InvalidOffLen")*/);

            int n = _length - _position;
            if (n > count) n = count;
            if (n <= 0)
                return 0;

            Array.Copy(_buffer, _position, buffer, offset, n);
            _position += n;
            return n;
        }

        /// <summary>
        /// Reads a byte from the current stream.
        /// </summary>
        /// <returns>The byte cast to a Int32, or -1 if the end of the stream has been reached.</returns>
        public override int ReadByte()
        {
            if (!_isOpen) throw new ObjectDisposedException();

            if (_position >= _length) return -1;
            return _buffer[_position++];
        }

        /// <summary>
        /// Sets the position within the current stream to the specified value.
        /// </summary>
        /// <param name="offset">The new position within the stream. This is relative to the loc parameter, and can be positive or negative.</param>
        /// <param name="origin">A value of type SeekOrigin, which acts as the seek reference point.</param>
        /// <returns>The new position within the stream, calculated by combining the initial reference point and the offset.</returns>
        public override long Seek(long offset, SeekOrigin origin)
        {
            if (!_isOpen) throw new ObjectDisposedException();

            if (offset > MemStreamMaxLength)
                throw new ArgumentOutOfRangeException(/*"offset", Environment.GetResourceString("ArgumentOutOfRange_MemStreamLength")*/);
            switch (origin)
            {
                case SeekOrigin.Begin:
                    if (offset < 0)
                        throw new IOException(/*Environment.GetResourceString("IO.IO_SeekBeforeBegin")*/);
                    _position = _origin + (int)offset;
                    break;

                case SeekOrigin.Current:
                    if (offset + _position < _origin)
                        throw new IOException(/*Environment.GetResourceString("IO.IO_SeekBeforeBegin")*/);
                    _position += (int)offset;
                    break;

                case SeekOrigin.End:
                    if (_length + offset < _origin)
                        throw new IOException(/*Environment.GetResourceString("IO.IO_SeekBeforeBegin")*/);
                    _position = _length + (int)offset;
                    break;

                default:
                    throw new ArgumentException(/*Environment.GetResourceString("Argument_InvalidSeekOrigin")*/);
            }

            return _position;
        }

        /*
         * Sets the length of the stream to a given value.  The new
         * value must be nonnegative and less than the space remaining in
         * the array, <var>Int32.MaxValue</var> - <var>origin</var>
         * Origin is 0 in all cases other than a MemoryStream created on
         * top of an existing array and a specific starting offset was passed
         * into the MemoryStream constructor.  The upper bounds prevents any
         * situations where a stream may be created on top of an array then
         * the stream is made longer than the maximum possible length of the
         * array (<var>Int32.MaxValue</var>).
         *
         * @exception ArgumentException Thrown if value is negative or is
         * greater than Int32.MaxValue - the origin
         * @exception NotSupportedException Thrown if the stream is readonly.
         */

        /// <summary>
        /// Sets the length of the current stream to the specified value.
        /// </summary>
        /// <param name="value">The value at which to set the length.</param>
        public override void SetLength(long value)
        {
            if (!_isOpen) throw new ObjectDisposedException();

            if (value > MemStreamMaxLength || value < 0)
                throw new ArgumentOutOfRangeException(/*"value", Environment.GetResourceString("ArgumentOutOfRange_MemStreamLength")*/);

            int newLength = _origin + (int)value;
            bool allocatedNewArray = EnsureCapacity(newLength);
            if (!allocatedNewArray && newLength > _length)
                Array.Clear(_buffer, _length, newLength - _length);
            _length = newLength;
            if (_position > newLength) _position = newLength;
        }

        /// <summary>
        /// Writes the stream contents to a byte array, regardless of the Position property.
        /// </summary>
        /// <returns>A new byte array.</returns>
        public virtual byte[] ToArray()
        {
            byte[] copy = new byte[_length - _origin];
            Array.Copy(_buffer, _origin, copy, 0, _length - _origin);
            return copy;
        }

        /// <summary>
        /// Writes a block of bytes to the current stream using data read from a buffer.
        /// </summary>
        /// <param name="buffer">The buffer to write data from.</param>
        /// <param name="offset">The zero-based byte offset in buffer at which to begin copying bytes to the current stream.</param>
        /// <param name="count">The maximum number of bytes to write.</param>
        public override void Write(byte[] buffer, int offset, int count)
        {
            if (!_isOpen) throw new ObjectDisposedException();

            if (buffer == null)
                throw new ArgumentNullException(/*"buffer", Environment.GetResourceString("ArgumentNull_Buffer")*/);
            if (offset < 0 || count < 0)
                throw new ArgumentOutOfRangeException(/*"offset", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum")*/);
            if (buffer.Length - offset < count)
                throw new ArgumentException(/*Environment.GetResourceString("Argument_InvalidOffLen")*/);

            int i = _position + count;
            // Check for overflow

            if (i > _length)
            {
                if (i > _capacity) EnsureCapacity(i);
                _length = i;
            }

            Array.Copy(buffer, offset, _buffer, _position, count);
            _position = i;
            return;
        }

        /// <summary>
        /// Writes a byte to the current stream at the current position.
        /// </summary>
        /// <param name="value">The byte to write.</param>
        public override void WriteByte(byte value)
        {
            if (!_isOpen) throw new ObjectDisposedException();

            if (_position >= _capacity)
            {
                EnsureCapacity(_position + 1);
            }

            _buffer[_position++] = value;

            if (_position > _length)
            {
                _length = _position;
            }
        }

        /*
         * Writes this MemoryStream to another stream.
         * @param stream Stream to write into
         * @exception ArgumentNullException if stream is null.
         */

        /// <summary>
        /// Writes the entire contents of this memory stream to another stream.
        /// </summary>
        /// <param name="stream">The stream to write this memory stream to.</param>
        public virtual void WriteTo(Stream stream)
        {
            if (!_isOpen) throw new ObjectDisposedException();

            if (stream == null)
                throw new ArgumentNullException(/*"stream", Environment.GetResourceString("ArgumentNull_Stream")*/);
            stream.Write(_buffer, _origin, _length - _origin);
        }
    }
}


