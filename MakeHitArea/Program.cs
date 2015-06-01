using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.IO;

namespace MakeHitArea
{
    class Program
    {
        static void Main(string[] args)
        {
            foreach (string s in args)
            {
                using (HitAreaBuilder builder = new HitAreaBuilder(s))
                {
                    builder.Build();
                }
            }
        }
    }

    public class HitAreaBuilder : IDisposable
    {
        Bitmap thePicture = null;
        string theOutName = string.Empty;
        int theWidth = 0;
        int theHeight = 0;
        byte[] theBaked = null;

        public HitAreaBuilder(string file)
        {
            try
            {
                using (Image image = Image.FromFile(file))
                {
                    thePicture = new Bitmap(image);
                }

                string path = Path.GetDirectoryName(file);
                if (string.IsNullOrEmpty(path))
                {
                    path = Environment.CurrentDirectory;
                }

                string filename = Path.GetFileNameWithoutExtension(file);
                theOutName = string.Format("{0}\\{1}.hit", path, filename);
            }
            catch
            {
                Console.Error.WriteLine("Can't load image {0}", file);
            }
        }

        public void Dispose()
        {
            if (thePicture != null)
            {
                thePicture.Dispose();
                thePicture = null;
            }
        }

        public void Build()
        {
            if (PrepareData())
            {
                SaveData();
            }
        }

        bool PrepareData()
        {
            bool succ = false;

            if (thePicture != null)
            {
                theWidth = thePicture.Width;
                theHeight = thePicture.Height;

                // Align the width for easier processing
                int linebytes = (theWidth + 7) / 8;
                theWidth = linebytes * 8;

                theBaked = new byte[linebytes * theHeight];

                for (int y = 0; y < theHeight; y++)
                {
                    for (int x = 0; x < linebytes; x++)
                    {
                        byte dat = 0;

                        for (int bit = 0; bit < 8; bit++)
                        {
                            if (x * 8 + bit < thePicture.Width)
                            {
                                // If the pixel is visible enough, we set the bit to hit-able.

                                Color c = thePicture.GetPixel(x * 8 + bit, y);
                                if (c.A >= 200)
                                {
                                    dat = (byte)(dat | (1 << bit));
                                }
                            }
                        }

                        theBaked[y * linebytes + x] = dat;
                    }
                }

                succ = true;
            }

            return succ;
        }

        void SaveData()
        {
            try
            {
                using (FileStream fs = new FileStream(theOutName, FileMode.Create, FileAccess.Write))
                {
                    using (BinaryWriter writer = new BinaryWriter(fs))
                    {
                        writer.Write((short)theWidth);
                        writer.Write((short)theHeight);
                        writer.Write(theBaked);
                    }
                }
            }
            catch
            {
                Console.Error.WriteLine("Write to file failed.");
            }
        }
    }
}
