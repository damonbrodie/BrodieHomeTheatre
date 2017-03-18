using System;
using System.IO;
using System.Threading;
using System.Windows.Forms;


namespace BrodieTheatre
{
    public partial class FormMain : Form
    {
        public void writeLog(string Message)
        {
            DateTime now = DateTime.Now;
            int counter = 0;
            bool success = false;
            while (!success && counter < 3)
            {

                try
                {
                    using (StreamWriter file = File.AppendText("logging.txt"))
                    {
                        file.WriteLine(now.ToString("yyyy-MM-dd HH:mm:ss") + " " + Message);
                        success = true;
                    }
                }
                catch
                {
                    counter += 1;
                    Thread.Sleep(50);
                }
            }
        }
    }
}