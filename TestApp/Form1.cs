using System;
using System.IO;
using System.Windows.Forms;

namespace TestApp
{
    public partial class Form1 : Form
    {
        Int64 f0;
        int Number = 500000;
        double[] Hn = new double[500000];
        Int16[] Hwav = new Int16[500000];
        double Hmax, Hmin, w2t, w1t, T, l1;
        double sras, sAuto, sWindows, sWheel, sLights;
        double newFd, Td;
        double t , fd;
        double S0, S1, S2, S3;
        double eps, k, v0, rWheel, fWheel, fForWheel, tForWheel, vOtkl, c, angleZenit, angleAzimut, l;
        double aAuto, bAuto, aWindow, bWindow , rLights, kProtect, nProtect, sProtect, lSProtect, lHProtect, hProtect, EPR;


        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            textBox1.Text = Convert.ToString(1.694);
            textBox2.Text = Convert.ToString(1.784);
            textBox3.Text = Convert.ToString(0.5);
            textBox4.Text = Convert.ToString(1.2);
            textBox5.Text = Convert.ToString(0.175);
            textBox6.Text = Convert.ToString(8000);
            textBox7.Text = Convert.ToString(50);
            textBox8.Text = Convert.ToString(0.03);
            textBox9.Text = Convert.ToString(0.01); 
            textBox10.Text = Convert.ToString(0.05);
            textBox11.Text = Convert.ToString(0.02);
            textBox12.Text = Convert.ToString(0.33);
            textBox13.Text = Convert.ToString(20);
            textBox14.Text = Convert.ToString(11);
            textBox15.Text = Convert.ToString(0.001);
            textBox16.Text = Convert.ToString(20);
            textBox18.Text = Convert.ToString(5000000000);
        }

        public Form1()
        {
            InitializeComponent();

            // l лямбда
            // подсказки
            ToolTip help = new ToolTip();
            help.AutoPopDelay = 5000;
            help.InitialDelay = 100;
            help.ReshowDelay = 50;
            help.ShowAlways = true;

            help.SetToolTip(this.label1, "Длина кузова авто");
            help.SetToolTip(this.label2, "Ширина кузова авто");
            help.SetToolTip(this.label3, "Длина лобового стекла");
            help.SetToolTip(this.label4, "Высота лобового стекла");
            help.SetToolTip(this.label5, "Радиус кривизны фар");
            help.SetToolTip(this.label8, "Количество элементов протектора");
            help.SetToolTip(this.label9, "Ширина протектора");
            help.SetToolTip(this.label10, "Ширина выступа протектора");
            help.SetToolTip(this.label11, "Ширина углубления протектора");
            help.SetToolTip(this.label12, "Высота протектора резины");
            help.SetToolTip(this.label13, "Расстояние от центра колеса до крепления"); // нафига, хз 
            help.SetToolTip(this.label14, "Скорость движения U0");
            help.SetToolTip(this.label15, "Частота вращения колес");
            help.SetToolTip(this.label16, "Я даж хз что тут");
            help.SetToolTip(this.label17, "Угол по зениту"); // альфа
            help.SetToolTip(this.label21, "Несущая частота");

            // дефолт значения

            kProtect = 1;
            eps = 0.99; // епселон, константа
            c = 300000000;
            t = 93.5;
            angleAzimut = Math.PI; // угол по азимуту бета
            angleZenit = 20;
            sras = 100;

            // вычисление всяких констант
            k = Math.Pow(((Math.Sqrt(eps) - 1) / (Math.Sqrt(eps) + 1)), 2); // good

        }

