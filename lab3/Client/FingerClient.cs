using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace FingerLib.Client
{
    public class FingerClient
    {
        private const int FingerPort = 79;

        private readonly ManualResetEvent _connection;
        private readonly ManualResetEvent _sending;
        private static ManualResetEvent _receiving;

        public List<string> ServerResponse { get; private set; }

        public FingerClient()
        {
            _connection = new ManualResetEvent(false);
            _sending = new ManualResetEvent(false);
            _receiving = new ManualResetEvent(false);
        }

        public void SendQuery(string remoteIPStr, string query)
        {
            try
            {
                var remoteIPAddress = IPAddress.Parse(remoteIPStr);
                var remoteEndPoint = new IPEndPoint(remoteIPAddress, FingerPort);

                var client = new Socket(remoteIPAddress.AddressFamily,
                    SocketType.Stream, ProtocolType.Tcp);

                client.BeginConnect(remoteEndPoint,
                    new AsyncCallback(ConnectCallback), client);

                _connection.WaitOne();

                SendData(client, query);
                _sending.WaitOne();

                Receive(client);
                _receiving.WaitOne();

                client.Shutdown(SocketShutdown.Both);
                client.Close();
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void ConnectCallback(IAsyncResult asyncResult)
        {
            try
            {
                var client = (Socket)asyncResult.AsyncState;

                client.EndConnect(asyncResult);

                _connection.Set();
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void SendData(Socket client, string query)
        {
            string pcName = Environment.MachineName + "\n";

            byte[] data = Encoding.ASCII.GetBytes(pcName + query);

            const int offset = 0;
            client.BeginSend(data, offset, data.Length, SocketFlags.None,
                new AsyncCallback(SendCallback), client);
        }

        private void SendCallback(IAsyncResult asyncResult)
        {
            try
            {
                var client = (Socket)asyncResult.AsyncState;

                int bytesSent = client.EndSend(asyncResult);

                _sending.Set();
            }
            catch
            {
                throw;
            }
        }

        private void Receive(Socket client)
        {
            try 
            {
                var state = new ClientInfo
                {
                    WorkSocket = client
                };

                client.BeginReceive(state.Buffer, 0, ClientInfo.BufferSize, 0,
                    new AsyncCallback(ReceiveCallback), state);
            }
            catch
            {
                throw;
            }
        }

        private void ReceiveCallback(IAsyncResult asyncResult)
        {
            try 
            {
                var state = (ClientInfo)asyncResult.AsyncState;
                Socket client = state.WorkSocket;

                int bytesRead = client.EndReceive(asyncResult);

                byte[] data = state.Buffer.Take(bytesRead).ToArray();

                ServerResponse = ClientNames.Deserialize(data).Names;

                _receiving.Set();
            }
            catch
            {
                throw;
            }
        }
    }
}
