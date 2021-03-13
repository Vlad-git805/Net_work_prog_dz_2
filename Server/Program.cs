using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace Server
{
    class Program
    {
        static int port = 8080; // порт для приема входящих запросов

        public static List<Persons> persons = new List<Persons>();

        static void Main(string[] args)
        {

            Console.OutputEncoding = Encoding.UTF8;

            XmlDocument xdoc = new XmlDocument();

            xdoc.Load("https://api.privatbank.ua/p24api/pubinfo?exchange&coursid=11");


            XmlNodeList xNodelst = xdoc.DocumentElement.SelectNodes("//exchangerates/row");

            // получаем адреса для запуска сокета
            IPAddress iPAddress = IPAddress.Parse("127.0.0.1");//Dns.GetHostEntry("localhost").AddressList[1]; //localhost
            IPEndPoint ipPoint = new IPEndPoint(iPAddress, port);

            // создаем сокет
            Socket listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            Semaphore semaphore = new Semaphore(2, 2);
            try
            {
                // связываем сокет с локальной точкой, по которой будем принимать данные
                listenSocket.Bind(ipPoint);

                // начинаем прослушивание
                listenSocket.Listen(4);

                Console.WriteLine("Server started! Waiting for connection...");

                while (true)
                {
                    Socket handler = listenSocket.Accept();

                    Task.Run(() => ServClient(handler, xNodelst, semaphore));
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                semaphore.Release();
            }

        }
        static void ServClient(Socket handler, XmlNodeList xNodelst, Semaphore semaphore)
        {
            if (!semaphore.WaitOne(200))
            {
                handler.Send(Encoding.Unicode.GetBytes("Server error!"));
                handler.Shutdown(SocketShutdown.Both);
                handler.Close();
                return;
            }

            try
            {
                int count = 0;
                while (true)
                {
                    // получаем сообщение
                    StringBuilder builder = new StringBuilder();
                    int bytes = 0; // количество полученных байтов
                    byte[] data = new byte[256]; // буфер для получаемых данных

                    do
                    {
                        bytes = handler.Receive(data);
                        builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
                    }
                    while (handler.Available > 0);

                    //Console.WriteLine(DateTime.Now.ToShortTimeString() + ": " + builder.ToString());
                    string inf_for_server = "";

                    string message_from_client = builder.ToString();

                    char[] wordsSplit = new char[] { ' ', ',', '!', '?', '.' };
                    string[] words = message_from_client.Split(wordsSplit, StringSplitOptions.RemoveEmptyEntries);
                    if (words[0] == "<reg>")
                    {
                        string name = words[1];
                        string pass = words[2];
                        Persons pers = new Persons(name, pass);
                        persons.Add(pers);

                        data = Encoding.Unicode.GetBytes("You regist acc");
                        handler.Send(data);
                    }
                    else if (words[0] == "<login>")
                    {
                        string name = words[1];
                        string pass = words[2];
                        bool ok = false;
                        foreach (var item in persons)
                        {
                            if (item.Name == name && item.Password == pass)
                            {
                                ok = true;
                                break;
                            }
                        }
                        if (ok == true)
                        {
                            data = Encoding.Unicode.GetBytes("You login in acc");
                            handler.Send(data);
                        }
                        else
                        {
                            data = Encoding.Unicode.GetBytes("Error, reg your acc");
                            handler.Send(data);
                        }
                    }
                    else
                    {

                        int select = int.Parse(words[0]);
                        int select2 = int.Parse(words[2]);
                        int count_your_valute = int.Parse(words[1]);

                        if (select == 1)
                            inf_for_server += "USD";
                        if (select == 2)
                            inf_for_server += "EUR";
                        if (select == 3)
                            inf_for_server += "RUB";
                        if (select == 4)
                            inf_for_server += "BTC";

                        inf_for_server += " " + count_your_valute.ToString() + " to ";

                        if (select2 == 1)
                            inf_for_server += "USD";
                        if (select2 == 2)
                            inf_for_server += "EUR";
                        if (select2 == 3)
                            inf_for_server += "RUB";
                        if (select2 == 4)
                            inf_for_server += "BTC";

                        Console.WriteLine(handler.RemoteEndPoint + DateTime.Now.ToShortTimeString() + ":  " + inf_for_server);

                        decimal price_your_valute = 0;
                        decimal price_convert_valute = 0;

                        int f = 0;
                        foreach (XmlNode xNode in xNodelst)
                        {

                            f++;
                            if (f == select)
                            {
                                string str = xNode.SelectSingleNode("exchangerate").SelectSingleNode("@buy").Value;
                                price_your_valute = decimal.Parse(str, CultureInfo.InvariantCulture);
                            }
                            if (f == select2)
                            {
                                string str = xNode.SelectSingleNode("exchangerate").SelectSingleNode("@buy").Value;
                                price_convert_valute = decimal.Parse(str, CultureInfo.InvariantCulture);
                            }
                        }

                        decimal result = 0;
                        price_your_valute = price_your_valute * count_your_valute;
                        result = price_your_valute / price_convert_valute;
                        //Console.WriteLine(result);

                        // отправляем ответ
                        if (count >= 5)
                        {
                            data = Encoding.Unicode.GetBytes("There are 5 operations!");
                            handler.Send(data);
                            break;
                        }
                        data = Encoding.Unicode.GetBytes(result.ToString());
                        handler.Send(data);
                       
                        count++;
                        // закрываем сокет
                        //}
                    }
                }
            }
            catch (Exception ex)
            {

                Console.WriteLine(ex.Message);
            }
            finally
            {
                handler.Shutdown(SocketShutdown.Both);
                handler.Close();
                semaphore.Release();
            }

        }
    }
}
