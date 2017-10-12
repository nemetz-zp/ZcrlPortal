using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text.RegularExpressions;

namespace ZcrlPortal.Extensions
{
    public static class HttpourFileBaseExtensions
    {
        private const int IMG_MIN_BYTES = 512;

        public static bool isValidFile(this HttpPostedFileBase ourFile)
        {
            if (ourFile == null)
            {
                return false;
            }

            if (!ourFile.InputStream.CanRead)
            {
                return false;
            }

            if(ourFile.ContentLength < 1)
            {
                return false;
            }

            return true;
        }

        public static bool IsImage(this HttpPostedFileBase ourFile)
        {
            if(ourFile == null)
            {
                return false;
            }

            //-------------------------------------------
            //  Check the image mime types
            //-------------------------------------------
            if (ourFile.ContentType.ToLower() != "image/jpg" &&
                        ourFile.ContentType.ToLower() != "image/jpeg" &&
                        ourFile.ContentType.ToLower() != "image/pjpeg" &&
                        ourFile.ContentType.ToLower() != "image/gif" &&
                        ourFile.ContentType.ToLower() != "image/x-png" &&
                        ourFile.ContentType.ToLower() != "image/png")
            {
                return false;
            }

            //-------------------------------------------
            //  Check the image extension
            //-------------------------------------------
            if (System.IO.Path.GetExtension(ourFile.FileName).ToLower() != ".jpg"
                && System.IO.Path.GetExtension(ourFile.FileName).ToLower() != ".png"
                && System.IO.Path.GetExtension(ourFile.FileName).ToLower() != ".gif"
                && System.IO.Path.GetExtension(ourFile.FileName).ToLower() != ".jpeg")
            {
                return false;
            }

            //-------------------------------------------
            //  Attempt to read the file and check the first bytes
            //-------------------------------------------
            try
            {
                if (!ourFile.InputStream.CanRead)
                {
                    return false;
                }

                if (ourFile.ContentLength < IMG_MIN_BYTES)
                {
                    return false;
                }

                byte[] buffer = new byte[512];
                ourFile.InputStream.Read(buffer, 0, 512);
                string content = System.Text.Encoding.UTF8.GetString(buffer);
                if (Regex.IsMatch(content, @"<script|<html|<head|<title|<body|<pre|<table|<a\s+href|<img|<plaintext|<cross\-domain\-policy",
                    RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Multiline))
                {
                    return false;
                }
            }
            catch (Exception)
            {
                return false;
            }

            //-------------------------------------------
            //  Try to instantiate new Bitmap, if .NET will throw exception
            //  we can assume that it's not a valid image
            //-------------------------------------------

            try
            {
                using (var bitmap = new System.Drawing.Bitmap(ourFile.InputStream))
                {
                }
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }
    }
}