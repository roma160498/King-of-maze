using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Threading;

namespace Labyrithm
{
    public partial class MainWindow : Window
    {
        const string FINISHING = "THE_END";
        //STATES
        const int GOODBYE = -1;
        const int FINISH = 0;
        const int CLIENTSENDPOS = 1;
        const int SERVERSENDPOS = 2;
        const int SERVERSENDLAB = 3;
        const int GAMEOVER = 4;
        const int CLIENTSENDPOINTS = 5;
        const int SERVERSENDPOINTS = 6;
        const int WINNERSENDPOINTS = 7;
        const int CLIENTGETCOIN = 8;
        const int REMOVECOIN = 9;



        string currentPosition; //X_Y
        Server.Cell[,] Cells;
        TcpClient client = null;
        NetworkStream stream;
        private Int32 _Width = 15, _Height = 15;
        int curPositionY = 0;
        int curPositionX = 0;
        int gamePoint = 0;
        int enemyres ;
       // int level = 1;
        string points = "";
        bool winner = false;
        int coinPosX;
        int coinPosY;
        Ellipse coin;


        private delegate void UpdateLogCallback(Server.Cell[,] Cells);
        private delegate void UpdateLogEnemyPosition(string position);
        private delegate void UpdateLogClearCoin();
        private delegate void UpdateLogChangePos();
        private delegate void UpdateLogChangeLevel(int level);

        public MainWindow()
        {
            InitializeComponent();
        }
               
        private void renderCells()
        {
            
            for (int y = 0; y < _Height; y++)
                for (int x = 0; x < _Width; x++)
                {
                    if (Cells[x, y].Top == Server.CellState.Close)
                        labyrithm.Children.Add(new Line()
                        {
                            Stroke = Brushes.Black,
                            StrokeThickness = 4,
                            
                            X1 = 30 * x-2,
                            Y1 = 30 * y,
                            X2 = 30 * x + 30+2,
                            Y2 = 30 * y
                        });

                    if (Cells[x, y].Left == Server.CellState.Close)
                        labyrithm.Children.Add(new Line()
                        {
                            Stroke = Brushes.Black,
                            StrokeThickness = 4,
                            X1 = 30 * x,
                            Y1 = 30 * y-2,
                            X2 = 30 * x,
                            Y2 = 30 * y + 30+2
                        });

                    if (Cells[x, y].Right == Server.CellState.Close)
                        labyrithm.Children.Add(new Line()
                        {
                            Stroke = Brushes.Black,
                            StrokeThickness = 4,
                            X1 = 30 * x + 30,
                            Y1 = 30 * y-2,
                            X2 = 30 * x + 30,
                            Y2 = 30 * y + 30+2
                        });

                    if (Cells[x, y].Bottom == Server.CellState.Close)
                        labyrithm.Children.Add(new Line()
                        {
                            Stroke = Brushes.Black,
                            StrokeThickness = 4,
                            X1 = 30 * x-2,
                            Y1 = 30 * y + 30,
                            X2 = 30 * x + 30+2,
                            Y2 = 30 * y + 30
                        });
                }
            labyrithm.Children.Add(new Rectangle()
            {
                Stroke = Brushes.Blue,
                Fill = Brushes.Blue,
                Width = 24,
                Height = 24,
                Margin = new Thickness(3,3,0,0)
            });
            labyrithm.Children.Add(new Rectangle()
            {
                Stroke = Brushes.Red,
                Fill = Brushes.Red,
                Width = 24,
                Height = 24,
                Margin = new Thickness(3, 3, 0, 0)
            });

            coin = new Ellipse()
            {
                Stroke = Brushes.Black,
                Fill = Brushes.Yellow,
                Width = 24,
                Height = 24,
                Margin = new Thickness(coinPosX * 30 + 3, coinPosY * 30 + 3, 0, 0),
            };
            labyrithm.Children.Add(coin);
        }

