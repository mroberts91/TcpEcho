using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace TcpEcho.Server
{
    public class EchoWriter
    {
        private readonly ChannelReader<ReadOnlySequence<byte>> _reader;
        public EchoWriter(Channel<ReadOnlySequence<byte>> channel)
        {
            _reader = channel.Reader;
            _ = WriteMessagesAsync();
        }

        private async Task WriteMessagesAsync()
        {
            await foreach (var bytes in ReadMessages())
                Console.WriteLine($"{Encoding.UTF8.GetString(bytes)}");
        }

        private IAsyncEnumerable<ReadOnlySequence<byte>> ReadMessages()
        {
            return _reader.ReadAllAsync();
        }
    }
}
