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
        public int[,] feature_left;

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
            int featureNum = 0;//特征点数目初始化
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




            //法二：均匀


            feature_left = featurePosition;
            //图像标记特征点



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
            string path = @"D:\code\ImageMatch\ImageMatch\res\u0369_panRight.bmp";
            Var_bmp = (Bitmap)Image.FromFile(path);
            pictureBox2.Image = Var_bmp.Clone() as Image;

            //彩色图像转灰度图像
            int Var_H = pictureBox1.Image.Height;                          //获取图象的高度
            int Var_W = pictureBox1.Image.Width;

            double[,] gray = new double[Var_W, Var_H];                       //用于存储各点灰度值
            double[,] iValue = new double[Var_W, Var_H];                   //用于点的兴趣值
            double[,] mark = new double[Var_W, Var_H];                      //用于点的标记：候选、兴趣点

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
            double[,] matchResult = new double[feature_left.GetLength(0), 5];    //存储特征点对应坐标与系数

            //对左相片特征点逐个进行匹配
            for (int m =0;m< feature_left.GetLength(0); m++)
            {
                double maxCorrelation = 0;    //最大系数
                int a = 0;    //存储系数最大点列
                int b = 0;    //存储系数最大点行

                //右相片逐像素匹配

                for (int i = r; i < Var_W - r; i++)//行
                {
                    for (int j = r; j < Var_H - r; j++)//列
                    {

                        //匹配窗口内计算相关系数:
                        //相关系数 p = (v1 - v2 * v3 / rSize*rSize) / Math.Sqrt((v3 - Math.Pow(v2, 2) / rSize*rSize) * (v5-Math.Pow(v4, 2) / rSize*rSize));

                        //1、v1:目标窗口与匹配窗口，各点灰度值相乘总和

                        //2、v2:目标窗口内灰度值和

                        //3、v3:目标窗口内灰度值平方和

                        //4、v4:匹配窗口内灰度值和

                        //5、v5:匹配窗口灰度值平方和





                    }






                }
                }







            }

    }

}
    