        private void mainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (submitting.Visibility == Visibility.Hidden)
            {
                if (e.Key == Key.Up)
                    if (curPositionY > 0)
                        if (Cells[curPositionX, curPositionY].Top == Server.CellState.Open)
                            curPositionY--;
                if (e.Key == Key.Down)
                    if (curPositionY < 15)
                        if (Cells[curPositionX, curPositionY].Bottom == Server.CellState.Open)
                            curPositionY++;
                if (e.Key == Key.Left)
                    if (curPositionX > 0)
                        if (Cells[curPositionX, curPositionY].Left == Server.CellState.Open)
                            curPositionX--;
                if (e.Key == Key.Right)
                    if (curPositionX < 15)
                        if (Cells[curPositionX, curPositionY].Right == Server.CellState.Open)
                            curPositionX++;
                foreach (var item in labyrithm.Children)
                    if (item is Rectangle)
                    {
                        Rectangle Figure = item as Rectangle;
                        if (Figure.Fill == Brushes.Red)
                            Figure.Margin = new Thickness(curPositionX * 30 + 3, curPositionY * 30 + 3 , 0, 0);
                    }
                byte[] byteMessage;
                if (curPositionX == coinPosX && curPositionY == coinPosY)
                {
                    labyrithm.Children.Remove(coin);
                    coinPosX = 0;
                    coinPosY = 0;
                    byteMessage = BitConverter.GetBytes(CLIENTGETCOIN);
                    stream.Write(byteMessage, 0, byteMessage.Length);
                    return;
                }
                if (e.Key == Key.Up || e.Key == Key.Down || e.Key == Key.Left|| e.Key == Key.Right)
                 //   if (curPositionX != 14 ||  curPositionY != 14)
                    {
               //     if (curPositionX != coinPosX && curPositionY != coinPosY)
                //    {
                        currentPosition = curPositionX.ToString(); ;
                        currentPosition += "_" + curPositionY.ToString();
                        byte[] position = Encoding.UTF8.GetBytes(currentPosition);
                        stream.Write(BitConverter.GetBytes(CLIENTSENDPOS), 0, BitConverter.GetBytes(CLIENTSENDPOS).Length);
                        stream.Write(position, 0, position.Length);
                //    }
                    }   
                if (e.Key == Key.Z)
                { curPositionX = 14;
                    curPositionY = 14;
                }
               
                if (curPositionX == 14 && curPositionY == 14)
                {
                    gamePoint += 10;
                    labyrithm.Children.Remove(coin);
                    curPositionX = 0;
                    curPositionY = 0;
                   // level++;
                    
                   // if (level == 7)
                 //   {
                  //      byteMessage = BitConverter.GetBytes(GAMEOVER);
                        //    Encoding.UTF8.GetBytes(FINISH);
                 //       stream.Write(byteMessage, 0, byteMessage.Length);
                //        winner = true;
                        //    byteMessage = BitConverter.GetBytes(WINNERSENDPOINTS);
                        //    byte[] res = new byte[8];
                        //    for (int i = 0; i < 4; i++)
                        //        res[i] = byteMessage[i];
                        //    byteMessage = BitConverter.GetBytes(gamePoint);
                        //    for (int i = 0; i < 4; i++)
                        //        res[i + 4] = byteMessage[i];
                        //    //stream.Write(byteMessage, 0, byteMessage.Length);
                        //    //byteMessage = BitConverter.GetBytes(gamePoint);
                        //    stream.Write(res, 0, res.Length);
                 //   }
                 //   else
                //    {
                        byteMessage = BitConverter.GetBytes(FINISH);
                        //    Encoding.UTF8.GetBytes(FINISH);
                        stream.Write(byteMessage, 0, byteMessage.Length);
                //    }
                    
                   // stream.Flush();
                }
            }
        }

        private void submitting_Click(object sender, RoutedEventArgs e)
        {
            string nickname = nickName.Text;
            string ipaddress = IPaddress.Text;
            ConnectToServer(nickname,ipaddress);
            Thread threadForReceive = new Thread(new ThreadStart(Listen));
            threadForReceive.Start();
        }

