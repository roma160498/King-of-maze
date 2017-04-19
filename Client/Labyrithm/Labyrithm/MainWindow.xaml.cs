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
        const int GOODBYE = -1;
        const int FINISH = 0;
        const int CLIENTSENDPOS = 1;
        const int SERVERSENDPOS = 2;
        const int SERVERSENDLAB = 3;
        const int GAMEOVER = 4;
        const int CLIENTGETCOIN = 8;
        const int REMOVECOIN = 9;
        string currentPosition; //X_Y
        Server.Cell[,] Cells;
        TcpClient client = null;
        NetworkStream stream;
        private Int32 _Width = 15, _Height = 15;
        int curPositionY = 0;
        int curPositionX = 0;
        int coinPosX;
        int coinPosY;
        Ellipse coin;
        private delegate void UpdateLogCallback(Server.Cell[,] Cells);
        private delegate void UpdateLogEnemyPosition(string position);
        private delegate void UpdateLogClearCoin();

        private delegate void UpdateLogPause();

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
                            StrokeThickness = 1,
                            X1 = 20 * x,
                            Y1 = 20 * y,
                            X2 = 20 * x + 20,
                            Y2 = 20 * y
                        });

                    if (Cells[x, y].Left == Server.CellState.Close)
                        labyrithm.Children.Add(new Line()
                        {
                            Stroke = Brushes.Black,
                            StrokeThickness = 1,
                            X1 = 20 * x,
                            Y1 = 20 * y,
                            X2 = 20 * x,
                            Y2 = 20 * y + 20
                        });

                    if (Cells[x, y].Right == Server.CellState.Close)
                        labyrithm.Children.Add(new Line()
                        {
                            Stroke = Brushes.Black,
                            StrokeThickness = 1,
                            X1 = 20 * x + 20,
                            Y1 = 20 * y,
                            X2 = 20 * x + 20,
                            Y2 = 20 * y + 20
                        });

                    if (Cells[x, y].Bottom == Server.CellState.Close)
                        labyrithm.Children.Add(new Line()
                        {
                            Stroke = Brushes.Black,
                            StrokeThickness = 1,
                            X1 = 20 * x,
                            Y1 = 20 * y + 20,
                            X2 = 20 * x + 20,
                            Y2 = 20 * y + 20
                        });
                }
            labyrithm.Children.Add(new Rectangle()
            {
                Stroke = Brushes.Blue,
                Fill = Brushes.Blue,
                Width = 20,
                Height = 20,
            });
            labyrithm.Children.Add(new Rectangle()
            {
                Stroke = Brushes.Red,
                Fill = Brushes.Red,
                Width = 20,
                Height = 20
            });
            coin = new Ellipse()
            {
                Stroke = Brushes.Black,
                Fill = Brushes.Yellow,
                Width = 20,
                Height = 20,
                Margin = new Thickness(coinPosX * 20 , coinPosY * 20, 0, 0),
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
                            Figure.Margin = new Thickness(curPositionX * 20, curPositionY * 20, 0, 0);
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
                //   if (curPositionX != 14 ||  curPositionY != 14) { 
                {     
                    }
                if (e.Key == Key.Z)
                {
                    curPositionX = 14;
                    curPositionY = 14;
                }
                if (curPositionX == 14 && curPositionY == 14)
                {
                    curPositionX = 0;
                    curPositionY = 0;
                    byteMessage = BitConverter.GetBytes(FINISH);
                    stream.Write(byteMessage, 0, byteMessage.Length);
                    //Thread.Sleep(5000);
                    //stream.Flush();
                }
                else {
                    currentPosition = curPositionX.ToString(); ;
                    currentPosition += "_" + curPositionY.ToString();
                    byte[] position = Encoding.UTF8.GetBytes(currentPosition);
                 //  stream.Write(BitConverter.GetBytes(CLIENTSENDPOS), 0, BitConverter.GetBytes(CLIENTSENDPOS).Length);
                 //   stream.Write(position, 0, position.Length);
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

        private void Listen()
        {
            while (true)
            {
                try
                {
                    byte[] data = new byte[12]; // буфер для получаемых данных
                    stream.Read(data, 0, data.Length);
                    int state = BitConverter.ToInt32(data, 0);
                    
                    if (state == SERVERSENDLAB)//говнище кривое!!!
                    {
                        byte[] lab = new byte[65000];
                        stream.Read(lab, 0, lab.Length);
                        BinaryFormatter formatter = new BinaryFormatter();
                        using (System.IO.BinaryWriter file = new System.IO.BinaryWriter(File.Open("temp.dat", FileMode.OpenOrCreate)))
                        {
                            foreach (byte i in lab)
                                file.Write(i);
                        }
                        using (FileStream fs = new FileStream("temp.dat", FileMode.OpenOrCreate))
                        {
                            Cells = (Server.Cell[,])formatter.Deserialize(fs);
                            fs.Close();
                        }
                        //Thread.Sleep(2000);
                        curPositionX = 0;
                        curPositionY = 0;
                        byte[] size = new byte[10];
                        stream.Read(size, 0, size.Length);
                        string tempSTR = Encoding.UTF8.GetString(size);
                        string[] arrayofpos = tempSTR.Split('.');
                        coinPosX = Convert.ToInt32(arrayofpos[0]);
                        coinPosY = Convert.ToInt32(arrayofpos[1]);
                        this.labyrithm.Dispatcher.Invoke(new UpdateLogCallback(UpdateLog), new object[] { Cells });
                        System.Threading.Thread.Sleep(300);
                        stream.Flush();
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

                    }
                    if (state == REMOVECOIN)
                    {
                        //this.labyrithm.Dispatcher.Invoke(new UpdateLogChangePos(ChangePos), new object[] { });
                        this.labyrithm.Dispatcher.Invoke(new UpdateLogClearCoin(DeleteCoin), new object[] { });
                        coinPosX = 0;
                        coinPosY = 0;
                    }
                    if (state == 10)//говнище кривое!!!
                    {
                        this.labyrithm.Dispatcher.Invoke(new UpdateLogPause(Pusee), new object[] {  });
                    }

                    }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);//возможна ошибка при подключении

                }
            }
        }
        private void DeleteCoin()
        {
            labyrithm.Children.Remove(coin);
        }
        private void Pusee()
        {
            Thread.Sleep(5000);
        }
        private void UpdateLog(Server.Cell[,] Cells)
        {
            labyrithm.Children.Clear();
            renderCells();
          //  Thread.Sleep(5000);
        }

        private void EnemyPosition(string position)
        {
            try
            {
                string[] array = position.Split('_');
                int x = Convert.ToInt32(array[0]), y = Convert.ToInt32(array[1]);
                foreach (var item in labyrithm.Children)
                    if (item is Rectangle)
                    {
                        Rectangle Figure = item as Rectangle;
                        if (Figure.Fill == Brushes.Blue)
                            Figure.Margin = new Thickness(x * 20, y * 20, 0, 0);
                    }
            }
            catch { }
            /*int x=0, y=0;
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
                        temp += position[i];
                    else
                        if (temp != "")
                    {
                        y = Convert.ToInt32(temp);
                    }
                }
                if (i > 7) break;
            }*/
            
        }

        private void mainWindow_Closed(object sender, EventArgs e)
        {
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
                byte[] size = new byte[10];
                stream.Read(size, 0, size.Length);
                data = new byte[BitConverter.ToInt32(size, 0)];
                //data = new byte[65000]; // буфер для получаемых данных
                StringBuilder builder = new StringBuilder();
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
                    fs.Close();
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

