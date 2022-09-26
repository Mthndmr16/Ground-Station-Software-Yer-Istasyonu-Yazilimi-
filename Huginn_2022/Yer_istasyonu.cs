using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.IO;
using Excel = Microsoft.Office.Interop.Excel;
using AForge;
using AForge.Video;
using AForge.Video.DirectShow;
using GMap.NET;
using GMap.NET.WindowsForms.Markers;
using GMap.NET.MapProviders;
using VisioForge.Types.OutputFormat;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using CefSharp;
using CefSharp.WinForms;
using VisioForge.Types;
using VisioForge.Types.VideoEffects;

namespace Huginn_2022
{
    public partial class Yer_istasyonu : Form
    {

        public Yer_istasyonu()
        {
            InitializeComponent();
        }

        Thread thread;
        float x = 0, y = 0, z = 0;        
        string[] paket;
        int i, j;
        string data;
        int b = 0;
        public Color renk1 = Color.White, renk2 = Color.Green;
        int video = 0;
              

        public CefSharp.WinForms.ChromiumWebBrowser chrome;
        public CefSharp.WinForms.ChromiumWebBrowser videoCapture;

        private void Form1_Load(object sender, EventArgs e)
        {
            FormBorderStyle = FormBorderStyle.Sizable;
            WindowState = FormWindowState.Maximized;

            btnServoAc.Enabled = true;
            btnServoKapat.Enabled = false;
            btnMotorAc.Enabled = true;
            btnMotorKapat.Enabled = false;
            btnServoKapat.SendToBack();
            btnMotorKapat.SendToBack();

            string[] portlar = SerialPort.GetPortNames();
            foreach (string portAdi in portlar)
            {
                cmbxSerialPort.Items.Add(portAdi);
                cmbxSerialPort.SelectedIndex = 0;
            }
            GL.ClearColor(Color.Black);
                                                                                    
            cmbxBaudRate.Items.Add(115200);
          
            cmbxBaudRate.SelectedIndex = 0;

            CefSettings settings = new CefSettings();
            Cef.Initialize(settings);

           // txtVideoCapture.Text = "";
            //txtWeb.Text = "http://192.168.1.1/upload";
            txtWeb.Text = "http://www.google.com/";
            txtVideoCapture.Text = "http://192.168.1.2/";
            chrome = new ChromiumWebBrowser(txtWeb.Text);
            videoCapture = new ChromiumWebBrowser(txtVideoCapture.Text);
            this.panel2.Controls.Add(chrome);
            this.panel3.Controls.Add(videoCapture);
            chrome.Dock = DockStyle.Fill;
            videoCapture.Dock = DockStyle.Fill;
            chrome.AddressChanged += Chrome_AddressChanged;
            videoCapture.AddressChanged += Chrome_AddressChanged;


            // Görev yükü GPS verileri

            //double latitude = 38.398160;   // enlem.
            //double longitude = 33.711094;  // boylam.
            double latitude = 41.258181;
            Double longitude = 36.554787;
            map.Position = new GMap.NET.PointLatLng(latitude, longitude);
            map.DragButton = MouseButtons.Left;
            map.MapProvider = GMapProviders.GoogleSatelliteMap;
            map.MinZoom = 5;
            map.MaxZoom = 95;
            map.Zoom = 15;

            // Taşıyıcı GPS verileri

            double latitude2 = 41.258185;
            double longitude2 = 36.554761;
            map2.Position = new GMap.NET.PointLatLng(latitude2, longitude2);
            map2.DragButton = MouseButtons.Left;
            map2.MapProvider = GMapProviders.GoogleSatelliteMap;
            map2.MinZoom = 5;
            map2.MaxZoom = 95;
            map2.Zoom = 15;


            // Telemetri Verileri

            Control.CheckForIllegalCrossThreadCalls = false;
            telemetriEkrani.ColumnCount = 23;
            telemetriEkrani.RowCount = 5000;
            telemetriEkrani.Columns[0].Name = "TAKIM NO";
            telemetriEkrani.Columns[1].Name = "PAKET NO";
            telemetriEkrani.Columns[2].Name = "GÖNDERME SAATİ";
            telemetriEkrani.Columns[3].Name = "BASINÇ1";
            telemetriEkrani.Columns[4].Name = "BASINÇ2";
            telemetriEkrani.Columns[5].Name = "YÜKSEKLİK1 ";
            telemetriEkrani.Columns[6].Name = "YÜKSEKLİK2 ";
            telemetriEkrani.Columns[7].Name = "İRTİFA FARKI ";
            telemetriEkrani.Columns[8].Name = "İNİŞ HIZI ";
            telemetriEkrani.Columns[9].Name = "SICAKLIK";
            telemetriEkrani.Columns[10].Name = "PİL GERİLİMİ";
            telemetriEkrani.Columns[11].Name = "GPS1 LATITUDE";
            telemetriEkrani.Columns[12].Name = "GPS1 LONGITUDE";
            telemetriEkrani.Columns[13].Name = "GPS1 ALTITUDE";
            telemetriEkrani.Columns[14].Name = "GPS2 LATITUDE";
            telemetriEkrani.Columns[15].Name = "GPS2 LONGITUDE";
            telemetriEkrani.Columns[16].Name = "GPS2 ALTITUDE";
            telemetriEkrani.Columns[17].Name = "UYDU STATÜSÜ";
            telemetriEkrani.Columns[18].Name = "PITCH";
            telemetriEkrani.Columns[19].Name = "ROLL";
            telemetriEkrani.Columns[20].Name = "YAW";
            telemetriEkrani.Columns[21].Name = "DÖNÜŞ SAYISI";
            telemetriEkrani.Columns[22].Name = "VİDEO AKTARIM BİLGİSİ";

 
            telemetriEkrani.ColumnHeadersDefaultCellStyle.BackColor = Color.Green; 
            telemetriEkrani.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.Green;
            telemetriEkrani.DefaultCellStyle.SelectionBackColor = Color.Black;

           
        }

