//
// Copyright (c) 2020 The nanoFramework project contributors
// Portions Copyright (c) Microsoft Corporation.  All rights reserved.
// See LICENSE file in the project root for full license information.
//
using System;
using System.Text;
using System.Collections;

namespace System.IO
{
    /// <summary>
    /// Represents a writer that can write a sequential series of characters. This class is abstract.
    /// </summary>
    [Serializable]
    public abstract class TextWriter : MarshalByRefObject, IDisposable
    {
        private const String InitialNewLine = "\r\n";


        /// <summary>
        /// Stores the newline characters used for this TextWriter.
        /// </summary>
        protected char[] CoreNewLine = new char[] { '\r', '\n' };

        /// <summary>
        /// Closes the current writer and releases any system resources associated with the writer.
        /// </summary>
        public virtual void Close()
        {
            Dispose();
        }

        /// <summary>
        /// Releases all resources used by the TextWriter object.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the TextWriter and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
        }

        /// <summary>
        /// Clears all buffers for the current writer and causes any buffered data to be written to the underlying device.
        /// </summary>
        public virtual void Flush()
        {
        }

        /// <summary>
        /// Return character encoding.
        /// </summary>
        public abstract Encoding Encoding
        {
            get;
        }

        /// <summary>
        /// Returns or set the new line.
        /// </summary>
        public virtual String NewLine
        {
            get { return new String(CoreNewLine); }
            set
            {
                if (value == null)
                    value = InitialNewLine;
                CoreNewLine = value.ToCharArray();
            }
        }

        /// <summary>
        /// Writes a character to the text string or stream.
        /// </summary>
        /// <param name="value"></param>
        public virtual void Write(char value)
        {
        }

        /// <summary>
        /// Writes a character array to the text string or stream.
        /// </summary>
        /// <param name="buffer">The character array to write to the text stream.</param>
        public virtual void Write(char[] buffer)
        {
            if (buffer != null) Write(buffer, 0, buffer.Length);
        }

        /// <summary>
        /// Writes a subarray of characters to the text string or stream.
        /// </summary>
        /// <param name="buffer">The character array to write data from.</param>
        /// <param name="index">The character position in the buffer at which to start retrieving data.</param>
        /// <param name="count">The number of characters to write.</param>
        public virtual void Write(char[] buffer, int index, int count)
        {
            if (buffer == null)
                throw new ArgumentNullException();
            if (index < 0)
                throw new ArgumentOutOfRangeException();
            if (count < 0)
                throw new ArgumentOutOfRangeException();
            if (buffer.Length - index < count)
                throw new ArgumentException();

            for (int i = 0; i < count; i++) Write(buffer[index + i]);
        }

        /// <summary>
        /// Writes the text representation of a Boolean value to the text string or stream.
        /// </summary>
        /// <param name="value">The Boolean value to write.</param>
        public virtual void Write(bool value)
        {
            Write(value);
        }

        /// <summary>
        /// Writes the text representation of a 4-byte signed integer to the text string or stream.
        /// </summary>
        /// <param name="value">The 4-byte signed integer to write.</param>
        public virtual void Write(int value)
        {
            Write(value.ToString());
        }

        /// <summary>
        /// Writes the text representation of a 4-byte unsigned integer to the text string or stream.
        /// </summary>
        /// <param name="value">The 4-byte unsigned integer to write.</param>
        public virtual void Write(uint value)
        {
            Write(value.ToString());
        }

        /// <summary>
        /// Writes the text representation of an 8-byte signed integer to the text string or stream.
        /// </summary>
        /// <param name="value">The 8-byte signed integer to write.</param>
        public virtual void Write(long value)
        {
            Write(value.ToString());
        }

        /// <summary>
        /// Writes the text representation of an 8-byte unsigned integer to the text string or stream.
        /// </summary>
        /// <param name="value">The 8-byte unsigned integer to write.</param>
        public virtual void Write(ulong value)
        {
            Write(value.ToString());
        }

        /// <summary>
        /// Writes the text representation of a 4-byte floating-point value to the text string or stream.
        /// </summary>
        /// <param name="value">The 4-byte floating-point value to write.</param>
        public virtual void Write(float value)
        {
            Write(value.ToString());
        }

        /// <summary>
        /// Writes the text representation of an 8-byte floating-point value to the text string or stream.
        /// </summary>
        /// <param name="value">The 8-byte floating-point value to write.</param>
        public virtual void Write(double value)
        {
            Write(value.ToString());
        }

