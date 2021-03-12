using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
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

namespace Client
{
    public partial class MainWindow : Window
    {
        string ipAddress = "127.0.0.1";
        int port = 8080;
        public MainWindow()
        {
            InitializeComponent();
        }

        bool is_allow_work = false;

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (nameBox.Text != "" && passwordBox.Text != "" && is_allow_work == true)
            {
                try
                {
                    //IPEndPoint ipPoint = new IPEndPoint(IPAddress.Parse(ipAddress), port);
                   

                    bool ok1 = false;
                    if (EUR_my_val.IsChecked == false && GRN_my_val.IsChecked == false && RUB_my_val.IsChecked == false && USD_my_val.IsChecked == false && Grn_convert_val.IsChecked == false && USD_convert_val.IsChecked == false && EUR_convert_val.IsChecked == false && RUB_convert_val.IsChecked == false)
                        ok1 = false;
                    else ok1 = true;
                    if (ok1 == false)
                    {
                        MessageBox.Show("Enter a message!");
                        return;
                    }

                    string message = "";
                    if (EUR_my_val.IsChecked == true)
                        message += "2";
                    else if (GRN_my_val.IsChecked == true)
                        message += "4";
                    else if (RUB_my_val.IsChecked == true)
                        message += "3";
                    else if (USD_my_val.IsChecked == true)
                        message += "1";

                    if (portBox.Text == "")
                        message += " 1 ";
                    else
                        message += " " + portBox.Text + " ";

                    if (EUR_convert_val.IsChecked == true)
                        message += "2";
                    else if (Grn_convert_val.IsChecked == true)
                        message += "4";
                    else if (RUB_convert_val.IsChecked == true)
                        message += "3";
                    else if (USD_convert_val.IsChecked == true)
                        message += "1";


                    Send_message(message);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        public void Send_message(string str)
        {
            if (string.IsNullOrWhiteSpace(str)) return;

            IPEndPoint ipPoint = new IPEndPoint(IPAddress.Parse(ipAddress), port);

            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            // подключаемся к удаленному хосту
            socket.Connect(ipPoint);



            byte[] data = Encoding.Unicode.GetBytes(str);
            socket.Send(data);

            // получаем ответ
            data = new byte[256]; // буфер для ответа
            StringBuilder builder = new StringBuilder();
            int bytes = 0; // количество полученных байт

            do
            {
                bytes = socket.Receive(data, data.Length, 0);
                builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
            }
            while (socket.Available > 0);

            if(builder.ToString() == "You regist acc")
            {
                list.Items.Add(builder.ToString());
            }

            if (builder.ToString() == "Error, reg your acc")
            {
                list.Items.Add(builder.ToString());
            }

            if (builder.ToString() == "You login in acc")
            {
                is_allow_work = true;
            }

            if (is_allow_work == true)
            {
                list.Items.Add(builder.ToString());
            }

            // закрываем сокет
            socket.Shutdown(SocketShutdown.Both);
            socket.Close();
        }

        private void Login_button_Click(object sender, RoutedEventArgs e)
        {
            if (nameBox.Text != "" && passwordBox.Text != "")
            {
                string str = "<login> " + nameBox.Text + " " + passwordBox.Text;
                Send_message(str);
            }
        }

        private void Reg_button_Click(object sender, RoutedEventArgs e)
        {
            if (nameBox.Text != "" && passwordBox.Text != "")
            {
                string str = "<reg> " + nameBox.Text + " " + passwordBox.Text;
                Send_message(str);
            }
        }
    }
}
