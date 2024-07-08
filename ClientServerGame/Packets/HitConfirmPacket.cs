using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientServerGame.Packets
{
    internal class HitConfirmPacket
    {
        public bool isMe;
        public bool isHit;
        public bool toggle;
        public int index;
    }
}