        /// <summary>
        /// Writes a string to the text string or stream.
        /// </summary>
        /// <param name="value">The string to write.</param>
        public virtual void Write(String value)
        {
            if (value != null) Write(value.ToCharArray());
        }

        /// <summary>
        /// Writes the text representation of an object to the text string or stream by calling the ToString method on that object.
        /// </summary>
        /// <param name="value">The object to write.</param>
        public virtual void Write(Object value)
        {
            if (value != null)
            {
                Write(value.ToString());
            }
        }

        /// <summary>
        /// Writes a line terminator to the text string or stream.
        /// </summary>
        public virtual void WriteLine()
        {
            Write(CoreNewLine);
        }

        /// <summary>
        /// Writes a character followed by a line terminator to the text string or stream.
        /// </summary>
        /// <param name="value"></param>
        public virtual void WriteLine(char value)
        {
            Write(value);
            WriteLine();
        }

        /// <summary>
        /// Writes an array of characters followed by a line terminator to the text string or stream.
        /// </summary>
        /// <param name="buffer">The character array from which data is read.</param>
        public virtual void WriteLine(char[] buffer)
        {
            Write(buffer);
            WriteLine();
        }

        /// <summary>
        /// Writes a subarray of characters followed by a line terminator to the text string or stream.
        /// </summary>
        /// <param name="buffer">The character array from which data is read.</param>
        /// <param name="index">The character position in buffer at which to start reading data.</param>
        /// <param name="count">The maximum number of characters to write.</param>
        public virtual void WriteLine(char[] buffer, int index, int count)
        {
            Write(buffer, index, count);
            WriteLine();
        }

        /// <summary>
        /// Writes the text representation of a Boolean value followed by a line terminator to the text string or stream.
        /// </summary>
        /// <param name="value">The Boolean value to write.</param>
        public virtual void WriteLine(bool value)
        {
            Write(value);
            WriteLine();
        }

        /// <summary>
        /// Writes the text representation of a 4-byte signed integer followed by a line terminator to the text string or stream.
        /// </summary>
        /// <param name="value">The 4-byte signed integer to write.</param>
        public virtual void WriteLine(int value)
        {
            Write(value);
            WriteLine();
        }

        /// <summary>
        /// Writes the text representation of a 4-byte unsigned integer followed by a line terminator to the text string or stream.
        /// </summary>
        /// <param name="value">The 4-byte unsigned integer to write.</param>
        public virtual void WriteLine(uint value)
        {
            Write(value);
            WriteLine();
        }

        /// <summary>
        /// Writes the text representation of an 8-byte signed integer followed by a line terminator to the text string or stream.
        /// </summary>
        /// <param name="value">The 8-byte signed integer to write.</param>
        public virtual void WriteLine(long value)
        {
            Write(value);
            WriteLine();
        }

        /// <summary>
        /// Writes the text representation of an 8-byte unsigned integer followed by a line terminator to the text string or stream.
        /// </summary>
        /// <param name="value">The 8-byte unsigned integer to write.</param>
        public virtual void WriteLine(ulong value)
        {
            Write(value);
            WriteLine();
        }

        /// <summary>
        /// Writes the text representation of a 4-byte floating-point value followed by a line terminator to the text string or stream.
        /// </summary>
        /// <param name="value">The 4-byte floating-point value to write.</param>
        public virtual void WriteLine(float value)
        {
            Write(value);
            WriteLine();
        }

        /// <summary>
        /// Writes the text representation of a 8-byte floating-point value followed by a line terminator to the text string or stream.
        /// </summary>
        /// <param name="value">The 8-byte floating-point value to write.</param>
        public virtual void WriteLine(double value)
        {
            Write(value);
            WriteLine();
        }

        /// <summary>
        /// Writes a string followed by a line terminator to the text string or stream.
        /// </summary>
        /// <param name="value">The string to write. If value is null, only the line terminator is written.</param>
        public virtual void WriteLine(String value)
        {
            Write(value);
            WriteLine();
        }

        /// <summary>
        /// Writes the text representation of an object by calling the ToString method on that object, 
        /// followed by a line terminator to the text string or stream.
        /// </summary>
        /// <param name="value">The object to write. If value is null, only the line terminator is written.</param>
        public virtual void WriteLine(Object value)
        {
            if (value == null)
            {
                WriteLine();
            }
            else
            {
                WriteLine(value.ToString());
            }
        }
    }
}


