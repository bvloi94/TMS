using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace TMS.Utils
{
    public class PDFUtil
    {
        public static byte[] ReportContent(IEnumerable<byte[]> imagesbyte)
        {
            MemoryStream ms = new MemoryStream();
            Document document = new Document(PageSize.A4);
            PdfWriter PDFWriter = PdfWriter.GetInstance(document, ms);

            document.Open();
            float width = PageSize.A4.Width - document.RightMargin - document.LeftMargin;
            float height = PageSize.A4.Height - document.TopMargin - document.BottomMargin;
            float leftHeight = height;

            foreach (byte[] imagebyte in imagesbyte)
            {
                Image image = Image.GetInstance(imagebyte);
                float h = image.ScaledHeight;
                float w = image.ScaledWidth;
                float scalePercent;

                if (w > width)
                {
                    scalePercent = width / w;
                    w = w * scalePercent;
                    h = h * scalePercent;
                    image.ScaleAbsolute(w, h);
                }

                //Image newImage = CropImage(image, PDFWriter, 0, 2 * h / 3, w, 2 * h / 3);
                //document.Add(newImage);

                if (h < leftHeight)
                {
                    document.Add(image);
                    leftHeight = leftHeight - h;
                }
                else
                {
                    float htemp = h;
                    if (htemp > leftHeight)
                    {
                        Image newImage = CropImage(image, PDFWriter, 0, htemp - leftHeight, w, leftHeight);
                        document.Add(newImage);
                        htemp = htemp - leftHeight;
                        leftHeight = height;
                    }
                    while (htemp > height)
                    {
                        Image newImage = CropImage(image, PDFWriter, 0, htemp - height, w, height);
                        document.Add(newImage);
                        htemp = htemp - height;
                    }
                    if (htemp < leftHeight)
                    {
                        Image newImage = CropImage(image, PDFWriter, 0, 0, w, htemp);
                        document.Add(newImage);
                        leftHeight = leftHeight - htemp;
                        htemp = 0;
                    }


                    //PdfContentByte canvas = PDFWriter.DirectContent;
                    //float htemp = h;
                    //if (htemp >= leftHeight && htemp <= leftHeight + document.BottomMargin)
                    //{
                    //    canvas.AddImage(image, w, 0, 0, h, document.LeftMargin, (document.BottomMargin + leftHeight) - htemp);
                    //    htemp = htemp - (document.BottomMargin + leftHeight);
                    //    leftHeight = height;
                    //    document.NewPage();
                    //}
                    //else if (htemp > leftHeight + document.BottomMargin && htemp < leftHeight + document.BottomMargin + PageSize.A4.Height)
                    //{
                    //    canvas.AddImage(image, w, 0, 0, h, document.LeftMargin, (document.BottomMargin + leftHeight) - htemp);
                    //    htemp = htemp - (document.BottomMargin + leftHeight);
                    //    leftHeight = height;
                    //    document.NewPage();
                    //}
                    //while (htemp > PageSize.A4.Height)
                    //{
                    //    canvas.AddImage(image, w, 0, 0, h, document.LeftMargin, PageSize.A4.Height - htemp);
                    //    htemp = htemp - PageSize.A4.Height;
                    //    leftHeight = height;
                    //    document.NewPage();
                    //}
                    //if (htemp > 0 && htemp < PageSize.A4.Height)
                    //{
                    //    canvas.AddImage(image, w, 0, 0, h, document.LeftMargin, PageSize.A4.Height - htemp);
                    //    leftHeight = PageSize.A4.Height - htemp - document.BottomMargin;
                    //    htemp = 0;
                    //}
                }
            }

            document.Close();
            return ms.ToArray();
        }

        public static Image CropImage(Image image, PdfWriter writer, float x, float y, float width, float height)
        {
            try
            {
                PdfContentByte cb = writer.DirectContent;
                PdfTemplate t = cb.CreateTemplate(width, height);
                float origWidth = image.ScaledWidth;
                float origHeight = image.ScaledHeight;
                t.AddImage(image, origWidth, 0, 0, origHeight, -x, -y);
                return Image.GetInstance(t);
            }
            catch
            {
                throw;
            }
        }
    }
}