        private void Chrome_AddressChanged(object sender, AddressChangedEventArgs e)
        {
            this.Invoke(new MethodInvoker(() =>
            {
                txtWeb.Text = e.Address;
            }));
        }

        private void ChromeAdressChanged(object sender, AddressChangedEventArgs a)
        {
            this.Invoke(new MethodInvoker(() =>
            {
                txtVideoCapture.Text = a.Address;
            }));
        }

        private void Zamanlayici_Tick(object sender, EventArgs e)
        {
           
            try
            {
                x = float.Parse(paket[23], System.Globalization.CultureInfo.InvariantCulture);
                y = float.Parse(paket[24], System.Globalization.CultureInfo.InvariantCulture);
                z = float.Parse(paket[25], System.Globalization.CultureInfo.InvariantCulture);

                lblPitch.Text = Convert.ToString(x);
                lblRoll.Text = Convert.ToString(y);
                lblYaw.Text = Convert.ToString(z);

                string a = paket[1];
                int c = int.Parse(a, System.Globalization.CultureInfo.InvariantCulture); 

                if (b < c)
                {
                    telemetriEkrani.Rows[i].Cells[j].Value = paket[0]; j++;           // takım no
                    telemetriEkrani.Rows[i].Cells[j].Value = paket[1]; j++;           // paket no 
                    telemetriEkrani.Rows[i].Cells[j].Value = paket[2] + "/" + paket[3] + "/" + paket[4] + "," + paket[5] + ":" + paket[6] + ":" + paket[7]; j++; // gönderme saati
                    telemetriEkrani.Rows[i].Cells[j].Value = paket[8] + " hPa"; j++;  // basınç 1
                    telemetriEkrani.Rows[i].Cells[j].Value = paket[9] + " hPa"; j++;  // basınç 2
                    telemetriEkrani.Rows[i].Cells[j].Value = paket[10] + " m"; j++;   // yükseklik 1
                    telemetriEkrani.Rows[i].Cells[j].Value = paket[11] + " m"; j++;   // yükseklik 2
                    telemetriEkrani.Rows[i].Cells[j].Value = paket[12] + " m"; j++;   // irtifa farkı
                    telemetriEkrani.Rows[i].Cells[j].Value = paket[13] + " m/s"; j++; // iniş hızı
                    telemetriEkrani.Rows[i].Cells[j].Value = paket[14] + " Cᵒ"; j++;  // sıcaklık
                    telemetriEkrani.Rows[i].Cells[j].Value = paket[15] + " V"; j++;   // pil gerilimi 
                    telemetriEkrani.Rows[i].Cells[j].Value = paket[16] + " ᵒ"; j++;   // gps1 latitude 
                    telemetriEkrani.Rows[i].Cells[j].Value = paket[17] + " ᵒ"; j++;   // gps1 longitude
                    telemetriEkrani.Rows[i].Cells[j].Value = paket[18] + " m"; j++;   // gps1 altitude
                    telemetriEkrani.Rows[i].Cells[j].Value = paket[19] + " ᵒ"; j++;   // gps2 latitude 
                    telemetriEkrani.Rows[i].Cells[j].Value = paket[20] + " ᵒ"; j++;   // gps2 longitude
                    telemetriEkrani.Rows[i].Cells[j].Value = paket[21] + " m"; j++;   // gps2 altitude
                    telemetriEkrani.Rows[i].Cells[j].Value = paket[22]; j++;          // uydu statüsü 
                    telemetriEkrani.Rows[i].Cells[j].Value = paket[23] + " ᵒ"; j++;   // pitch
                    telemetriEkrani.Rows[i].Cells[j].Value = paket[24] + " ᵒ"; j++;   // roll
                    telemetriEkrani.Rows[i].Cells[j].Value = paket[25] + " ᵒ"; j++;   // yaw 
                    telemetriEkrani.Rows[i].Cells[j].Value = paket[26]; j++;          // dönüş sayısı
                    telemetriEkrani.Rows[i].Cells[j].Value = paket[27]; j++;          // video aktarım bilgisi               
                    i++;
                    j = 0;
                    b = c;                                 
                    try
                    {
                        this.chrtBasınç.Series["BASINÇ1 (hPa)"].Points.AddXY(DateTime.Now.ToLongTimeString(), paket[8]);
                        this.chrtBasınç.Series["BASINÇ2  (hPa)"].Points.AddXY(DateTime.Now.ToLongTimeString(), paket[9]);
                        this.chrtYükseklik.Series["YÜKSEKLİK 1 (m)"].Points.AddXY(DateTime.Now.ToLongTimeString(), paket[10]);
                        this.chrtYükseklik.Series["YÜKSEKLİK 2 (m)"].Points.AddXY(DateTime.Now.ToLongTimeString(), paket[11]);
                        this.chrtİrtifaFarkı.Series["İRTİFA FARKI   (m)"].Points.AddXY(DateTime.Now.ToLongDateString(), paket[12]);
                        this.chrtİnişHızı.Series["İNİŞ HIZI   (m/s)"].Points.AddXY(DateTime.Now.ToLongTimeString(), paket[13]);
                        this.chrtSıcaklık.Series["SICAKLIK   (Cᵒ)"].Points.AddXY(DateTime.Now.ToLongTimeString(), paket[14]);
                        this.chrtPilGerilimi.Series["PİL GERİLİMİ   (V)"].Points.AddXY(DateTime.Now.ToLongTimeString(), paket[15]);
                    }
                    catch (Exception )
                    {

                    }
                    int l = i - 9;
                    telemetriEkrani.FirstDisplayedScrollingRowIndex = l;
                }            
            }
            catch (Exception)
            {  
                
            }
        }

