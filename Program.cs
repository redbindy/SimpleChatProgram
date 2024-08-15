using System;
using System.Diagnostics;

namespace SimpleChatProgram
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                using (Server? s = Server.Instance)
                {
                    Debug.Assert(s != null);

                    s.Run();
                }
            }
            else
            {
                using (Client? c = Client.Instance)
                {
                    Debug.Assert(c != null);

                    Console.WriteLine("사용할 이름을 입력해주세요.");
                    Console.Write("> ");
                    string? name = Console.ReadLine();

                    Debug.Assert(name != null);
                    c.Connect("127.0.0.1", name);

                    c.Run();
                }
            }
        }
    }
}
