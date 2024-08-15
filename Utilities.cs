using System.Text;

namespace SimpleChatProgram
{
    public static class Utilities
    {
        public static string DecodeData(byte[] data)
        {
            string chatStr = Encoding.UTF8.GetString(data);
            chatStr = chatStr.Trim().TrimEnd('\0');

            return chatStr;
        }
    }
}
