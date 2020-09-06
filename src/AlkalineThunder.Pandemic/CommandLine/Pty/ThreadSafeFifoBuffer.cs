using System;
using System.Collections.Generic;
using System.IO;

namespace AlkalineThunder.Pandemic.CommandLine.Pty
{
    internal class ThreadSafeFifoBuffer : Stream
    {
        private bool _interrupt;
        private object _mutex = new object();
        private Queue<byte> _buffer = new Queue<byte>();
        private bool _hasClosed;
        
        public void SetInterrupt()
        {
            _interrupt = true;
        }
        
        public override void Flush()
        {
            // what?
        }

        private void ThrowIfClosed()
        {
            if (_hasClosed)
                throw new ObjectDisposedException("Your mother");
        }
        
        public override int Read(byte[] buffer, int offset, int count)
        {
            ThrowIfClosed();
            
            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer));

            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count));

            if (offset < 0 || offset + count > buffer.Length)
                throw new IndexOutOfRangeException();

            var bytesRead = 0;

            lock (_mutex)
            {
                for (var i = offset; i < offset + count; i++)
                {
                    ThrowIfClosed();
                    
                    if (_buffer.Count > 0)
                    {
                        buffer[i] = _buffer.Dequeue();
                        bytesRead++;
                    }
                }

                if (_buffer.Count == 0 && _interrupt)
                {
                    _interrupt = false;
                    throw new ProcessInterruptedException();
                }
            }
            
            return bytesRead;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            ThrowIfClosed();
            
            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer));

            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count));
            
            if (offset < 0 || offset + count > buffer.Length)
                throw new IndexOutOfRangeException();

            lock (_mutex)
            {
                for (var i = offset; i < offset + count; i++)
                {
                    ThrowIfClosed();
                    _buffer.Enqueue(buffer[i]);
                }
            }
        }

        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => _buffer.Count;

        public override long Position
        {
            get => 0;
            set => throw new NotSupportedException();
        }

        public override void Close()
        {
            lock (_mutex)
            {
                _buffer.Clear();
                _hasClosed = true;
            }

            base.Close();
        }
    }
}