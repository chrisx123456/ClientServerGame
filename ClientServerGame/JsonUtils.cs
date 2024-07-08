using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
namespace ClientServerGame.Packets
{
    static class JsonUtils
    {
        public static string Serialize(object data) 
        {
            string prefix = data.GetType().Name + ":";
            string serialized = JsonConvert.SerializeObject(data);
            return prefix + serialized;
        }
        public static object Deserialize(string data)
        {
            string[] split = data.Split(':',2);
            string type = split[0];
            string jsonData = split[1];
            switch (type)
            {
                case nameof(InitPacket):
                    return JsonConvert.DeserializeObject<InitPacket>(jsonData);
                case nameof(ClientClosedPacket):
                    return JsonConvert.DeserializeObject<ClientClosedPacket>(jsonData);
                case nameof(ClientCloseRequestPacket):
                    return JsonConvert.DeserializeObject<ClientCloseRequestPacket>(jsonData);
                case nameof(PortPacket):
                    return JsonConvert.DeserializeObject<PortPacket>(jsonData);
                case nameof(PrepStartPacket):
                    return JsonConvert.DeserializeObject<PrepStartPacket>(jsonData);
                case nameof(PrepStopPacket):
                    return JsonConvert.DeserializeObject<PrepStopPacket>(jsonData);
                case nameof(SetShipPacket):
                    return JsonConvert.DeserializeObject<SetShipPacket>(jsonData);
                case nameof(GameStartPacket):
                    return JsonConvert.DeserializeObject<GameStartPacket>(jsonData);
                case nameof(HitCheckPacket):
                    return JsonConvert.DeserializeObject<HitCheckPacket>(jsonData);
                case nameof(HitConfirmPacket):
                    return JsonConvert.DeserializeObject<HitConfirmPacket>(jsonData);
                case nameof(EndGamePacket):
                    return JsonConvert.DeserializeObject<EndGamePacket>(jsonData);
                case nameof(RematchAskPacket):
                    return JsonConvert.DeserializeObject<RematchAskPacket>(jsonData);
                case nameof(RematchResultPacket):
                    return JsonConvert.DeserializeObject<RematchResultPacket>(jsonData);
                default:
                    throw new Exception("Invalid Data type");
            }
        }
    }
}
