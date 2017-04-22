using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Labyrithm
{
    class PlayerStat
    {
        string playerName;
        string playerPoints;

        public string PlayerName
        {
            get { return playerName; }
            set { playerName = value; }
        }

        public string PlayerPoints
        {
            get { return playerPoints; }
            set { playerPoints = value; }
        }
        public PlayerStat()
        {
        }

        public PlayerStat(string playerName, string playerPoints)
        {
            PlayerName = playerName;
            PlayerPoints = playerPoints;
        }
    }
}
