using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace Huginn_2022
{
    static class Program
    {
        /// <summary>
        /// Uygulamanın ana girdi noktası.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Yer_istasyonu());
            try
            {
              /*  if (!File.Exists(@"C:\Users\yigit\Desktop\HUGINN_Telemetri.xlsx"))
                {
                    Form1.objbook.SaveAs(@"C:\Users\yigit\Desktop\HUGINN_Telemetri.xlsx");
                }
                else
                {
                    File.Delete(@"C:\Users\yigit\Desktop\HUGINN_Telemetri.xlsx");
                    Form1.objbook.SaveAs(@"C:\Users\yigit\Desktop\HUGINN_Telemetri.xlsx");
                }
                Form1.objbook.Close();*/
                
            }
            catch
            {

            }

        }
    }
}