        private string ParseResult(string inputResult)
        {
           // int err=0;
            string[] arrayOfResults = inputResult.Split('.');
            //if (Int32.TryParse(arrayOfResults[0],out err)==Int32.TryParse(arrayOfResults[1],out err))
            
            if (Convert.ToInt32(arrayOfResults[0]) > Convert.ToInt32(arrayOfResults[1]))
            {
                if (winner)
                    return $"You WIN. Your points: {arrayOfResults[0]}. Your's enemy points: {arrayOfResults[1]}";
                else
                    return $"You LOSE. Your points: {arrayOfResults[1]}. Your's enemy points: {arrayOfResults[0]}";
            }
            if (Convert.ToInt32(arrayOfResults[0]) < Convert.ToInt32(arrayOfResults[1]))
            {
                if (winner)
                    return $"You WIN. Your points: {arrayOfResults[1]}. Your's enemy points: {arrayOfResults[0]}";
                else
                    return $"You LOSE. Your points: {arrayOfResults[0]}. Your's enemy points: {arrayOfResults[1]}";
            }
            return "";
        }

        private void Listen()
        {
            int level = 1;
            while (true)
            {
                try
                {
                    byte[] data = new byte[12]; // буфер для получаемых данных
                    stream.Read(data, 0, data.Length);
                    int state = BitConverter.ToInt32(data, 0);
                    if (state == SERVERSENDLAB)
                    {
                        byte[] lab = new byte[65000];
                        stream.Read(lab, 0, lab.Length);
                        //  if (data[1] != 95 && data[2] != 95)//говнище кривое!!!
                        // {
                        BinaryFormatter formatter = new BinaryFormatter();
                        using (System.IO.BinaryWriter file = new System.IO.BinaryWriter(File.Open("temp.dat", FileMode.OpenOrCreate)))
                        {
                            foreach (byte i in lab)
                                file.Write(i);
                        }
                        using (FileStream fs = new FileStream("temp.dat", FileMode.OpenOrCreate))
                        {
                            Cells = (Server.Cell[,])formatter.Deserialize(fs);
                        }
                        // Thread.Sleep(2000);
                        curPositionX = 0;
                        curPositionY = 0;
                        byte[] size = new byte[10];
                        stream.Read(size, 0, size.Length);
                        string tempSTR = Encoding.UTF8.GetString(size);
                        string[] arrayofpos = tempSTR.Split('.');
                        coinPosX = Convert.ToInt32(arrayofpos[0]);
                        coinPosY = Convert.ToInt32(arrayofpos[1]);
                        level++;
                        this.labyrithm.Dispatcher.Invoke(new UpdateLogCallback(UpdateLog), new object[] { Cells });
                        this.labyrithm.Dispatcher.Invoke(new UpdateLogChangeLevel(ChanheLevel), new object[] { level });


                    }
                    if (state == SERVERSENDPOS)
                    {
                        byte[] pos = new byte[5];
                        stream.Read(pos, 0, pos.Length);
                        this.labyrithm.Dispatcher.Invoke(new UpdateLogEnemyPosition(EnemyPosition), new object[] { Encoding.UTF8.GetString(pos) });
                    }

                    if (state == GAMEOVER)
                    {
                        byte[] posa = new byte[45];
                        stream.Read(posa, 0, posa.Length);
                        string aaa = Encoding.UTF8.GetString(posa);
                        MessageBox.Show(aaa, "rtkgnrtng");
                        //ПАУЗА (возможно заблокировать) клавиатуру
                        //byte[] po = BitConverter.GetBytes(CLIENTSENDPOINTS);
                        //stream.Write(po, 0, po.Length);
                        //po = BitConverter.GetBytes(gamePoint);
                        //stream.Write(po, 0, po.Length);
                    }
                    if (state == SERVERSENDPOINTS)
                    {
                        //byte[] data1 = new byte[5];
                        //stream.Read(data1, 0, data1.Length);
                        points = Encoding.UTF8.GetString(data, 4, 6);
                        //enemyres = BitConverter.ToInt32(data,4);
                        MessageBox.Show(ParseResult(points), "rtkgnrtng");
                    }
                    if (state == REMOVECOIN)
                    {
                        this.labyrithm.Dispatcher.Invoke(new UpdateLogChangePos(ChangePos), new object[] { });
                        this.labyrithm.Dispatcher.Invoke(new UpdateLogClearCoin(DeleteCoin), new object[] { });
                        coinPosX = 0;
                        coinPosY = 0;
                    }
                }
                catch
                {

                }
            }
        }
        private void ChangePos()
        {
            foreach (var item in labyrithm.Children)
                if (item is Rectangle)
                {
                    Rectangle Figure = item as Rectangle;
                    if (Figure.Fill == Brushes.Blue)
                        Figure.Margin = new Thickness(coinPosX * 30 + 3, coinPosY * 30 + 3, 0, 0);
                }
        }
        private void ChanheLevel(int level)
        {
            label.Content = level.ToString();

        }



