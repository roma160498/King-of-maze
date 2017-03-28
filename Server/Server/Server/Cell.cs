using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;


namespace Server
{
    public enum CellState { Close, Open };
    [Serializable]
    public class Cell
    {
        public Cell(Point currentPosition)
        {
            Visited = false;
            Position = currentPosition;
        }

        public CellState Left { get; set; }
        public CellState Right { get; set; }
        public CellState Bottom { get; set; }
        public CellState Top { get; set; }
        public Boolean Visited { get; set; }
        public Point Position { get; set; }
    }
}
