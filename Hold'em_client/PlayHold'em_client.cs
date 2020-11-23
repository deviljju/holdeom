using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Hold_em_client.Properties;
using System.Net;
using System.Net.Sockets;

namespace Hold_em_client
{
    public partial class PlayHold_em_client : Form
    {
        class Card
        {
            public int code { get; set; }
            public int num { get; set; }        //2~14, J11 Q12 K13 A 14
            public int shape { get; set; }      //0 클로버 1 다이아 2 하트 3 스페이드
            public Card(int code, int shape, int num)
            {
                this.code = code;
                this.shape = shape;
                this.num = num;
            }
        }
        //test
        delegate void AppendTextDelegate(Control ctrl, string s);
        AppendTextDelegate _textAppender;
        delegate void listDelegate(int ind);
        listDelegate _listAppender;

        Client cli = null;

        Random r = new Random();
        List<Image> cardmap = new List<Image>();
        List<Card> Deck = new List<Card>(52);
        List<Card> commit = new List<Card>(5);
        List<Card> Hand = new List<Card>(2);
        List<Card> Elee = new List<Card>(7);
        List<Card> ques = new List<Card>();
        List<int> scores = new List<int>();
        String[] card1 = new string[4] { "♣", "◆", "♥", "♠" };
        String[] grade = new string[9] { "하이카드", "페어", "투페어", "트리플", "스트레이트", "플러쉬", "풀하우스", "포카드", "스트레이트플러쉬" };

        Bitmap carddk = new Bitmap(Properties.Resources.back);
        Bitmap chip1 = new Bitmap(Properties.Resources.chip_1);
        Bitmap chip10 = new Bitmap(Properties.Resources.chip_10);
        Bitmap chip100 = new Bitmap(Properties.Resources.chip_100);
        Bitmap chip1000 = new Bitmap(Properties.Resources.chip_1000);
        Bitmap turnchip = new Bitmap(Properties.Resources.turn);
        Bitmap close = new Bitmap(Properties.Resources.close);

        public string Pname;
        String s;
        String grd;

        int PlayerNum = -1;

        bool next = false;
        bool track = false;
        bool Myturn = false;
        bool open = false;

        int p0_bet = 0;
        int p1_bet = 0;
        int p2_bet = 0;
        int p3_bet = 0;

        int num = 0;
        int indx = 0;   //카드 덱 남은 개수
        int Fl_shape = 0;
        int Th_num = -1;
        bool bet = false;
        int score = 1;
        int winnerHands = 0;
        int bigHand = 0;
        int betmoney = 0;
        public int money; //자기돈
        int ante = 10;
        int pot = 0; //테이블에 쌓여있는 돈
        int my_bet = 0; // 내가 베팅한 돈
        int other_bet = 0; //전 단계에서 베팅한 돈
        int max_score=0;
        //타이머
        System.Timers.Timer countdownTimer = new System.Timers.Timer(1000);
        int tikcount = 0;

        public PlayHold_em_client(Client c)
        {
            countdownTimer.Elapsed += sec;
            InitializeComponent();
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.DoubleBuffer, true);
            cli = c;
            cli.Hide();
            _textAppender = new AppendTextDelegate(AppendText);
            //card 덱을 만듬 코드는 0~51
            for (int i = 0; i < 4; i++)
            {
                for (int j = 2; j <= 14; j++)
                {
                    Card tmp = new Card(indx, i, j);
                    Deck.Add(tmp);
                    String FileName;
                    FileName = String.Format("Card{0:0}_{1:00}", i, j);
                    object rm = Resources.ResourceManager.GetObject(FileName);
                    cardmap.Add((Bitmap)rm); //cardmap에 있는 카드 이미지를 코드indx로 찾을 수 있음
                    //Shuffle<Card>(Deck);
                    indx++;
                }
            }
            indx--;
        }

        public void sec(object sender, EventArgs e)
        {
            tikcount--;
            if (tikcount == 0)
            {
                if (Myturn)
                {
                    fold_A(); //시간초과시 다이
                    Invalidate();
                }
            }
            Rectangle r1 = new Rectangle(786, 474, 57, 34);
            Invalidate(r1);
        }

