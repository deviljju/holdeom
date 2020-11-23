using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Hold_em_server.Properties;
using System.Net;
using System.Net.Sockets;

namespace Hold_em_server
{
    public partial class PlayHold_em_host : Form
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

        Sever_create svr = null;
        int num = 0;

        Random r = new Random();
        List<Image> cardmap = new List<Image>();
        List<Card> Deck = new List<Card>(52);
        List<Card> cardinfo = new List<Card>(52);

        List<Card> commit = new List<Card>(5);
        List<Card> Hand = new List<Card>(2);
        List<Card> Elee = new List<Card>(7);
        List<Card> ques = new List<Card>();
        List<int> Hands = new List<int>();

        List<int> list = new List<int>();
        List<int> scores = new List<int>();
        public List<string> Pnames = new List<string>();

        String[] card1 = new string[4] { "♣", "◆", "♥", "♠" };
        String[] grade = new string[9] { "하이카드", "페어", "투페어", "트리플", "스트레이트", "플러쉬",  "풀하우스", "포카드", "스트레이트플러쉬" };

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
        public int startmoney;
        public int money;
        public int ante = 10;
        int PlayerNum = 0;
        bool turns = false;
        bool track = false;
        bool bet = false;
        bool open = false;
        int indx = 0;   //카드 덱 남은 개수
        int Fl_shape = 0;
        int Th_num = -1;
        int score = 1; //0:die 1:bet
        int winnerHands = 0;
        int state = 0; //0: 시작, 1: 핸드, 2:액션 3:커뮤니티 4: 게임종료
        int head = 1;
        int turn = 0;
        int betMoney = 0;
        int pot = 0;
        int my_bet = 0;
        int other_bet = 0;
        int bigHand = 0;
        int p0_bet = 0;
        int p1_bet = 0;
        int p2_bet = 0;
        int p3_bet = 20;
        int max_score = 0;
        System.Timers.Timer countdownTimer = new System.Timers.Timer(1000);
        int tikcount = 0;

