using NetMessage;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Network
{
    public class MessageHandler
    {
        public MessageHandler()
        {

        }
        public static byte[] Package(Message message)
        {
            byte[] msg = null;
            using (MemoryStream ms = new MemoryStream())
            {
                ProtoBuf.Serializer.Serialize(ms, message);
                msg = new byte[ms.Length + 4];

                Buffer.BlockCopy(BitConverter.GetBytes(ms.Length), 0, msg, 0, 4);
                Buffer.BlockCopy(ms.ToArray(), 0, msg, 4,(int)ms.Length);
            }

            return msg;
        }

        public static Message DeserializePackage(byte[] data, bool isCut = false)
        {
            if (data == null || data.Length <= 0) { return null; }
            byte[] messageBytes = null;

            if(isCut)
            {
                messageBytes = new byte[data.Length];
                Array.Copy(data, 0, messageBytes, 0, data.Length);
            }
            else
            {
                byte[] lengthBytes = new byte[4];
                Array.Copy(data, lengthBytes, 4);

                int length = BitConverter.ToInt32(lengthBytes, 0);

                messageBytes = new byte[data.Length - 4];

                Array.Copy(data, 4, messageBytes,0 ,data.Length-4);
            }
            using MemoryStream ms = new MemoryStream(messageBytes);
            return Serializer.Deserialize<Message>(ms);

        }
    }
}
