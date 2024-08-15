using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleChatProgram
{
    public class Client : IDisposable
    {
        private static Client? mInstance;
        public static Client? Instance
        {
            get
            {
                if (mInstance == null)
                {
                    mInstance = new Client();
                }

                return mInstance;
            }

            private set
            {
                mInstance = value;
            }
        }

        private readonly Socket mSoceket;
        private int mNameLength;

        private Client()
        {
            mSoceket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }
        public void Dispose()
        {
            mSoceket.Close();
        }

        public void Connect(string serverIP, string name)
        {
            Debug.Assert(serverIP != null);
            Debug.Assert(name != null);
            Debug.Assert(name.Length < byte.MaxValue);

            mNameLength = name.Length;

            IPAddress ipAddr = IPAddress.Parse(serverIP);

            IPEndPoint ep = new IPEndPoint(ipAddr, Server.PORT);

            mSoceket.Connect(ep);

            byte[] sendingData = Encoding.UTF8.GetBytes(name);
            mSoceket.Send(sendingData);
        }

        public void Run()
        {
            Task.Run(() => ReceiveChat());

            while (true)
            {
                string? chat = Console.ReadLine();
                Debug.Assert(chat != null);
                if (chat.Length >= Server.MAX_STR_LENGTH)
                {
                    chat = chat.Substring(0, Server.MAX_STR_LENGTH - mNameLength - 2);
                }

                byte[] data = Encoding.UTF8.GetBytes(chat);
                mSoceket.Send(data);

                Monitor.Enter(Console.Out);
                {
                    Console.Out.Flush();
                }
                Monitor.Exit(Console.Out);
            }
        }

        private void ReceiveChat()
        {
            byte[] data = new byte[Server.MAX_STR_LENGTH];
            while (true)
            {
                Array.Clear(data);
                mSoceket.Receive(data);

                string chat = Utilities.DecodeData(data);

                Monitor.Enter(Console.Out);
                {
                    Console.WriteLine();
                    Console.WriteLine(chat);
                    Console.Out.Flush();
                }
                Monitor.Exit(Console.Out);
            }
        }
    }
}
