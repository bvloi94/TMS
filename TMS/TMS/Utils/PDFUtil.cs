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

                //float hTemp = h;
                //while (hTemp > leftHeight)
                //{
                //    PdfContentByte canvas = PDFWriter.DirectContent;
                //    canvas.AddImage(image, w, 0, 0, h, 0, - h / 3);
                //    document.NewPage();
                //    hTemp = hTemp - leftHeight;
                //    leftHeight = height;
                //}
                PdfContentByte canvas = PDFWriter.DirectContent;
                canvas.AddImage(image, 2 * w / 3, 1 * w / 3, 0, h, document.LeftMargin, document.BottomMargin);
                document.NewPage();
                //document.Add(image);
            }

            document.Close();
            return ms.ToArray();
        }
    }
}