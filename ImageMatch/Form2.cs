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
        //全局变量声明
        int[,] tempFeature;
        int[,] finalFeature;
        double[,] featureTemplate;
        double[,] gray_left;//左片灰度值
        int Ksize;//像素兴趣值计算窗口
        int k;//像素兴趣值计算窗口/2
        double multiple;//阈值迭代递增倍数
        int fpNum;//特征点数目
        int fpNumMax;//特征点数目最大值，循环终止条件
        double tExtract;//特征点提取阈值
        int Hsize;//非极大值抑制窗口大小
        int Asize;//均匀提取特征点窗口大小
        int afpNumMax;//均匀提取窗口特征值数量最大值
        double[,] gray_rihgt;//右片灰度值
        int Osize;//匹配左片目标窗口大小
        int o;//匹配左片目标窗口大小/2
        double tCorr;//相关系数阈值
        int X_l;//目视左右同名点，确定视差
        int Y_l;
        int X_r;
        int Y_r;
        double x_;//左右像片视差_x方向
        double y_; //左右像片视差_y方向
        int Ssize;//右片搜索窗口大小

        private void button1_Click(object sender, EventArgs e)//打开左图像
        {
            //设置文件的类型

            openFileDialog1.Filter = "*.jpg,*.jpeg,*.bmp,*.gif,*.ico,*.png,*.tif,*.wmf|*.jpg;*.jpeg;*.bmp;*.gif;*.ico;*.png;*.tif;*.wmf";

            if (openFileDialog1.ShowDialog() == DialogResult.OK)                            //打开文件对话框
            {
                //根据文件的路径创建Image对象

                Image myImage = System.Drawing.Image.FromFile(openFileDialog1.FileName);

                pictureBox1.Image = myImage;//显示打开的图片
                checkBox1.Enabled = true;
                checkBox2.Enabled = true;
                
            }

        }

        private void button2_Click(object sender, EventArgs e)//特征点提取
        {

            //交互输入赋值
            Ksize = Convert.ToInt32(textBox1.Text);
            k = Ksize / 2;
            fpNumMax = Convert.ToInt32(textBox3.Text);
            multiple = Convert.ToInt32(textBox2.Text);//阈值迭代递增倍数
            Hsize = Convert.ToInt32(textBox4.Text);
            int h = Hsize / 2;

            Image myImage = System.Drawing.Image.FromFile(openFileDialog1.FileName);
            pictureBox1.Image = myImage;                                          //显示打开的图片

            toolStripProgressBar1.Visible = true;   //进度条可视   
            toolStripProgressBar1.Maximum = 7;    //设置进度条最大长度值
            toolStripProgressBar1.Value = 0;        //设置进度条当前值
            toolStripProgressBar1.Step = 1;        //设置进度条步长
            toolStripProgressBar1.Value += toolStripProgressBar1.Step;     //进度条前进

            int Var_H = pictureBox1.Image.Height;                          //获取图象的高度

            int Var_W = pictureBox1.Image.Width;                           //获取图象的宽度

            Bitmap Var_bmp = (Bitmap)pictureBox1.Image;                    //根据图象的大小创建Bitmap对象

            gray_left = new double[Var_W, Var_H];                       //用于存储各点灰度值
            for (int i = 0; i < Var_W; i++)
            {
                for (int j = 0; j < Var_H; j++)
                {
                    Color tem_color = Var_bmp.GetPixel(i, j);              //获取当前像素的颜色值
                    gray_left[i, j] = tem_color.R * 0.299 + tem_color.G * 0.587 + tem_color.B * 0.114;       //各点灰度值
                }
            }


            toolStripProgressBar1.Value += toolStripProgressBar1.Step;

            double[,] xingquzhi = new double[Var_W, Var_H];                      //用于存储各点兴趣值            
            for (int i = k; i < Var_W - k; i++)
            {
                for (int j = k; j < Var_H - k; j++)//防止溢出
                {
                    double V1 = 0;
                    for (int m = 0; m < Ksize-1; m++)//防止溢出
                    {
                        V1 = V1 + Math.Pow(gray_left[i - k + m, j] - gray_left[i - k + m+1, j], 2);    //计算V1方向(0°)相邻像素灰度差平方和
                    }
                    double V2 = 0;
                    for (int m = 0; m < Ksize-1; m++)
                    {
                        V2 = V2 + Math.Pow(gray_left[i - k + m, j - k + m] - gray_left[i - k + m+1, j - k + m+1], 2);    //计算V2方向(45°_顺时针标记角度）相邻像素灰度差平方和
                    }
                    double V3 = 0;
                    for (int m = 0; m < Ksize-1; m++)
                    {
                       
                        V3 = V3 + Math.Pow(gray_left[i, j - k + m] - gray_left[i, j - k + m+1], 2);    //计算V3方向（90°)相邻像素灰度差平方和
                    }
                    double V4 = 0;
                    for (int m = 0; m < Ksize-1; m++)
                    {
                        V4 = V4 + Math.Pow(gray_left[i - k + m, j + k - m] - gray_left[i - k + m+1, j + k - m-1], 2);    //计算V4方向(135°）相邻像素灰度差平方和
                    }
                    xingquzhi[i, j] = Math.Min(Math.Min(Math.Min(V1, V2), V3), V4);//从V1、V2、V3、V4中取最小值作为该点兴趣值
                }
            }

            toolStripProgressBar1.Value += toolStripProgressBar1.Step;

            double sum = 0;
            for (int i = 0; i < Var_W; i++)
            {
                for (int j = 0; j < Var_H; j++)
                {
                    sum += xingquzhi[i, j];
                }
            }
            double average_gray = sum / (Var_W * Var_H);//阈值初步设定基础：灰度值均值

            toolStripProgressBar1.Value += toolStripProgressBar1.Step;

            int houxuan = 0;                                                     //统计候选特征点数目         
            fpNum = fpNumMax + 1;                                        //初始值设定，用以除此进入特征点提取循环                          
            while (fpNum > fpNumMax)
            {
                fpNum = 0;
                tExtract = average_gray * multiple;    //设定阈值
                double[,] houxuanFlag = new double[Var_W, Var_H];//是否为候选点标记矩阵
                for (int i = 0; i < Var_W; i++)
                {
                    for (int j = 0; j < Var_H; j++)
                    {
                        if (xingquzhi[i, j] <= tExtract)
                        {
                            houxuanFlag[i, j] = 0;        //选取兴趣值大于阈值的点作为特征候选点，其他点兴趣值归零
                        }
                        else
                        {
                            houxuanFlag[i, j] = 1;
                            houxuan++;
                        }
                    }
                }

                int[,] temp = new int[houxuan, 2];         //假定一个数组能容纳所有点皆为特征点的像素坐标矩阵
                

                for (int i = h; i < Var_W - h; i = i + Hsize)
                {
                    for (int j = h; j < Var_H - h; j = j + Hsize)
                    {
                        double MAX = 0;                          //假定滑动窗口最大值起始值为第一个元素值
                        int a = 0;                                          //设a为最大值行
                        int b = 0;                                          //设b为最大值列
                        for (int m = 0; m < Hsize; m++)
                        {
                            for (int n = 0; n < Hsize; n++)
                            {
                                if (houxuanFlag[i - h + m, j - h + n] == 1)
                                {
                                    if (MAX < xingquzhi[i - h + m, j - h + n])
                                    {
                                        MAX = xingquzhi[i - h + m, j - h + n];    //获取滑动窗口中最大值
                                        a = i - h + m;                            //获取最大值列
                                        b = j - h + n;                            //获取最大值行
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
                            temp[fpNum, 0] = a;             //存储特征点列
                            temp[fpNum, 1] = b;             //存储特征点行
                            fpNum++;                               //每有一个既不为0也不重复的最大值特征点数目加一   
                        }
                    }
                }
                tempFeature = temp;
                multiple += 1;
            }



            toolStripProgressBar1.Value += toolStripProgressBar1.Step;
            toolStripProgressBar1.Value += toolStripProgressBar1.Step;

            int[,] final = new int[fpNum, 2];      //定义一个数组存储像素坐标
            for (int i = 0; i < fpNum; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    final[i, j] = tempFeature[i, j];
                }
            }
            finalFeature = final;

            //特征点绘制

            Image img = pictureBox1.Image;                         //将pictureBox1中图像存储入另一个变量
            Bitmap bmp = new Bitmap(img.Width, img.Height);        //创建Bitmap对象
            Graphics g = Graphics.FromImage(bmp);                  //创建Graphics对象
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;     //设置高质量双三次插值法 
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;                    //设置高质量,低速度呈现平滑程度
            g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;          //设置绘制到此 Graphics 的合成图像的呈现质量
            g.DrawImage(img, 0, 0, img.Width, img.Height);         //以img为原本重新于（0,0）点绘制
            g.Dispose();                                           //释放资源
            for (int i = 0; i < fpNum; i++)
            {
                Graphics myGraphics = Graphics.FromImage(bmp);    //创建Graphics对象
                myGraphics.DrawLine(new Pen(Color.Red, 1), new Point(final[i, 0], final[i, 1] - 5), new Point(final[i, 0], final[i, 1] + 5));    //画出竖直方向直线
                myGraphics.DrawLine(new Pen(Color.Red, 1), new Point(final[i, 0] - 5, final[i, 1]), new Point(final[i, 0] + 5, final[i, 1]));    //画出水平方向直线
                myGraphics.Dispose();                                 //释放资源
                pictureBox1.Image = bmp;                          //显示含有“+”的图
            }

            toolStripProgressBar1.Value += toolStripProgressBar1.Step;

            
            toolStripProgressBar1.Visible = false;              //隐藏进度条

            MessageBox.Show("共有" + fpNum.ToString() + "个特征点！");
            label12.Text = fpNum.ToString();
            label11.Visible = true;
            button3.Enabled = true;
   

            
        }

        private void button3_Click(object sender, EventArgs e)//打开右片
        {
            //设置文件的类型

            openFileDialog2.Filter = "*.jpg,*.jpeg,*.bmp,*.gif,*.ico,*.png,*.tif,*.wmf|*.jpg;*.jpeg;*.bmp;*.gif;*.ico;*.png;*.tif;*.wmf";

            if (openFileDialog2.ShowDialog() == DialogResult.OK)                            //打开文件对话框
            {
                //根据文件的路径创建Image对象

                Image myImage2 = System.Drawing.Image.FromFile(openFileDialog2.FileName);

                pictureBox2.Image = myImage2;//显示打开的图片
                checkBox3.Enabled = true;
                checkBox4.Enabled = true;
               
            }
            
        }

        private void button4_Click(object sender, EventArgs e)//影像匹配
        {

            //交互操作赋值

            Osize = Convert.ToInt32(textBox6.Text); ;
            o = Osize / 2;
            tCorr = Convert.ToDouble(textBox7.Text); ;

            //左片特征模板
            
            double[,] featureTemplate_temp = new double[fpNum, Osize * Osize];
            for (int i = 0; i < fpNum; i++)
            {
                int t = 0;
                for (int m = 0; m < Osize; m++)
                {
                    for (int n = 0; n < Osize; n++)
                    {
                        featureTemplate_temp[i, t] = gray_left[finalFeature[i, 0] - o + m, finalFeature[i, 1] - o + n];    //存储目标窗口灰度
                        t++;
                    }
                }
            }
            featureTemplate = featureTemplate_temp;

            Image myImage2 = System.Drawing.Image.FromFile(openFileDialog2.FileName);
            pictureBox2.Image = myImage2;                                          //显示打开的图片

            toolStripProgressBar1.Visible = true;   //进度条可视
            toolStripProgressBar1.Maximum = 4;    //设置进度条最大长度值
            toolStripProgressBar1.Value = 0;        //设置进度条当前值
            toolStripProgressBar1.Step = 1;        //设置进度条步长
            toolStripProgressBar1.Value += toolStripProgressBar1.Step;     //进度条前进

            int Var_H = pictureBox2.Image.Height;                          //获取图象的高度

            int Var_W = pictureBox2.Image.Width;                           //获取图象的宽度

            Bitmap Var_bmp = (Bitmap)pictureBox2.Image;                    //根据图象的大小创建Bitmap对象

            gray_rihgt = new double[Var_W, Var_H];                       //用于存储各点灰度值

            for (int i = 0; i < Var_W; i++)
            {
                for (int j = 0; j < Var_H; j++)
                {
                    Color tem_color = Var_bmp.GetPixel(i, j);              //获取当前像素的颜色值
                    gray_rihgt[i, j] = tem_color.R * 0.299 + tem_color.G * 0.587 + tem_color.B * 0.114;       //各点灰度值
                }
            }

            toolStripProgressBar1.Value += toolStripProgressBar1.Step;

            double[,] zuobiao = new double[finalFeature.GetLength(0), 5];    //存储特征点对应坐标与系数
            for (int i = 0; i < zuobiao.GetLength(0); i++)//拷贝
            {
                zuobiao[i, 0] = finalFeature[i, 0];
                zuobiao[i, 1] = finalFeature[i, 1];
            }

            int dianshu = 0;
           
            for (int c = 0; c < featureTemplate.GetLength(0) ; c++)
            {
                double maxxishu = 0;    //最大系数
                int a = 0;    //存储系数最大点列
                int b = 0;    //存储系数最大点行
                for (int i = o; i < Var_W - o; i++)
                {
                    for (int j = o; j < Var_H - o; j++)
                    {
                        double chenghe = 0;    //各点灰度值相乘总和
                        int t = 0;
                        for (int m = 0; m < Osize; m++)
                        {
                            for (int n = 0; n < Osize; n++)
                            {

                                double sdavx = featureTemplate[c, t];
                                chenghe += featureTemplate[c, t] * gray_rihgt[i - o + m, j - o + n];




                                t++;
                            }
                        }


                        double tezhenghe = 0;    //特征点灰度值和
                        double tezhengji = 0;    //特征点灰度值平方和
                        for (int m = 0; m < Osize*Osize; m++)
                        {
                            tezhenghe += featureTemplate[c, m];
                            tezhengji += Math.Pow(featureTemplate[c, m], 2);
                        }

           
                        double xiangsuhe = 0;    //像素点灰度值和
                        double xiangsuji = 0;    //像素点灰度值平方和
                        for (int m = 0; m < Osize; m++)
                        {
                            for (int n = 0; n < Osize; n++)
                            {
                                xiangsuhe += gray_rihgt[i - o + m, j - o + n];
                                xiangsuji += Math.Pow(gray_rihgt[i - o + m, j - o + n], 2);
                            }
                        }


                      
                        double xishu = 0;
                        xishu = (chenghe - tezhenghe * xiangsuhe / (Osize * Osize)) / Math.Sqrt((tezhengji - Math.Pow(tezhenghe, 2) / (Osize * Osize)) * (xiangsuji - Math.Pow(xiangsuhe, 2) / (Osize * Osize)));
                        if (maxxishu < xishu)
                        {
                            maxxishu = xishu;
                            a = i; b = j;
                        }
                    }

                }

               
                if ((maxxishu > tCorr) && (maxxishu <= 1))
                {
                    zuobiao[c, 2] = a;
                    zuobiao[c, 3] = b;
                    zuobiao[c, 4] = maxxishu;
                    dianshu++;
                }
            }
            toolStripProgressBar1.Value += toolStripProgressBar1.Step;

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
            toolStripProgressBar1.Value += toolStripProgressBar1.Step;
            
            toolStripProgressBar1.Visible = false;              //隐藏进度条
            MessageBox.Show("共匹配" + dianshu.ToString() + "个点！");
            label13.Visible = true;
            label14.Text = dianshu.ToString();
        }

        private void button5_Click(object sender, EventArgs e)
        {

            //交互操作赋值
            //交互输入赋值
            Ksize = Convert.ToInt32(textBox1.Text);
            k = Ksize / 2;
            fpNumMax = Convert.ToInt32(textBox3.Text);
            multiple = Convert.ToInt32(textBox2.Text);//阈值迭代递增倍数                     
            Asize = Convert.ToInt32(textBox4.Text);                          //定义均匀提取特征的窗口大小模板
            int aWindow = Asize / 2;
            afpNumMax = Convert.ToInt32(textBox5.Text);


            Image myImage = System.Drawing.Image.FromFile(openFileDialog1.FileName);
            pictureBox1.Image = myImage;                                          //显示打开的图片

            toolStripProgressBar1.Visible = true;   //进度条可视   
            toolStripProgressBar1.Maximum = 7;    //设置进度条最大长度值
            toolStripProgressBar1.Value = 0;        //设置进度条当前值
            toolStripProgressBar1.Step = 1;        //设置进度条步长
            toolStripProgressBar1.Value += toolStripProgressBar1.Step;     //进度条前进

            int Var_H = pictureBox1.Image.Height;                          //获取图象的高度

            int Var_W = pictureBox1.Image.Width;                           //获取图象的宽度

            Bitmap Var_bmp = (Bitmap)pictureBox1.Image;                    //根据图象的大小创建Bitmap对象

            double[,] gray_left = new double[Var_W, Var_H];                       //用于存储各点灰度值

            for (int i = 0; i < Var_W; i++)
            {
                for (int j = 0; j < Var_H; j++)
                {
                    Color tem_color = Var_bmp.GetPixel(i, j);              //获取当前像素的颜色值
                    gray_left[i, j] = tem_color.R * 0.299 + tem_color.G * 0.587 + tem_color.B * 0.114;       //各点灰度值
                }
            }


            toolStripProgressBar1.Value += toolStripProgressBar1.Step;

            double[,] xingquzhi = new double[Var_W, Var_H];                      //用于存储各点兴趣值
           
            for (int i = k; i < Var_W - k; i++)
            {
                for (int j = k; j < Var_H - k; j++)//防止溢出
                {
                    double V1 = 0;
                    for (int m = 0; m < Ksize - 1; m++)
                    {
                        V1 = V1 + Math.Pow(gray_left[i - k + m, j] - gray_left[i - k + m + 1, j], 2);    //计算V1方向(0°)相邻像素灰度差平方和
                    }
                    double V2 = 0;
                    for (int m = 0; m < Ksize - 1; m++)
                    {
                        V2 = V2 + Math.Pow(gray_left[i - k + m, j - k + m] - gray_left[i - k + m + 1, j - k + m + 1], 2);    //计算V2方向(45°_顺时针标记角度）相邻像素灰度差平方和
                    }
                    double V3 = 0;
                    for (int m = 0; m < Ksize - 1; m++)
                    {

                        V3 = V3 + Math.Pow(gray_left[i, j - k + m] - gray_left[i, j - k + m + 1], 2);    //计算V3方向（90°)相邻像素灰度差平方和
                    }
                    double V4 = 0;
                    for (int m = 0; m < Ksize - 1; m++)
                    {
                        V4 = V4 + Math.Pow(gray_left[i - k + m, j + k - m] - gray_left[i - k + m + 1, j + k - m - 1], 2);    //计算V4方向(135°）相邻像素灰度差平方和
                    }
                    xingquzhi[i, j] = Math.Min(Math.Min(Math.Min(V1, V2), V3), V4);//从V1、V2、V3、V4中取最小值作为该点兴趣值
                }
            }

            toolStripProgressBar1.Value += toolStripProgressBar1.Step;

            double sum = 0;
            for (int i = 0; i < Var_W; i++)
            {
                for (int j = 0; j < Var_H; j++)
                {
                    sum += xingquzhi[i, j];
                }
            }
            double average = sum / (Var_W * Var_H);

            toolStripProgressBar1.Value += toolStripProgressBar1.Step;

            int houxuan = 0;                                                     //统计候选特征点数目
            
            fpNum = fpNumMax + 1;                                        //获取准确特征点数目     
           

            while (fpNum > 100)
            {
                fpNum = 0;
                tExtract = average * multiple;    //设定阈值
                double[,] houxuanFlag = new double[Var_W, Var_H];
                for (int i = 0; i < Var_W; i++)
                {
                    for (int j = 0; j < Var_H; j++)
                    {
                        if (xingquzhi[i, j] <= tExtract)
                        {
                            houxuanFlag[i, j] = 0;        //选取兴趣值大于阈值的点作为特征候选点，其他点兴趣值归零
                        }
                        else
                        {
                            houxuanFlag[i, j] = 1;
                            houxuan++;
                        }
                    }
                }


                int[,] tempFP = new int[houxuan, 2];         //假定一个数组能容纳所有点皆为特征点的像素坐标矩阵

               
               





                for (int i = aWindow; i < Var_W - aWindow; i = i + Asize)
                {
                    for (int j = aWindow; j < Var_H - aWindow; j = j + Asize)
                    {                                    
                        int [] a = new int [Asize*Asize];                                          //设a为最大值行
                        int[] b = new int[Asize*Asize];                                              //设b为最大值列
                        int aWinhouxuanNum = 0;//该均匀窗口候选点数                       
                        double [] arrSortTemp = new double[Asize*Asize];//待排序兴趣值数组
                        for (int m = 0; m < Asize; m++)
                        {
                            for (int n = 0; n < Asize; n++)
                            {
                                //遍历阈值模板-冒泡法排序，得到前afpNumMax个候选点作为特征点
                                if (houxuanFlag[i - aWindow + m, j - aWindow + n] == 1)
                                {
                                    arrSortTemp[aWinhouxuanNum] = xingquzhi[i - aWindow + m, j - aWindow + n];
                                    a[aWinhouxuanNum] = i - aWindow + m;                            //获取最大值列
                                    b[aWinhouxuanNum] = j - aWindow + n;                            //获取最大值行
                                    aWinhouxuanNum++;                                   
                                 
                                }                               
                            }
                        }
                        if ((a[0] != 0) && (b[0] != 0))//非空，含候选点
                        {
                            if (aWinhouxuanNum< afpNumMax+1)//无需排序
                            {
                                for (int p = 0; p< aWinhouxuanNum;p++)
                                {
                                    tempFP[fpNum, 0] = a[p];             //存储特征点列
                                    tempFP[fpNum, 1] = b[p];             //存储特征点行
                                    fpNum++;                               //每有一个既不为0也不重复的最大值特征点数目加一   
                                }
                            }
                            else//排序
                            {
                                double[] arrSort = new double[aWinhouxuanNum];
                                int[] index = new int[aWinhouxuanNum];
                                for (int fg = 0; fg <aWinhouxuanNum; fg++)//拷贝
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
                                //更新前afpNumMax候选点作为特征点
                                for (int p = 0; p < afpNumMax; p++)
                                {
                                    tempFP[fpNum, 0] = a[index[p]];             //存储特征点列
                                    tempFP[fpNum, 1] = b[index[p]];             //存储特征点行
                                    fpNum++;                               //每有一个既不为0也不重复的最大值特征点数目加一   
                                }
                            }                         
                        }
                    }
                }             
                tempFeature = tempFP;
                multiple += 1;
            }



            toolStripProgressBar1.Value += toolStripProgressBar1.Step;
            toolStripProgressBar1.Value += toolStripProgressBar1.Step;

            int[,] final = new int[fpNum, 2];      //定义一个数组存储像素坐标
            for (int i = 0; i < fpNum; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    final[i, j] = tempFeature[i, j];
                }
            }
            finalFeature = final;

            Image img = pictureBox1.Image;                         //将pictureBox1中图像存储入另一个变量
            Bitmap bmp = new Bitmap(img.Width, img.Height);        //创建Bitmap对象
            Graphics g = Graphics.FromImage(bmp);                  //创建Graphics对象
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;     //设置高质量双三次插值法 
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;                    //设置高质量,低速度呈现平滑程度
            g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;          //设置绘制到此 Graphics 的合成图像的呈现质量
            g.DrawImage(img, 0, 0, img.Width, img.Height);         //以img为原本重新于（0,0）点绘制
            g.Dispose();                                           //释放资源
            for (int i = 0; i < fpNum; i++)
            {
                Graphics myGraphics = Graphics.FromImage(bmp);    //创建Graphics对象
                myGraphics.DrawLine(new Pen(Color.Red, 1), new Point(final[i, 0], final[i, 1] - 5), new Point(final[i, 0], final[i, 1] + 5));    //画出竖直方向直线
                myGraphics.DrawLine(new Pen(Color.Red, 1), new Point(final[i, 0] - 5, final[i, 1]), new Point(final[i, 0] + 5, final[i, 1]));    //画出水平方向直线
                myGraphics.Dispose();                                 //释放资源
                pictureBox1.Image = bmp;                          //显示含有“+”的图
            }

            toolStripProgressBar1.Value += toolStripProgressBar1.Step;

          
            toolStripProgressBar1.Visible = false;              //隐藏进度条

            MessageBox.Show("共有" + fpNum.ToString() + "个特征点！");
            label11.Visible = true;
            label12.Text = fpNum.ToString();
            button3.Enabled = true;
            Osize = 5;
            o = Osize / 2;
            double[,] featureTemplate_temp = new double[fpNum, Osize * Osize];
            for (int i = 0; i < fpNum; i++)
            {
                int t = 0;
                for (int m = 0; m < Osize; m++)
                {
                    for (int n = 0; n < Osize; n++)
                    {
                        featureTemplate_temp[i, t] = gray_left[final[i, 0] - o + m, final[i, 1] - o + n];    //存储目标窗口灰度
                        t++;
                    }
                }
            }
            featureTemplate = featureTemplate_temp;
      


        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            
            checkBox2.Checked = false;
            panel2.Visible = true;
            button2.Enabled = true;
            button5.Enabled = false;


        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
           
            checkBox1.Checked = false;
            panel2.Visible = true;
            label6.Visible = true;
            textBox5.Visible = true;
            button5.Enabled = true;
            button2.Enabled = false;
            textBox4.Text = "15";
        }

        private void button6_Click(object sender, EventArgs e)
        {
            //交互操作赋值

            Osize = Convert.ToInt32(textBox6.Text); 
            o = Osize / 2;
            tCorr = Convert.ToDouble(textBox7.Text);
            Ssize = Convert.ToInt32(textBox8.Text);
            int s = Ssize / 2;

            x_ = X_r-X_l;//确定左右视差（右-左）
;           y_ = Y_r - Y_l;

            //左片特征模板

            double[,] featureTemplate_temp = new double[fpNum, Osize * Osize];
            for (int i = 0; i < fpNum; i++)
            {
                int t = 0;
                for (int m = 0; m < Osize; m++)
                {
                    for (int n = 0; n < Osize; n++)
                    {
                        featureTemplate_temp[i, t] = gray_left[finalFeature[i, 0] - o + m, finalFeature[i, 1] - o + n];    //存储目标窗口灰度
                        t++;
                    }
                }
            }
            featureTemplate = featureTemplate_temp;

            Image myImage2 = System.Drawing.Image.FromFile(openFileDialog2.FileName);
            pictureBox2.Image = myImage2;                                          //显示打开的图片

            toolStripProgressBar1.Visible = true;   //进度条可视
            toolStripProgressBar1.Maximum = 4;    //设置进度条最大长度值
            toolStripProgressBar1.Value = 0;        //设置进度条当前值
            toolStripProgressBar1.Step = 1;        //设置进度条步长
            toolStripProgressBar1.Value += toolStripProgressBar1.Step;     //进度条前进

            int Var_H = pictureBox2.Image.Height;                          //获取图象的高度

            int Var_W = pictureBox2.Image.Width;                           //获取图象的宽度

            Bitmap Var_bmp = (Bitmap)pictureBox2.Image;                    //根据图象的大小创建Bitmap对象

            gray_rihgt = new double[Var_W, Var_H];                       //用于存储各点灰度值

            for (int i = 0; i < Var_W; i++)
            {
                for (int j = 0; j < Var_H; j++)
                {
                    Color tem_color = Var_bmp.GetPixel(i, j);              //获取当前像素的颜色值
                    gray_rihgt[i, j] = tem_color.R * 0.299 + tem_color.G * 0.587 + tem_color.B * 0.114;       //各点灰度值
                }
            }

            toolStripProgressBar1.Value += toolStripProgressBar1.Step;

            double[,] zuobiao = new double[finalFeature.GetLength(0), 5];    //存储特征点对应坐标与系数
            for (int i = 0; i < zuobiao.GetLength(0); i++)//拷贝
            {
                zuobiao[i, 0] = finalFeature[i, 0];
                zuobiao[i, 1] = finalFeature[i, 1];
            }

            int dianshu = 0;

            for (int c = 0; c < featureTemplate.GetLength(0); c++)
            {
                double maxxishu = 0;    //最大系数
                int a = 0;    //存储系数最大点列
                int b = 0;    //存储系数最大点行
                double X = zuobiao[c, 0];//X+x_-s,X+x_+s 搜索窗口4顶点
                double Y = zuobiao[c, 1];//Y+y_-s,Y+y_+s
                //判断粗略匹配点是否存在
                if ((X + x_ <0) || (X + x_ > Var_W) || (Y + y_ < 0) || Y + y_ > Var_H)
                {
                    //溢出
                    continue;//进行下一次循环
                }
                //判断搜索窗口是否存在
                int x_min =(int)( X + x_ - s);
                int x_max = (int)(X + x_ + s );
                int y_min  = (int)(Y + y_ - s );
                int y_max = (int)(Y + y_ + s );
                if ((X + x_ - s)<0)
                {
                    x_min = 0;
                }
                if ((X + x_ + s) > Var_W)
                {
                    x_max = Var_W;
                }
                if ((Y + y_ - s) < 0)
                {
                    y_min = 0;
                }
                if ((Y + y_ + s) > Var_W)
                {
                    y_max = Var_H;
                }

                for (int i =  o+ x_min; i < x_max - o ; i++)//搜索窗口
                {
                    for (int j = y_min  + o; j < y_max  - o; j++)
                    {
                        double chenghe = 0;    //各点灰度值相乘总和
                        int t = 0;
                        for (int m = 0; m < Osize; m++)
                        {
                            for (int n = 0; n < Osize; n++)
                            {
                                double sdavx = featureTemplate[c, t];
                                chenghe += featureTemplate[c, t] * gray_rihgt[i - o + m, j - o + n];
                                t++;
                            }
                        }
                        double tezhenghe = 0;    //特征点灰度值和
                        double tezhengji = 0;    //特征点灰度值平方和
                        for (int m = 0; m < Osize * Osize; m++)
                        {
                            tezhenghe += featureTemplate[c, m];
                            tezhengji += Math.Pow(featureTemplate[c, m], 2);
                        }
                        double xiangsuhe = 0;    //像素点灰度值和
                        double xiangsuji = 0;    //像素点灰度值平方和
                        for (int m = 0; m < Osize; m++)
                        {
                            for (int n = 0; n < Osize; n++)
                            {
                                xiangsuhe += gray_rihgt[i - o + m, j - o + n];
                                xiangsuji += Math.Pow(gray_rihgt[i - o + m, j - o + n], 2);
                            }
                        }
                        double xishu = 0;
                        xishu = (chenghe - tezhenghe * xiangsuhe / (Osize * Osize)) / Math.Sqrt((tezhengji - Math.Pow(tezhenghe, 2) / (Osize * Osize)) * (xiangsuji - Math.Pow(xiangsuhe, 2) / (Osize * Osize)));
                        if (maxxishu < xishu)
                        {
                            maxxishu = xishu;
                            a = i; b = j;
                        }
                    }
                }


                if ((maxxishu > tCorr) && (maxxishu <= 1))
                {
                    zuobiao[c, 2] = a;
                    zuobiao[c, 3] = b;
                    zuobiao[c, 4] = maxxishu;
                    dianshu++;
                }
            }
            toolStripProgressBar1.Value += toolStripProgressBar1.Step;

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
            toolStripProgressBar1.Value += toolStripProgressBar1.Step;

            toolStripProgressBar1.Visible = false;              //隐藏进度条
            MessageBox.Show("共匹配" + dianshu.ToString() + "个点！");
            label13.Visible = true;
            label14.Text = dianshu.ToString();
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)//获取目视同名点视差
        {
            X_l = e.X;
            Y_l = e.Y;
        }

        private void pictureBox2_MouseDown(object sender, MouseEventArgs e)
        {
            X_r = e.X;
            Y_r = e.Y;
        }

        private void checkBox4_CheckedChanged(object sender, EventArgs e)
        {
            checkBox3.Checked = false;
            panel4.Visible = true;
            button4.Enabled = true;
            button6.Enabled = false;
        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            checkBox4.Checked = false;
            panel4.Visible = true;
            label9.Visible = true;
            textBox8.Visible = true;
            button6.Enabled = true;
            button4.Enabled = false;
            MessageBox.Show("请点击左右图像同名点以确定左右视差点！");

        }

      
    }
}
