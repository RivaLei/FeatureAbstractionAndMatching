using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ImageMatch
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
        }

        int[,] tezhengzhi;
        int[,] tezheng;
        double[,] tezhengmuban;

        private void button1_Click(object sender, EventArgs e)
        {
            //设置文件的类型

            openFileDialog1.Filter = "*.jpg,*.jpeg,*.bmp,*.gif,*.ico,*.png,*.tif,*.wmf|*.jpg;*.jpeg;*.bmp;*.gif;*.ico;*.png;*.tif;*.wmf";

            if (openFileDialog1.ShowDialog() == DialogResult.OK)                            //打开文件对话框
            {
                //根据文件的路径创建Image对象

                Image myImage = System.Drawing.Image.FromFile(openFileDialog1.FileName);

                pictureBox1.Image = myImage;                                          //显示打开的图片
                this.button2.Enabled = true;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Image myImage = System.Drawing.Image.FromFile(openFileDialog1.FileName);
            pictureBox1.Image = myImage;                                          //显示打开的图片

            //toolStripProgressBar1.Visible = true;   //进度条可视   
            //toolStripProgressBar1.Maximum = 7;    //设置进度条最大长度值
            //toolStripProgressBar1.Value = 0;        //设置进度条当前值
            //toolStripProgressBar1.Step = 1;        //设置进度条步长
            //toolStripProgressBar1.Value += toolStripProgressBar1.Step;     //进度条前进

            int Var_H = pictureBox1.Image.Height;                          //获取图象的高度

            int Var_W = pictureBox1.Image.Width;                           //获取图象的宽度

            Bitmap Var_bmp = (Bitmap)pictureBox1.Image;                    //根据图象的大小创建Bitmap对象

            double[,] huiduzhi = new double[Var_W, Var_H];                       //用于存储各点灰度值

            for (int i = 0; i < Var_W; i++)
            {
                for (int j = 0; j < Var_H; j++)
                {
                    Color tem_color = Var_bmp.GetPixel(i, j);              //获取当前像素的颜色值
                    huiduzhi[i, j] = tem_color.R * 0.299 + tem_color.G * 0.587 + tem_color.B * 0.114;       //各点灰度值
                }
            }


            //toolStripProgressBar1.Value += toolStripProgressBar1.Step;

            double[,] xingquzhi = new double[Var_W, Var_H];                      //用于存储各点兴趣值

            for (int i = 2; i < Var_W - 2; i++)
            {
                for (int j = 2; j < Var_H - 2; j++)
                {
                    double V1 = 0;
                    for (int m = 0; m < 4; m++)
                    {

                        V1 = V1 + Math.Pow(huiduzhi[i - 2 + m, j] - huiduzhi[i - 1 + m, j], 2);    //计算V1方向相邻像素灰度差平方和
                    }
                    double V2 = 0;
                    for (int m = 0; m < 4; m++)
                    {
                        V2 = V2 + Math.Pow(huiduzhi[i - 2 + m, j - 2 + m] - huiduzhi[i - 1 + m, j - 1 + m], 2);    //计算V2方向相邻像素灰度差平方和
                    }
                    double V3 = 0;
                    for (int m = 0; m < 4; m++)
                    {
                        double ttt = huiduzhi[i, j - 2 + m];
                        V3 = V3 + Math.Pow(huiduzhi[i, j - 2 + m] - huiduzhi[i, j - 1 + m], 2);    //计算V3方向相邻像素灰度差平方和
                    }
                    double V4 = 0;
                    for (int m = 0; m < 4; m++)
                    {
                        V4 = V4 + Math.Pow(huiduzhi[i - 2 + m, j + 2 - m] - huiduzhi[i - 1 + m, j + 1 - m], 2);    //计算V4方向相邻像素灰度差平方和
                    }
                    xingquzhi[i, j] = Math.Min(Math.Min(Math.Min(V1, V2), V3), V4);
                    double tttt = xingquzhi[i, j];//从V1、V2、V3、V4中取最小值作为该点兴趣值
                }
            }

            //toolStripProgressBar1.Value += toolStripProgressBar1.Step;

            double sum = 0;
            for (int i = 0; i < Var_W; i++)
            {
                for (int j = 0; j < Var_H; j++)
                {
                    sum += xingquzhi[i, j];
                }
            }
            double pingjunzhi = sum / (Var_W * Var_H);

            //toolStripProgressBar1.Value += toolStripProgressBar1.Step;

            int houxuan = 0;                                                     //统计候选特征点数目
            int c = 101;                                        //获取准确特征点数目     
            double zeng = 4;

            while (c > 100)
            {
                c = 0;
                double yuzhi = pingjunzhi * zeng;    //设定阈值
                double[,] jianding = new double[Var_W, Var_H];
                for (int i = 0; i < Var_W; i++)
                {
                    for (int j = 0; j < Var_H; j++)
                    {
                        if (xingquzhi[i, j] <= yuzhi)
                        {
                            jianding[i, j] = 0;        //选取兴趣值大于阈值的点作为特征候选点，其他点兴趣值归零
                        }
                        else
                        {
                            jianding[i, j] = 1;
                            houxuan++;
                        }
                    }
                }

                int[,] tezhengzhi1 = new int[houxuan, 2];         //假定一个数组能容纳所有点皆为特征点的像素坐标矩阵
                int yuzhimuban = 5;                                 //定义阈值模板
                int mubanbanchuang = 2;

                for (int i = mubanbanchuang; i < Var_W - mubanbanchuang; i = i + yuzhimuban)
                {
                    for (int j = mubanbanchuang; j < Var_H - mubanbanchuang; j = j + yuzhimuban)
                    {
                        double MAX = 0;                          //假定5*5模板最大值起始值为第一个元素值
                        int a = 0;                                          //设a为最大值行
                        int b = 0;                                          //设b为最大值列
                        for (int m = 0; m < yuzhimuban; m++)
                        {
                            for (int n = 0; n < yuzhimuban; n++)
                            {
                                if (jianding[i - mubanbanchuang + m, j - mubanbanchuang + n] == 1)
                                {
                                    if (MAX < xingquzhi[i - mubanbanchuang + m, j - mubanbanchuang + n])
                                    {

                                        MAX = xingquzhi[i - mubanbanchuang + m, j - mubanbanchuang + n];    //获取5*5模板中最大值
                                        a = i - mubanbanchuang + m;                            //获取最大值列
                                        b = j - mubanbanchuang + n;                            //获取最大值行
                                    }
                                }
                                else
                                {
                                    a = 0; b = 0;
                                }
                            }
                        }
                        if ((a != 0) && (b != 0))
                        {
                            tezhengzhi1[c, 0] = a;             //存储特征点列
                            tezhengzhi1[c, 1] = b;             //存储特征点行
                            c++;                               //每有一个既不为0也不重复的最大值特征点数目加一   
                        }
                    }
                }
                tezhengzhi = tezhengzhi1;
                zeng += 1;
            }



            //toolStripProgressBar1.Value += toolStripProgressBar1.Step;
            //toolStripProgressBar1.Value += toolStripProgressBar1.Step;

            int[,] tezhengdian2 = new int[c, 2];      //定义一个数组存储像素坐标
            for (int i = 0; i < c; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    tezhengdian2[i, j] = tezhengzhi[i, j];
                }
            }
            tezheng = tezhengdian2;

            Image img = pictureBox1.Image;                         //将pictureBox1中图像存储入另一个变量
            Bitmap bmp = new Bitmap(img.Width, img.Height);        //创建Bitmap对象
            Graphics g = Graphics.FromImage(bmp);                  //创建Graphics对象
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;     //设置高质量双三次插值法 
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;                    //设置高质量,低速度呈现平滑程度
            g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;          //设置绘制到此 Graphics 的合成图像的呈现质量
            g.DrawImage(img, 0, 0, img.Width, img.Height);         //以img为原本重新于（0,0）点绘制
            g.Dispose();                                           //释放资源
            for (int i = 0; i < c; i++)
            {
                Graphics myGraphics = Graphics.FromImage(bmp);    //创建Graphics对象
                myGraphics.DrawLine(new Pen(Color.Red, 1), new Point(tezhengdian2[i, 0], tezhengdian2[i, 1] - 5), new Point(tezhengdian2[i, 0], tezhengdian2[i, 1] + 5));    //画出竖直方向直线
                myGraphics.DrawLine(new Pen(Color.Red, 1), new Point(tezhengdian2[i, 0] - 5, tezhengdian2[i, 1]), new Point(tezhengdian2[i, 0] + 5, tezhengdian2[i, 1]));    //画出水平方向直线
                myGraphics.Dispose();                                 //释放资源
                pictureBox1.Image = bmp;                          //显示含有“+”的图
            }

            //toolStripProgressBar1.Value += toolStripProgressBar1.Step;

            ////新建一个datatable用于保存读入的数据
            //DataTable dt = new DataTable();
            ////给datatable添加二个列
            //dt.Columns.Add("列", typeof(String));
            //dt.Columns.Add("行", typeof(String));
            //for (int i = 0; i < c; i++)
            //{
            //    DataRow dr = dt.NewRow();
            //    dr[0] = tezhengdian2[i, 0];                        //将列数据赋给表
            //    dr[1] = tezhengdian2[i, 1];                         //将行数据赋给表
            //    dt.Rows.Add(dr);                                    //将这行数据加入到datatable中
            //}
            //this.dataGridView1.DataSource = dt;                 //将datatable绑定到datagridview上显示结果
            //dataGridView1.AllowUserToAddRows = false;

            //groupBox1.Text = "特征点像素坐标" + "(" + c.ToString() + "个" + ")";
            //toolStripProgressBar1.Visible = false;              //隐藏进度条

            //MessageBox.Show("共有" + c.ToString() + "个特征点！");
            this.button3.Enabled = true;

            double[,] tezhengmuban2 = new double[c, 25];
            for (int i = 0; i < c; i++)
            {
                int t = 0;
                for (int m = 0; m < 5; m++)
                {
                    for (int n = 0; n < 5; n++)
                    {
                        tezhengmuban2[i, t] = huiduzhi[tezhengdian2[i, 0] - 2 + m, tezhengdian2[i, 1] - 2 + n];    //存储5*5模板灰度
                        t++;
                    }
                }
            }
            tezhengmuban = tezhengmuban2;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            //设置文件的类型

            openFileDialog2.Filter = "*.jpg,*.jpeg,*.bmp,*.gif,*.ico,*.png,*.tif,*.wmf|*.jpg;*.jpeg;*.bmp;*.gif;*.ico;*.png;*.tif;*.wmf";

            if (openFileDialog2.ShowDialog() == DialogResult.OK)                            //打开文件对话框
            {
                //根据文件的路径创建Image对象

                Image myImage2 = System.Drawing.Image.FromFile(openFileDialog2.FileName);

                pictureBox2.Image = myImage2;                                          //显示打开的图片
                this.button4.Enabled = true;
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Image myImage2 = System.Drawing.Image.FromFile(openFileDialog2.FileName);
            pictureBox2.Image = myImage2;                                          //显示打开的图片

            //toolStripProgressBar1.Visible = true;   //进度条可视
            //toolStripProgressBar1.Maximum = 4;    //设置进度条最大长度值
            //toolStripProgressBar1.Value = 0;        //设置进度条当前值
            //toolStripProgressBar1.Step = 1;        //设置进度条步长
            //toolStripProgressBar1.Value += toolStripProgressBar1.Step;     //进度条前进

            int Var_H = pictureBox2.Image.Height;                          //获取图象的高度

            int Var_W = pictureBox2.Image.Width;                           //获取图象的宽度

            Bitmap Var_bmp = (Bitmap)pictureBox2.Image;                    //根据图象的大小创建Bitmap对象

            double[,] huiduzhi = new double[Var_W, Var_H];                       //用于存储各点灰度值

            for (int i = 0; i < Var_W; i++)
            {
                for (int j = 0; j < Var_H; j++)
                {
                    Color tem_color = Var_bmp.GetPixel(i, j);              //获取当前像素的颜色值
                    huiduzhi[i, j] = tem_color.R * 0.299 + tem_color.G * 0.587 + tem_color.B * 0.114;       //各点灰度值
                }
            }

            //toolStripProgressBar1.Value += toolStripProgressBar1.Step;

            double[,] zuobiao = new double[tezheng.GetLength(0), 5];    //存储特征点对应坐标与系数
            for (int i = 0; i < zuobiao.GetLength(0); i++)
            {
                zuobiao[i, 0] = tezheng[i, 0];
                zuobiao[i, 1] = tezheng[i, 1];
            }

            int dianshu = 0;
            for (int c = 0; c < tezhengmuban.GetLength(0) ; c++)
            {
                double maxxishu = 0;    //最大系数
                int a = 0;    //存储系数最大点列
                int b = 0;    //存储系数最大点行
                for (int i = 2; i < Var_W - 2 - 400; i++)
                {
                    for (int j = 2; j < Var_H - 2 - 400; j++)
                    {
                        double chenghe = 0;    //各点灰度值相乘总和
                        int t = 0;
                        for (int m = 0; m < 5; m++)
                        {
                            for (int n = 0; n < 5; n++)
                            {

                                double sdavx = tezhengmuban[c, t];
                                chenghe += tezhengmuban[c, t] * huiduzhi[i - 2 + m, j - 2 + n];




                                t++;
                            }
                        }
                        double bhx = chenghe;

                        double tezhenghe = 0;    //特征点灰度值和
                        double tezhengji = 0;    //特征点灰度值平方和
                        for (int m = 0; m < 25; m++)
                        {
                            tezhenghe += tezhengmuban[c, m];
                            tezhengji += Math.Pow(tezhengmuban[c, m], 2);
                        }

                        double fvs = tezhenghe;
                        double vhd = tezhengji;

                        double xiangsuhe = 0;    //像素点灰度值和
                        double xiangsuji = 0;    //像素点灰度值平方和
                        for (int m = 0; m < 5; m++)
                        {
                            for (int n = 0; n < 5; n++)
                            {
                                xiangsuhe += huiduzhi[i - 2 + m, j - 2 + n];
                                xiangsuji += Math.Pow(huiduzhi[i - 2 + m, j - 2 + n], 2);
                            }
                        }


                        double sfgd = xiangsuhe;
                        double ghjc = xiangsuji;
                        double xishu = 0;
                        xishu = (chenghe - tezhenghe * xiangsuhe / 25) / Math.Sqrt((tezhengji - Math.Pow(tezhenghe, 2) / 25) * (xiangsuji - Math.Pow(xiangsuhe, 2) / 25));
                        if (maxxishu < xishu)
                        {
                            maxxishu = xishu;
                            a = i; b = j;
                        }
                    }

                }

                double www = maxxishu;
                if ((maxxishu > 0) && (maxxishu <= 1))
                {
                    zuobiao[c, 2] = a;
                    zuobiao[c, 3] = b;
                    zuobiao[c, 4] = maxxishu;
                    dianshu++;
                }
            }
            //toolStripProgressBar1.Value += toolStripProgressBar1.Step;

            double[,] zuobiao2 = new double[dianshu, 5];    //明确存储特征点对应坐标与相关系数
            int de = 0;
            for (int i = 0; i < zuobiao.GetLength(0); i++)
            {
                if (zuobiao[i, 4] != 0)
                {
                    for (int j = 0; j < 5; j++)
                    {
                        zuobiao2[de, j] = zuobiao[i, j];

                    }
                    de++;
                }
            }

            Image img = pictureBox2.Image;                         //将pictureBox2中图像存储入另一个变量
            Bitmap bmp = new Bitmap(img.Width, img.Height);        //创建Bitmap对象
            Graphics g = Graphics.FromImage(bmp);                  //创建Graphics对象
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;     //设置高质量双三次插值法 
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;                    //设置高质量,低速度呈现平滑程度
            g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;          //设置绘制到此 Graphics 的合成图像的呈现质量
            g.DrawImage(img, 0, 0, img.Width, img.Height);         //以img为原本重新于（0,0）点绘制
            g.Dispose();                                           //释放资源
            for (int i = 0; i < dianshu; i++)
            {
                Graphics myGraphics = Graphics.FromImage(bmp);    //创建Graphics对象
                myGraphics.DrawLine(new Pen(Color.Red, 1), new Point(Convert.ToInt32(zuobiao2[i, 2]), Convert.ToInt32(zuobiao2[i, 3] - 5)), new Point(Convert.ToInt32(zuobiao2[i, 2]), Convert.ToInt32(zuobiao2[i, 3] + 5)));    //画出竖直方向直线
                myGraphics.DrawLine(new Pen(Color.Red, 1), new Point(Convert.ToInt32(zuobiao2[i, 2] - 5), Convert.ToInt32(zuobiao2[i, 3])), new Point(Convert.ToInt32(zuobiao2[i, 2]) + 5, Convert.ToInt32(zuobiao2[i, 3])));    //画出水平方向直线
                //myGraphics.DrawString(i.ToString(), new Font("华文行楷", 8), new SolidBrush(Color.DarkOrange), Convert.ToInt32(zuobiao2[i, 2]), Convert.ToInt32(zuobiao2[i, 3]));
                myGraphics.Dispose();                                 //释放资源
                pictureBox2.Image = bmp;                          //显示含有“+”的图
            }
            //toolStripProgressBar1.Value += toolStripProgressBar1.Step;

            ////新建一个datatable用于保存读入的数据
            //DataTable dt = new DataTable();
            ////给datatable添加二个列
            //dt.Columns.Add("列（左）", typeof(String));
            //dt.Columns.Add("行（左）", typeof(String));
            //dt.Columns.Add("列（右）", typeof(String));
            //dt.Columns.Add("行（右）", typeof(String));
            //dt.Columns.Add("相关系数", typeof(String));
            //for (int i = 0; i < zuobiao2.GetLength(0); i++)
            //{
            //    DataRow dr = dt.NewRow();
            //    dr[0] = zuobiao2[i, 0];                         //将列（左）数据赋给表
            //    dr[1] = zuobiao2[i, 1];                         //将行（左）数据赋给表
            //    dr[2] = zuobiao2[i, 2];
            //    dr[3] = zuobiao2[i, 3];
            //    dr[4] = zuobiao2[i, 4].ToString("F6");
            //    dt.Rows.Add(dr);                                    //将这行数据加入到datatable中
            //}
            //this.dataGridView1.DataSource = dt;                 //将datatable绑定到datagridview上显示结果
            //dataGridView1.AllowUserToAddRows = false;

            //groupBox1.Text = "影像匹配点坐标" + "(" + dianshu.ToString() + "个" + ")";
            //toolStripProgressBar1.Visible = false;              //隐藏进度条
        }

        private void button5_Click(object sender, EventArgs e)
        {
            Image myImage = System.Drawing.Image.FromFile(openFileDialog1.FileName);
            pictureBox1.Image = myImage;                                          //显示打开的图片

            //toolStripProgressBar1.Visible = true;   //进度条可视   
            //toolStripProgressBar1.Maximum = 7;    //设置进度条最大长度值
            //toolStripProgressBar1.Value = 0;        //设置进度条当前值
            //toolStripProgressBar1.Step = 1;        //设置进度条步长
            //toolStripProgressBar1.Value += toolStripProgressBar1.Step;     //进度条前进

            int Var_H = pictureBox1.Image.Height;                          //获取图象的高度

            int Var_W = pictureBox1.Image.Width;                           //获取图象的宽度

            Bitmap Var_bmp = (Bitmap)pictureBox1.Image;                    //根据图象的大小创建Bitmap对象

            double[,] huiduzhi = new double[Var_W, Var_H];                       //用于存储各点灰度值

            for (int i = 0; i < Var_W; i++)
            {
                for (int j = 0; j < Var_H; j++)
                {
                    Color tem_color = Var_bmp.GetPixel(i, j);              //获取当前像素的颜色值
                    huiduzhi[i, j] = tem_color.R * 0.299 + tem_color.G * 0.587 + tem_color.B * 0.114;       //各点灰度值
                }
            }


            //toolStripProgressBar1.Value += toolStripProgressBar1.Step;

            double[,] xingquzhi = new double[Var_W, Var_H];                      //用于存储各点兴趣值

            for (int i = 2; i < Var_W - 2; i++)
            {
                for (int j = 2; j < Var_H - 2; j++)
                {
                    double V1 = 0;
                    for (int m = 0; m < 4; m++)
                    {

                        V1 = V1 + Math.Pow(huiduzhi[i - 2 + m, j] - huiduzhi[i - 1 + m, j], 2);    //计算V1方向相邻像素灰度差平方和
                    }
                    double V2 = 0;
                    for (int m = 0; m < 4; m++)
                    {
                        V2 = V2 + Math.Pow(huiduzhi[i - 2 + m, j - 2 + m] - huiduzhi[i - 1 + m, j - 1 + m], 2);    //计算V2方向相邻像素灰度差平方和
                    }
                    double V3 = 0;
                    for (int m = 0; m < 4; m++)
                    {
                        double ttt = huiduzhi[i, j - 2 + m];
                        V3 = V3 + Math.Pow(huiduzhi[i, j - 2 + m] - huiduzhi[i, j - 1 + m], 2);    //计算V3方向相邻像素灰度差平方和
                    }
                    double V4 = 0;
                    for (int m = 0; m < 4; m++)
                    {
                        V4 = V4 + Math.Pow(huiduzhi[i - 2 + m, j + 2 - m] - huiduzhi[i - 1 + m, j + 1 - m], 2);    //计算V4方向相邻像素灰度差平方和
                    }
                    xingquzhi[i, j] = Math.Min(Math.Min(Math.Min(V1, V2), V3), V4);
                    double tttt = xingquzhi[i, j];//从V1、V2、V3、V4中取最小值作为该点兴趣值
                }
            }

            //toolStripProgressBar1.Value += toolStripProgressBar1.Step;

            double sum = 0;
            for (int i = 0; i < Var_W; i++)
            {
                for (int j = 0; j < Var_H; j++)
                {
                    sum += xingquzhi[i, j];
                }
            }
            double pingjunzhi = sum / (Var_W * Var_H);

            //toolStripProgressBar1.Value += toolStripProgressBar1.Step;

            int houxuan = 0;                                                     //统计候选特征点数目
            int c = 101;                                        //获取准确特征点数目     
            double zeng = 4;

            while (c > 100)
            {
                c = 0;
                double yuzhi = pingjunzhi * zeng;    //设定阈值
                double[,] jianding = new double[Var_W, Var_H];
                for (int i = 0; i < Var_W; i++)
                {
                    for (int j = 0; j < Var_H; j++)
                    {
                        if (xingquzhi[i, j] <= yuzhi)
                        {
                            jianding[i, j] = 0;        //选取兴趣值大于阈值的点作为特征候选点，其他点兴趣值归零
                        }
                        else
                        {
                            jianding[i, j] = 1;
                            houxuan++;
                        }
                    }
                }


                int[,] tezhengzhi1 = new int[houxuan, 2];         //假定一个数组能容纳所有点皆为特征点的像素坐标矩阵

                //5-1；15-9
                int yuzhimuban = 15;                                 //定义阈值模板
                int mubanbanchuang = 7;



                // int[,] tezhengzhi1 = new int[houxuan, 2];         //假定一个数组能容纳所有点皆为特征点的像素坐标矩阵
                //int yuzhimuban = 5;                                 //定义阈值模板
                //int mubanbanchuang = 2;
                for (int i = mubanbanchuang; i < Var_W - mubanbanchuang; i = i + yuzhimuban)
                {
                    for (int j = mubanbanchuang; j < Var_H - mubanbanchuang; j = j + yuzhimuban)
                    {

                      

                        
                                              
                        int [] a = new int [225];                                          //设a为最大值行
                        int[] b = new int[225];                                              //设b为最大值列
                        int chuangtiHouxuandianNum = 0;
                        double [] arrSortTemp = new double[225];//待排序兴趣值数组
                        for (int m = 0; m < yuzhimuban; m++)
                        {
                            for (int n = 0; n < yuzhimuban; n++)
                            {

                                //遍历阈值模板-冒泡法排序，得到前9个候选点作为特征点
                                if (jianding[i - mubanbanchuang + m, j - mubanbanchuang + n] == 1)
                                {
                                    arrSortTemp[chuangtiHouxuandianNum] = xingquzhi[i - mubanbanchuang + m, j - mubanbanchuang + n];
                                    a[chuangtiHouxuandianNum] = i - mubanbanchuang + m;                            //获取最大值列
                                    b[chuangtiHouxuandianNum] = j - mubanbanchuang + n;                            //获取最大值行
                                    chuangtiHouxuandianNum++;
                                   
                                  
                                }
                               
                            }
                        }
                        if ((a[0] != 0) && (b[0] != 0))//非空，含候选点
                        {
                            if (chuangtiHouxuandianNum<10)//无需排序
                            {
                                for (int p = 0; p< chuangtiHouxuandianNum;p++)
                                {
                                    tezhengzhi1[c, 0] = a[p];             //存储特征点列
                                    tezhengzhi1[c, 1] = b[p];             //存储特征点行
                                    c++;                               //每有一个既不为0也不重复的最大值特征点数目加一   

                                }
                            }
                            else//排序
                            {

                                double[] arrSort = new double[chuangtiHouxuandianNum];
                                int[] index = new int[chuangtiHouxuandianNum];
                                for (int fg = 0; fg <chuangtiHouxuandianNum; fg++)//拷贝
                                {
                                    arrSort[fg] = arrSortTemp[fg];
                                    index[fg] = fg; 
                                }
                                double temp;
                                int tempIndex;
                               
                                for (int drg = 0; drg < arrSort.Length; drg++)
                                {
                                    for (int h = drg + 1; h < arrSort.Length; h++)
                                    {
                                        if (arrSort[h] < arrSort[drg])
                                        {
                                            temp = arrSort[h];
                                            tempIndex = h;
                                            arrSort[h] = arrSort[drg];
                                            index[h] = index[drg];
                                            arrSort[drg] = temp;
                                            index[drg] =tempIndex;
                                        }
                                    }
                                }


                                //更新前9候选点作为特征点

                                for (int p = 0; p < 9; p++)
                                {
                                    tezhengzhi1[c, 0] = a[index[p]];             //存储特征点列
                                    tezhengzhi1[c, 1] = b[index[p]];             //存储特征点行
                                    c++;                               //每有一个既不为0也不重复的最大值特征点数目加一   

                                }


                            }

                          
                        }
                    }
                }

                //for (int i = mubanbanchuang; i < Var_W - mubanbanchuang; i = i + yuzhimuban)
                //{
                //    for (int j = mubanbanchuang; j < Var_H - mubanbanchuang; j = j + yuzhimuban)
                //    {
                //        double MAX = 0;                          //假定5*5模板最大值起始值为第一个元素值
                //        int a = 0;                                          //设a为最大值行
                //        int b = 0;                                          //设b为最大值列
                //        for (int m = 0; m < yuzhimuban; m++)
                //        {
                //            for (int n = 0; n < yuzhimuban; n++)
                //            {
                //                if (jianding[i - mubanbanchuang + m, j - mubanbanchuang + n] == 1)
                //                {
                //                    if (MAX < xingquzhi[i - mubanbanchuang + m, j - mubanbanchuang + n])
                //                    {

                //                        MAX = xingquzhi[i - mubanbanchuang + m, j - mubanbanchuang + n];    //获取5*5模板中最大值
                //                        a = i - mubanbanchuang + m;                            //获取最大值列
                //                        b = j - mubanbanchuang + n;                            //获取最大值行
                //                    }
                //                }
                //                else
                //                {
                //                    a = 0; b = 0;
                //                }
                //            }
                //        }
                //        if ((a != 0) && (b != 0))
                //        {
                //            tezhengzhi1[c, 0] = a;             //存储特征点列
                //            tezhengzhi1[c, 1] = b;             //存储特征点行
                //            c++;                               //每有一个既不为0也不重复的最大值特征点数目加一   
                //        }
                //    }
                //}
                tezhengzhi = tezhengzhi1;
                zeng += 1;
            }



            //toolStripProgressBar1.Value += toolStripProgressBar1.Step;
            //toolStripProgressBar1.Value += toolStripProgressBar1.Step;

            int[,] tezhengdian2 = new int[c, 2];      //定义一个数组存储像素坐标
            for (int i = 0; i < c; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    tezhengdian2[i, j] = tezhengzhi[i, j];
                }
            }
            tezheng = tezhengdian2;

            Image img = pictureBox1.Image;                         //将pictureBox1中图像存储入另一个变量
            Bitmap bmp = new Bitmap(img.Width, img.Height);        //创建Bitmap对象
            Graphics g = Graphics.FromImage(bmp);                  //创建Graphics对象
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;     //设置高质量双三次插值法 
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;                    //设置高质量,低速度呈现平滑程度
            g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;          //设置绘制到此 Graphics 的合成图像的呈现质量
            g.DrawImage(img, 0, 0, img.Width, img.Height);         //以img为原本重新于（0,0）点绘制
            g.Dispose();                                           //释放资源
            for (int i = 0; i < c; i++)
            {
                Graphics myGraphics = Graphics.FromImage(bmp);    //创建Graphics对象
                myGraphics.DrawLine(new Pen(Color.Red, 1), new Point(tezhengdian2[i, 0], tezhengdian2[i, 1] - 5), new Point(tezhengdian2[i, 0], tezhengdian2[i, 1] + 5));    //画出竖直方向直线
                myGraphics.DrawLine(new Pen(Color.Red, 1), new Point(tezhengdian2[i, 0] - 5, tezhengdian2[i, 1]), new Point(tezhengdian2[i, 0] + 5, tezhengdian2[i, 1]));    //画出水平方向直线
                myGraphics.Dispose();                                 //释放资源
                pictureBox1.Image = bmp;                          //显示含有“+”的图
            }

            //toolStripProgressBar1.Value += toolStripProgressBar1.Step;

            ////新建一个datatable用于保存读入的数据
            //DataTable dt = new DataTable();
            ////给datatable添加二个列
            //dt.Columns.Add("列", typeof(String));
            //dt.Columns.Add("行", typeof(String));
            //for (int i = 0; i < c; i++)
            //{
            //    DataRow dr = dt.NewRow();
            //    dr[0] = tezhengdian2[i, 0];                        //将列数据赋给表
            //    dr[1] = tezhengdian2[i, 1];                         //将行数据赋给表
            //    dt.Rows.Add(dr);                                    //将这行数据加入到datatable中
            //}
            //this.dataGridView1.DataSource = dt;                 //将datatable绑定到datagridview上显示结果
            //dataGridView1.AllowUserToAddRows = false;

            //groupBox1.Text = "特征点像素坐标" + "(" + c.ToString() + "个" + ")";
            //toolStripProgressBar1.Visible = false;              //隐藏进度条

            //MessageBox.Show("共有" + c.ToString() + "个特征点！");
            this.button3.Enabled = true;

            double[,] tezhengmuban2 = new double[c, 25];
            for (int i = 0; i < c; i++)
            {
                int t = 0;
                for (int m = 0; m < 5; m++)
                {
                    for (int n = 0; n < 5; n++)
                    {
                        tezhengmuban2[i, t] = huiduzhi[tezhengdian2[i, 0] - 2 + m, tezhengdian2[i, 1] - 2 + n];    //存储5*5模板灰度
                        t++;
                    }
                }
            }
            tezhengmuban = tezhengmuban2;
        }
    }
}
