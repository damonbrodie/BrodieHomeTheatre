using System;
using System.IO;
using System.Windows.Forms;


namespace BrodieTheatre
{
    public partial class FormMain : Form
    {
        public void writeLog(string Message)
        {
            DateTime now = DateTime.Now;
            using (StreamWriter file = File.AppendText("logging.txt"))
            {
                file.WriteLine(now.ToString("yyyy-MM-dd HH:mm:ss") + " " + Message);
            }
        }
    }
}