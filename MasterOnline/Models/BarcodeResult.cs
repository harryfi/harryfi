using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Drawing;
using System.Drawing.Imaging;

namespace MasterOnline.Models
{
    public class BarcodeResult : ActionResult
    {
        private readonly string _text;
        public BarcodeResult(string textt)
        {
            _text = textt;
        }
        public override void ExecuteResult(ControllerContext context)
        {
            BarcodeLib.Barcode b;
            b = new BarcodeLib.Barcode();
            b.Alignment = BarcodeLib.AlignmentPositions.CENTER;
            BarcodeLib.TYPE type = BarcodeLib.TYPE.CODE128;
            if (type != BarcodeLib.TYPE.UNSPECIFIED)
            {
                b.IncludeLabel = true;
                //b.RotateFlipType = (RotateFlipType)Enum.Parse(typeof(RotateFlipType), "rotatenonflipnone", true);
                if (_text != null || _text != "" || _text != "-")
                {
                    Bitmap bitmap = new Bitmap(b.Encode(type, _text.Trim(), 350, 60));
                    context.HttpContext.Response.ContentType = "image/jpg";
                    bitmap.Save(context.HttpContext.Response.OutputStream, ImageFormat.Jpeg);
                }
            }
        }
    }
}