        void NewGame()
        {
            commit.Clear();
            Hand.Clear();
            Elee.Clear();
            ques.Clear();
            listBox1.Items.Clear();
            listBox2.Items.Clear();
            listBox3.Items.Clear();
            my_bet = 0;
            betmoney = 0;
            other_bet = 0;
            winnerHands = 0;
            max_score = 0;
            score = 1;
            bigHand = 0;
            p0_bet = 0;
            p1_bet = 0;
            p2_bet = 0;
            p3_bet = 0;
            next = false;
            bet = true;
            for (int i = 0; i < num; i++)
            {
                scores[i] = 0;
            }
            //Shuffle<Card>(Deck);
            Fl_shape = 5;
            money -= ante;
            label1.Text = "";
            label2.Text = "";
            label10.Text = "";
            label3.Text = "";
            label4.Text = "";
            label5.Text = "";
            label20.Text = "♠";
            countdown.Text = "";
            Invalidate();
        }
        private static DateTime Delay(int MS)
        {
            DateTime ThisMoment = DateTime.Now;
            TimeSpan duration = new TimeSpan(0, 0, 0, 0, MS);
            DateTime AfterWards = ThisMoment.Add(duration);

            while (AfterWards >= ThisMoment)
            {
                System.Windows.Forms.Application.DoEvents();
                ThisMoment = DateTime.Now;
            }

            return DateTime.Now;
        }
        void AppendText(Control ctrl, string s)
        {
            if (ctrl.InvokeRequired) ctrl.Invoke(_textAppender, ctrl, s);
            else
            {
                string source = ctrl.Text; //기존에 있는 문자열
                ctrl.Text = source + Environment.NewLine + s;
                decoder(s);
            }
        }
        void Appendlist(int ind)
        {
            if (this.listBox1.InvokeRequired) this.listBox1.Invoke(_listAppender, ind);
            else
            {
                listBox1.Items.Add(card1[Deck[ind].shape] + " " + Deck[ind].num.ToString());
            }
        }
        //test
        void decoder(string s)
        { //게임시작 : gs110000, 프리플랍 : Hd0x00xx, 콜 : Cl0xxxxx, 베팅 : bt0xxxxx, 폴드 : Fd0x0000, 플랍 : Cs1100xx, 점수산출후 보내기 : Sc0xxxxx, 상금획득 : Cg0xxxxx 
          // 0   1  2    
            string[] code = s.Split(','); //00,0x,0000, 나머진 널값 (code : 명령어,플레이어번호,내용)
            int player;
            if (code[0].Equals("PN")) //플레이어 들어온 순서, 테이블 번호 입력받음 PN,0x,1111
            {
                if (PlayerNum < 0)
                {
                    PlayerNum = int.Parse(code[1]);
                    label6.Text = PlayerNum.ToString();
                    label20.Text = "♠";
                    string sources = textBox4.Text;
                    string sss = "[Host]이름 : " + code[3] + "  자본 : " + code[2];
                    textBox4.Text = sss + Environment.NewLine + sources;
                    //textBox4.AppendText(Environment.NewLine + sss);
                    string ttt = string.Format("Ji,{0:D2},{1:D}," + Pname + ",", PlayerNum, money);
                    Send_Data(ttt);
                }
                string source = textBox4.Text;
                string ss = "[System] : 플레이어" + code[1] + "게임 참여";
                textBox4.AppendText(Environment.NewLine + ss);
            }
            else if (code[0].Equals("Ji")) //"Ji,{0:D2},{1:D}," + Pname + ",", PlayerNum, money);
            {
                return;
            }
            else if (code[0].Equals("gs"))
            {
                if (money <= 0)
                {
                    if (MessageBox.Show("파산하셨습니다. 이대로 포기하시겠습니까?", "충전", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        Application.Exit();
                    }
                    else
                    {
                        MessageBox.Show("행운을 빕니다. 호갱님");
                        money = 100;
                    }
                }
                if (num < 1)
                {
                    num = int.Parse(code[1]); //플레이어수
                    for (int i = 0; i < num; i++)
                    {
                        scores.Add(0);
                    }
                }
                pot = int.Parse(code[2]);
                ante = pot / num;
                label20.Text = "♠게임시작";
                NewGame();
                string source = textBox4.Text;
                string ss = "[System] : New Game (인원:"+num+"/23)===";
                textBox4.AppendText(Environment.NewLine + ss);
                //textBox4.Text = ss + Environment.NewLine + source;
            }
            else if (code[0].Equals("Hd"))
            {
                if (Hand.Count == 0)
                {
                    if (PlayerNum == int.Parse(code[1]))
                    {
                        int cardnum = int.Parse(code[2]);
                        int code1 = cardnum / 100;
                        int code2 = cardnum % 100;
                        pre_Flop(code1, code2);
                    }
                }
                label20.Text = "♠핸드지급";
            }
            else if (code[0].Equals("De"))
            {
                if (PlayerNum == int.Parse(code[1]))
                {
                    other_bet = int.Parse(code[2]); //other_bet
                    Action_A();
                    label20.Text = "♠나의 턴";
                    tikcount = 20;
                    countdownTimer.Start();
                }
                else
                {
                    label20.Text = "♠" + code[3] + "의턴";
                    Action_End();
                }
            }
            else if (code[0].Equals("Cl"))
            { //상대가 콜했는지 확인
                if (PlayerNum != int.Parse(code[1]))
                {
                    if (int.Parse(code[1]) == 0)
                    {
                        p0_bet += int.Parse(code[2]);
                    }
                    else if (int.Parse(code[1]) == 1)
                    {
                        p1_bet += int.Parse(code[2]);
                    }
                    else if (int.Parse(code[1]) == 2)
                    {
                        p2_bet += int.Parse(code[2]);
                    }
                    else if (int.Parse(code[1]) == 3)
                    {
                        p3_bet += int.Parse(code[2]);
                    }
                }
                else
                {
                    if (int.Parse(code[1]) == 0)
                    {
                        p0_bet =my_bet ;
                    }
                    else if (int.Parse(code[1]) == 1)
                    {
                        p1_bet = my_bet;
                    }
                    else if (int.Parse(code[1]) == 2)
                    {
                        p2_bet = my_bet;
                    }
                    else if (int.Parse(code[1]) == 3)
                    {
                        p3_bet = my_bet;
                    }
                }
                string ss = "[System] : "+code[3];
                string sss = string.Format("이(가) {0:D}칩을 콜했습니다.", int.Parse(code[2]));
                label10.Text = ss + sss;
                pot += int.Parse(code[2]);
            }
            else if (code[0].Equals("bt"))
            {
                if (PlayerNum != int.Parse(code[1]))
                {
                    if (int.Parse(code[1]) == 0)
                    {
                        p0_bet += int.Parse(code[2]);
                    }
                    else if (int.Parse(code[1]) == 1)
                    {
                        p1_bet += int.Parse(code[2]);
                    }
                    else if (int.Parse(code[1]) == 2)
                    {
                        p2_bet += int.Parse(code[2]);
                    }
                    else if (int.Parse(code[1]) == 3)
                    {
                        p3_bet += int.Parse(code[2]);
                    }
                }
                else
                {
                    if (int.Parse(code[1]) == 0)
                    {
                        p0_bet = my_bet;
                    }
                    else if (int.Parse(code[1]) == 1)
                    {
                        p1_bet = my_bet;
                    }
                    else if (int.Parse(code[1]) == 2)
                    {
                        p2_bet = my_bet;
                    }
                    else if (int.Parse(code[1]) == 3)
                    {
                        p3_bet = my_bet;
                    }
                }
                string ss = "[System] : "+code[3];
                string sss = string.Format("이(가) {0:D}칩만큼 베팅했습니다.", int.Parse(code[2]));
                label10.Text = ss + sss;
                other_bet = int.Parse(code[2]);
                pot += int.Parse(code[2]);
            }
            else if (code[0].Equals("Al"))
            {
                if (PlayerNum != int.Parse(code[1]))
                {
                    if (int.Parse(code[1]) == 0)
                    {
                        p1_bet += int.Parse(code[2]);
                    }
                    else if (int.Parse(code[1]) == 1)
                    {
                        p1_bet += int.Parse(code[2]);
                    }
                    else if (int.Parse(code[1]) == 2)
                    {
                        p2_bet += int.Parse(code[2]);
                    }
                    else if (int.Parse(code[1]) == 3)
                    {
                        p3_bet += int.Parse(code[2]);
                    }
                }
                else
                {
                    if (int.Parse(code[1]) == 0)
                    {
                        p0_bet = my_bet;
                    }
                    else if (int.Parse(code[1]) == 1)
                    {
                        p1_bet = my_bet;
                    }
                    else if (int.Parse(code[1]) == 2)
                    {
                        p2_bet = my_bet;
                    }
                    else if (int.Parse(code[1]) == 3)
                    {
                        p3_bet = my_bet;
                    }
                }
                string source = textBox4.Text;
                string ss = "[System] : "+code[3];
                string sss = string.Format("이(가) 올인했습니다.");
                //textBox4.Text = ss + sss + Environment.NewLine + source;
                textBox4.AppendText(Environment.NewLine + ss+sss);
                label10.Text = ss + sss;
                other_bet = int.Parse(code[2]);
                pot += int.Parse(code[2]);
            }
            else if (code[0].Equals("Fd"))
            {
                string source = textBox4.Text;
                string ss = "[System] : "+code[3];
                string sss = string.Format("이(가) 다이쳤습니다.");
                //textBox4.Text = ss + sss + Environment.NewLine + source;
                textBox4.AppendText(Environment.NewLine + ss+sss);
                label10.Text = ss + sss;
                other_bet = int.Parse(code[2]);
            }
            else if (code[0].Equals("Cs") && commit.Count < 5) //플랍 커뮤니티 카드 받는거
            {
                if (int.Parse(code[1]) == 11)
                {
                    int cardnum = int.Parse(code[2]);
                    flop(cardnum);
                }
            }
            else if (code[0].Equals("Sc"))
            {
                if (listBox3.Items.Count != num)
                {
                    scores[int.Parse(code[1])] = int.Parse(code[2]);
                    //if (PlayerNum != int.Parse(code[1]))
                    string rank;
                    rank = score_decoder(scores[int.Parse(code[1])]);
                    if (scores[int.Parse(code[1])] > 1)
                    {
                        listBox3.Items.Add(code[1] + " : " + rank);
                    }
                    else if (scores[int.Parse(code[1])] == 1)
                    {
                        listBox3.Items.Add("Win");
                    }
                    else
                    {
                        listBox3.Items.Add("Die");
                    }
                }
            }
            else if (code[0].Equals("GS"))
            { //점수를 보내라
                countdownTimer.Stop();
                string ss = string.Format("Sc,{0:D2},{1:D5}," + Pname + ",", PlayerNum, score);//Cl,0x,xxxx,
                Send_Data(ss);
            }
            else if (code[0].Equals("Cg"))
            {
                string t = "우승자는 " + code[3];
                winnerHands = int.Parse(code[2]);
                label20.Text = t;
                Invalidate();
                score_max();
                if (PlayerNum == int.Parse(code[1]))
                { //내가 이겨따~
                    money += pot;
                }
                else if (int.Parse(code[1]) == 12)
                { //1명이랑 Draw
                    t = "1명과 비겼습니다.";
                    winnerHands = int.Parse(code[2]);
                    label20.Text = t;
                    Invalidate();
                    if (max_score == score)
                    { //비긴애들만 받아야함
                        money += (pot / 2);
                        pot = 0;
                    }
                }
                else if (int.Parse(code[1]) == 13)
                { //2명Draw
                    t = "2명과 비겼습니다.";
                    winnerHands = int.Parse(code[2]);
                    label20.Text = t;
                    Invalidate();
                    if (max_score == score)
                    { //내가 이겨따~
                        money += (pot / 3);
                        pot = 0;
                    }
                }
                else if (int.Parse(code[1]) == 14)
                { //3명 전부 Draw
                    t = "3명과 비겼습니다.";
                    winnerHands = int.Parse(code[2]);
                    label20.Text = t;
                    Invalidate();
                    if (max_score == score)
                    { //내가 이겨따~
                        money += (pot / 4);
                        pot = 0;
                    }
                }
                else
                {
                    pot = 0;
                }
                string source = textBox4.Text;
                string ss = "[System] : ";
                textBox4.AppendText(Environment.NewLine + ss);
                //textBox4.Text = ss + t + Environment.NewLine + source;
                label10.Text = ss + t;
            }
            else if (code[0].Equals("Lv"))
            {
                if (int.Parse(code[1]) <= PlayerNum)
                {
                    PlayerNum--;
                }
                string source = textBox4.Text;
                string ss = "[System] : " + code[3] + " 게임 나감";
                textBox4.AppendText(Environment.NewLine + ss);
                //textBox4.Text = ss + Environment.NewLine + source;
                //m_ClientSocket[int.Parse(code[1])].Close();
                num--;
                Invalidate();
            }
            else if (code[0].Equals("Mg")) //"Mg,{0:D2},", PlayerNum)+textBox2.Text+",";
            {
                    string source = textBox4.Text;
                    string ss = "[" + code[3] + "]:" + code[2];
                    textBox4.AppendText(Environment.NewLine + ss);
                  //textBox4.Text= ss+Environment.NewLine+ source;
            }
            else
            {
                return;
                //AppendText(textBox1, s);
            }
        }

        void pre_Flop(int c1, int c2) //Hd,0x,00xx
        { //플레이어가 핸드 2장을 받음
            //내핸드
            Hand.Clear();
            int big = 0;
            Hand.Add(Deck[c1]);
            Elee.Add(Deck[c1]);
            label1.Text = card1[Hand[0].shape] + " " + Hand[0].num.ToString();
            Hand.Add(Deck[c2]);
            Elee.Add(Deck[c2]);
            label2.Text = card1[Hand[1].shape] + " " + Hand[1].num.ToString();
            for (int i = 0; i < Hand.Count; i++)
            {
                if (Hand[i].num > big)
                {
                    big = Hand[i].num;
                }
            }
            bigHand = big;
            Invalidate();
        }
        //Action 파트
        void Action_A()  //De,0x,xxxx, (지금 걸린 돈을 받음)
        {
            //버튼 활성화
            Myturn = true;
            button3.Enabled = true;
            button4.Enabled = true;
            button5.Enabled = true;
            textBox3.Enabled = true;
            trackBar1.Enabled = true;
            Invalidate();
        }
        void call_A() //Cl,0x,xxxxf
        {            
            int coin = other_bet - betmoney;
            if (coin >= 0)
            {
                my_bet += coin;
                if (money - coin >= 0)
                {
                    money -= coin;
                }
                else
                {
                    MessageBox.Show("소지금을 초과했습니다.");
                    return;
                }
            
            string s = string.Format("Cl,{0:D2},{1:D4}," + Pname + ",", PlayerNum, coin);//Cl,0x,xxxx
            //other_bet = 0;
            Send_Data(s);
            textBox3.Text = "";
            Action_End();
                countdownTimer.Stop();
            }
        }
        void bet_A() //bt,0x,xxxx
        {
            if (textBox3.Text.Equals("All in!"))
            {
                betmoney = money;
                my_bet += betmoney;
                money -= betmoney;
                string s = string.Format("Al,{0:D2},{1:D4}," + Pname + ",", PlayerNum, betmoney);//Cl,0x,xxxx
                Send_Data(s);
                other_bet = betmoney;
                textBox3.Text = "";
                Action_End();
                countdownTimer.Stop();
            }
            else if (int.Parse(textBox3.Text) >= other_bet)
            {
                betmoney = int.Parse(textBox3.Text);
                my_bet += betmoney;
                money -= betmoney;
                string s = string.Format("bt,{0:D2},{1:D4}," + Pname + ",", PlayerNum, betmoney);//Cl,0x,xxxx
                Send_Data(s);
                other_bet = betmoney;
                textBox3.Text = "";
                Action_End();
                countdownTimer.Stop();
            }
            else
            {
                MessageBox.Show("베팅금이 작거나 같습니다. call 눌러주세요");
            }
        }
        void fold_A() //Fd,0x,xxxx
        {
            countdownTimer.Stop();
            string s = string.Format("Fd,{0:D2},{1:D4}," + Pname + ",", PlayerNum, other_bet);//Cl,0x,xxxx,
            Send_Data(s);
            textBox3.Text = "";
            bet = false;
            score = 0;
            Action_End();
            
        }
        void flop(int ind) //Cs,11,00xx
        { //커뮤니티카드 3장 오픈
            if (commit.Count() < 4)
            {
                commit.Add(Deck[ind]);
                Elee.Add(Deck[ind]);
                Appendlist(ind); //listBox1.Items.Add(card1[Deck[ind].shape] + " " + Deck[ind].num.ToString());
            }
            else
            {
                commit.Add(Deck[ind]);
                Elee.Add(Deck[ind]);
                Appendlist(ind); //listBox1.Items.Add(card1[Deck[ind].shape] + " " + Deck[ind].num.ToString());
                Elesort();
                Ele();
            }
            betmoney = 0;
            Invalidate();
        }
        //턴카드 : 커뮤니티 카드를 오픈함. 턴, 리버때 사용
        public void TurnCard(int ind) //Cs,11,00xx
        {
            if (commit.Count() < 4)
            {
                commit.Add(Deck[ind]);
                Elee.Add(Deck[ind]);
                Appendlist(ind);
                //listBox1.Items.Add(card1[Deck[ind].shape] + " " + Deck[ind].num.ToString());
                indx--;
            }
            else if (next)
            {
                NewGame();
                next = false;
            }
            else
            {
                commit.Add(Deck[ind]);
                Elee.Add(Deck[ind]);
                Appendlist(ind);
                //listBox1.Items.Add(card1[Deck[ind].shape] + " " + Deck[ind].num.ToString());
                indx--;
                //listBox2.Items.Clear();
                Elesort();
                Ele();

                next = true;
            }
            Invalidate();
        }
        string score_decoder(int sc)
        {
            string res;
            int gr = sc / 10000; //천의자리 X0000
            int n = (sc % 10000) / 100; //백,십의 자리 0XX00
            int s = (sc % 10000) % 100; //000XX 하이카드
            if (sc >= 81400)
            {
                res = "로얄스트레이트플러쉬!";
            }
            else
            {
                res = n.ToString() + grade[gr];
            }
            return res;
        }
        void score_max()
        {
            for (int i = 0; i < scores.Count; i++) {
                if (scores[i] > max_score)
                {
                    max_score = scores[i];
                }
            } 
        }
        public void DataReceived(IAsyncResult ar)
        {
            //BeginReceive 에서 받은 데이터를 AsyncObject 형식으로 변환
            AsyncObject obj = (AsyncObject)ar.AsyncState;
            try
            {
                int received = obj.WorkingSocket.EndReceive(ar); //수신후 수신종료

                if (received <= 0) // 받아온 데이터가 없을시
                {
                    obj.WorkingSocket.Close();
                    return;
                }

                //test
                string text = Encoding.UTF8.GetString(obj.Buffer);
                AppendText(textBox1, text); //obj로 받은 데이터를 textbox1에 입력
                                            //test

                //버퍼를 비워주고 다시 수신대기로 전환
                obj.ClearBuffer();
                obj.WorkingSocket.BeginReceive(obj.Buffer, 0, 4096, 0, DataReceived, obj);
            }
            catch
            {
                MessageBox.Show("서버와 연결이 끊어졌습니다.");
                Application.Exit();
            }
        }
        public void Send_Data(string s)
        {
            try { 
            byte[] bDts = Encoding.UTF8.GetBytes(s);

            cli.senddata(bDts); //client의 소켓으로 텍스트 데이터를 전달

            AppendText(textBox1, s); //전달한 데이터를 자신에게도 표시
            }
                catch(Exception e)
            {
                MessageBox.Show("서버와 연결이 끊어졌습니다.");
                Application.Exit();
            }
        }
        private void button1_Click(object sender, EventArgs e)
        {
            String tmp = string.Format("Mg,{0:D2},", PlayerNum) + textBox2.Text+"," + Pname + ",";
            Send_Data(tmp);
            textBox2.Text = "";
        }

        //paint부분=============================================================================================
        private void PlayHold_em_client_Paint(object sender, PaintEventArgs e)
        {

            int pot_T = pot / 1000; //pot 1000
            int pot_H = (pot % 1000) / 100; //pot 100
            int pot_D = (pot % 100) / 10; //pot 10
            int pot_b = pot % 10; //pot 1

            int money_T = money / 1000; //검 1000칩
            int money_H = (money % 1000) / 100; //노 100칩
            int money_D = (money % 100) / 10; //빨 10칩
            int money_b = money % 10; //파란 1칩

            label24.Text = p0_bet.ToString();
            label25.Text = p1_bet.ToString();
            label26.Text = p2_bet.ToString();
            label27.Text = p3_bet.ToString();
            label3.Text = grd;
            label6.Text = PlayerNum.ToString();
            label7.Text = money.ToString(); //내돈
            label8.Text = pot.ToString();
            label9.Text = my_bet.ToString();
            label21.Text = other_bet.ToString();
            e.Graphics.DrawImage(carddk, 100, 200, 80, 110);

            if (Hand.Count >= 2)
            {
                if (open)
                {
                    e.Graphics.DrawImage(cardmap[Hand[0].code], 344, 533 - 120, 80, 110);
                    e.Graphics.DrawImage(cardmap[Hand[1].code], 344 + 60, 533 - 120, 80, 110);
                }
                else
                {
                    e.Graphics.DrawImage(close, 344, 533 - 120, 80, 110);
                    e.Graphics.DrawImage(close, 344 + 60, 533 - 120, 80, 110);
                }
            }

            if (Myturn)
            {
                e.Graphics.DrawImage(turnchip, 580, 320, 100, 100);
                countdown.Text = tikcount.ToString();
            }

            //커뮤니티 패
            for (int i = 0; i < commit.Count; i++)
            {
                e.Graphics.DrawImage(cardmap[commit[i].code], 344-80 + (50 * i), 200, 80, 110);
            }


            if (money > 0)
            {
                for (int t = 0; t < money_b; t++)
                {
                    e.Graphics.DrawImage(chip1, 650 + 220, 364 - (6 * (t + 1)), 50, 40);
                }
                for (int t = 0; t < money_H; t++)
                {
                    e.Graphics.DrawImage(chip100, 595 + 220, 365 - (6 * (t + 1)), 50, 40);
                }
                for (int t = 0; t < money_D; t++)
                {
                    e.Graphics.DrawImage(chip10, 635 + 220, 390 - (6 * (t + 1)), 50, 40);
                }
                for (int t = 0; t < money_T; t++)
                {
                    e.Graphics.DrawImage(chip1000, 570 + 220, 391 - (6 * (t + 1)), 50, 40);
                }
                if (money_T > 9)
                {
                    for (int t = 0; t < money_T - 9; t++)
                    {
                        e.Graphics.DrawImage(chip1000, 565 + 220, 393 - (6 * (t + 1)), 50, 40);
                    }
                }
            }
            if (winnerHands > 0)
            {
                e.Graphics.DrawImage(cardmap[winnerHands / 100], 334, 30, 80, 110);
                e.Graphics.DrawImage(cardmap[winnerHands % 100], 334 + 60, 30, 80, 110);
            }
            else //판돈 칩 이미지 출력
            {
                for (int t = 0; t < pot_b; t++)
                {
                    e.Graphics.DrawImage(chip1, 414, 118 - (6 * (t + 1)), 50, 40);
                }
                for (int t = 0; t < pot_H; t++)
                {
                    e.Graphics.DrawImage(chip100, 349, 119 - (6 * (t + 1)), 50, 40);
                }
                for (int t = 0; t < pot_D; t++)
                {
                    e.Graphics.DrawImage(chip10, 389, 144 - (6 * (t + 1)), 50, 40);
                }
                for (int t = 0; t < pot_T; t++)
                {
                    e.Graphics.DrawImage(chip1000, 334, 145 - (6 * (t + 1)), 50, 40);
                }
                if (pot_T > 9)
                {
                    for (int t = 0; t < pot_T - 9; t++)
                    {
                        e.Graphics.DrawImage(chip1000, 329, 147 - (6 * (t + 1)), 50, 40);
                    }
                }
            }

        }

        private void button2_Click(object sender, EventArgs e)
        {
            TurnCard(indx);
        }

        //여기서부터 족보확인코드================================================
        void Elesort()
        {
            Card tmp;
            for (int i = 0; i < Elee.Count; i++)
            {
                for (int j = 0; j < Elee.Count; j++)
                {
                    if (Elee[i].num < Elee[j].num)
                    {
                        tmp = Elee[i];
                        Elee[i] = Elee[j];
                        Elee[j] = tmp;
                    }
                }
            }
            for (int i = 0; i < Elee.Count; i++)
            {
                listBox2.Items.Add(card1[Elee[i].shape] + " " + Elee[i].num.ToString());
            }
        }
        void Ele()
        {
            if (StraightFlush())
                label3.Text = grd;
            else if (FourOfKind())
                label3.Text = grd;
            else if (FullHouse())
                label3.Text = grd;
            else if (Flush(Elee))
                label3.Text = grd;
            else if (Straight())//스트레이트
                label3.Text = grd;
            else if (ThreeOfKind(Elee))
                label3.Text = grd;
            else if (TwoPair(Elee))
                label3.Text = grd;
            else if (OnePair(Elee))
                label3.Text = grd;
            else            //하이카드
            {
                int max = 0;
                for (int i = 0; i < Hand.Count; i++)
                {
                    if (Hand[i].num > max)         //max보다 크면 교체함
                    {
                        max = Hand[i].num;
                        Fl_shape = Hand[i].shape;
                    }
                }
                if (bet) { 
                grd = "하이카드";
                label4.Text = max.ToString() + "card";
                    score = max * 100 + Fl_shape;
                }
            }
            label5.Text = score.ToString();
        }
        bool StraightFlush()
        {
            int max = 0;
            if (Flush(Elee))
            {
                for (int i = 0; i < Elee.Count; i++)
                {
                    if (Elee[i].shape == Fl_shape)
                    {
                        ques.Add(Elee[i]); //같은 모양만 큐에 넣음
                    }
                } //큐에 다 담음
                if (ques.Count >= 5)
                {
                    for (int i = 0; i < ques.Count - 4; i++)
                    {
                        if (ques[i].num + 1 == ques[i + 1].num &&
                            ques[i + 1].num + 1 == ques[i + 2].num &&
                            ques[i + 2].num + 1 == ques[i + 3].num &&
                            ques[i + 3].num + 1 == ques[i + 4].num)
                        {
                            if (ques[i + 4].num > max)
                            {
                                max = ques[i + 4].num;
                            }
                        }
                    }
                    if (max > 0)
                    {  if (bet)
                        {
                        grd = "스트레이트플러쉬";
                        s = card1[Fl_shape];
                        s += string.Format("{0:D} straight Flush", max);
                        label4.Text = s;
                        /*점수 계산*/
                      
                            score = 80000 + max * 100 + bigHand;
                        }
                        return true;
                    }
                }
            }
            return false;
        }
        bool FourOfKind()
        {
            for (int i = 0; i < Elee.Count; i++)
            {
                for (int j = i + 1; j < Elee.Count; j++)
                {
                    for (int k = j + 1; k < Elee.Count; k++)
                    {
                        for (int m = k + 1; m < Elee.Count; m++)
                        {
                            if (Elee[i].num == Elee[j].num && Elee[i].num == Elee[k].num && Elee[i].num == Elee[m].num) //포카드
                            { if (bet)
                                {
                                grd = "포카드";
                                s = string.Format("{0:D} FourCard", Elee[i].num);
                                label4.Text = s;
                                /*점수 계산*/
                               
                                    score = 70000 + Elee[i].num * 100 + bigHand;
                                }
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }
        bool FullHouse()
        {   //트리플+페어, 트리플이 같을 순 없음 트리플로 순위 비교하는게 맞는듯 트리플이 나오면 elee에서 빼고 남은걸로 페어 검사
            int numb = 0;
            List<Card> tmp = Elee;
            for (int i = 0; i < tmp.Count; i++)
            {
                for (int j = i + 1; j < tmp.Count; j++)
                {
                    for (int k = j + 1; k < tmp.Count; k++)
                    {
                        if (tmp[i].num == tmp[j].num && tmp[i].num == tmp[k].num) //트리플
                        {
                            ques.Add(tmp[i]);
                            tmp.RemoveAt(i);
                            ques.Add(tmp[j - 1]);
                            tmp.RemoveAt(j - 1);
                            ques.Add(tmp[k - 2]);
                            tmp.RemoveAt(k - 2);
                            numb = ques[0].num;
                            if (ThreeOfKind(tmp))
                            {
                                if (numb < Th_num)
                                { if (bet)
                                    {
                                    numb = Th_num;
                                    grd = "풀하우스";
                                    s = string.Format("{0:D} FullHouse", numb);
                                    label4.Text = s;
                                    /*점수 계산*/
                                   
                                        score = 60000 + numb * 100 + bigHand;
                                    }
                                    return true;
                                }
                            }
                            //else if (TwoPair(tmp))
                            //{

                            //}
                            else if (OnePair(tmp))
                            {if (bet)
                                {
                                grd = "풀하우스";
                                s = string.Format("{0:D} FullHouse", numb);
                                label4.Text = s;
                                /*점수 계산*/
                                
                                    score = 60000 + numb * 100 + bigHand;
                                }
                                return true;
                            }
                            else
                            {                                if (bet)
                                {
                                grd = "트리플";
                                s = string.Format("{0:D} triple", numb);
                                label4.Text = s;
                                Th_num = numb;
                                /*점수 계산*/

                                    score = 30000 + Th_num * 100 + bigHand;
                                }
                                return true;
                            }
                        }
                    }
                }
            }//트리플
            numb = 0;
            return false;
        }
        bool Flush(List<Card> list)
        {
            int S_num = 0, D_num = 0, H_num = 0, C_num = 0;
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].shape == 0)
                {
                    C_num++;
                }
                else if (list[i].shape == 1)
                {
                    D_num++;
                }
                else if (list[i].shape == 2)
                {
                    H_num++;
                }
                else if (list[i].shape == 3)
                {
                    S_num++;
                }
            }
            if (S_num >= 5)
            {                if (bet)
                {
                grd = "플러쉬";
                int max = topflush(list, 3);
                s = string.Format("{0:D} 스페이드 플러쉬", max);
                label4.Text = s;
                Fl_shape = 3;
                /*점수 계산*/

                    score = 50000 + max * 100 + bigHand;
                }
                return true;
            }
            else if (H_num >= 5)
            {                if (bet)
                {
                grd = "플러쉬";
                int max = topflush(list, 2);
                s = string.Format("{0:D} 하트 플러쉬", max);
                label4.Text = s;
                Fl_shape = 2;
                /*점수 계산*/

                    score = 50000 + max * 100 + bigHand;
                }
                return true;
            }
            else if (D_num >= 5)
            {                if (bet)
                {
                grd = "플러쉬";
                int max = topflush(list, 1);
                s = string.Format("{0:D} 다이아 플러쉬", max);
                label4.Text = s;
                Fl_shape = 1;
                /*점수 계산*/

                    score = 50000 + max * 100 + bigHand;
                }
                return true;
            }
            else if (C_num >= 5)
            {
                if (bet)
                {
                    grd = "플러쉬";
                int max = topflush(list, 0);
                s = string.Format("{0:D} 클로버 플러쉬", max);
                label4.Text = s;
                Fl_shape = 0;
                /*점수 계산*/

                    score = 50000 + max * 100 + bigHand;
                }
                return true;
            }
            return false;
        }
        int topflush(List<Card> list, int sha)
        {
            int max = 0;
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].shape == sha && list[i].num > max)
                {
                    max = list[i].num;
                }
            }
            return max;
        }
        bool Straight()
        {
            bool conx = true;
            int max = 0;
            //List<int> que = new List<int>();
            for (int i = 0; i < Elee.Count; i++)
            {
                conx = true;
                if (ques.Count < 1) { ques.Add(Elee[i]); }
                else
                {
                    for (int j = 0; j < ques.Count; j++)
                    {
                        if (ques[j].num == Elee[i].num)
                        {
                            conx = false;
                            break;
                        }
                    }
                    if (conx)
                    {
                        ques.Add(Elee[i]);
                    }
                }
            } //que에 중복 숫자 거르고 다 들어감
            for (int i = 0; i < ques.Count - 4; i++)
            {
                if (ques[i].num + 1 == ques[i + 1].num &&
                    ques[i + 1].num + 1 == ques[i + 2].num &&
                    ques[i + 2].num + 1 == ques[i + 3].num &&
                    ques[i + 3].num + 1 == ques[i + 4].num)
                {
                    if (ques[i + 4].num > max)
                    {
                        max = ques[i + 4].num;
                        Fl_shape = ques[i + 4].shape;
                    }
                }
            }
            if (max > 0)
            {                
                if (bet)
                {
                grd = "스트레이트";
                s = string.Format("{0:D} straight", max);
                label4.Text = s;
                /*점수 계산*/

                    score = 40000 + max * 100 + bigHand;
                }
                return true;
            }
            return false;
        }
        bool ThreeOfKind(List<Card> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                for (int j = i + 1; j < list.Count; j++)
                {
                    for (int k = j + 1; k < list.Count; k++)
                    {
                        if (list[i].num == list[j].num && list[i].num == list[k].num) //트리플
                        {
                            if (bet)
                            {
                                grd = "트리플";
                             s = string.Format("{0:D} triple", list[i].num);
                            label4.Text = s;
                            Th_num = list[i].num;
                            /*점수 계산*/

                                score = 30000 + Th_num * 100 + bigHand; //문양 필요없음 핸드 우선순위
                            }
                            return true;
                        }
                    }
                }
            }
            return false;
        }
        bool TwoPair(List<Card> list)
        {
            bool conx = true;
            List<int> que = new List<int>();
            for (int i = 0; i < list.Count; i++)
            {
                for (int j = i + 1; j < list.Count; j++)
                {
                    if (list[i].num == list[j].num) //페어
                    {
                        if (que.Count < 2)
                        {
                            que.Add(list[i].num);
                            que.Sort();
                        }
                        else
                        {
                            for (int k = 0; k < que.Count; k++)
                            {
                                if (list[i].num == que[k])
                                { //중복이면
                                    break;
                                }
                                else
                                {//중복아니면 큐값들 보다 클경우 추가됨
                                    for (int m = 0; m < que.Count; m++)
                                    {
                                        if (list[i].num > que[k])
                                        {
                                            que[k] = list[i].num;
                                            que.Sort();
                                            conx = false;
                                            break;
                                        }
                                    }
                                    if (!conx)
                                    {
                                        conx = true;
                                        break;
                                    }
                                }
                            }
                        }
                    }//페어일경우
                }
            }
            if (que.Count == 2)
            {
                if (bet)
                {
                    grd = "투페어";
                    s = string.Format("{0:D} {1:D} two pair", que[que.Count - 1], que[que.Count - 2]);
                    label4.Text = s;
                    /*점수 계산 */
                    score = 20000 + que[que.Count - 1] * 100 + bigHand;
                }
                return true;
            }
            return false;
        }
        bool OnePair(List<Card> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                for (int j = i + 1; j < list.Count; j++)
                {
                    if (list[i].num == list[j].num)
                    {
                        grd = "페어";
                        s = string.Format("{0:D} pair + {1:D}top", list[i].num, bigHand);
                        label4.Text = s;
                        /*점수 계산 */
                        if (bet)
                        {
                            score = 10000 + list[i].num * 100 + bigHand;
                        }
                        return true;
                    }
                }
            }
            return false;
        }
        //족보 계산 끝==========================================================

        private void Button3_Click(object sender, EventArgs e) //call
        {
            call_A();
            Invalidate();
        }

        private void Button4_Click(object sender, EventArgs e)
        {
            bet_A();
            Invalidate();
        }

        private void Button5_Click(object sender, EventArgs e)
        {
            fold_A();
            Invalidate();
        }

        private void TextBox3_TextChanged(object sender, EventArgs e)
        {
            if (!track)
            {
                if (money == 0)
                {
                    trackBar1.Value = 0;
                }
                else
                {
                    int value;
                    if (int.TryParse(textBox3.Text, out value))
                    {
                        if ((int.Parse(textBox3.Text) > money))
                        {
                            textBox3.Text = money.ToString();
                            MessageBox.Show("소지금을 초과했습니다.");
                            return;
                        }
                        else
                        {
                            trackBar1.Value = (int)((int.Parse(textBox3.Text) * 100) / money);
                        }
                    }
                    else
                    {
                        textBox3.Text = "0";
                    }
                }
            }
        }

        private void TrackBar1_Scroll(object sender, EventArgs e)
        {
            track = true;
            if (track)
            {
                textBox3.Text = (money * trackBar1.Value / 100).ToString();
                if (trackBar1.Value == 100) //최대치
                {
                    textBox3.Text = "All in!"; //money.ToString();
                }
            }
        }
        void Action_End()
        {
            Myturn = false;
            button3.Enabled = false;
            button4.Enabled = false;
            button5.Enabled = false;
            textBox3.Enabled = false;
            trackBar1.Value = 0;
            trackBar1.Enabled = false;
            countdown.Text = "";
            Invalidate();
        }
        private void TextBox3_MouseClick(object sender, MouseEventArgs e)
        {
            track = false;
        }

        private void PlayHold_em_client_FormClosed(object sender, FormClosedEventArgs e)
        {
            try
            {
                if (PlayerNum > 0)
                {
                    string ss = string.Format("Lv,{0:D2},0000," + Pname + ",", PlayerNum); //나감
                    Send_Data(ss);
                }
                Delay(1000);
                Application.Exit();
            }
            catch (Exception)
            {
                MessageBox.Show("서버와 연결이 끊어졌습니다.");
                Application.Exit();
            }
        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            help help = new help();
            help.Show();
        }

        private void textBox2_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                String tmp = string.Format("Mg,{0:D2},", PlayerNum) + textBox2.Text + "," + Pname + ",";
                Send_Data(tmp);
                textBox2.Text = "";
                e.SuppressKeyPress = true;
            }
        }

        private void label31_Click(object sender, EventArgs e)
        {

        }

        private void PlayHold_em_client_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.X >= 344 && e.X <= 344 + 140 && e.Y >= 413 && e.Y <= 523)
            {
                open = true;
            }
            else
            {
                open = false;
            }
            Invalidate();
        }
    }
}