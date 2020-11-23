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

namespace Hold_em_client
{
    public partial class Client : Form
    {
        delegate void AppendTextDelegate(Control ctrl, string s);
        public Socket mainSock;

        PlayHold_em_client client;
        string m_lIP;

        public Client()
        {
            InitializeComponent();
            IP.Text = m_lIP;
        }

        private void FormLoaad(object sender, EventArgs e) // 폼생성시 발생되는 이벤트
        {
            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName()); //로컬 ip 주소확인
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    m_lIP = ip.ToString();
                }
            }

            IP.Text = m_lIP;
        }

        private void Enter_Server_Click(object sender, EventArgs e)
        {
            mainSock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
            client = new PlayHold_em_client(this);
            client.Pname = textBox1.Text;
            client.money = int.Parse(textBox2.Text);
            if (mainSock.Connected)
            {
                MessageBox.Show("이미 연결되어 있습니다!");
                return;
            }

            int port;
            if (!int.TryParse(PORT.Text, out port))
            {
                MessageBox.Show("포트 번호가 잘못 입력되었거나 입력되지 않았습니다.");
                PORT.Focus();
                PORT.SelectAll();
                return;
            }

            try { mainSock.Connect(IP.Text, port); }
            catch (Exception ex)
            {
                MessageBox.Show("연결에 실패했습니다!\n오류 내용: {0}", ex.Message);
                return;
            }

            AsyncObject obj = new AsyncObject(4096);
            obj.WorkingSocket = mainSock;
            mainSock.BeginReceive(obj.Buffer, 0, obj.BufferSize, 0, client.DataReceived, obj);

            //MessageBox.Show("서버에 입장 중입니다.");
            client.Show(); //클라이언트창오픈
            this.Hide();
        }

        public void senddata(byte[] bDts)
        {
            try
            {
                mainSock.Send(bDts);
            }
            catch (Exception)
            {
            }
        }

        private void Form_close_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}