        public PlayHold_em_host(Sever_create s)
        {
            InitializeComponent(); 
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.DoubleBuffer, true);
            countdownTimer.Elapsed += sec;
            _textAppender = new AppendTextDelegate(AppendText); //test
            svr = s;
            //Deck은 진짜 카드 뭉치
            //cardinfo라는 데이터에서 카드 정보를 읽어와야함
            for (int i = 0; i < 4; i++)
            {
                for (int j = 2; j <= 14; j++)
                {
                    Card tmp = new Card(indx, i, j);
                    Deck.Add(tmp);
                    cardinfo.Add(tmp);
                    String FileName;
                    FileName = String.Format("Card{0:0}_{1:00}", i, j);
                    object rm = Resources.ResourceManager.GetObject(FileName);
                    cardmap.Add((Bitmap)rm); //cardmap에 있는 카드 이미지를 코드indx로 찾을 수 있음
                    Shuffle<Card>(Deck);
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
                if (turns)
                {
                    fold_A(); //시간초과시 다이
                    Invalidate();
                }
            }
            Rectangle r1 = new Rectangle(786, 474, 57, 34);
            Invalidate(r1);
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
        //디코더=========================================================================================================
        void decoder(string s)
        { //게임시작 : gs110000, 프리플랍 : Hd0x00xx, 콜 : Cl0xxxxx, 베팅 : bt0xxxxx, 폴드 : Fd0x0000, 플랍 : Cs1100xx, 점수산출후 보내기 : Sc0xxxxx, 상금획득 : Cg0xxxxx 
          // 0   1  2    
            CheckForIllegalCrossThreadCalls = false;
            string[] code = s.Split(','); //00,0x,0000, 나머진 널값 (code : 명령어,플레이어번호,내용)
            if (code[0].Equals("PN")) //플레이어 들어온 순서, 테이블 번호 입력받음 PN,0x,1111
            {
                if (PlayerNum < 0)
                {
                    PlayerNum = int.Parse(code[1]);
                    label6.Text = PlayerNum.ToString();
                }
                string source = textBox4.Text;
                string ss = "[System] : 플레이어" + code[1] + "게임 참여";
                textBox4.AppendText(Environment.NewLine + ss);
                //textBox4.Text = ss + Environment.NewLine + source;
            }
            else if (code[0].Equals("Ji")) //"Ji,{0:D2},{1:D}," + Pname + ",", PlayerNum, money);
            {
                if (PlayerNum != int.Parse(code[1]))
                {
                    Pnames[int.Parse(code[1])] = code[3];
                    string source = textBox4.Text;
                    string ss = "[Client" + code[1] + "] 이름 : "+code[3]+"  자본 : "+code[2]+ Environment.NewLine+" 인원 : " +(m_ClientSocket.Count+1).ToString()+"/23";
                    textBox4.AppendText(Environment.NewLine + ss);
                    //textBox4.Text = ss + Environment.NewLine + source;
                }
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
                label14.Text = "♠게임시작";
                state = 0;
                num = int.Parse(code[1]); //플레이어수(호스트니까 -1할..까?)
                head = int.Parse(code[1]);
                pot = int.Parse(code[2]);
                NewGame();
                string source = textBox4.Text;
                string ss = "[System] : New Game (인원:" + num + "/23)===";
                textBox4.AppendText(Environment.NewLine + ss);
                //textBox4.Text = ss + Environment.NewLine + source;
            }
            else if (code[0].Equals("Hd"))
            {
                if (PlayerNum == int.Parse(code[1]))
                {
                    int cardnum = int.Parse(code[2]);
                    int code1 = cardnum / 100;
                    int code2 = cardnum % 100;
                    pre_Flop(code1, code2);
                }
                label14.Text = "♠핸드지급";

            }
            else if (code[0].Equals("De")) //너의 베팅차례 
            {
                if (PlayerNum == int.Parse(code[1]))
                {
                    label14.Text = "♠나의턴";
                    other_bet = int.Parse(code[2]); //other_bet
                    Action_A();
                    tikcount = 20;
                    countdownTimer.Start();
                }
                else
                {
                    label14.Text = "♠"+code[3]+"의턴";
                    Action_End();
                }
                Invalidate();
                nextt();
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
                string sss = string.Format("이(가) {0:D}칩을 콜했습니다.", int.Parse(code[2]));
                label2.Text = ss + sss;
                list[int.Parse(code[1])] = 1;
                pot += int.Parse(code[2]);
                Action_H();
            }
            else if (code[0].Equals("bt"))
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
                for (int i = 0; i < list.Count; i++)
                {
                    if (list[i] == 1)
                    {
                        list[i] = 0;
                    }
                }
                string ss = "[System] : "+code[3];
                string sss = string.Format("이(가) {0:D}칩만큼 베팅했습니다.", int.Parse(code[2]));
                label2.Text = ss + sss;

                list[int.Parse(code[1])] = 1;
                other_bet = int.Parse(code[2]);
                pot += int.Parse(code[2]);
                Action_H();
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
                for (int i = 0; i < list.Count; i++)
                {
                    if (list[i] == 1)
                    {
                        list[i] = 0;
                    }
                }
                string source = textBox4.Text;
                string ss = "[System] : "+code[3];
                string sss = string.Format("이(가) 올인했습니다.");
                //textBox4.Text = ss + sss + Environment.NewLine + source;
                textBox4.AppendText(Environment.NewLine + ss+sss);

                label2.Text = ss + sss;

                list[int.Parse(code[1])] = 2; //action 1||2
                other_bet = int.Parse(code[2]);
                pot += int.Parse(code[2]);
                Action_H();
            }
            else if (code[0].Equals("Fd"))
            {
                string source = textBox4.Text;
                string ss = "[System] : "+code[3];
                string sss = string.Format("이(가) 다이쳤습니다.");
                //textBox4.Text = ss + sss + Environment.NewLine + source;
                textBox4.AppendText(Environment.NewLine + ss+sss);
                label2.Text = ss + sss;
                list[int.Parse(code[1])] = 2;
                other_bet = int.Parse(code[2]);
                Action_H();
            }
            else if (code[0].Equals("Cs") && commit.Count < 5) //플랍 커뮤니티 카드 받는거
            {
                if (int.Parse(code[1]) == 11)
                {
                    int cardnum = int.Parse(code[2]);
                    flop(cardnum);
                }
                if (commit.Count >= 3)
                {
                    Action_H();
                }
            }
            else if (code[0].Equals("GS")) 
            { //점수를 보내라
                countdownTimer.Stop();
                string ss = string.Format("Sc,{0:D2},{1:D5},"+Pname+",", PlayerNum, score);//Cl,0x,xxxx,
                DataSend(ss);
                decoder(ss);
            }
            else if (code[0].Equals("Sc"))
            { //점수보낸걸 받음
                if (listBox3.Items.Count < num)
                {
                    scores[int.Parse(code[1])] = int.Parse(code[2]);
                    //if (PlayerNum != int.Parse(code[1]))
                    string rank;
                    rank = score_decoder(scores[int.Parse(code[1])]);
                    //for(int i = 0; i < list.Count; i++)
                    //{
                    //    list[i] = 0;
                    //}
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
                    //string s = string.Format("Fd,{0:D2},{1:D4},", PlayerNum, other_bet);//Cl,0x,xxxx,
                    if (listBox3.Items.Count == num)
                    {
                        int Wincode;
                        int winner = score_compare();
                        Wincode = winner;
                        if (winner > 10)
                        {
                            Wincode -= 11;
                        }
                        string ss = string.Format("Cg,{0:D2},{1:D4}," + Pnames[Wincode] + ",", winner, Hands[Wincode]);
                        DataSend(ss);
                        Delay(300);
                        decoder(ss);
                    }
                }
            }
            else if (code[0].Equals("Cg"))
            {
                string t = "우승자는 " + code[3];
                winnerHands = int.Parse(code[2]);
                label14.Text = t;
                button9.Enabled = true;
                Invalidate();
                state = 4;
                if (PlayerNum == int.Parse(code[1]))
                { //내가 이겨따~
                    money += pot;
                }
                else if (int.Parse(code[1])==12)
                { //1명이랑 Draw
                    t = "1명과 비겼습니다.";
                    winnerHands = Hand[0].code * 100 + Hand[1].code; //int.Parse(code[2]);
                    label14.Text = t;
                    Invalidate();
                    if (max_score==score)
                    { //내가 이겨따~
                        money += (pot/2);
                        pot = 0;
                    }
                }
                else if (int.Parse(code[1]) == 13)
                { //2명Draw
                    t = "2명과 비겼습니다.";
                    winnerHands = Hand[0].code * 100 + Hand[1].code;
                    label14.Text = t;
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
                    winnerHands = Hand[0].code * 100 + Hand[1].code;
                    label14.Text = t;
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
                //textBox4.Text = ss + t + Environment.NewLine + source;
                textBox4.AppendText(Environment.NewLine + ss);
                label2.Text = ss + t;
            }
            else if (code[0].Equals("Lv"))
            {
                //m_ClientSocket[int.Parse(code[1])].Close();
                m_ClientSocket.RemoveAt(int.Parse(code[1])-1);
                list.RemoveAt(int.Parse(code[1]));
                scores.RemoveAt(int.Parse(code[1]));
                Pnames.RemoveAt(int.Parse(code[1]));
                num--;
                head--;
                string source = textBox4.Text;
                string ss = "[System] : " + code[3] + " 게임 나감";
                //textBox4.Text = ss + Environment.NewLine + source;
                textBox4.AppendText(Environment.NewLine + ss);
                if (turn == int.Parse(code[1]))
                {
                    nextt();
                    Action_H();
                }
            }
            else if(code[0].Equals("Mg")) //"Mg,{0:D2},", PlayerNum)+textBox2.Text+",";
            {
                string source = textBox4.Text;
                string ss = "[" + code[3] + "]:" + code[2];
                textBox4.AppendText(Environment.NewLine + ss);
                //textBox4.Text = ss + Environment.NewLine + source;
            }
            else
            {
                return;
                //AppendText(textBox1, s);
            }
        }
        void Action_A()  //De,0x,xxxx, (지금 걸린 돈을 받음)
        {
            //버튼 활성화
            turns = true;
            button5.Enabled = true;
            button6.Enabled = true;
            button7.Enabled = true;
            textBox3.Enabled = true;
            trackBar1.Enabled = true;
        }
        void nextt()
        {
            turn++;
            if (turn >= head) //m_ClientSocket.Count + 1)
            {
                turn = 0;
            }
            if (list[turn] == 2)
            {
                nextt();
            }
        }
        void GameClear()
        {   listBox1.Items.Clear();
            listBox2.Items.Clear();
            listBox3.Items.Clear();

            commit.Clear();
            Hand.Clear();
            Hands.Clear();
            Elee.Clear();
            ques.Clear();

            nextt();
            indx = 51;
            pot = 0;
            score = 1;
            betMoney = 0;
            my_bet = 0;
            other_bet = 0;
            winnerHands = 0;
            bigHand = 0;
            p0_bet = 0;
            p1_bet = 0;
            p2_bet = 0;
            p3_bet = 0;
            turns = false;
            track = false;
            bet = true;
            for (int i = 0; i < num; i++) //m_ClientSocket.Count + 1
            {
                list[i] = 0;
                scores[i] = 0;
            }
            Fl_shape = 5;
            pot = ante * num;
            money -= ante;
            label2.Text = "";
            label3.Text = "";
            label4.Text = "";
            label5.Text = "";
            label14.Text = "♠";
            label10.Text = "";
            label11.Text = "";
            pre_Flop_H();
            Invalidate();
        }
        void NewGame()
        {
            Shuffle<Card>(Deck);
            GameClear();
        }
        void pre_Flop_H()
        {
            state = 1;
            for(int i = 0; i < num; i++) //m_ClientSocket.Count + 1
            {
                string tmp;
                tmp = string.Format("Hd,{0:D2},{1:D2}{2:D2},",i,Deck[indx].code,Deck[indx-1].code);
                string h = string.Format("{0:D2}{1:D2}", Deck[indx].code, Deck[indx - 1].code);
                Hands.Add(int.Parse(h));
                DataSend(tmp);
                decoder(tmp);
                indx -= 2;
                Delay(1000);
            }
            Action_H();
        }
        void pre_Flop(int c1, int c2) //Hd,0x,00xx
        { //플레이어가 핸드 2장을 받음
            //내핸드
            Hand.Clear();
            int big=0;
            Hand.Add(cardinfo[c1]);
            Elee.Add(cardinfo[c1]);
            label10.Text = card1[Hand[0].shape] + " " + Hand[0].num.ToString();
            Hand.Add(cardinfo[c2]);
            Elee.Add(cardinfo[c2]);
            label11.Text = card1[Hand[1].shape] + " " + Hand[1].num.ToString();
            Invalidate();
            for(int i=0;i< Hand.Count; i++)
            {
                if (Hand[i].num > big)
                {
                    big = Hand[i].num;
                }
            }
            bigHand = big;
        }
        void Flop_H()
        { //첫 한장의 커뮤니티 카드 오픈
            for (int i = 0; i < num; i++) //m_ClientSocket.Count + 1
            {
                if (list[i] != 2)
                {
                    list[i] = 0;
                }
            }
            string tmp;
            tmp = string.Format("Cs,11,{0:D4},", Deck[indx].code);
            indx--;
            DataSend(tmp);
            decoder(tmp);

            //알아서 커뮤니티 보내주기
        }
        void flop(int ind) //Cs,11,00xx
        { //커뮤니티카드 3장 오픈
            if (commit.Count() < 4)
            {
                commit.Add(cardinfo[ind]);
                Elee.Add(cardinfo[ind]);
                //Appendlist(ind); 
                listBox1.Items.Add(card1[cardinfo[ind].shape] + " " + cardinfo[ind].num.ToString());
            }
            else
            {
                commit.Add(cardinfo[ind]);
                Elee.Add(cardinfo[ind]);
                //Appendlist(ind); 
                listBox1.Items.Add(card1[cardinfo[ind].shape] + " " + cardinfo[ind].num.ToString());
                Elesort();
                Ele();
            }
            betMoney = 0;
            other_bet = 0;
            Invalidate();
        }
        void call_A() //Cl,0x,xxxx
        {
            int coin = other_bet - betMoney;
            if (coin >= 0)
            {
                if (money - coin >= 0)
                {
                    my_bet += coin;
                    money -= coin;
                }
                else
                {
                    MessageBox.Show("소지금을 초과했습니다.");
                    return;
                }

            
            string s = string.Format("Cl,{0:D2},{1:D4}," + Pname + ",", PlayerNum, coin);//Cl,0x,xxxx
            //other_bet = 0;
            DataSend(s);
            decoder(s);
            textBox3.Text = "";
            Action_End();
                countdownTimer.Stop();
            }
        }
        void bet_A() //bt,0x,xxxx
        {
            if(textBox3.Text.Equals("All in!"))
            {
                betMoney = money;
                my_bet += betMoney;
                money -= betMoney;
                string s = string.Format("Al,{0:D2},{1:D4}," + Pname + ",", PlayerNum, betMoney);//Cl,0x,xxxx
                DataSend(s);
                other_bet = betMoney;
                decoder(s);
                textBox3.Text = "";
                Action_End();
                countdownTimer.Stop();
            }
            else if (int.Parse(textBox3.Text) >= other_bet)
            {
                betMoney= int.Parse(textBox3.Text);// = trackBar1.Value.ToString();
                my_bet += betMoney;
                money -= betMoney;
                string s = string.Format("bt,{0:D2},{1:D4}," + Pname + ",", PlayerNum, betMoney);//Cl,0x,xxxx
                DataSend(s);
                other_bet = betMoney;
                decoder(s);
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
            DataSend(s);
            bet = false;
            score = 0;
            decoder(s);
            textBox3.Text = "";
            Action_End();
            
        }
        string score_decoder(int sc)
        {
            string res;
            int gr = sc / 10000; //천의자리 X0000
            int n = (sc % 10000) / 100; //백,십의 자리 0XX00
            int s = (sc%10000) % 100; //000XX 하이카드
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
        int score_compare()
        {
            int cnum = 0;
            int Dnum = 0;
            //점수 누가큰지
            for (int i = 0; i < num; i++)
            {
                if (scores[i] > max_score)
                {
                    max_score = scores[i];
                    cnum = i;
                }
            }
            //같은점수가 있는지
            for (int i = 0; i < num; i++) {
                if (scores[i] == max_score)
                {
                    Dnum++;
                }
            }
            if (Dnum > 1)
            {
                Dnum += 10;
                return Dnum;
            }
            else
            {
                return cnum;
            }
        }
        void Shuffle<T>(List<T> list)
        {
            int random1;
            int random2;
            T tmp;
            for (int i = 0; i < list.Count; ++i)
            {
                random1 = r.Next(0, list.Count);
                random2 = r.Next(0, list.Count);

                tmp = list[random1];
                list[random1] = list[random2];
                list[random2] = tmp;
            }
        }

        //데이터 전송코드====================================================
        List<Socket> m_ClientSocket = new List<Socket>();
        //크로스스레드 문제 해결용 (다만 일부오류로 사용하지 못함)
        void AppendText(Control ctrl, string s)
        {
            if (ctrl.InvokeRequired) ctrl.Invoke(_textAppender, ctrl, s);
            else
            {
                string source = ctrl.Text;
                ctrl.Text = source + Environment.NewLine + s;
            }
        }
        void Appendlist(int ind)
        {
            if (this.listBox1.InvokeRequired) this.listBox1.Invoke(_listAppender, ind);
            else
            {
                listBox1.Items.Add(card1[cardinfo[ind].shape] + " " + cardinfo[ind].num.ToString());  //수정필요
            }
        }
        public void AcceptCallback(IAsyncResult ar)
        {
            string tmp;
            //클라이언트 연결 수락
            Socket client = svr.mainSock.EndAccept(ar);

            //추가 클라이언트 연결 대기
            svr.mainSock.BeginAccept(AcceptCallback, null);

            AsyncObject obj = new AsyncObject(4096);
            obj.WorkingSocket = client;
            if (m_ClientSocket.Count < 22)
            {
                m_ClientSocket.Add(client);
                list.Add(2);
                scores.Add(0);
                Pnames.Add("");
                if (num < m_ClientSocket.Count) //11:42 수정
                {
                    num++;
                }
                if (num >= m_ClientSocket.Count + 1)
                {
                    tmp = string.Format("PN,{0:D2},{1:D},"+Pname+",", num - 1,money);
                }
                else
                {
                    tmp = string.Format("PN,{0:D2},{1:D}," + Pname + ",", num,money);
                }
                DataSend(tmp);
                decoder(tmp);
                CheckForIllegalCrossThreadCalls = false;
            }
            else
            {
                MessageBox.Show("정원초과");
            }
            client.BeginReceive(obj.Buffer, 0, 4096, 0, DataReceived, obj);
        }
        void DataReceived(IAsyncResult ar) //데이터 받아서 처리하는 부분
        {
            AsyncObject obj = (AsyncObject)ar.AsyncState;
            try
            {
                int received = obj.WorkingSocket.EndReceive(ar);

                if (received <= 0)
                {
                    obj.WorkingSocket.Close();
                    return;
                }

            //test //클라이언트가 호스트한태 입력한것을 textbox1에 출력 
            string text = Encoding.UTF8.GetString(obj.Buffer);
            AppendText(textBox1, text);
            decoder(text);
            //test

            for (int i = m_ClientSocket.Count - 1; i >= 0; i--)
            {
                Socket socket = m_ClientSocket[i];
                if (socket != obj.WorkingSocket)
                {
                    try { socket.Send(obj.Buffer); }
                    catch
                    {
                        // 오류 발생하면 전송 취소하고 리스트에서 삭제한다.
                        try { socket.Dispose(); } catch { }
                        m_ClientSocket.RemoveAt(i);
                    }
                }
            }

            obj.ClearBuffer();
            obj.WorkingSocket.BeginReceive(obj.Buffer, 0, 4096, 0, DataReceived, obj);
            }
            catch (Exception)
            {
                if (num == 1)
                {
                    MessageBox.Show("클라이언트 없음. 메인메뉴로 이동");
                    button4.Visible = true;
                    button4.Enabled = true;
                    Action_End();
                    GameClear();
                    money = startmoney; //초기머니
                    Invalidate();
                    return;
                }
            }
        }
        void DataSend(String s)
        {
            byte[] bDts = Encoding.UTF8.GetBytes(s);

            // 연결된 모든 클라이언트에게 전송한다.
            for (int i = m_ClientSocket.Count - 1; i >= 0; i--)
            {
                Socket socket = m_ClientSocket[i];
                try { socket.Send(bDts);}
                catch
                {
                    // 오류 발생하면 전송 취소하고 리스트에서 삭제한다.
                    try { socket.Dispose(); } catch { }
                    m_ClientSocket.RemoveAt(i);
                }
            }
            //테스트파트
            if (Encoding.UTF8.GetString(bDts) == "05") { this.Text = "02받음"; }
            AppendText(textBox1, s);//전송한 데이터를 자신에게도 표시

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
                grd = "하이카드";
                label3.Text = grd;
                label4.Text = max.ToString() + "card";
               if (bet)
                {
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
                    {
                        
                        /*점수 계산*/
                       if (bet)
                        {
                            grd = "스트레이트플러쉬";
                            s = card1[Fl_shape];
                            s += string.Format("{0:D} straight Flush", max);
                            label4.Text = s;
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
                            {
                                
                                /*점수 계산*/
                               if (bet)
                                {
                                    grd = "포카드";
                                    s = string.Format("{0:D} FourCard", Elee[i].num);
                                    label4.Text = s;
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
                                {
                                    /*점수 계산*/
                                   if (bet)
                                    {
                                        numb = Th_num;
                                        grd = "풀하우스";
                                        s = string.Format("{0:D} FullHouse", numb);
                                        label4.Text = s;
                                        score = 60000 + numb * 100 + bigHand;
                                    }
                                    return true;
                                }
                            }
                            //else if (TwoPair(tmp))
                            //{

                            //}
                            else if (OnePair(tmp))
                            {
                                /*점수 계산*/
                               if (bet)
                                {
                                    grd = "풀하우스";
                                    s = string.Format("{0:D} FullHouse", numb);
                                    label4.Text = s;
                                    score = 60000 + numb * 100 + bigHand;
                                }
                                return true;
                            }
                            else
                            {
                                /*점수 계산*/
                               if (bet)
                                {
                                    grd = "트리플";
                                    s = string.Format("{0:D} triple", numb);
                                    label4.Text = s;
                                    Th_num = numb;
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
            {

                /*점수 계산*/
               if (bet)
                {
                    grd = "플러쉬";
                    int max = topflush(list, 3);
                    s = string.Format("{0:D} 스페이드 플러쉬", max);
                    label4.Text = s;
                    Fl_shape = 3;
                    score = 50000 + max * 100 + bigHand;
                }
                return true;
            }
            else if (H_num >= 5)
            {

                /*점수 계산*/
               if (bet)
                {
                    grd = "플러쉬";
                    int max = topflush(list, 2);
                    s = string.Format("{0:D} 하트 플러쉬", max);
                    label4.Text = s;
                    Fl_shape = 2;
                    score = 50000 + max * 100 + bigHand;
                }
                return true;
            }
            else if (D_num >= 5)
            {
                
                /*점수 계산*/
               if (bet)
                {
                    grd = "플러쉬";
                    int max = topflush(list, 1);
                    s = string.Format("{0:D} 다이아 플러쉬", max);
                    label4.Text = s;
                    Fl_shape = 1;
                    score = 50000 + max * 100 + bigHand;
                }
                return true;
            }
            else if (C_num >= 5)
            {
               
                /*점수 계산*/
               if (bet)
                {
                    grd = "플러쉬";
                    int max = topflush(list, 0);
                    s = string.Format("{0:D} 클로버 플러쉬", max);
                    label4.Text = s;
                    Fl_shape = 0;
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
               
                /*점수 계산*/
               if (bet)
                {
                    grd = "스트레이트";
                    s = string.Format("{0:D} straight", max);
                    label4.Text = s;
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
                            /*점수 계산*/
                           if (bet)
                            {
                                grd = "트리플";
                                s = string.Format("{0:D} triple", list[i].num);
                                label4.Text = s;
                                Th_num = list[i].num;
                                score = 30000 + Th_num * 100 + bigHand; //문양 필요없음 핸드 우선순위
                            }
                            return true;
                        }
                    }
                }
            }
            return false;
        }
        bool TwoPair(List<Card>list)
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
        bool OnePair(List<Card>list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                for (int j = i + 1; j < list.Count; j++)
                {
                    if (list[i].num == list[j].num)
                    {

                        /*점수 계산 */
                       if (bet)
                        {
                            grd = "페어";
                            s = string.Format("{0:D} pair + {1:D}top", list[i].num, bigHand);
                            label4.Text = s;
                            score = 10000 + list[i].num * 100 + bigHand;
                        }
                        return true;
                    }
                }
            }
            return false;
        }
        //족보 계산 끝==========================================================

        private void button1_Click(object sender, EventArgs e) // 전송버튼
        {
            //지금은 명령어 직접 적어서 보내는 중 ("Hd,01,1213,")
            String tmp = string.Format("Mg,{0:D2}," + Pname + ",", PlayerNum)+textBox2.Text+",";
            DataSend(tmp);
            decoder(tmp);
            textBox2.Text = "";
        }
        private void button2_Click_1(object sender, EventArgs e) //족보버튼
        {
            족보 help = new 족보();
            help.Show();
        }
        private void button3_Click(object sender, EventArgs e) //뉴게임
        {
            //게임시작: gs110000
            state = 0;
            string tmp;
            pot = ante * num;//(m_ClientSocket.Count+1);
            tmp = string.Format("gs,{0:D2},{1:D4},",(m_ClientSocket.Count+1),pot);
            DataSend(tmp);
            decoder(tmp);
        }
        private void Button4_Click(object sender, EventArgs e) //게임시작
        {
            if (num > 0)
            {
                list.Clear();
                scores.Clear();
                for (int i = 0; i < m_ClientSocket.Count + 1; i++) //m_ClientSocket.Count + 1
                {
                    list.Add(0);
                    scores.Add(0);
                }
                state = 0;
                string tmp;
                pot = ante * (m_ClientSocket.Count + 1);
                tmp = string.Format("gs,{0:D2},{1:D4},", (m_ClientSocket.Count + 1), pot);
                DataSend(tmp);
                decoder(tmp);
                button4.Enabled = false;
                button4.Visible = false;
            }
            else
            {
                MessageBox.Show("플레이어가 부족합니다.");
            }
        }
        private void Button5_Click(object sender, EventArgs e)
        {            fold_A();        }
        private void Button6_Click(object sender, EventArgs e)
        {            bet_A();        }
        private void Button7_Click(object sender, EventArgs e)
        {            call_A();        }
        private void Button9_Click(object sender, EventArgs e) //다음판
        { //액션
            list.Clear();
            scores.Clear();
            for (int i = 0; i < m_ClientSocket.Count+1; i++)
            {
                list.Add(0);
                scores.Add(0);
            }
            state = 0;
            string tmp;
            pot = ante * (m_ClientSocket.Count + 1);
            tmp = string.Format("gs,{0:D2},{1:D4},", (m_ClientSocket.Count + 1), pot);
            DataSend(tmp);
            decoder(tmp);
            button9.Enabled = false;
            button4.Visible = false;
        }
        void Action_End()
        {
            turns = false;
            button5.Enabled = false;
            button6.Enabled = false;
            button7.Enabled = false;
            textBox3.Enabled = false;
            trackBar1.Value = 0;
            trackBar1.Enabled = false;
            label33.Text = "";
            Invalidate();
        }
        void Action_H()
        {   //플레이어들에게 액션권한을 준다.   //De,0x,xxxx, (지금 걸린 돈을 받음)
            Delay(300);
            try
            {
                string tmp;
                int nextp = 0;
                int fateN = 0;
                int ChalN = 0;
                for (int i = 0; i < num; i++) //m_ClientSocket.Count + 1; i++)
                {
                    if (list[i] != 0)
                    {
                        if (list[i] == 2)
                        {
                            fateN++;
                        }
                        else if (list[i] == 1)
                        {
                            ChalN++;
                        }
                        nextp++;
                    }
                }
                if (fateN > 0 && num - fateN == ChalN && ChalN == 1)
                {
                    if (commit.Count < 5)
                    {
                        for (int i = commit.Count; i < 5; i++)
                        {
                            Flop_H();
                            Delay(500);
                        }
                    }
                    tmp = string.Format("GS,11,0000,"); //점수 보내라고 요청
                    DataSend(tmp);
                    decoder(tmp);
                    return;
                }
                else if (nextp == num && commit.Count < 3) //m_ClientSocket.Count + 1
                { //3장
                    for (int i = 0; i < 3; i++)
                    {
                        Flop_H();
                        Delay(300);
                    }
                }
                else if (nextp == num && commit.Count < 5)
                { //num대신 m_ClientSocket.Count+1
                  //1장
                    Flop_H(); //알아서 커뮤니티 보내주기
                }
                else if (listBox3.Items.Count == 0 && nextp == num && commit.Count == 5)
                { //모두 콜을 했고 이제 결과 확인
                    tmp = string.Format("GS,11,0000,"); //점수 보내라고 요청
                    DataSend(tmp);
                    decoder(tmp);
                }
                else
                {
                    tmp = string.Format("De,{0:D2},{1:D4}," + Pnames[turn] + ",", turn, other_bet);
                    DataSend(tmp);
                    decoder(tmp);
                }
            }
            catch (Exception)
            {
                if (num == 1)
                {
                    button4.Visible = true;
                    button4.Enabled = true;
                    Action_End();
                    GameClear();
                    money = startmoney; //초기머니
                    Invalidate();
                    return;
                }
            }
        }
        //페인트 ==============================================================
        private void PlayHold_em_host_Paint(object sender, PaintEventArgs e)
        {
            int pot_T = pot/1000; //pot 1000
            int pot_H = (pot%1000)/100; //pot 100
            int pot_D = (pot%100)/10; //pot 10
            int pot_b = pot%10; //pot 1

            int money_T = money/1000; //검 1000칩
            int money_H = (money%1000)/100; //노 100칩
            int money_D = (money%100)/10; //빨 10칩
            int money_b = money%10; //파란 1칩
            
            label25.Text = p0_bet.ToString();
            label26.Text = p1_bet.ToString();
            label27.Text = p2_bet.ToString();
            label28.Text = p3_bet.ToString();
            label6.Text = PlayerNum.ToString();
            label7.Text = money.ToString(); //내돈
            label8.Text = pot.ToString();
            label9.Text = my_bet.ToString();
            label12.Text = other_bet.ToString();
            e.Graphics.DrawImage(carddk, 100, 200, 80, 110);

            if (Hand.Count >= 2)
            {
                if (open)
                {
                        e.Graphics.DrawImage(cardmap[Hand[0].code], 344, 533 - 120, 80, 110);
                        e.Graphics.DrawImage(cardmap[Hand[1].code], 344 + 60, 533 - 120, 80, 110);
                        label13.Text = list[0].ToString();
                }
                else
                {
                    e.Graphics.DrawImage(close, 344, 533 - 120, 80, 110);
                    e.Graphics.DrawImage(close, 344 + 60, 533 - 120, 80, 110);
                }
            }
            if (turns)
            {
                e.Graphics.DrawImage(turnchip, 580, 320, 100, 100);
                label33.Text = tikcount.ToString();
            }

            //커뮤니티 패
            for (int i = 0; i < commit.Count; i++)
            {
                e.Graphics.DrawImage(cardmap[commit[i].code], 344-80 + (50 * i), 200, 80, 110);
            }

            //칩
            if (money > 0)
            {
                for (int t = 0; t < money_b; t++)
                {
                    e.Graphics.DrawImage(chip1, 650+220, 364 - (6 * (t + 1)), 50, 40);
                }
                for (int t = 0; t < money_H; t++)
                {
                    e.Graphics.DrawImage(chip100, 595+220, 365 - (6 * (t + 1)), 50, 40);
                }
                for (int t = 0; t < money_D; t++)
                {
                    e.Graphics.DrawImage(chip10, 635+220, 390 - (6 * (t + 1)), 50, 40);
                }
                for (int t = 0; t < money_T; t++)
                {
                    e.Graphics.DrawImage(chip1000, 570+220, 391 - (6 * (t + 1)), 50, 40);
                }
                if (money_T > 9)
                {
                    for (int t = 0; t < money_T - 9; t++)
                    {
                        e.Graphics.DrawImage(chip1000, 565+220, 393 - (6 * (t + 1)), 50, 40);
                    }
                }
            }
            if (winnerHands > 0)
            {
                e.Graphics.DrawImage(cardmap[winnerHands/100], 334, 30, 80, 110);
                e.Graphics.DrawImage(cardmap[winnerHands%100], 334+60, 30, 80, 110);
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
                    e.Graphics.DrawImage(chip10, 389, 144 - (6 * (t + 1)), 50,40);
                }
                for (int t = 0; t < pot_T; t++)
                {
                    e.Graphics.DrawImage(chip1000, 334, 145-(6*(t+1)), 50,40);
                }
                if (pot_T > 9)
                {
                    for (int t = 0; t < pot_T-9; t++)
                    {
                        e.Graphics.DrawImage(chip1000, 329, 147 - (6 * (t + 1)), 50, 40);
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
        private void TextBox3_MouseClick(object sender, MouseEventArgs e)
        {
            track = false;
        }

        private void PlayHold_em_host_FormClosing(object sender, FormClosingEventArgs e)
        {

            Application.Exit();
        }
        private void PlayHold_em_host_Load(object sender, EventArgs e)
        {
            
        }
        private void textBox2_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                String tmp = string.Format("Mg,{0:D2},", PlayerNum) + textBox2.Text + "," + Pname + ",";
                DataSend(tmp);
                decoder(tmp);
                textBox2.Text = "";
                e.SuppressKeyPress = true;
            }
        }
        private void PlayHold_em_host_MouseMove(object sender, MouseEventArgs e)
        {
            if(e.X>=344&&e.X<=344+140&&e.Y>=413&&e.Y<=523)
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