        private void TelemetriVeri()
        {
            try
            {
                if (serialPortOkuma.IsOpen == false)
                {
                    serialPortOkuma.PortName = cmbxSerialPort.Text;
                    serialPortOkuma.BaudRate = 115200;
                    try
                    {
                        serialPortOkuma.Open();
                    }
                    catch (Exception)
                    {
                        MessageBox.Show("Port bağlantısı yapılamadı.");
                    }
                }
                for (; ; )
                {
                    try
                    {                                                                    
                            data = serialPortOkuma.ReadLine();
                            paket = data.Split('*');
                            int deger = data.Length;
                            Console.WriteLine(data.Length);
                            Console.ReadLine();
                            Thread.Sleep(200);
                    }
                    catch (Exception)
                    {
                        if (!serialPortOkuma.IsOpen == true)
                        {
                            txtData.Text = "0";
                            b = 0;
                            Zamanlayici.Stop();                           
                            thread.Abort();
                            serialPortOkuma.Close();
                        }
                    }
                }
            }
            catch
            {
            }
        }      
       
        private void BtnConnect_Click(object sender, EventArgs e)
        {
            //if (cmbxSerialPort.Text == "")
            //return;
            //Zamanlayici.Start();
            //timer1.Start();
            //thread = new Thread(TelemetriVeri);
            //thread.Start();
            //txtConnect.Text = "Sistem başladı...";
            //txtConnect.ForeColor = Color.DarkRed;
            MessageBox.Show("Sistem Başladı");
        }
       
