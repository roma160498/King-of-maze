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
        string currentPosition; //X_Y
        Server.Cell[,] Cells;
        TcpClient client = null;
        NetworkStream stream;
        private Int32 _Width = 15, _Height = 15;
        int curPositionY = 0;
        int curPositionX = 0;

        private delegate void UpdateLogCallback(Server.Cell[,] Cells);
        private delegate void UpdateLogEnemyPosition(string position);

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
                if (e.Key == Key.Up || e.Key == Key.Down || e.Key == Key.Left|| e.Key == Key.Right)
                    if (curPositionX != 14 ||  curPositionY != 14) { 
                        currentPosition = curPositionX.ToString(); ;
                        currentPosition += "_" + curPositionY.ToString();
                        byte[] position = Encoding.UTF8.GetBytes(currentPosition);
                        stream.Write(position, 0, position.Length);
                    }   
                if (curPositionX == 14 && curPositionY == 14)
                {
                    curPositionX = 0;
                    curPositionY = 0;
                    byte[] byteMessage = Encoding.UTF8.GetBytes(FINISHING);
                    stream.Write(byteMessage, 0, byteMessage.Length);
                    stream.Flush();
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
                    byte[] data = new byte[65000]; // буфер для получаемых данных
                    stream.Read(data, 0, data.Length);
                    if (data[1] != 95 && data[2]!=95)//говнище кривое!!!
                    {
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
                        Thread.Sleep(2000);
                        this.labyrithm.Dispatcher.Invoke(new UpdateLogCallback(UpdateLog), new object[] { Cells });
                    }else

                    this.labyrithm.Dispatcher.Invoke(new UpdateLogEnemyPosition(EnemyPosition), new object[] { Encoding.UTF8.GetString(data) });

                }
                catch
                {
                    
                }
            }
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
                        temp += position[i];
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
                        Figure.Margin = new Thickness(x * 20, y * 20, 0, 0);
                }
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
                data = new byte[65000]; // буфер для получаемых данных
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
                }
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

