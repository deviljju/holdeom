using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.Net;
using System.Net.Sockets;

namespace Hold_em_server
{
    public partial class Sever_create : Form
    {
        public Socket mainSock;
        IPAddress thisAddress;

        PlayHold_em_host host;

        String m_lIP = "INVALID";
        public Sever_create()
        {
            InitializeComponent();
            IP.Text = m_lIP;
        }

        private void FormLoad(object sender, EventArgs e)
        {
            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName()); //로컬 ip 주소확인
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    m_lIP = ip.ToString();
                    thisAddress = ip;
                }
            }

            IP.Text = m_lIP ;
        }

        private void Create_Server_Click(object sender, EventArgs e)
        {
            host = new PlayHold_em_host(this);
            mainSock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
            host.Pname = textBox1.Text;
            host.money = int.Parse(textBox2.Text);
            host.ante = int.Parse(textBox3.Text);
            host.startmoney= int.Parse(textBox2.Text);
            host.Pnames.Add(textBox1.Text);
            int port;
            if (!int.TryParse(PORT.Text, out port))
            {
                MessageBox.Show("포트 번호가 잘못 입력되었거나 입력되지 않았습니다.");
                PORT.Focus();
                PORT.SelectAll();
                return;
            }

            //클라이언트 연결 요청을 위해 소켓 오픈
            IPEndPoint serverEP = new IPEndPoint(thisAddress, port);
            mainSock.Bind(serverEP);
            mainSock.Listen(10);

            mainSock.BeginAccept(host.AcceptCallback, null);

            host.Show();
            this.Hide();
        }

        private void Form_close_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}
