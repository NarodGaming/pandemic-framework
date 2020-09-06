using System;
using System.IO;
using System.Threading;

namespace AlkalineThunder.Pandemic.CommandLine.Pty
{
    internal class PseudoTerminal : Stream
    {
        private ThreadSafeFifoBuffer _input;
        private ThreadSafeFifoBuffer _output;
        private bool _isMaster;
        
        
        public override bool CanRead => true;
        public override bool CanWrite => true;
        public override bool CanSeek => false;

        public override long Length => -1;

        private PseudoTerminal(ThreadSafeFifoBuffer input, ThreadSafeFifoBuffer output, bool master)
        {
            _input = input;
            _output = output;
            _isMaster = master;
        }
        
        public override long Position
        {
            get => -1;
            set => throw new NotSupportedException();
        }

        private void WriteOutput(byte b)
        {
            _output.WriteByte(b);
        }

        private void WriteInput(byte b)
        {
            lock (_input)
            {
                if (b == KernelCharacters.ProcessInterruptSignal)
                {
                    _input.SetInterrupt();
                }
                else
                {
                    _input.WriteByte(b);
                }

                Monitor.PulseAll(_input);
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (_isMaster)
            {
                int i;

                lock (_input)
                {

                    while ((i = _input.Read(buffer, offset, count)) == 0)
                    {
                        Monitor.Wait(_input);
                    }
                }

                return i;
            }
            else
            {
                return _output.Read(buffer, offset, count);
            }
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Flush()
        {
            
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (_isMaster)
            {
                for (var i = offset; i < offset + count; i++)
                {
                    WriteOutput(buffer[i]);
                }
            }
            else
            {
                for (var i = offset; i < offset + count; i++)
                {
                    WriteInput(buffer[i]);
                }
            }
        }

        public static void CreatePair(out PseudoTerminal master, out PseudoTerminal slave)
        {
            var input = new ThreadSafeFifoBuffer();
            var output = new ThreadSafeFifoBuffer();

            master = new PseudoTerminal(input, output, true);
            slave = new PseudoTerminal(input, output, false);
        }

        public override void Close()
        {
            _input.Close();
            _output.Close();
            base.Close();
        }
    }
}