using System.Net.Sockets;
using System.Text;

namespace FingerLib.Client
{
    class ClientInfo
    {
        public const int BufferSize = 65536;

        public Socket WorkSocket { get; set; }

        public byte[] Buffer { get; set; }

        public ClientInfo()
        {
            WorkSocket = null;
            Buffer = new byte[BufferSize];
        }


    }
}
