using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
namespace Server
{
    class ClientTreatment
    {
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

        protected internal TcpClient Socket { get; set; }
        ServerListening server;
        protected internal NetworkStream Stream { get; private set; }
        protected internal string ClientName { get; private set; }
        //TcpClient socket 
        protected internal int gamePoints { get; private set; }
        //private int gamePoints = 0;
        public int level = 1;

        public ClientTreatment(TcpClient clientSocket, ServerListening serverClass )
        {
            Socket = clientSocket;
            server = serverClass;
            this.gamePoints = 0;
        }

        public void Process(object array)
        {
            try
            {
                byte[] byteMessage = new byte[64];
                Stream = Socket.GetStream();
                Stream.Read(byteMessage,0,byteMessage.Length);
                ClientName = Encoding.UTF8.GetString(byteMessage);
                BinaryFormatter formatter = new BinaryFormatter();
                using (FileStream fs = new FileStream("people.dat", FileMode.OpenOrCreate))
                {
                    formatter.Serialize(fs, (Cell[,])array);
                    Console.WriteLine("Объект сериализован");
                    fs.Close();
                }
                byteMessage = System.IO.File.ReadAllBytes("people.dat");
                int sizeInt = byteMessage.Length;
                byte[] size = BitConverter.GetBytes(sizeInt);
                Stream.Write(size, 0, size.Length);
                Stream.Write(byteMessage, 0, byteMessage.Length);
                string coinPos = server.posX.ToString() + '.' + server.posY.ToString();
                size = Encoding.UTF8.GetBytes(coinPos);
                Stream.Write(size, 0, size.Length);
                while (true)
                {
                    try
                    {
                        byte[] data = new byte[8];
                        Stream.Read(data, 0, data.Length);
                        string temp = Encoding.UTF8.GetString(data);
                        int state = BitConverter.ToInt32(data, 0);
                        //int points = bi
                        // int sizeint = 0;
                        //  if (temp.Substring(0, 7) == "THE_END")
                        if (state == FINISH)
                        {

                            server.Generation();
                            using (FileStream fs = new FileStream("people.dat", FileMode.OpenOrCreate))
                            {
                                formatter.Serialize(fs, server.Cells);
                                Console.WriteLine("Объект сериализован");
                                fs.Close();
                            }
                            byteMessage = System.IO.File.ReadAllBytes("people.dat");
                            //sizeInt = byteMessage.Length;
                            size = BitConverter.GetBytes(SERVERSENDLAB);
                            // System.Threading.Thread.Sleep(1000); убрал сейчас
                            server.SendLabyrinth(size);
                            server.SendLabyrinth(byteMessage);
                            this.gamePoints += 10;
                            /*     Random generateCoinPos = new Random();
                                 server.posX = generateCoinPos.Next(0, 14);
                                 server.posY = generateCoinPos.Next(0, 14);*/
                            coinPos = server.posX.ToString() + '.' + server.posY.ToString();
                            size = Encoding.UTF8.GetBytes(coinPos);
                            server.SendLabyrinth(size);
                            //Stream.Write(size, 0, size.Length);
                            if (server.Lvevel == 5)
                            {
                                data = BitConverter.GetBytes(GAMEOVER);
                                server.SendLabyrinth(data);
                                server.SendPoints();
                            }
                            else
                                server.Lvevel++;

                            //}
                            //else
                            //{
                            //    size = BitConverter.GetBytes(GAMEOVER);
                            //    // System.Threading.Thread.Sleep(1000); убрал сейчас
                            //    server.SendPosition(this,size);

                            //    //size = BitConverter.GetBytes(server.amountOfPlayers);
                            //    //server.SendLabyrinth(size);
                            //    //size = BitConverter.GetBytes(gamePoints);
                            //    //server.se
                            //   // size = BitConverter.GetBytes(poi);
                            //   // server.SendLabyrinth();
                            //    //server.SendLabyrinth(byteMessage);
                            //}
                            //System.Threading.Thread.Sleep(1000);
                            //  flag = false;
                        }

                        if (state == CLIENTSENDPOS)
                        {
                            //    if (flag)
                            //   {
                            byte[] data2 = new byte[64];
                            Stream.Read(data2, 0, data2.Length);
                            sizeInt = data2.Length;
                            size = BitConverter.GetBytes(SERVERSENDPOS);
                            //   string tmp = Encoding.UTF8.GetString(data);
                            // byteMessage = Encoding.UTF8.GetBytes(temp);
                            server.SendPosition(this, size);
                            server.SendPosition(this, data2);
                            Stream.Flush();
                            //   }
                            //     else flag = true;
                        }
                        if (state == WINNERSENDPOINTS)
                        {
                            int tate = BitConverter.ToInt32(data, 4);
                            byte[] aaa = BitConverter.GetBytes(tate);
                            //Stream.Read(data, 0, data.Length);
                            size = BitConverter.GetBytes(SERVERSENDPOINTS);
                            byte[] res = new byte[8];
                            for (int i = 0; i < 4; i++)
                                res[i] = size[i];
                            for (int i = 0; i < 4; i++)
                                res[i + 4] = aaa[i];
                            server.SendPosition(this, res);
                            //Stream.Flush();
                            //System.Threading.Thread.Sleep(1000);
                            //server.SendPosition(this, data);

                        }
                        //if (state == GOODBYE)
                        //    server.CloseConnection(this);
                        if (state == GAMEOVER)
                        {
                            this.gamePoints += 10;
                            server.Lvevel++;
                            data = BitConverter.GetBytes(GAMEOVER);
                            server.SendLabyrinth(data);
                            server.SendPoints();
                        }
                        if (state == CLIENTSENDPOINTS)
                        {
                            Stream.Read(data, 0, data.Length);
                            size = BitConverter.GetBytes(SERVERSENDPOS);
                            // server.SendLabyrinth(size);
                            //server.SendLabyrinth(data);

                            server.SendPosition(this, size);
                            server.SendPosition(this, data);
                        }
                        if (state == CLIENTGETCOIN)
                        {
                            data = BitConverter.GetBytes(REMOVECOIN);
                            server.SendPosition(this, data);
                            this.gamePoints += 20;
                        }
                    }
                    catch
                    {
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private string ParseTheString(string inputString)
        {
            foreach (char i in inputString)
                if (i == '\0')
                    inputString.Remove(i);
            return inputString;
        }
    }
    }
