using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Card_Game_Simulations
{
    public partial class Form_Main : Form
    {
        public Form_Main()
        {
            InitializeComponent();

            FormClosed += Form_Main_FormClosed;
        }

        void button_Ult_Click(object sender, EventArgs e)
        {
            Form_UltimateTexasHoldem form_UltimateTexasHoldem = new Form_UltimateTexasHoldem();

            form_UltimateTexasHoldem.Show();

            Hide();
        }

        void Form_Main_Load(object sender, EventArgs e)
        {
            CenterToScreen();
        }
        void Form_Main_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
        }
    }
}
