using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientServerGame
{
    internal class GameData
    {
        public byte[,] board = new byte[2,64];
        public int ShipsNumber0;
        public int ShipsNumber1;
        public bool isBothSet= false;
        public bool? Rematch0 = null;
        public bool? Rematch1 = null;
        public GameData(int sn) { 
            this.ShipsNumber0 = sn;
            this.ShipsNumber1 = sn;
        }
        public bool CheckForWin(int internalId)
        {
            for (int i = 0; i < 64; i++)
            {
                if (board[internalId, i] != 0) return false;
            }
            return true;
        }
        public bool CheckForHit(int internalId, int index)
        {
            if (board[internalId, index] == 1)
            {
                board[internalId, index] = 0;
                return true;
            } 
            else return false;
        }
        public bool SetShip(int internalId, int index)
        {
            if(internalId == 0)
            {
                ShipsNumber0--;
                board[0, index] = 1;
                if(ShipsNumber0 == 0 && ShipsNumber1 == 0) isBothSet = true;
                if (this.ShipsNumber0 == 0) return false;
                else return true;
            }
            else
            {
                ShipsNumber1--;
                board[1, index] = 1;
                if (ShipsNumber0 == 0 && ShipsNumber1 == 0) isBothSet = true;
                if (this.ShipsNumber1 == 0) return false;
                else return true;
            }
        }
    }

}
