using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using Hold_em_client;
using Hold_em_server;
namespace Shanghai_hold_em
{
    public partial class Main : Form
    {
        public Main()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Process.Start("Hold'em_server");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Process.Start("Hold'em_client");

        }
    }
}
