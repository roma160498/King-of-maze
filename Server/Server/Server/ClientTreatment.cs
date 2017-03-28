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
        static TcpClient client;
        ServerListening server;
        protected internal NetworkStream Stream { get; private set; }

        public ClientTreatment(TcpClient clientSocket, ServerListening serverClass)
        {
            client = clientSocket;
            server = serverClass;
        }

        public void Process(object array)
        {
            try
            {
                byte[] byteMessage = new byte[64];
                Stream = client.GetStream();
                Stream.Read(byteMessage,0,byteMessage.Length);
                Console.WriteLine(ParseTheString(Encoding.UTF8.GetString(byteMessage)));
                BinaryFormatter formatter = new BinaryFormatter();
                using (FileStream fs = new FileStream("people.dat", FileMode.OpenOrCreate))
                {
                    formatter.Serialize(fs, (Cell[,])array);
                    Console.WriteLine("Объект сериализован");
                    fs.Close();
                }
                byteMessage = System.IO.File.ReadAllBytes("people.dat");
                Stream.Write(byteMessage, 0, byteMessage.Length);
              //  bool flag = true;
                while (true)
                {
                    try
                    {
                       //
                        byte[] data= new byte[64];
                        Stream.Read(data,0,data.Length);
                        string temp = Encoding.UTF8.GetString(data);
                        if (temp.Substring(0, 7) == "THE_END")
                        {
                            server.Generation();
                            using (FileStream fs = new FileStream("people.dat", FileMode.OpenOrCreate))
                            {
                                formatter.Serialize(fs, server.Cells);
                                Console.WriteLine("Объект сериализован");
                                fs.Close();
                            }
                            byteMessage = System.IO.File.ReadAllBytes("people.dat");
                           // System.Threading.Thread.Sleep(1000); убрал сейчас
                            server.SendMessage_A(this, byteMessage);
                            //System.Threading.Thread.Sleep(1000);
                          //  flag = false;
                        }
                        else
                        {
                        //    if (flag)
                         //   {
                                byteMessage = Encoding.UTF8.GetBytes(temp);
                                server.SendMessage(this, byteMessage);
                                Stream.Flush();
                         //   }
                       //     else flag = true;
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
