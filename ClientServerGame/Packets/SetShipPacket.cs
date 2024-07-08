using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientServerGame.Packets
{
    internal class SetShipPacket
    {
        public int externalID;
        public int internalID;
        public int shipIndex;
    }
}
