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
        const int CLIENTGETCOIN = 8;
        const int REMOVECOIN = 9;
        static TcpClient client;
        ServerListening server;
        protected internal NetworkStream Stream { get; private set; }
        protected internal TcpClient Socket { get; set; }
        protected internal int gamePoints { get; private set; }
        protected internal string ClientName { get; private set; }

        public ClientTreatment(TcpClient clientSocket, ServerListening serverClass)
        {
            client = clientSocket;
            server = serverClass;
            this.gamePoints = 0;
        }

        public void Process(object array)
        {
            try
            {
                byte[] byteMessage = new byte[64];
                Stream = client.GetStream();
                Stream.Read(byteMessage,0,byteMessage.Length);

                ClientName = Encoding.UTF8.GetString(byteMessage);
                Console.WriteLine(ParseTheString(Encoding.UTF8.GetString(byteMessage)));
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
                Console.WriteLine("OTPRAVLENO");
                //Stream.Write(size, 0, size.Length);
                
              //  bool flag = true;
                while (true)
                {
                    try
                    {
                       //
                        byte[] data= new byte[8];
                        if (server.Level == 6)
                        {
                            data = BitConverter.GetBytes(GAMEOVER);
                            server.SendLabyrinth(data);
                            server.SendPoints();
                        }
                        Stream.Read(data,0,data.Length);
                        string temp = Encoding.UTF8.GetString(data);
                        int state = BitConverter.ToInt32(data, 0);
                        if (state == FINISH)
                        {
                           // size = BitConverter.GetBytes(10);
                           // server.SendLabyrinth(size);
                            server.Generation();
                            using (FileStream fs = new FileStream("people.dat", FileMode.OpenOrCreate))
                            {
                                formatter.Serialize(fs, server.Cells);
                                Console.WriteLine("Объект сериализован");
                                fs.Close();
                            }
                            byteMessage = System.IO.File.ReadAllBytes("people.dat");
                            // System.Threading.Thread.Sleep(1000); убрал сейчас
                            
                            size = BitConverter.GetBytes(SERVERSENDLAB);
                            server.SendLabyrinth(size);
                            server.SendLabyrinth(byteMessage);
                            this.gamePoints += 10;
                            server.Level++;
                            coinPos = server.posX.ToString() + '.' + server.posY.ToString();
                            size = Encoding.UTF8.GetBytes(coinPos);
                            server.SendLabyrinth(size);
                            // server.SendMessage_A(this, byteMessage);
                            System.Threading.Thread.Sleep(1000);
                          //  flag = false;
                        }
                        if (state == CLIENTSENDPOS)
                        {
                            byte[] data2 = new byte[64];
                            Stream.Read(data2, 0, data2.Length);
                            sizeInt = data2.Length;
                            size = BitConverter.GetBytes(SERVERSENDPOS);
                            //   string tmp = Encoding.UTF8.GetString(data);
                            // byteMessage = Encoding.UTF8.GetBytes(temp);
                            server.SendPosition(this, size);
                            server.SendPosition(this, data2);
                            //Stream.Flush();
                            //    if (flag)
                            //   {
                            /* byteMessage = Encoding.UTF8.GetBytes(temp);
                                 server.SendMessage(this, byteMessage);
                                 Stream.Flush();*/
                            //   }
                            //     else flag = true;
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
