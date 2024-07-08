using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientServerGame.Packets
{
    internal class EndGamePacket
    {
        public EndGamePacket(bool win) { this.win = win; }
        public bool win;
    }
}
