using System.Net.Sockets;

namespace FingerLib.Server
{
    class ServerInfo
    {
        public const int BufferSize = 65536;

        public Socket WorkSocket { get; set; }

        public byte[] Buffer { get; set; }

        public string PcName { get; set; }

        public ServerInfo()
        {
            WorkSocket = null;
            Buffer = new byte[BufferSize];
            PcName = string.Empty;
        }
    }
}
