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
    /// Represents a reader that can read a sequential series of characters.
    /// </summary>
    [Serializable()]
    public abstract class TextReader : MarshalByRefObject, IDisposable
    {
        /// <summary>
        /// Initializes a new instance of the TextReader class.
        /// </summary>
        protected TextReader() { }

        /// <summary>
        /// Closes the TextReader and releases any system resources associated with the TextReader.
        /// </summary>
        public virtual void Close()
        {
            Dispose();
        }

        /// <summary>
        /// Releases all resources used by the TextReader object.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the TextReader and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
        }

        /// <summary>
        /// Reads the next character without changing the state of the reader or the character source. 
        /// Returns the next available character without actually reading it from the reader.
        /// </summary>
        /// <returns>An integer representing the next character to be read, or -1 if 
        /// no more characters are available or the reader does not support seeking.</returns>
        public virtual int Peek()
        {
            return -1;
        }

        /// <summary>
        /// Reads the next character from the text reader and advances the character position by one character.
        /// </summary>
        /// <returns>The next character from the text reader, or -1 if no more characters are available. The default implementation returns -1.</returns>
        public virtual int Read()
        {
            return -1;
        }

        /// <summary>
        /// Reads a specified maximum number of characters from the current reader and writes the 
        /// data to a buffer, beginning at the specified index.
        /// </summary>
        /// <param name="buffer">When this method returns, contains the specified character array with the values between index and (index + count - 1) 
        /// replaced by the characters read from the current source.</param>
        /// <param name="index">The position in buffer at which to begin writing.</param>
        /// <param name="count">The maximum number of characters to read. If the end of the reader is reached before the specified number of characters is read into the buffer, the method returns.</param>
        /// <returns>The number of characters that have been read. The number will be less than or equal to count, depending on whether the data is available within the reader. This method returns 0 (zero) 
        /// if it is called when no more characters are left to read.</returns>
        public virtual int Read(char[] buffer, int index, int count)
        {
            return -1;
        }

        /// <summary>
        /// Reads a specified maximum number of characters from the current text reader and writes the data to a buffer, 
        /// beginning at the specified index.
        /// </summary>
        /// <param name="buffer">When this method returns, this parameter contains the specified character array with the values between index and (index + count -1) replaced 
        /// by the characters read from the current source.</param>
        /// <param name="index">The position in buffer at which to begin writing.</param>
        /// <param name="count">The maximum number of characters to read.</param>
        /// <returns>The number of characters that have been read. The number will be less than or equal to count, depending on whether all input characters have been read.</returns>
        public virtual int ReadBlock(char[] buffer, int index, int count)
        {
            int i, n = 0;
            do
            {
                n += (i = Read(buffer, index + n, count - n));
            } while (i > 0 && n < count);
            return n;
        }

        /// <summary>
        /// Reads all characters from the current position to the end of the text reader and returns them as one string.
        /// </summary>
        /// <returns>A string that contains all characters from the current position to the end of the text reader.</returns>
        public virtual String ReadToEnd()
        {
            return null;
        }

        /// <summary>
        /// Reads a line of characters from the text reader and returns the data as a string.
        /// </summary>
        /// <returns>The next line from the reader, or null if all characters have been read.</returns>
        public virtual String ReadLine()
        {
            return null;
        }

    }
}


