using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace NWebSpider
{
    public partial class Form1 : Form
    {
        private static Form1 _instance;
        public Form1()
        {
            InitializeComponent();
            _instance = this;
            System.Windows.Forms.Control.CheckForIllegalCrossThreadCalls = false;

        }
        public static Form1 getInstance()
        {
            return Form1._instance;
        }

        UrlThread urlthread;
        private void button1_Click(object sender, EventArgs e)
        {
            timer1.Enabled = true;
            startTime = Environment.TickCount;
            timer1.Start();
            urlthread = new UrlThread(textBox1.Text, Convert.ToInt32(numericUpDown1.Value));
            urlthread.Start();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            System.Environment.Exit(0);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (button2.Text == "暂停")
            {
                button2.Text = "继续";
                urlthread.Suspend();
                timer1.Stop();
            }
            else
            {
                button2.Text = "暂停";
                urlthread.Resume();
                startTime = Environment.TickCount;
                timer1.Start();
            }
        }

        int startTime = Environment.TickCount;
        private void timer1_Tick(object sender, EventArgs e)
        {

            label4.Text = "耗时：" + ((Environment.TickCount - startTime) * 0.001).ToString();
        }
        Bitmap bmp;
        //byte[] srcHash = new byte[64]; 
        private void button3_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            try
            {
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    bmp = new Bitmap(ofd.FileName);
                    pictureBox1.Image = bmp;
                    bmp = new Bitmap(bmp, 8, 8);
                    ImageClass.SrcHash = ImageClass.ImageHash(bmp);
                }
            }
            catch (Exception)
            {
                MessageBox.Show("图片不符合要求！");
            }
        }
    }
}
