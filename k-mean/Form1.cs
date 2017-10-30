using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace k_mean
{
    public struct Cluster {
        public SolidBrush centerBrush { get; set; }
        public SolidBrush groupBrush { get; set; }
        public int cluster { get; set; }
        public Point centerPoint { get; set; }
        public List<Point> groupPoint { get; set; }
    }
    public partial class Form1 : Form
    {
        public Random rnd = new Random(Guid.NewGuid().GetHashCode());
        public Graphics graphics;
        public Color[] _color = { Color.Red, Color.Green,Color.Yellow };
        public Pen blackPen = new Pen(Color.Black);
        public List<Cluster> clusterList = new List<Cluster>();
        public int N = 0, C = 0;
        public Thread work;
        public string msg = "";

        public delegate void Dlgt_WriteListBox(string msg, ListBox listbox, bool bAutoScroll);

        public static void WriteListBox(string msg, ListBox listbox, bool bAutoScroll)
        {
            try
            {
                if (listbox.InvokeRequired)
                {
                    listbox.BeginInvoke(new Dlgt_WriteListBox(WriteListBox), new object[] { msg, listbox, bAutoScroll });
                }
                else
                {
                    listbox.Items.Add(msg);
                    if(bAutoScroll)
                        listbox.TopIndex = listbox.Items.Count - 1;
                }
            }catch(Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        public Form1()
        {
            InitializeComponent();
            work = new Thread(doWork);

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            graphics = pictureBox1.CreateGraphics();
            textBox1.Text = "50";
            textBox2.Text = "2";
            msg ="等待執行...";
            WriteListBox(msg, listBox1, true);
            work.IsBackground = true;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            listBox1.Items.Clear();
            try
            {
                if (!work.IsAlive)
                {
                    work.Start();
                }
            }catch(Exception ex){

            }
        }

        public void doWork()
        {
            if (textBox1.Text != "" && IsNumeric(textBox1.Text) &&
                textBox2.Text != "" && IsNumeric(textBox2.Text))
            {
                N = int.Parse(textBox1.Text);
                C = int.Parse(textBox2.Text);
                graphics.Clear(Color.White);
                int width = pictureBox1.Width;
                int height = pictureBox1.Height;
                List<Point> circlePoint = new List<Point>();
                List<Point> rndPoint = new List<Point>();
                msg = "亂數生成點....";
                WriteListBox(msg, listBox1, true);
                for (int i = 0; i < N; i++)
                {
                    int x = rnd.Next(0, width - 10);
                    int y = rnd.Next(0, height - 10);
                    Point _point = new Point(x, y);
                    circlePoint.Add(_point);
                    graphics.DrawEllipse(blackPen, x, y, 10, 10);
                    Thread.Sleep(10);
                }
                msg = "生成完成，N= " + N + " ,Clustering= " + C;
                WriteListBox(msg, listBox1, true);
                for (int i = 0; i < C; i++)
                {
                    int x = rnd.Next(0, width - 10);
                    int y = rnd.Next(0, height - 10);
                    Point _point = new Point(x, y);
                    rndPoint.Add(_point);
                    SolidBrush _group = new SolidBrush(_color[i]);
                    SolidBrush _center = new SolidBrush(Color.Yellow);
                    graphics.FillEllipse(_center, x, y, 10, 10);
                    clusterList.Add(new Cluster { cluster = i, centerBrush = _center, groupBrush = _group, centerPoint = _point, groupPoint = new List<Point>() });
                }
                msg = "第一階段亂數中心點已生成";
                WriteListBox(msg, listBox1, true);
                double[] Dis = new double[C];
                msg = "第一階段分群開始...";
                WriteListBox(msg, listBox1, true);
                for (int i = 0; i < N; i++)
                {
                    for (int j = 0; j < C; j += 2)
                    {
                        double D1 = calcDistance(circlePoint[i], rndPoint[j]);
                        double D2 = calcDistance(circlePoint[i], rndPoint[j + 1]);
                        if (D1 < D2)
                        {
                            clusterList[j].groupPoint.Add(circlePoint[i]);
                            graphics.FillEllipse(clusterList[j].groupBrush, circlePoint[i].X, circlePoint[i].Y, 10, 10);
                            Thread.Sleep(10);
                        }
                        else
                        {
                            clusterList[j + 1].groupPoint.Add(circlePoint[i]);
                            graphics.FillEllipse(clusterList[j + 1].groupBrush, circlePoint[i].X, circlePoint[i].Y, 10, 10);
                            Thread.Sleep(10);
                        }
                    }
                }
                msg = "第一階段分群完成...";
                WriteListBox(msg, listBox1, true);
                bool isCenterChange = true;
                while (isCenterChange)
                {
                    List<Point> tempCenter = new List<Point>();
                    for (int i = 0; i < C; i++)
                    {
                        graphics.FillEllipse(clusterList[i].groupBrush, clusterList[i].centerPoint.X, clusterList[i].centerPoint.Y, 10, 10);
                        tempCenter.Add(clusterList[i].centerPoint);
                    }
                    msg = "尋找各群新中心點...";
                    WriteListBox(msg, listBox1, true);
                    findCenter();
                    msg = "找尋完成...";
                    WriteListBox(msg, listBox1, true);
                    for (int i = 0; i < C; i++)
                    {
                        clusterList[i].groupPoint.Clear();
                        if (tempCenter[i].Equals(clusterList[i].centerPoint))
                        {
                            msg = "各群中心點無改變...";
                            WriteListBox(msg, listBox1, true);
                            isCenterChange = false;
                        }
                        else
                        {
                            SolidBrush _center = new SolidBrush(Color.Yellow);
                            graphics.FillEllipse(_center, clusterList[i].centerPoint.X, clusterList[i].centerPoint.Y, 10, 10);
                            break;
                        }
                    }
                    if (!isCenterChange)
                        break;
                    msg = "中心點有改變，將重新分群...";
                    WriteListBox(msg, listBox1, true);
                    for (int i = 0; i < N; i++)
                    {
                        for (int j = 0; j < C; j += 2)
                        {
                            double D1 = calcDistance(circlePoint[i], clusterList[j].centerPoint);
                            double D2 = calcDistance(circlePoint[i], clusterList[j + 1].centerPoint);
                            if (D1 < D2)
                            {
                                clusterList[j].groupPoint.Add(circlePoint[i]);
                                graphics.FillEllipse(clusterList[j].groupBrush, circlePoint[i].X, circlePoint[i].Y, 10, 10);
                                Thread.Sleep(10);
                            }
                            else
                            {
                                clusterList[j + 1].groupPoint.Add(circlePoint[i]);
                                graphics.FillEllipse(clusterList[j + 1].groupBrush, circlePoint[i].X, circlePoint[i].Y, 10, 10);
                                Thread.Sleep(10);
                            }
                        }
                    }
                    msg = "完成階段分群...";
                    WriteListBox(msg, listBox1, true);
                }
            }
            msg = "等待執行...";
            WriteListBox(msg, listBox1, true);
            work.Abort();
        }

        public double calcDistance(Point p1,Point p2)
        {
            return Math.Sqrt(Math.Pow((p1.X - p2.X),2) + Math.Pow((p1.Y - p2.Y),2));
        }

        public void findCenter()
        {
            for(int i = 0;i<C;i++)
            {
                int size = clusterList[i].groupPoint.Count(), x = 0, y = 0;
                for (int j = 0; j < size; j++)
                {
                    x += clusterList[i].groupPoint[j].X;
                    y += clusterList[i].groupPoint[j].Y;
                }
                Point newCenter = new Point(x / size, y / size);

                Cluster temp = clusterList[i];
                temp.centerPoint = newCenter;
                clusterList[i] = temp;
            } 
        }

        public static bool IsNumeric(string TextBoxValue)
        {
            try
            {
                int i = Convert.ToInt32(TextBoxValue);
                return true;
            }
            catch
            {
                try
                {
                    double i = Convert.ToDouble(TextBoxValue);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }
    }
}
