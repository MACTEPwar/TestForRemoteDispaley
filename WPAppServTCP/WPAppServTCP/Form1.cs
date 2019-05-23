using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WPAppServTCP
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private static byte[] ReceiveVarData(Socket s)
        {
            int total = 0;
            int recv;
            byte[] datasize = new byte[4];

            recv = s.Receive(datasize, 0, 4, 0);
            int size = BitConverter.ToInt32(datasize, 0);
            int dataleft = size;
            byte[] data = new byte[size];


            while (total < size)
            {
                recv = s.Receive(data, total, dataleft, 0);
                if (recv == 0)
                {
                    break;
                }
                total += recv;
                dataleft -= recv;
            }
            return data;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            new Thread(() => {
                Console.WriteLine("Server is starting...");
                byte[] data = new byte[1024];
                IPEndPoint ipep = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 5678);

                Socket newsock = new Socket(AddressFamily.InterNetwork,
                                SocketType.Stream, ProtocolType.Tcp);

                newsock.Bind(ipep);
                newsock.Listen(10);
                Console.WriteLine("Waiting for a client...");

                Socket client = newsock.Accept();
                IPEndPoint newclient = (IPEndPoint)client.RemoteEndPoint;
                Console.WriteLine("Connected with {0} at port {1}",
                                newclient.Address, newclient.Port);

                while (true)
                {
                    data = ReceiveVarData(client);
                    MemoryStream ms = new MemoryStream(data);
                    try
                    {
                        Image bmp = Image.FromStream(ms);
                        pictureBox1.Image = bmp;
                    }
                    catch (ArgumentException ee)
                    {
                        Console.WriteLine("something broke");
                    }


                    if (data.Length == 0)
                        newsock.Listen(10);
                }
                client.Close();
                newsock.Close();
            }).Start();
            
        }
    }
}