        private void button1_Click(object sender, EventArgs e)
        {
            
            // чистим графики
            chart1.Series[0].Points.Clear();
            chart2.Series[0].Points.Clear();
            chart3.Series[0].Points.Clear();


            // попытка сделать функцию
            double vt(double t) => v0 + vOtkl * (Math.Sin(2 * (Math.PI / nProtect)) * kProtect + fWheel); // good функция v(t)
            double v1t(double t) => vt(t) + v0 * Math.Cos(2 * (Math.PI / tForWheel) * t); // good функция v1(t)
            double f1t(double t) => fd + 2 * vt(t)/c * f0; // good функция f1(t)
            double f2t(double t) => fd + 2 * v1t(t) / c * f0; // good функция f2(t)
            double B0(double t) => sAuto * Math.Sin(Simpson(f2t, 0, t, 1000));
            double B1(double t) => sWindows * Math.Sin(Simpson(f2t, 0, t, 1000));
            double B2(double t) => sWheel * Math.Sin(Simpson(f1t, 0, t, 1000));
            double B3(double t) => sLights * Math.Sin(Simpson(f2t, 0, t, 1000));

            double Y(double t) => B0(t)+B1(t)+B2(t)+B3(t);


            S0 = abS(aAuto, bAuto); // good 
            S1 = abS(aWindow, bWindow); // good
            S2 = abS2(sProtect, lSProtect, lHProtect, hProtect); // good
            S3 = abS3(); // good
            EPR = S0 + S1 + S2 + S3;
            fd = 2 * v0 / c * f0; // good 

            w1t = Simpson(f1t, 0 , t, 1000); // f1(t) dt
            w2t = Simpson(f2t, 0, t, 1000); // f2(t) dt

            sAuto = S0 / sras; 
            sWindows = S1 / sras; 
            sWheel = S2 / sras;  
            sLights = S3 / sras;
            Td = 1 / newFd;
            Hmax = 0;
            Hmin = 0;

            for (int i = 0; i < Number; i++)
            {
                Hn[i] = Y(Td * i);
                if (Hn[i] > Hmax)
                {
                    Hmax = Hn[i];
                }
            }

            for (int i = 0; i < Number; i++)
            {
                // Hwav[i] = (int)(((Hn[i]- Hmin) / (Hmax - Hmin)) * 255)*100;
                 Hwav[i] = Convert.ToInt16((Hn[i]/Hmax)*6000);
            }

            // рисование графиков

            double step = 0.5;
            
            for (double t = 0; t < 100; t += step)
            {
                chart1.Series[0].Points.AddXY(t, vt(t) + v0 * Math.Cos(2 * (Math.PI / tForWheel) * t));
            }
            for (double t = 0; t < 500; t += step)
            {
                chart2.Series[0].Points.AddXY(t, Y(t));
            }
            for (int i = 0; i < 500; i++)
            {
                chart3.Series[0].Points.AddXY(i, Hwav[i] );
            }

        }
        private void button2_Click(object sender, EventArgs e)
        {

            FileStream fi = new FileStream("abc.wav", FileMode.Create);
            
            BinaryWriter wr = new BinaryWriter(fi);

            wr.Write(0x46464952); // riff
            wr.Write(0x0007876A); // chunk size *** 0x000F4266
            wr.Write(0x45564157); // wave
            wr.Write(0x20746D66); // fmt 
            wr.Write(0x00000012); // subchunksize
            wr.Write(0x00020001); // audioformat каналы 0x00010001
            wr.Write(0x00001F40); // fd 0x0000AC44
            wr.Write(0x00007D00); // Sample rate 0x00015888
            wr.Write(0x00100004); // Average bytes per second 0x00100002
            wr.Write(0x61660000); // extra
            wr.Write(0x00047463); // extra
            wr.Write(0xE1CE0000); // extra
            wr.Write(0x61640001); // da
            wr.Write(0x87386174); // ta 42 и 0F 0x42406174
            wr.Write(0x01540007); // 0x0000000F
           // wr.Write(0x01580D30);
            //wr.Write(0x07F903FD);


            for (int i = 0; i < Number; i+=1)
               {
                 wr.Write(Hwav[i]);
            }

         label7.Text = "All right";
            
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            aAuto = Convert.ToDouble(textBox1.Text);
        }
        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            bAuto = Convert.ToDouble(textBox2.Text);
        }
        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            aWindow = Convert.ToDouble(textBox3.Text);
        }
        private void textBox4_TextChanged(object sender, EventArgs e)
        {
            bWindow = Convert.ToDouble(textBox4.Text);
        }
        private void textBox5_TextChanged(object sender, EventArgs e)
        {
            rLights = Convert.ToDouble(textBox5.Text);
        }
        private void textBox6_TextChanged(object sender, EventArgs e)
        {
            newFd = Convert.ToDouble(textBox6.Text);
        }
        private void textBox7_TextChanged(object sender, EventArgs e)
        {
            nProtect = Convert.ToDouble(textBox7.Text);
        }
        private void textBox8_TextChanged(object sender, EventArgs e)
        {
            sProtect = Convert.ToDouble(textBox8.Text);
        }
        private void textBox9_TextChanged(object sender, EventArgs e)
        {
            lSProtect = Convert.ToDouble(textBox9.Text);
        }
        private void textBox10_TextChanged(object sender, EventArgs e)
        {
            lHProtect = Convert.ToDouble(textBox10.Text);
        }
        private void textBox11_TextChanged(object sender, EventArgs e)
        {
            hProtect = Convert.ToDouble(textBox11.Text);
        }
        private void textBox12_TextChanged(object sender, EventArgs e)
        {
            rWheel = Convert.ToDouble(textBox12.Text); ;
        }
        private void textBox13_TextChanged(object sender, EventArgs e)
        {
            v0 = Convert.ToDouble(textBox13.Text); ;
        }
        private void textBox14_TextChanged(object sender, EventArgs e)
        {
            fWheel = Convert.ToDouble(textBox14.Text);
            fForWheel = fWheel / (Math.PI * 2); // good
            tForWheel = 1 / fForWheel; // good
            l1 = v0 / fForWheel; // good
        }
        private void textBox15_TextChanged(object sender, EventArgs e)
        {
            vOtkl = Convert.ToDouble(textBox15.Text);
        }
        private void textBox16_TextChanged(object sender, EventArgs e)
        {
            angleZenit = Convert.ToDouble(textBox16.Text);
        }
        private void textBox18_TextChanged(object sender, EventArgs e)
        {
            f0 = Convert.ToInt64(textBox18.Text);
            T = (double)1/f0;
            l = (double)c /f0;
        }
        public double abS(double a, double b) // good 
        {
            double summS = 0;
            summS = ((4 * Math.PI * Math.Pow(a * b, 2)) / Math.Pow(l,2)) * Math.Pow(((Math.Sin(((2 * Math.PI)/l) * a * Math.Sin(angleZenit))) / (((2 * Math.PI) / l) * a * Math.Sin(angleZenit))) , 2) * Math.Pow(((Math.Sin(((2 * Math.PI) / l) * b * Math.Sin(angleAzimut))) / (((2 * Math.PI) / l) * b * Math.Sin(angleAzimut))), 2);
            return summS;
        }
        public double abS2(double l0, double l1, double l2, double h) // good 
        {
            double summ1, summ2, summ3, summS = 0;

            double x1, x2, x3, x4, x5, x6, x7, x8, x9;
            x1 = (4 * Math.PI * k * eps) / (l0 * l1);
            x2 = Math.Sin(((2 * Math.PI) / (l)) * l1 * Math.Sin(2 * Math.PI * (rWheel / nProtect) * kProtect * fWheel * t));
            x3 = ((2 * Math.PI) / (l)) * l1 * Math.Sin(2 * Math.PI * (rWheel / nProtect) * kProtect * fWheel * t);

            summ1 = x1 * x2 / x3;

            x4 = (4 * Math.PI * k * eps) / (l0 * l2);
            x5 = Math.Sin(((2 * Math.PI) / (l)) * l2 * Math.Sin(2 * Math.PI * (rWheel / nProtect) * kProtect * fWheel * t));
            x6 = ((2 * Math.PI) / (l)) * l2 * Math.Sin(2 * Math.PI * (rWheel / nProtect) * kProtect * fWheel * t);
            summ2 = x4 * x5 / x6;

            x7 = (4 * Math.PI * k * eps) / (l0 * h);
            x8 = Math.Sin(((2 * Math.PI) / (l)) * h * Math.Sin(2 * Math.PI * (rWheel / nProtect) * kProtect * fWheel * t));
            x9 = ((2 * Math.PI) / (l)) * h * Math.Sin(2 * Math.PI * (rWheel / nProtect) * kProtect * fWheel * t);
            summ3 = x7 * x8 / x9;

            summS = summ1 + summ2 + summ3;

            //summS = (4 * Math.PI * k * eps) / (l0 * l1) * Math.Sin(((2 * Math.PI) / (l)) * l1 * Math.Sin(2 * Math.PI * (rWheel / nProtect) * kProtect * fWheel * t)) / ((2 * Math.PI) / (l)) * l1 * Math.Sin(2 * Math.PI * (rWheel / nProtect) * kProtect * fWheel * t);
            //summS += (((4 * Math.PI * k * eps) / (l0 * l2)) * (Math.Sin(((2 * Math.PI) / (l)) * l2 * Math.Sin(2 * Math.PI * (rWheel / nProtect) * kProtect * fWheel * t))) / ((2 * Math.PI) / (l)) * l2 * Math.Sin(2 * Math.PI * (rWheel / nProtect) * kProtect * fWheel * t));
            //summS += (((4 * Math.PI * k * eps) / (l0 * h)) * (Math.Sin(((2 * Math.PI) / (l)) * h * Math.Sin(2 * Math.PI * (rWheel / nProtect) * kProtect * fWheel * t))) / ((2 * Math.PI) / (l)) * h * Math.Sin(2 * Math.PI * (rWheel / nProtect) * kProtect * fWheel * t));

            return summS;
        }
        public double abS3() // good 
        {
            double summ = 0;
            summ = k * Math.PI * Math.Pow(rLights, 2);
            return summ;
        }
        private static double Simpson(Func<double, double> f, double a, double b, int n)
        {
            var h = (b - a) / n;
            var sum1 = 0d;
            var sum2 = 0d;
            for (var k = 1; k <= n; k++)
            {
                var xk = a + k * h;
                if (k <= n - 1)
                {
                    sum1 += f(xk);
                }

                var xk_1 = a + (k - 1) * h;
                sum2 += f((xk + xk_1) / 2);
            }

            var result = h / 3d * (1d / 2d * f(a) + sum1 + 2 * sum2 + 1d / 2d * f(b));
            return result;
        }
        
    }

}
