namespace Hold_em_client
{
    partial class Client
    {
        /// <summary>
        /// 필수 디자이너 변수입니다.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 사용 중인 모든 리소스를 정리합니다.
        /// </summary>
        /// <param name="disposing">관리되는 리소스를 삭제해야 하면 true이고, 그렇지 않으면 false입니다.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form 디자이너에서 생성한 코드

        /// <summary>
        /// 디자이너 지원에 필요한 메서드입니다. 
        /// 이 메서드의 내용을 코드 편집기로 수정하지 마세요.
        /// </summary>
        private void InitializeComponent()
        {
            this.Form_close = new System.Windows.Forms.Button();
            this.Enter_Server = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.PORT = new System.Windows.Forms.TextBox();
            this.IP = new System.Windows.Forms.TextBox();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // Form_close
            // 
            this.Form_close.Location = new System.Drawing.Point(286, 40);
            this.Form_close.Name = "Form_close";
            this.Form_close.Size = new System.Drawing.Size(75, 23);
            this.Form_close.TabIndex = 4;
            this.Form_close.Text = "닫기";
            this.Form_close.UseVisualStyleBackColor = true;
            this.Form_close.Click += new System.EventHandler(this.Form_close_Click);
            // 
            // Enter_Server
            // 
            this.Enter_Server.Location = new System.Drawing.Point(286, 10);
            this.Enter_Server.Name = "Enter_Server";
            this.Enter_Server.Size = new System.Drawing.Size(75, 23);
            this.Enter_Server.TabIndex = 3;
            this.Enter_Server.Text = "서버 입장";
            this.Enter_Server.UseVisualStyleBackColor = true;
            this.Enter_Server.Click += new System.EventHandler(this.Enter_Server_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(3, 39);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(46, 12);
            this.label2.TabIndex = 9;
            this.label2.Text = "PORT :";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(26, 14);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(24, 12);
            this.label1.TabIndex = 8;
            this.label1.Text = "IP :";
            // 
            // PORT
            // 
            this.PORT.Location = new System.Drawing.Point(60, 37);
            this.PORT.Name = "PORT";
            this.PORT.Size = new System.Drawing.Size(176, 21);
            this.PORT.TabIndex = 2;
            this.PORT.Text = "1234";
            // 
            // IP
            // 
            this.IP.Location = new System.Drawing.Point(60, 10);
            this.IP.Name = "IP";
            this.IP.Size = new System.Drawing.Size(176, 21);
            this.IP.TabIndex = 1;
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(60, 62);
            this.textBox1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(153, 21);
            this.textBox1.TabIndex = 10;
            this.textBox1.Text = "(client)";
            // 
            // textBox2
            // 
            this.textBox2.Location = new System.Drawing.Point(274, 69);
            this.textBox2.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.textBox2.Name = "textBox2";
            this.textBox2.Size = new System.Drawing.Size(88, 21);
            this.textBox2.TabIndex = 11;
            this.textBox2.Text = "1000";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(3, 65);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(49, 12);
            this.label3.TabIndex = 12;
            this.label3.Text = "닉네임 :";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(236, 71);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(29, 12);
            this.label4.TabIndex = 13;
            this.label4.Text = "칩 : ";
            // 
            // Client
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ClientSize = new System.Drawing.Size(374, 130);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.textBox2);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.Form_close);
            this.Controls.Add(this.Enter_Server);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.PORT);
            this.Controls.Add(this.IP);
            this.MaximumSize = new System.Drawing.Size(390, 169);
            this.MinimumSize = new System.Drawing.Size(390, 169);
            this.Name = "Client";
            this.Text = "Hold\'em(Client)";
            this.Load += new System.EventHandler(this.FormLoaad);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button Form_close;
        private System.Windows.Forms.Button Enter_Server;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox PORT;
        private System.Windows.Forms.TextBox IP;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        public System.Windows.Forms.TextBox textBox1;
        public System.Windows.Forms.TextBox textBox2;
    }
}

