using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace FingerLib.Server
{
    public class FingerServer
    {
        private const int MaxConnections = 50;
        private const int FingerPort = 79;

        private readonly ManualResetEvent _allDone;

        private readonly ClientNames _clientNames;

        public FingerServer()
        {
            _allDone = new ManualResetEvent(false);
            _clientNames = new ClientNames();
        }

        public void Start(string ipString)
        {
            try
            {
                var ipAddress = IPAddress.Parse(ipString);
                var endPoint = new IPEndPoint(ipAddress, FingerPort);

                var server = new Socket(ipAddress.AddressFamily,
                    SocketType.Stream, ProtocolType.Tcp);

                server.Bind(endPoint);
                server.Listen(MaxConnections);

                while(true)
                {
                    _allDone.Reset();

                    server.BeginAccept(new AsyncCallback(AcceptCallback), server);

                    _allDone.WaitOne();
                }
            }
            catch
            {
                throw;
            }
        }

        private void AcceptCallback(IAsyncResult asyncResult)
        {
            _allDone.Set();

            var server = (Socket)asyncResult.AsyncState;
            var socket = server.EndAccept(asyncResult);

            var state = new ServerInfo
            {
                WorkSocket = socket
            };

            const int offset = 0;
            socket.BeginReceive(state.Buffer, offset,
                ServerInfo.BufferSize, SocketFlags.None,
                new AsyncCallback(ReceiveCallback), state);
        }

        private void ReceiveCallback(IAsyncResult asyncResult)
        {
            var state = (ServerInfo)asyncResult.AsyncState;
            var socket = state.WorkSocket;

            int bytesRead = socket.EndReceive(asyncResult);

            if (bytesRead > 0)
            {
                (string pcName, string query) = ParseReceivedData(state.Buffer, bytesRead);

                state.PcName = pcName;

                byte[] data = ParseQuery(query);

                Send(state, data);
            }
        }

        private void Send(ServerInfo state, byte[] data)
        {
            var socket = state.WorkSocket;

            socket.BeginSend(data, 0, data.Length, 0,
                new AsyncCallback(SendCallback), state);
        }

        private void SendCallback(IAsyncResult asyncResult)
        {
            try
            {
                var state = (ServerInfo)asyncResult.AsyncState;
                var socket = state.WorkSocket;

                int bytesSent = socket.EndSend(asyncResult);

                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
            }
            catch
            {
                throw;
            }
        }

        private (string, string) ParseReceivedData(byte[] data, int count)
        {
            string receivedData = Encoding.ASCII.GetString(data, 0, count);

            string[] dataArray = receivedData.Split("\n");

            _clientNames.Names.Add(dataArray[0]);

            return (dataArray[0], dataArray[1]);
        }

        private byte[] ParseQuery(string query)
        {
            string[] partsOfQuery = query.Split(" ");

            if (partsOfQuery.Length > 1)
            {
                var resultClientNames = new ClientNames();
                resultClientNames.Names.Add(_clientNames.GetUserName(partsOfQuery[1]));

                return ClientNames.Serialize(resultClientNames);

            }

            return ClientNames.Serialize(_clientNames);
        }
    }

    
}
