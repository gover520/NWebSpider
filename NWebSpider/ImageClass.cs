using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace NWebSpider
{
    static class ImageClass
    {
        public static int MatchMax = 0;
        public static byte[] SrcHash = new byte[64];
        // 计算图像的Hash值 8*8求01编码
        public static byte[] ImageHash(Bitmap bmp)
        {
            byte[] pix = new byte[64];
            Rectangle rect=new Rectangle(0,0,8,8);
            System.Drawing.Imaging.BitmapData bmpdata=bmp.LockBits(rect,System.Drawing.Imaging.ImageLockMode.ReadOnly,bmp.PixelFormat);
            IntPtr ptr=bmpdata.Scan0;
            byte[] rgb;
            int ave = 0;
            if (bmp.PixelFormat != System.Drawing.Imaging.PixelFormat.Format8bppIndexed)
            {
                rgb = new byte[64 * 3];
                System.Runtime.InteropServices.Marshal.Copy(ptr, rgb, 0, 64 * 3);
                for (int i = 0; i < 8; i++)
                    for (int j = 0; j < 8; j++)
                    {
                        pix[i * 8 + j] = (byte)(rgb[i * 8 * 3 + j] * 0.11 + rgb[i * 8 * 3 + j + 1] * 0.59 + rgb[i * 8 * 3 + j + 2] * 0.3);
                        ave += pix[i * 8 + j];
                    }
            }
            else
            {
                System.Runtime.InteropServices.Marshal.Copy(ptr, pix, 0, 64);
                for (int i = 0; i < 64; i++){
                        ave += pix[i];
                    }
            }
            bmp.UnlockBits(bmpdata);
            ave /= 64;
            for (int i = 0; i < 64; i++)
            {
                if (pix[i] > ave)
                    pix[i] = 1;
                else pix[i] = 0;
            }
            return pix;
        }
        // 计算两条Hash编码
        public static int MatchHash(byte[] hash1, byte[] hash2)
        {
            byte[,] t = new byte[65, 65];
            Array.Clear(t, 0, t.Length);
            int max = 0;
            for (int i = 0; i <= 64; i++)
            {
                t[i, 0] = 0;
                t[0, i] = 0;
            }
            for (int i = 1; i <= 64; i++)
                for (int j = 1; j <= 64; j++)
                {
                    if (hash1[i - 1] == hash2[j - 1])
                    {
                        t[i, j] = (byte)(t[i - 1, j - 1] + 1);
                        //max = max > t[i, j] ? max : t[i, j];
                    }
                    else t[i, j] = t[i - 1, j] > t[i, j - 1] ? t[i - 1, j] : t[i, j - 1];
                }
            max = t[64, 64];
            if (MatchMax < max) MatchMax = max - 1;
            return max;
        }
        public static int MatchHash1(byte[] hash1, byte[] hash2)
        {
            int max = 0;

            for (int i = 0; i < 64; i++)
                if (hash1[i] == hash2[i]) max++;
            if (MatchMax < max) MatchMax = max - 1;
            return max;
        }
    }
}
