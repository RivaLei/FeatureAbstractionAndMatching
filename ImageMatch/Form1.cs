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
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        public double[,] gray_left;//全局变量声明
        public double[,] gray_right;
        public int[,] featureResult;
        public int featureNum ;//特征点数目
        //public int[,] featureResult;
        public double[,] tezhengmuban;//相关系数匹配时，左相片所有特征目标区灰度值（从上到下、从左到右）
        public int matchPointNun;//成功匹配特征点数目

        public int[,] tezhengzhi;
        public int[,] tezheng;
        static public int c;
        static public int dianshu;


        private void btn_feature_Click(object sender, EventArgs e)
        {
            
            Bitmap Var_bmp;
            string path = @"D:\code\ImageMatch\ImageMatch\res\u0369_panLeft.bmp";
            Var_bmp = (Bitmap)Image.FromFile(path);                        
            pictureBox1.Image = Var_bmp.Clone() as Image;

            //彩色图像转灰度图像
            int Var_H = pictureBox1.Image.Height;                          //获取图象的高度
            int Var_W = pictureBox1.Image.Width;

            double[,] gray = new double[Var_W, Var_H];                       //用于存储各点灰度值
            double[,] iValue = new double[Var_W, Var_H];                   //用于点的兴趣值
            double[,] mark= new double[Var_W, Var_H];                      //用于点的标记：候选、兴趣点


            for (int i = 0; i < Var_W; i++)
            {
                for (int j = 0; j < Var_H; j++)
                {
                    Color tem_color = Var_bmp.GetPixel(i, j);              //获取当前像素的颜色值
                    gray[i, j] = tem_color.R * 0.299 + tem_color.G * 0.587 + tem_color.B * 0.114;       //各点灰度值
                }
            }
            gray_left = gray;

            double threshold = 0;//阈值初始化
            int rSize = 5;//搜索窗口
            int r = rSize / 2;
            int[,] featurePosition = null ; //最终特征点数组
            int candidateNum = 0;//候选点数目初始化
            featureNum = 0;//特征点数目初始化
            int featureNumTop = 100;//特征点数目上限
            int qSize = 5;//非极大值抑制窗口
            int q = qSize / 2;
            


            //Moravec




            for (int i = r; i < Var_W - r; i++)//行
            {
                for (int j = r; j < Var_H - r; j++)//列
                {
                    //逐像素计算兴趣值

                    double V1,V2,V3,V4;
                    V1 = V2 = V3 = V4 = 0;
                 
                    for (int k = -r; k < r; k++)

                    {
                        //采用bmp处理灰色图像，R\G\B分量相同，故采用R通道进行计算

                        //计算水平方向窗内的兴趣值                        
                              V1 += (gray[i, j +k ] - gray[i , j +k+1])
                                * (gray[i, j + k] - gray[i, j + k + 1]);

                          //计算45°方向窗内的兴趣值  
                              V2 += (gray[i+k, j + k] - gray[i+k+1, j + k + 1])
                            * (gray[i + k, j + k]- gray[i + k + 1, j + k + 1]);

                           //计算垂直方向窗内的兴趣值  
                               V3 += (gray[i+k , j ] - gray[i+k+1 , j ])
                              * (gray[i + k, j] - gray[i + k+1, j]);

                           //计算135°方向窗内的兴趣值  
                               V4 += (gray[i + k, j -k] - gray[i + k + 1, j - k -1])
                              * (gray[i + k, j - k] - gray[i + k + 1, j - k - 1]);


                    }


                     //从V1、V2、V3、V4中取最小值作为该点兴趣值
                    iValue[i,j] = Math.Min(Math.Min(Math.Min(V1, V2), V3), V4);
                   


                }
            }


            double sum = 0;
            for (int i = 0; i < Var_W; i++)
            {
                for (int j = 0; j < Var_H; j++)
                {
                    sum += iValue[i, j];
                }
            }
            double iMean = sum / (Var_W * Var_H);//兴趣值均值
            int multiple = 4;//阈值扩大因子


            featureNum = 101;//作为初次进入循环的条件
           
            while (featureNum> featureNumTop)//
            {
                featureNum = 0;//恢复初始状态
                threshold = iMean * multiple;
                for (int i = 0; i < Var_W; i++)
                {
                    for (int j = 0; j < Var_H; j++)
                    {
                        if (iValue[i, j] > threshold)
                        {
                            //候选点
                            mark[i, j] = 1;
                            candidateNum++;

                        }
                        else
                        {
                            mark[i, j] = 0;
                        }


                    }
                }

                //非极大值抑制

                //法一：随机

                int[,] featurePositionTemp = new int[candidateNum, 2];         //临时特征点数组，假定一个数组能容纳所有点皆为特征点的像素坐标矩阵
                

                for (int i = q; i < Var_W - q; i = i + qSize)
                {
                    for (int j = q; j < Var_H - q; j = j + qSize)
                    {
                        double MAX = 0;                          //假定模板最大值起始值为第一个元素值
                        int a = 0;                                          //设a为最大值行
                        int b = 0;                                          //设b为最大值列
                        for (int m = 0; m < qSize; m++)
                        {
                            for (int n = 0; n < qSize; n++)
                            {
                                if (mark[i - q + m, j - q + n] == 1)
                                {
                                    if (MAX < iValue[i - q + m, j - q + n])
                                    {

                                        MAX = iValue[i - q + m, j - q + n];    //获取模板中最大值
                                        a = i - q + m;                            //获取最大值列
                                        b = j - q + n;                            //获取最大值行
                                    }
                                }
                                //else
                                //{
                                //    a = 0; b = 0;
                                //}

                            }
                        }
                        if ((a != 0) && (b != 0))
                        {
                            featurePositionTemp[featureNum, 0] = a;             //存储特征点列
                            featurePositionTemp[featureNum, 1] = b;             //存储特征点行
                            featureNum++;                               //每有一个既不为0也不重复的最大值特征点数目加一   
                        }
                    }
                    
                }

                featurePosition = featurePositionTemp;
                multiple++;

            }


            int sss = featureNum;
            int[,] tezhengdian2 = new int[featureNum, 2];      //定义一个数组存储像素坐标
            for (int i = 0; i < featureNum; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    tezhengdian2[i, j] = featurePosition[i, j];
                }
            }
            featureResult = tezhengdian2;//图像标记特征点



            //法二：均匀



            



            Image img = pictureBox1.Image;                         //将pictureBox1中图像存储入另一个变量
            Bitmap bmp = new Bitmap(img.Width, img.Height);        //创建Bitmap对象
            Graphics g = Graphics.FromImage(bmp);                  //创建Graphics对象
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;     //设置高质量双三次插值法 
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;                    //设置高质量,低速度呈现平滑程度
            g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;          //设置绘制到此 Graphics 的合成图像的呈现质量
            g.DrawImage(img, 0, 0, img.Width, img.Height);         //以img为原本重新于（0,0）点绘制
            g.Dispose();                                           //释放资源
            for (int i = 0; i < featureNum; i++)
            {
                Graphics myGraphics = Graphics.FromImage(bmp);    //创建Graphics对象
                myGraphics.DrawLine(new Pen(Color.Red, 1), new Point(featurePosition[i, 0], featurePosition[i, 1] - 5), new Point(featurePosition[i, 0], featurePosition[i, 1] + 5));    //画出竖直方向直线
                myGraphics.DrawLine(new Pen(Color.Red, 1), new Point(featurePosition[i, 0] - 5, featurePosition[i, 1]), new Point(featurePosition[i, 0] + 5, featurePosition[i, 1]));    //画出水平方向直线
                myGraphics.Dispose();                                 //释放资源
                pictureBox1.Image = bmp;                          //显示含有“+”的图
            }

        }

        private void btn_correlation_Click(object sender, EventArgs e)
        {
            Bitmap Var_bmp;
            string path = @"D:\code\ImageMatch\ImageMatch\res\u0367_panRight.bmp";
            Var_bmp = (Bitmap)Image.FromFile(path);
            pictureBox2.Image = Var_bmp.Clone() as Image;

            //彩色图像转灰度图像
            int Var_H = pictureBox2.Image.Height;                          //获取图象的高度
            int Var_W = pictureBox2.Image.Width;

            double[,] gray = new double[Var_W, Var_H];                       //用于存储各点灰度值
            double[,] iValue = new double[Var_W, Var_H];                   //用于点的兴趣值
            double[,] mark = new double[Var_W, Var_H];                      //用于点的标记：候选、兴趣点
            double threshold = 0.5;//相关系数阈值


            for (int i = 0; i < Var_W; i++)
                {
                    for (int j = 0; j < Var_H; j++)
                    {
                        Color tem_color = Var_bmp.GetPixel(i, j);              //获取当前像素的颜色值
                        gray[i, j] = tem_color.R * 0.299 + tem_color.G * 0.587 + tem_color.B * 0.114;       //各点灰度值
                    }
                }

            gray_right = gray;

            //搜索区域
            //法一：全图

            int rSize = 5;//目标窗口大小
            int r = rSize / 2;


            double[,] tezhengmuban2 = new double[featureNum, rSize* rSize];
            for (int i = 0; i < featureNum; i++)
            {
                int t = 0;
                for (int m = 0; m < rSize; m++)
                {
                    for (int n = 0; n < rSize; n++)
                    {
                        tezhengmuban2[i, t] = gray_left[featureResult[i, 0] - r + m, featureResult[i, 1] - r + n];    //存储rSize*rSize模板灰度,存储顺序：从上到下，从左往右
                        t++;
                    }
                }
            }
            tezhengmuban = tezhengmuban2;


            double[,] matchResult = new double[featureResult.GetLength(0), 5];    //存储特征点对应坐标与系数.

            for (int i = 0; i <  matchResult.GetLength(0); i++)
            {
                 matchResult[i, 0] = featureResult[i, 0];
                 matchResult[i, 1] = featureResult[i, 1];
            }//特征点拷贝

            //对左相片特征点逐个进行匹配
            matchPointNun = 0;
            for (int m =0;m< tezhengmuban.GetLength(0)-90; m++)
            {
                double maxCorrelation = 0;    //最大系数
                int a = 0;    //存储系数最大点列
                int b = 0;    //存储系数最大点行

                //右相片逐像素匹配(全局搜索）

                for (int i = r; i < Var_W - r-400; i++)//行
                {
                    for (int j = r; j < Var_H - r-400; j++)//列（防止溢出）
                    {

                        //匹配窗口内计算相关系数:
                        //相关系数 p = (v1 - v2 * v3 / rSize*rSize) / Math.Sqrt((v3 - Math.Pow(v2, 2) / rSize*rSize) * (v5-Math.Pow(v4, 2) / rSize*rSize));

                        //1、v1:目标窗口与匹配窗口，各点灰度值相乘总和
                        double v1 = 0;
                        int t = 0;//t and for 循环实现目标区和匹配窗口的遍历（从上到下，从左到右）
                        for (int u = 0; u < rSize; u++)
                        {
                            for (int w = 0; w < rSize; w++)
                            {
                                double dfcx = tezhengmuban[m, t];
                                v1 += tezhengmuban[m, t] * gray_right[i - r + u, j - r + w];
                                t++;
                            }
                        }

                        double qwe = v1;
                        //2、v2:目标窗口内灰度值和;v3:目标窗口内灰度值平方和
                      
                        double v2 = 0;    //特征点灰度值和
                        double v3 = 0;    //特征点灰度值平方和
                        for (int u = 0; u < rSize* rSize; u++)
                        {
                            v2 += tezhengmuban[m, u];
                            v3 += Math.Pow(tezhengmuban[m, u], 2);
                        }

                        double qwer = v2;
                        double asdf = v3;
                   
                        //3、v4:匹配窗口内灰度值和;v5:匹配窗口灰度值平方和
                        double v4 = 0;    //像素点灰度值和
                        double v5 = 0;    //像素点灰度值平方和
                        for (int u = 0; u < rSize ; u++)
                        {
                            for (int n = 0; n < rSize ; n++)
                            {
                                v4 += gray_right[i - r + u, j - r + n];
                                v5 += Math.Pow(gray_right[i - r + u, j - r + n], 2);
                            }
                        }

                        double dfg = v4;
                        double dfcv = v5;

                        double correlation = 0;
                        correlation = (v1 - v2 * v4 / rSize*rSize) / Math.Sqrt((v3 - Math.Pow(v2, 2) / rSize * rSize) * (v5 - Math.Pow(v4, 2) / rSize * rSize));
                        if (maxCorrelation < correlation)
                        {
                            maxCorrelation = correlation;
                            a = i; b = j;
                        }

                    }

                }

                double www = maxCorrelation;

                //阈值判断
                if ((maxCorrelation > threshold) && (maxCorrelation <= 1))
                {
                     matchResult[m, 2] = a;
                     matchResult[m, 3] = b;
                     matchResult[m, 4] = maxCorrelation;
                    matchPointNun++;
                }

            }

            double[,] zuobiao2 = new double[matchPointNun, 5];    //明确存储特征点对应坐标与相关系数
            int de = 0;
            for (int i = 0; i < matchResult.GetLength(0); i++)
            {
                if (matchResult[i, 4] != 0)
                {
                    for (int j = 0; j < 5; j++)
                    {
                        zuobiao2[de, j] = matchResult[i, j];

                    }
                    de++;
                }
            }

            //显示匹配结果
            int sssss = matchPointNun;

            Image img = pictureBox2.Image;                         //将pictureBox2中图像存储入另一个变量
            Bitmap bmp = new Bitmap(img.Width, img.Height);        //创建Bitmap对象
            Graphics g = Graphics.FromImage(bmp);                  //创建Graphics对象
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;     //设置高质量双三次插值法 
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;                    //设置高质量,低速度呈现平滑程度
            g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;          //设置绘制到此 Graphics 的合成图像的呈现质量
            g.DrawImage(img, 0, 0, img.Width, img.Height);         //以img为原本重新于（0,0）点绘制
            g.Dispose();                                           //释放资源
            for (int i = 0; i < matchPointNun; i++)
            {
                Graphics myGraphics = Graphics.FromImage(bmp);    //创建Graphics对象
                myGraphics.DrawLine(new Pen(Color.Red, 1), new Point(Convert.ToInt32(matchResult[i, 2]), Convert.ToInt32(matchResult[i, 3] - 5)), new Point(Convert.ToInt32(matchResult[i, 2]), Convert.ToInt32(matchResult[i, 3] + 5)));    //画出竖直方向直线
                myGraphics.DrawLine(new Pen(Color.Red, 1), new Point(Convert.ToInt32(matchResult[i, 2] - 5), Convert.ToInt32(matchResult[i, 3])), new Point(Convert.ToInt32(matchResult[i, 2]) + 5, Convert.ToInt32(matchResult[i, 3])));    //画出水平方向直线
                //myGraphics.DrawString(i.ToString(), new Font("华文行楷", 8), new SolidBrush(Color.DarkOrange), Convert.ToInt32(matchResult[i, 2]), Convert.ToInt32(matchResult[i, 3]));
                myGraphics.Dispose();                                 //释放资源
                pictureBox2.Image = bmp;                          //显示含有“+”的图
            }









        }

        private void button1_Click(object sender, EventArgs e)
        {

            Bitmap Var_bmp;
            string path = @"D:\code\ImageMatch\ImageMatch\res\u0369_panLeft.bmp";
            Var_bmp = (Bitmap)Image.FromFile(path);
            pictureBox1.Image = Var_bmp.Clone() as Image;

            int Var_H = pictureBox1.Image.Height;                          //获取图象的高度

            int Var_W = pictureBox1.Image.Width;                           //获取图象的宽度

            //Bitmap Var_bmp = (Bitmap)pictureBox1.Image;                    //根据图象的大小创建Bitmap对象

            double[,] huiduzhi = new double[Var_W, Var_H];                       //用于存储各点灰度值

            for (int i = 0; i < Var_W; i++)
            {
                for (int j = 0; j < Var_H; j++)
                {
                    Color tem_color = Var_bmp.GetPixel(i, j);              //获取当前像素的颜色值
                    huiduzhi[i, j] = tem_color.R * 0.299 + tem_color.G * 0.587 + tem_color.B * 0.114;       //各点灰度值
                }
            }


           

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

           

            double sum = 0;
            for (int i = 0; i < Var_W; i++)
            {
                for (int j = 0; j < Var_H; j++)
                {
                    sum += xingquzhi[i, j];
                }
            }
            double pingjunzhi = sum / (Var_W * Var_H);

           

            int houxuan = 0;                                                     //统计候选特征点数目
             c = 101;                                        //获取准确特征点数目     
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

            int sdsa = c;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Bitmap Var_bmp;
            string path = @"D:\code\ImageMatch\ImageMatch\res\u0367_panRight.bmp";
            Var_bmp = (Bitmap)Image.FromFile(path);
            pictureBox2.Image = Var_bmp.Clone() as Image;

            int Var_H = pictureBox2.Image.Height;                          //获取图象的高度

            int Var_W = pictureBox2.Image.Width;                           //获取图象的宽度

           // Bitmap Var_bmp = (Bitmap)pictureBox2.Image;                    //根据图象的大小创建Bitmap对象

            double[,] huiduzhi = new double[Var_W, Var_H];                       //用于存储各点灰度值

            for (int i = 0; i < Var_W; i++)
            {
                for (int j = 0; j < Var_H; j++)
                {
                    Color tem_color = Var_bmp.GetPixel(i, j);              //获取当前像素的颜色值
                    huiduzhi[i, j] = tem_color.R * 0.299 + tem_color.G * 0.587 + tem_color.B * 0.114;       //各点灰度值
                }
            }

          

            double[,] zuobiao = new double[tezheng.GetLength(0), 5];    //存储特征点对应坐标与系数
            for (int i = 0; i < zuobiao.GetLength(0); i++)
            {
                zuobiao[i, 0] = tezheng[i, 0];
                zuobiao[i, 1] = tezheng[i, 1];
            }

            dianshu = 0;
            for (int c = 0; c < tezhengmuban.GetLength(0) - 90; c++)
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
                if ((maxxishu > 0.5) && (maxxishu <= 1))
                {
                    zuobiao[c, 2] = a;
                    zuobiao[c, 3] = b;
                    zuobiao[c, 4] = maxxishu;
                    dianshu++;
                }
            }
           

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

            int xvdgsa = dianshu;

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
        }
       
    }

}
    

