using System.Buffers;
using System.IO.Pipelines;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using TcpEcho.Server;
using static System.Console;
using static TcpEcho.Shared.Configuration;

var webSocket = new Socket(SocketType.Stream, ProtocolType.Tcp);
webSocket.Bind(new IPEndPoint(IPAddress.Loopback, TcpPort));
webSocket.Listen(120);
WriteLine($"Listening on port {TcpPort}");

var writeChannel = Channel.CreateUnbounded<ReadOnlySequence<byte>>(new UnboundedChannelOptions
{
    AllowSynchronousContinuations = true,
    SingleWriter = true,
    SingleReader = true,
});
var writer = new EchoWriter(writeChannel);


while (true)
{
    var socket = await webSocket.AcceptAsync();
    _ = ProcessLinesAsync(socket);
}

async Task ProcessLinesAsync(Socket socket)
{
    WriteLine($"[{socket.RemoteEndPoint}]: connected");

    await using var stream = new NetworkStream(socket);
    var reader = PipeReader.Create(stream);

    while (true)
    {
        ReadResult result = await reader.ReadAsync();
        ReadOnlySequence<byte> buffer = result.Buffer;

        while (TryReadLine(ref buffer, out ReadOnlySequence<byte> line))
            await writeChannel.Writer.WriteAsync(line);

        reader.AdvanceTo(buffer.Start, buffer.End);

        if (result.IsCompleted)
            break;
    }

    // Mark the PipeReader as complete.
    await reader.CompleteAsync();

    WriteLine($"[{socket.RemoteEndPoint}]: disconnected");
}

bool TryReadLine(ref ReadOnlySequence<byte> buffer, out ReadOnlySequence<byte> line)
{
    // Look for a EOL in the buffer.
    var position = buffer.PositionOf((byte)'\n');

    if (position is null)
    {
        line = default;
        return false;
    }

    // Skip the line + the \n.
    line = buffer.Slice(0, position.Value);
    buffer = buffer.Slice(buffer.GetPosition(1, position.Value));
    return true;
}