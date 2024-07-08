using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ClientServerGame.Packets
{
    static class TCPUtils
    {
        public static object ReadMessage(NetworkStream _stream)
        {
            StreamReader reader = new StreamReader(_stream);
            string json = reader.ReadLine();
            if (json == null) return null;
            return JsonUtils.Deserialize(json);
        }

        public static Task SendData<T>(T data, NetworkStream _stream)
        {
            return Task.Run(() => {
                string json = JsonUtils.Serialize(data);
                byte[] dataToSend = Encoding.UTF8.GetBytes(json + Environment.NewLine);
                _stream.Write(dataToSend, 0, dataToSend.Length);
            });
        }
    }
}