        private void BtnDisconnect_Click(object sender, EventArgs e)
        {
            //if (cmbxSerialPort.Text == "")
            //    return;
            //Zamanlayici.Stop();
            //thread.Abort();
            //serialPortOkuma.Close();
            MessageBox.Show("Bağlantı Kesildi.");
        }

        private void BtnChrome_Click(object sender, EventArgs e)
        {
            if (txtWeb.Text =="http://192.168.1.1" || txtWeb.Text == "http://huginn.local/" )
            {
                chrome.Load(txtWeb.Text);
            }           
        }
        
        private void CopyAlltoClipboard()
        {
            telemetriEkrani.SelectAll();
            DataObject dataObj = telemetriEkrani.GetClipboardContent();
            if (dataObj != null)
                Clipboard.SetDataObject(dataObj);
        }

        private void BtnExcel_Click(object sender, EventArgs e)
        {
            try
            {
                CopyAlltoClipboard();
                Microsoft.Office.Interop.Excel.Application xlexcel;
                Microsoft.Office.Interop.Excel.Workbook xlWorkBook;
                Microsoft.Office.Interop.Excel.Worksheet xlWorkSheet;
                object misValue = System.Reflection.Missing.Value;
                Excel.Application application = new Excel.Application();
                xlexcel = application;
                xlexcel.Visible = true;
                xlWorkBook = xlexcel.Workbooks.Add(misValue);
                xlWorkSheet = (Excel.Worksheet)xlWorkBook.Worksheets.get_Item(1);
                Excel.Range CR = (Excel.Range)xlWorkSheet.Cells[1, 1];
                CR.Select();
                xlWorkSheet.PasteSpecial(CR, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, true);
            }
            catch (Exception hata)
            {
                MessageBox.Show(hata.StackTrace);
            }
        }
   
        
        //3 BOYUTLU SİMÜLASYON
        private void Renk_ataması(float step)
        {
            if (step < 45)
                GL.Color3(renk2);
            else if (step < 90)
                GL.Color3(renk1);
            else if (step < 135)
                GL.Color3(renk2);
            else if (step < 180)
                GL.Color3(renk1);
            else if (step < 225)
                GL.Color3(renk2);
            else if (step < 270)
                GL.Color3(renk1);
            else if (step < 315)
                GL.Color3(renk2);
            else if (step < 360)
                GL.Color3(renk1);
        }


