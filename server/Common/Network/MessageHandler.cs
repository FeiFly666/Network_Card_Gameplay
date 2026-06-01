using NetMessage;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Network
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
        public static void ParseAndDispatch(ISession session, MemoryStream buffer)
        {
            int readOffset = 0;
            Parse(session,ref readOffset,buffer);
        }
        private static void Parse(ISession session, ref int readOffset, MemoryStream m_buffer)
        {
            readOffset = 0;
            byte[] bufferArray = m_buffer.GetBuffer();
            long currentPos = m_buffer.Position;
            while(readOffset + 4 <= currentPos)
            {
                int packageLength = BitConverter.ToInt32(m_buffer.GetBuffer(), readOffset);
                if(packageLength <= 0 || packageLength > 1024 * 1024 )
                {
                    m_buffer.Position = 0;
                    return;
                }
                if (packageLength + readOffset + 4 <= m_buffer.Position) // 包有效
                {
                    byte[] messageBytes = new byte[packageLength];
                    Array.ConstrainedCopy(m_buffer.GetBuffer(), readOffset + 4, messageBytes, 0, (int)packageLength);

                    Message message = MessageHandler.DeserializePackage(messageBytes, true);

                    if (message == null)
                    {
                        throw new Exception("invaild package");
                    }
                    MessageDispatcher.Instance.Dispatch(session, message);
                    readOffset += packageLength + 4;

                }
                else
                {
                    break;
                }
            }
            if (readOffset > 0)
            {
                int remaining = (int)currentPos - readOffset;
                if (remaining > 0)
                {
                    Array.Copy(bufferArray, readOffset, bufferArray, 0, remaining);
                }

                m_buffer.Position = remaining;
            }
          
        }
    }

}
