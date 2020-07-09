using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using static System.Console;
using static TcpEcho.Shared.Configuration;

var clientSocket = new Socket(SocketType.Stream, ProtocolType.Tcp);

WriteLine($"Connecting to port {TcpPort}");

clientSocket.Connect(new IPEndPoint(IPAddress.Loopback, TcpPort));
var stream = new NetworkStream(clientSocket);

await Console.OpenStandardInput().CopyToAsync(stream);