        private void Silindir(float step, float topla, float radius, float dikey1, float dikey2)
        {
            float eski_step = 0.1f;
            GL.Begin(PrimitiveType.Quads); // dairenin Y ekseninin çizimi .
            while (step <= 360)
            {
                Renk_ataması(step);
                float ciz1_x = (float)(radius * Math.Cos(step * Math.PI / 180F));
                float ciz1_y = (float)(radius * Math.Sin(step * Math.PI / 180F));
                GL.Vertex3(ciz1_x, dikey1, ciz1_y);

                float ciz2_x = (float)(radius * Math.Cos((step + 2) * Math.PI / 180F));
                float ciz2_y = (float)(radius * Math.Sin((step + 2) * Math.PI / 180F));
                GL.Vertex3(ciz2_x, dikey1, ciz2_y);

                GL.Vertex3(ciz1_x, dikey2, ciz1_y);
                GL.Vertex3(ciz2_x, dikey2, ciz2_y);
                step += topla;
            }
            GL.End();
            GL.Begin(PrimitiveType.Lines);
            step = eski_step;
            topla = step;


            // ÜST KAPAK
            while (step <= 180)
            {
                Renk_ataması(step);
                float ciz1_x = (float)(radius * Math.Cos(step * Math.PI / 180F));
                float ciz1_y = (float)(radius * Math.Sin(step * Math.PI / 180F));
                GL.Vertex3(ciz1_x, dikey1, ciz1_y);

                float ciz2_x = (float)(radius * Math.Cos((step + 180) * Math.PI / 180F));
                float ciz2_y = (float)(radius * Math.Sin((step + 180) * Math.PI / 180F));
                GL.Vertex3(ciz2_x, dikey1, ciz2_y);

                GL.Vertex3(ciz1_x, dikey1, ciz1_y);
                GL.Vertex3(ciz2_x, dikey1, ciz2_y);
                step += topla;
            }
            step = eski_step;
            topla = step;



            //ALT KAPAK.
            while (step <= 180)
            {
                Renk_ataması(step);

                float ciz1_x = (float)(radius * Math.Cos(step * Math.PI / 180F));
                float ciz1_y = (float)(radius * Math.Sin(step * Math.PI / 180F));
                GL.Vertex3(ciz1_x, dikey2, ciz1_y);

                float ciz2_x = (float)(radius * Math.Cos((step + 180) * Math.PI / 180F));
                float ciz2_y = (float)(radius * Math.Sin((step + 180) * Math.PI / 180F));
                GL.Vertex3(ciz2_x, dikey2, ciz2_y);

                GL.Vertex3(ciz1_x, dikey2, ciz1_y);
                GL.Vertex3(ciz2_x, dikey2, ciz2_y);
                step += topla;
            }
            GL.End();
        }


        private void Koni(float step, float topla, float radius1, float radius2, float dikey1, float dikey2)
        {
            float eski_step = 0.1f;


            //DAİRENİN Y EKSENİNİN ÇİZİMİ.
            GL.Begin(PrimitiveType.Lines);
            while (step <= 360)
            {
                Renk_ataması(step);
                float ciz1_x = (float)(radius1 * Math.Cos(step * Math.PI / 180F));
                float ciz1_y = (float)(radius1 * Math.Sin(step * Math.PI / 180F));
                GL.Vertex3(ciz1_x, dikey1, ciz1_y);

                float ciz2_x = (float)(radius2 * Math.Cos(step * Math.PI / 180F));
                float ciz2_y = (float)(radius2 * Math.Sin(step * Math.PI / 180F));
                GL.Vertex3(ciz2_x, dikey2, ciz2_y);
                step += topla;
            }
            GL.End();

            GL.Begin(PrimitiveType.Lines);
            step = eski_step;
            topla = step;

            //ÜST KAPAK.
            while (step <= 180)
            {
                Renk_ataması(step);
                float ciz1_x = (float)(radius2 * Math.Cos(step * Math.PI / 180F));
                float ciz1_y = (float)(radius2 * Math.Sin(step * Math.PI / 180F));
                GL.Vertex3(ciz1_x, dikey2, ciz1_y);

                float ciz2_x = (float)(radius2 * Math.Cos((step + 180) * Math.PI / 180F));
                float ciz2_y = (float)(radius2 * Math.Sin((step + 180) * Math.PI / 180F));
                GL.Vertex3(ciz2_x, dikey2, ciz2_y);

                GL.Vertex3(ciz1_x, dikey2, ciz1_y);
                GL.Vertex3(ciz2_x, dikey2, ciz2_y);
                step += topla;
            }
            step = eski_step;
#pragma warning disable IDE0059 // Unnecessary assignment of a value
            topla = step;
#pragma warning restore IDE0059 // Unnecessary assignment of a value
            GL.End();
        }


