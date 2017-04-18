using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows;
using System.Runtime.Serialization.Formatters.Binary;

namespace Server
{
    class ServerListening
    {
        private Int32 _Width, _Height;
        public Cell[,] Cells;
        public static List<ClientTreatment> allClients= new List<ClientTreatment>();

        protected internal void StartListen()
        {
            try
            {
                this.Generation();
                string hostname = Dns.GetHostName();
                //работаю без интеренетаааа
                IPAddress addr = Dns.GetHostByName(hostname).AddressList[2];
               // IPAddress addr = IPAddress.Parse("127.0.0.1");
                TcpListener Listener = new TcpListener(addr, 2200);
                Listener.Start();
               // IPEndPoint endPoint = new IPEndPoint(addr, 5555);
              //  Listener = new Socket(addr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
             //   Listener.Bind(endPoint);
              //  Listener.Listen(2);
                Console.WriteLine("Сервер работает. IP: {0}", addr);
                while (true)
                {
                    TcpClient clientSocket = Listener.AcceptTcpClient();
                    //allClients.Add(clientSocket);
                    ClientTreatment client = new ClientTreatment(clientSocket, this);
                    allClients.Add(client);
                    Thread thread = new Thread(new ParameterizedThreadStart(client.Process));
                    thread.Start(Cells);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        protected internal void SendLabyrinth(byte[] msg)
        {
            for (int i = 0; i < allClients.Count; i++)
            {
                allClients[i].Stream.Write(msg, 0, msg.Length);
                Thread.Sleep(300);
            }
        }
        protected internal void SendPosition(ClientTreatment client, byte[] msg)
        {
            for (int i = 0; i < allClients.Count; i++)
            {
                if (client != allClients[i])
                    allClients[i].Stream.Write(msg, 0, msg.Length);
            }
        }
        protected internal void SendMessage(ClientTreatment client, byte[] msg)
        {
            for (int i = 0; i < allClients.Count; i++)
            {
                if (client != allClients[i])
                    allClients[i].Stream.Write(msg, 0, msg.Length);
            //    else
              //      allClients[i].Stream.Write(msg, 0, msg.Length);
               // Thread.Sleep(1500);
            }
        }
        protected internal void SendMessage_A(ClientTreatment client, byte[] msg)
        {
            for (int i = 0; i < allClients.Count; i++)
            {
                //if (client != allClients[i])
                allClients[i].Stream.Write(msg, 0, msg.Length);
                Thread.Sleep(100);
            }
        }

        //protected internal void closeSocket(Socket socket)
        //{
        //    for (int i = 0; i < allClients.Count; i++)
        //    {
        //        if (socket == allClients[i])
        //        {
        //            allClients[i].Shutdown(SocketShutdown.Both);
        //            allClients[i].Close();
        //            allClients.Remove(allClients[i]);
        //        }

        //    }
        //}
        protected internal void Generation()
        {
            _Width = 15;
            _Height = 15;
            Cells = new Cell[_Width, _Height];

            for (int y = 0; y < _Height; y++)
                for (int x = 0; x < _Width; x++)
                    Cells[x, y] = new Cell(new Point(x, y));

            Random rand = new Random();
            Int32 startX = rand.Next(_Width);
            Int32 startY = rand.Next(_Height);

            Stack<Cell> path = new Stack<Cell>();

            Cells[startX, startY].Visited = true;
            path.Push(Cells[startX, startY]);

            while (path.Count > 0)
            {
                Cell _cell = path.Peek();

                List<Cell> nextStep = new List<Cell>();
                if (_cell.Position.X > 0 && !Cells[Convert.ToInt32(_cell.Position.X - 1), Convert.ToInt32(_cell.Position.Y)].Visited)
                    nextStep.Add(Cells[Convert.ToInt32(_cell.Position.X) - 1, Convert.ToInt32(_cell.Position.Y)]);
                if (_cell.Position.X < _Width - 1 && !Cells[Convert.ToInt32(_cell.Position.X) + 1, Convert.ToInt32(_cell.Position.Y)].Visited)
                    nextStep.Add(Cells[Convert.ToInt32(_cell.Position.X) + 1, Convert.ToInt32(_cell.Position.Y)]);
                if (_cell.Position.Y > 0 && !Cells[Convert.ToInt32(_cell.Position.X), Convert.ToInt32(_cell.Position.Y) - 1].Visited)
                    nextStep.Add(Cells[Convert.ToInt32(_cell.Position.X), Convert.ToInt32(_cell.Position.Y) - 1]);
                if (_cell.Position.Y < _Height - 1 && !Cells[Convert.ToInt32(_cell.Position.X), Convert.ToInt32(_cell.Position.Y) + 1].Visited)
                    nextStep.Add(Cells[Convert.ToInt32(_cell.Position.X), Convert.ToInt32(_cell.Position.Y) + 1]);

                if (nextStep.Count() > 0)
                {
                    Cell next = nextStep[rand.Next(nextStep.Count())];

                    if (next.Position.X != _cell.Position.X)
                    {
                        if (_cell.Position.X - next.Position.X > 0)
                        {
                            _cell.Left = CellState.Open;
                            next.Right = CellState.Open;
                        }
                        else
                        {
                            _cell.Right = CellState.Open;
                            next.Left = CellState.Open;
                        }
                    }
                    if (next.Position.Y != _cell.Position.Y)
                    {
                        if (_cell.Position.Y - next.Position.Y > 0)
                        {
                            _cell.Top = CellState.Open;
                            next.Bottom = CellState.Open;
                        }
                        else
                        {
                            _cell.Bottom = CellState.Open;
                            next.Top = CellState.Open;
                        }
                    }

                    next.Visited = true;
                    path.Push(next);
                }
                else
                {
                    path.Pop();
                }
            }

            //  renderCells();
            //Enemy en = new Enemy(_Width, _Height,Cells,mCanvas);
            //en.ChangePosition(mCanvas);
            // chpos();
            //  act();
            //   System.Threading.Thread enemyThread = new System.Threading.Thread(new System.Threading.ThreadStart(act));
            //    enemyThread.Start();
            // en.Action(mCanvas, Cells);
        }
    }
    
}
