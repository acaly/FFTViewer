using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FFTViewer
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            var stream = new NAudio.Wave.WaveFileWriter(@"E:\C1_single.wav", new NAudio.Wave.WaveFormat(44100, 2));
            var writer = new System.IO.BinaryWriter(stream);
            for (int i = 0; i < 44100 * 10; ++i)
            {
                short value = (short)(Math.Cos(2 * Math.PI * 261 * i / 44100) * 16384); // Not too loud

                writer.Write(value);
                writer.Write(value);
            }
            writer.Flush();
            stream.Flush();
            stream.Close();
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new FormFourier());
        }
    }
}
