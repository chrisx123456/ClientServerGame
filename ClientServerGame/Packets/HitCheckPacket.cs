using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientServerGame.Packets
{
    internal class HitCheckPacket
    {
        public int externalId;
        public int internalId;
        public int shipIndex;
    }
}
