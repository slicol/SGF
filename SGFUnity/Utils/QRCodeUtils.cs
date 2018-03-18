using System;
using System.Runtime.CompilerServices;
using SGF;
using UnityEngine;
using ZXing;
using ZXing.Common;


namespace OT.Foundation
{
    public class QRCodeUtils
    {
        public const string LOG_TAG = "QRCodeUtils";

        public static Texture2D EncodeToImage(string content, int width, int height)
        {
			Debuger.Log(LOG_TAG, "EncodeToImage", "content:{0}, width:{1}, height:{2}", content, width, height);

            Texture2D texEncoded = null;
            BitMatrix bm = null;

            try
            {
                MultiFormatWriter mfw = new MultiFormatWriter();
                bm = mfw.encode(content, BarcodeFormat.QR_CODE, width, height);
            }
            catch (Exception e)
            {
                Debuger.LogError(LOG_TAG, "EncodeToImage", e.Message);
                return null;
            }

            texEncoded = new Texture2D(width, height);

            for (int x = 0; x < bm.Height; x++)
            {
                for (int y = 0; y < bm.Width; y++)
                {
                    int py = x;
                    int px = y;

                    if (bm[x, y])
                    {
                        
                        texEncoded.SetPixel(px,py, Color.black);
                    }
                    else
                    {
                        texEncoded.SetPixel(px, py, Color.white);
                    }
                }
            }

            texEncoded.Apply();
            return texEncoded;
        }

        
        public static string DecodeFromImage(Texture2D image)
        {
            Debuger.Log(LOG_TAG, "DecodeFromImage");

            try
            {
                Color32LuminanceSource src = new Color32LuminanceSource(image.GetPixels32(), image.width, image.height);

                Binarizer bin = new GlobalHistogramBinarizer(src);
                BinaryBitmap bmp = new BinaryBitmap(bin);

                MultiFormatReader mfr = new MultiFormatReader();
                Result result = mfr.decode(bmp);

                if (result != null)
                {
                    return result.Text;
                }
                else
                {
                    Debuger.LogError(LOG_TAG, "DecodeFromImage", "Decode 失败！");
                }
            }
            catch (Exception e)
            {
                Debuger.LogError(LOG_TAG, "DecodeFromImage",  e.Message);
            }

            return "";

        }
    }
}
