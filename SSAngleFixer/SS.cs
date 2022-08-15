using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace ReplayTableFixer
{
    internal class SS
    {
        private byte[] ssBuffer;
        private int rtOffset;
        private readonly byte[] xgd3 = new byte[] { 0xE1, 0x0F, 0x31, 0x10, 0x00, 0x03, 0x41, 0x00, 0x00, 0xFC, 0xAE, 0xFF, 0x00, 0x23, 0x8E, 0x0F };
        private readonly byte[] xgd2 = new byte[] { 0xE1, 0x0F, 0x31, 0x10, 0x00, 0x04, 0xFB, 0x20, 0x00, 0xFB, 0x04, 0xDF, 0x00, 0x20, 0x33, 0x9F };
        private bool isLoaded;

        public SS()
        {
            isLoaded = false;
            ssBuffer = Array.Empty<byte>();
        }

        public bool LoadSS(string file)
        {
            try
            {
                using (FileStream fs = new FileStream(file, FileMode.Open))
                {
                    if (fs.Length != 2048)
                    {
                        isLoaded = false;
                        return false;
                    }
                    var binReader = new BinaryReader(fs);
                    ssBuffer = binReader.ReadBytes(2048);
                }

                byte[] ssFormatID = new byte[16];
                Array.Copy(ssBuffer, ssFormatID, 16);

                if (xgd2.SequenceEqual(ssFormatID))
                    rtOffset = 0x200;
                else if (xgd3.SequenceEqual(ssFormatID))
                    rtOffset = 0x20;
                else
                {
                    isLoaded = false;
                    return false;
                }
                isLoaded = true;
                return true;
            }
            catch (Exception)
            {
                isLoaded = false;
                return false;
            }
        }

        public byte[] GetResponseRT(int entry)
        {
            byte[] response = new byte[5];
            int offset = rtOffset + (9 * (entry - 1)) + 4;
            for (int i = 0; i < 5; i++)
                response[i] = ssBuffer[offset + i];

            return response;
        }

        public bool writeSS(string filename, string[] entries)
        {
            if (!isLoaded) return false;

            if (entries.Length != 4) return false;

            for (int i = 0; i < 4; i++)
            {
                if (entries[i].Length != 10 || !IsHex(entries[i]))
                    return false;
            }

            for (int n = 5; n < 9; n++)
            {
                int offset = rtOffset + (9 * (n - 1)) + 4;
                for (int b = 0; b < 5; b++)
                {
                    ssBuffer[offset + b] = (byte)Int32.Parse(entries[n - 5].Substring(b * 2, 2),
                        System.Globalization.NumberStyles.HexNumber);
                }
            }

            try
            {
                File.WriteAllBytes(filename, ssBuffer);
            }
            catch
            {
                return false;
            }

            return true;
        }

        // compares the all rt data besides angles to make sure correct log is loaded
        public bool compareRestOfRT(string fullRT)
        {
            if (!isLoaded) return false;

            for (int i = 0; i < 40; i++)
            {
                byte b = (byte)Int32.Parse(fullRT.Substring(i * 2, 2),
                    System.Globalization.NumberStyles.HexNumber);
                if (b != ssBuffer[rtOffset + i])
                    return false;
            }

            for (int i = 72; i < 81; i++)
            {
                byte b = (byte)Int32.Parse(fullRT.Substring(i * 2, 2),
                    System.Globalization.NumberStyles.HexNumber);
                if (b != ssBuffer[rtOffset + i])
                    return false;
            }

            return true;
        }

        public static bool IsHex(IEnumerable<char> chars)
        {
            bool isHex;
            foreach (var c in chars)
            {
                isHex = ((c >= '0' && c <= '9') ||
                    (c >= 'a' && c <= 'f') ||
                    (c >= 'A' && c <= 'F'));

                if (!isHex)
                    return false;
            }
            return true;
        }

        public bool isXGD3()
        {
            return rtOffset == 0x20;
        }
    }

}