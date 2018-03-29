using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FFTViewer
{
    class ReverseTest
    {
        private static string OpenFile()
        {
            string filename;
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Filter = "*.mp3|*.mp3";
                if (dialog.ShowDialog() == DialogResult.Cancel)
                {
                    return null;
                }
                filename = dialog.FileName;
            }
            return filename;
        }

        [STAThread]
        public unsafe static void Main()
        {
            byte[] convertedData;
            WaveFormat waveFormat;
            {
                int size;
                WaveFormat f = new WaveFormat(44100, 16, 2);
                using (Mp3FileReader r = new Mp3FileReader(OpenFile()))
                {
                    using (var conv = new WaveFormatConversionStream(f, r))
                    {
                        int len = (int)conv.Length;
                        size = len / 4;
                        convertedData = new byte[len];
                        conv.Read(convertedData, 0, len);
                    }
                }
                waveFormat = f;
            }
            const int frameLength = 4;
            int frameCount = convertedData.Length / frameLength;

            var bufferLength = 8192;
            var windowRange = 4.0f;
            float[] windowFunction;
            {
                windowFunction = new float[bufferLength];
                float sigma = bufferLength / 2 / windowRange;
                int offset = bufferLength / 2;
                for (int i = 0; i < bufferLength; ++i)
                {
                    float x = (i - offset);
                    windowFunction[i] = 1;// (float)Math.Exp(-x * x / 2 / sigma / sigma);
                }
            }

            FFTW.Complex* inputBuffer, outputBuffer;
            IntPtr fftwPlanForward, fftwPlanBackward;
            {
                UIntPtr allocSize = (UIntPtr)(8 * (uint)bufferLength);
                inputBuffer = FFTW.Malloc(allocSize);
                outputBuffer = FFTW.Malloc(allocSize);
                fftwPlanForward = FFTW.PlanDft1D(bufferLength, inputBuffer, outputBuffer, -1, 0 /* FFTW_MEASURE */);
                fftwPlanBackward = FFTW.PlanDft1D(bufferLength, outputBuffer, inputBuffer, 1, 0 /* FFTW_MEASURE */);
                for (int i = 0; i < bufferLength; ++i)
                {
                    inputBuffer[i].Imaginary = 0; //Clear only imaginary
                }
            }

            byte[] output = new byte[convertedData.Length];

            void ReadMp3Buffer(int frame)
            {
                fixed (byte* ptrByte = convertedData)
                {
                    short* ptr = (short*)(ptrByte + frameLength * frame);
                    for (int i = 0; i < bufferLength; ++i)
                    {
                        var r = ptr[i * 2] / 32768f * windowFunction[i];
                        if (float.IsNaN(r))
                        {

                        }
                        inputBuffer[i].Real = r;
                        inputBuffer[i].Imaginary = 0;
                    }
                }
            }

            void WriteOutputBuffer(int frame)
            {
                fixed (byte* ptrByte = output)
                {
                    short* ptr = (short*)(ptrByte + frameLength * frame);
                    for (int i = 0; i < bufferLength; ++i)
                    {
                        if (float.IsNaN((inputBuffer[i].Real)))
                        {

                        }
                        float floatData = inputBuffer[i].Real * windowFunction[i];
                        short data = (short)(floatData * 32768f / 10);
                        ptr[i * 2] += data;
                        ptr[i * 2 + 1] += data;
                    }
                }
            }

            //Main loop
            {
                const int step = 1000;
                const int reportPercentage = 5;
                int nextReportFrame = 0;
                int nextReportPercentage = 0;

                float[] compare = new float[bufferLength * 2];
                for (int pos = 0; pos + bufferLength <= frameCount; pos += step)
                {
                    if (pos > nextReportFrame)
                    {
                        System.Diagnostics.Debug.Print("{0}%", nextReportPercentage);
                        nextReportPercentage += reportPercentage;
                        nextReportFrame = (int)(nextReportPercentage / 100f * frameCount);
                    }

                    ReadMp3Buffer(pos);

                    FFTW.Execute(fftwPlanForward);
                    System.Runtime.InteropServices.Marshal.Copy(new IntPtr(inputBuffer), compare, 0, bufferLength * 2);

                    //Perform transform
                    for (int i = 0; i < bufferLength; ++i)
                    {
                        if (i == 0 && inputBuffer[i].Real != 0)
                        {

                        }
                        if (float.IsNaN(outputBuffer[i].Imaginary + outputBuffer[i].Real))
                        {

                        }
                    }

                    FFTW.Execute(fftwPlanBackward);
                    WriteOutputBuffer(pos);
                }
            }

            var outputFileName = @"E:\test.wav";
            if (File.Exists(outputFileName))
            {
                File.Delete(outputFileName);
            }
            using (var outputStream = File.Create(outputFileName))
            {
                using (var outputProvider = new RawSourceWaveStream(output, 0, output.Length, waveFormat))
                {
                    WaveFileWriter.WriteWavFileToStream(outputStream, outputProvider);
                }
            }
        }
    }
}
