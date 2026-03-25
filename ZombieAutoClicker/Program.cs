using System;
using System.Windows.Forms;

namespace ZombieAutoClicker
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            // 这里非常关键：把默认的 new Form1() 改成 new MainForm()
            Application.Run(new MainForm());
        }
    }
}