        private void Pervane(float yukseklik, float uzunluk, float kalinlik, float egiklik)
        {
            //float radius;
            //float angle;
            //  radius = 10f; 
            // angle = 45.0f;
            GL.Begin(PrimitiveType.Quads);

            GL.Color3(renk2);
            GL.Vertex3(uzunluk, yukseklik, kalinlik);
            GL.Vertex3(uzunluk, yukseklik + egiklik, -kalinlik);
            GL.Vertex3(0, yukseklik + egiklik, -kalinlik);
            GL.Vertex3(0, yukseklik, kalinlik);

            GL.Color3(renk2);
            GL.Vertex3(-uzunluk, yukseklik + egiklik, kalinlik);
            GL.Vertex3(-uzunluk, yukseklik, -kalinlik);
            GL.Vertex3(0, yukseklik, -kalinlik);
            GL.Vertex3(0, yukseklik + egiklik, kalinlik);

            GL.Color3(renk1);
            GL.Vertex3(kalinlik, yukseklik, -uzunluk);
            GL.Vertex3(-kalinlik, yukseklik + egiklik, -uzunluk);
            GL.Vertex3(-kalinlik, yukseklik + egiklik, 0.0);//+
            GL.Vertex3(kalinlik, yukseklik, 0.0);//-

            GL.Color3(renk1);
            GL.Vertex3(kalinlik, yukseklik + egiklik, +uzunluk);
            GL.Vertex3(-kalinlik, yukseklik, +uzunluk);
            GL.Vertex3(-kalinlik, yukseklik, 0.0);
            GL.Vertex3(kalinlik, yukseklik + egiklik, 0.0);
            GL.End();
        }

        private void GlControl1_Paint(object sender, PaintEventArgs e)
        {
            float step = 1.0f;   //Adım genişliği .
            float topla = step;   //Tampon.
            float radius = 3.5f;    // Model uydunun yarıçapı.
            GL.Clear(ClearBufferMask.ColorBufferBit);   //Buffer temizlenmez ise görüntüler üst üste biner o yüzden temizliyoruz
            GL.Clear(ClearBufferMask.DepthBufferBit);

            Matrix4 perspective = Matrix4.CreatePerspectiveFieldOfView(1.04f, 4 / 3, 1, 10000);
            Matrix4 lookat = Matrix4.LookAt(35, 0, 0, 0, 0, 0, 0, 1, 0);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.LoadMatrix(ref perspective);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();
            GL.LoadMatrix(ref lookat);
            GL.Viewport(0, 0, glControl1.Width, glControl1.Height);
            GL.Enable(EnableCap.DepthTest);
            GL.DepthFunc(DepthFunction.Less);


            //Asagidaki fonksiyonlar simülasyonumuzu hareket ettirmemizi sağlıyor.
            GL.Rotate(x, 0.2, 0.0, 0.0);
            GL.Rotate(z, 0.0, 1.0, 0.0);
            GL.Rotate(y, 0.0, 0.0, 1.0);


            //Çizim Fonksiyonları
            Silindir(step, topla, radius, 3, -5);
            Koni(0.01f, 0.01f, radius, 3.0f, 3, 5);//Ust koni
            Koni(0.01f, 0.01f, radius, 2.0f, -5.0f, -10.0f);//Alt koni
            Silindir(0.01f, topla, 0.07f, 9, 3);// rotor      


            //Pervane(Yükseklik,Pervane Uzunluğu,Pervane Genişliği,Pervane açısı)
            Silindir(0.01f, topla, 0.2f, 9, 9.3f);
            Pervane(9.0f, 7.0f, 0.3f, 0.3f);
            Silindir(0.01f, topla, 0.2f, 7.3f, 7f);
            Pervane(7.0f, 7.0f, 0.3f, 0.3f);


            //// AŞAĞIDA X, Y, Z EKSEN CİZGELERİ ÇİZDİRİLİYOR
            GL.Begin(PrimitiveType.Lines);

            GL.Color3(Color.FromArgb(250, 0, 0));
            GL.Vertex3(-1000, 0, 0);
            GL.Vertex3(1000, 0, 0);

            GL.Color3(Color.FromArgb(25, 150, 100));
            GL.Vertex3(0, 0, -1000);
            GL.Vertex3(0, 0, 1000);

            GL.Color3(Color.White);
            GL.Vertex3(0, 1000, 0);
            GL.Vertex3(0, -1000, 0);

            GL.End();
            glControl1.SwapBuffers();
        }
        private void GlControl1_Load(object sender, EventArgs e)
        {
            GL.ClearColor(0.0f, 0.0f, 0.0f, 0.0f);
            GL.Enable(EnableCap.DepthTest);

        }

        private void TelemetriEkrani_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            foreach (DataGridViewRow row in telemetriEkrani.Rows)
            {            
                   row.DefaultCellStyle.BackColor = Color.Green;                                                                     
            }         
        }