        private void DeleteCoin()
        {
            labyrithm.Children.Remove(coin);
        }

        private void UpdateLog(Server.Cell[,] Cells)
        {
            labyrithm.Children.Clear();
            renderCells();
        }
        
        private void EnemyPosition(string position)
        {
            int x=0, y=0;
            string temp="";
            bool flag = false;
            for (int i = 0; i < position.Length; i++)
            {
                if (!flag)
                if (position[i] != '_' )
                    temp += position[i];
                else
                {
                    x = Convert.ToInt32(temp);
                    temp = "";
                    flag = true;
                }
                if (flag)
                {
                    if (position[i] != '\0' && position[i] != '_')
                    {
                        temp += position[i];
                        y = Convert.ToInt32(temp);
                    }
                    else
                        if (temp != "")
                    {
                        y = Convert.ToInt32(temp);
                    }
                }
                if (i > 7) break;
            }
            foreach (var item in labyrithm.Children)
                if (item is Rectangle)
                {
                    Rectangle Figure = item as Rectangle;
                    if (Figure.Fill == Brushes.Blue)
                        Figure.Margin = new Thickness(x * 30 +3, y * 30+3, 0, 0);
                }
        }

        private void mainWindow_Closed(object sender, EventArgs e)
        {
            byte[] data = BitConverter.GetBytes(GOODBYE);
            stream.Write(data, 0, data.Length);
            Environment.Exit(0);
        }

        private void ConnectToServer(string nickname,string ipaddress)
        {      
            try
            {
                client = new TcpClient(ipaddress, 2200);
                stream = client.GetStream();

                byte[] data = Encoding.UTF8.GetBytes(nickname);
                
                stream.Write(data, 0, data.Length);
                // data = new byte[65000]; // буфер для получаемых данных
                byte[] size = new byte[10];
                stream.Read(size,0,size.Length);
                data = new byte[BitConverter.ToInt32(size,0)];
                stream.Read(data, 0, data.Length);
                BinaryFormatter formatter = new BinaryFormatter();
                using (System.IO.BinaryWriter file = new System.IO.BinaryWriter(File.Open("temp.dat", FileMode.OpenOrCreate)))
                {
                    foreach (byte i in data)
                        file.Write(i);
                }
                using (FileStream fs = new FileStream("temp.dat", FileMode.OpenOrCreate))
                {
                    Cells = (Server.Cell[,])formatter.Deserialize(fs);
                }

                stream.Read(size, 0, size.Length);
                string tempSTR = Encoding.UTF8.GetString(size);
                string[] arrayofpos = tempSTR.Split('.');
                coinPosX = Convert.ToInt32(arrayofpos[0]);
                coinPosY = Convert.ToInt32(arrayofpos[1]);
                renderCells();
                nickName.Visibility = Visibility.Hidden;
                IPaddress.Visibility = Visibility.Hidden;
                submitting.Visibility = Visibility.Hidden;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);//возможна ошибка при подключении
            }
        }
    }

}

