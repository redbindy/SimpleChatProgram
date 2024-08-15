using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleChatProgram
{
    public class Server : IDisposable
    {
        public const int PORT = 25565;
        public const int MAX_USER_COUNT = 8;
        public const int MAX_STR_LENGTH = 4096;

        private static Server? mInstance;
        public static Server? Instance
        {
            get
            {
                if (mInstance == null)
                {
                    mInstance = new Server();
                }

                return mInstance;
            }

            private set
            {
                mInstance = value;
            }
        }

        private readonly Socket mSocket;
        private readonly List<UserObject> mUsers;
        private readonly Queue<string> mChatData;

        private const int MAX_CHAT_COUNT = MAX_USER_COUNT << 2;

        private Server()
        {
            mSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            IPEndPoint ep = new IPEndPoint(IPAddress.Any, PORT);
            mSocket.Bind(ep);

            mUsers = new List<UserObject>(MAX_USER_COUNT);
            mChatData = new Queue<string>(MAX_USER_COUNT);
        }

        public void Dispose()
        {
            mSocket.Close();
        }

        public void Run()
        {
            mSocket.Listen(MAX_USER_COUNT);

            while (mUsers.Count < MAX_USER_COUNT)
            {
                Socket userSocket = mSocket.Accept();

                byte[] userNameData = new byte[byte.MaxValue];
                userSocket.Receive(userNameData);

                string name = Utilities.DecodeData(userNameData);

                UserObject userObject;
                userObject.Name = name;
                userObject.Socket = userSocket;
                userObject.Buffer = new byte[MAX_STR_LENGTH];

                mUsers.Add(userObject);

                Task task = Task.Run(() => ReceiveChat(userObject));

                Console.WriteLine($"유저 \"{name}\" 연결됨.");
                mChatData.Enqueue($"{name} 님이 입장하셨습니다.");

                dispatchChat();
            }

            while (true)
            {

            }
        }

        private void ReceiveChat(UserObject userObject)
        {
            while (true)
            {
                Array.Clear(userObject.Buffer);
                userObject.Socket.Receive(userObject.Buffer);

                string chat = Utilities.DecodeData(userObject.Buffer);
                string sendingStr = $"{userObject.Name}> {chat}";

                enqueueChatData(sendingStr);
                dispatchChat();
            }
        }

        private void dispatchChat()
        {
            Monitor.Enter(mChatData);
            {
                while (mChatData.Count != 0)
                {
                    string chat = mChatData.Dequeue();
                    byte[] chatData = Encoding.UTF8.GetBytes(chat);

                    foreach (UserObject user in mUsers)
                    {
                        if (chat.Contains(user.Name))
                        {
                            continue;
                        }

                        user.Socket.Send(chatData);
                    }
                }
            }
            Monitor.Exit(mChatData);
        }

        private void enqueueChatData(string chatStr)
        {
            Monitor.Enter(mChatData);
            {
                mChatData.Enqueue(chatStr);
            }
            Monitor.Exit(mChatData);
        }
    }

    struct UserObject
    {
        public string Name;

        public Socket Socket;
        public byte[] Buffer;
    }
}