        private void GlControl1_BackColorChanged(object sender, EventArgs e)
        {
            glControl1.BackColor = Color.FromArgb(0, 0, 0);

        }

        private void Timer1_Tick(object sender, EventArgs e)
        {
            
            try
            {               
               


                map.MapProvider = GMapProviders.GoogleSatelliteMap;
                map.Overlays.Clear();
                map2.MapProvider = GMapProviders.GoogleSatelliteMap;
                map2.Overlays.Clear();
                string lat = paket[16];
                double lat_d = double.Parse(lat, System.Globalization.CultureInfo.InvariantCulture);
                string longt = paket[17];
                double longt_d = double.Parse(longt, System.Globalization.CultureInfo.InvariantCulture);
                string lat2 = paket[19];
                double lat2_d = double.Parse(lat2, System.Globalization.CultureInfo.InvariantCulture);
                string longt2 = paket[20];
                double longt2_d = double.Parse(longt2, System.Globalization.CultureInfo.InvariantCulture); 

                GMarkerGoogle markerGoogle = new GMarkerGoogle(new PointLatLng(lat_d, longt_d), GMarkerGoogleType.red); 
                map.Position = new PointLatLng(lat_d, longt_d);
                GMarkerGoogle marker2Google = new GMarkerGoogle(new PointLatLng(lat2_d, longt2_d), GMarkerGoogleType.blue);
                map2.Position = new PointLatLng(lat2_d, longt2_d);
                data = "";
                glControl1.Invalidate();

            }
            catch (Exception)
            {             
            }
        }

        private void BtnKameraAc_Click(object sender, EventArgs e)
        {
            if (txtVideoCapture.Text != "http://192.168.1.2")
            {
                txtVideoCapture.Text = "http://192.168.1.2";
                videoCapture.Load(txtVideoCapture.Text);
            }
            panel3.Visible = true;
            btnKameraKapat.Enabled = true;
        }

        private void BtnKameraKapat_Click(object sender, EventArgs e)
        {
            panel3.Visible = false;
            btnKameraKapat.Enabled = false;
        }

        private void BtnServoAc_Click(object sender, EventArgs e)
        {
            //serialPortOkuma.Write("!" + txtServoKontrol.Text.ToString());
            //serialPortOkuma.Write("!");
            btnServoAc.Enabled = false;
            btnServoKapat.Enabled = true;
            btnServoAc.SendToBack();
            txtServoKontrol.Text = "Kilitli";
        }

        private void BtnServoKapat_Click(object sender, EventArgs e)
        {
            //serialPortOkuma.Write("N" + txtServoKontrol.Text.ToString());
            //serialPortOkuma.Write("@");
            btnServoKapat.Enabled = false;
            btnServoAc.Enabled = true;
            btnServoKapat.SendToBack();
            txtServoKontrol.Text = "Kilidi Açık";

        }

        private void BtnMotorAc_Click(object sender, EventArgs e)           //alt + 2
        {
            //serialPortOkuma.Write("%" + txtServoKontrol.Text.ToString());
            //serialPortOkuma.Write("%");
            btnMotorAc.Enabled = false;
            btnMotorKapat.Enabled = true;
            btnMotorAc.SendToBack();
            txtMotorKontrol.Text = "Açık";
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            if (txtWeb.Text == "http://192.168.1.1/fupload" && video == 0)
            {
                serialPortOkuma.Write("é");
                video++;
            }
        }

        private void BtnMotorKapat_Click(object sender, EventArgs e)        //alt + 3
        {
            //serialPortOkuma.Write("&" + txtServoKontrol.Text.ToString());
            //serialPortOkuma.Write("&");
            btnMotorKapat.Enabled = false;
            btnMotorAc.Enabled = true;
            btnMotorKapat.SendToBack();
            txtMotorKontrol.Text = "Kapalı";
        }

        private void OnError(object sender, ErrorsEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine(e.Message);
        }

        private void NumericUpDown1_ValueChanged_1(object sender, EventArgs e)
        {
            glControl1.Invalidate();
        }

        private void NumericUpDown2_ValueChanged_1(object sender, EventArgs e)
        {
            glControl1.Invalidate();

        }
    }
}
