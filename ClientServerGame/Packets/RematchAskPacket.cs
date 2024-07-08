using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientServerGame.Packets
{
    internal class RematchAskPacket
    {
        public int internalId;
        public int externalId;
        public bool rematch;
    }
}
