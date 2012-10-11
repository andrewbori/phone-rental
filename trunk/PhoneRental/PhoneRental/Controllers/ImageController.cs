using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PhoneRental.Controllers
{
    public class ImageController : Controller
    {
        [HttpPost]
        public ContentResult Upload()
        {
            foreach (string uploadedFile in Request.Files)
            {
                HttpPostedFileBase file = Request.Files[uploadedFile];
                if (file != null && file.ContentLength > 0)
                {
                    // Checking the extension of the file
                    string uFileName = Server.HtmlEncode(file.FileName);
                    string extension = System.IO.Path.GetExtension(uFileName);
                    if (extension.ToUpper() != ".JPG" && extension.ToUpper() != ".JPG" &&
                        extension.ToUpper() != ".PNG" && extension.ToUpper() != ".BMP" && extension.ToUpper() != ".GIF")
                    {
                        return Content("{ \"result\" : \"Nem támogatott formátum! :(\" }", "application/json");
                    }

                    // Image resizing and saving
                    System.Drawing.Image image = System.Drawing.Image.FromStream(file.InputStream);
                    string filename = DateTime.Now.Ticks + ".jpg";
                    string filePath1 = Path.Combine(HttpContext.Server.MapPath("../Images/Devices"), filename);
                    string filePath2 = Path.Combine(HttpContext.Server.MapPath("../Images/Devices"), "thumb_" + filename);

                    resizeAndSave(image, filePath1, 800, 800);
                    resizeAndSave(image, filePath2, 128, 128);

                    //string filePath = Path.Combine(HttpContext.Server.MapPath("../Images/Upload"), filename);
                    //file.SaveAs(filePath);

                    return Content("{ \"result\" : \"Successful\", \"name\" : \"" + filename + "\" }", "application/json");

                }
            }
            return Content("{ \"result\" : \"Sikertelen feltöltés! :(\" }", "application/json");
        }

        [NonAction]
        public void resizeAndSave(System.Drawing.Image image, string filePath, int maxHeight, int maxWidth)
        {
            int imageHeight = image.Height;
            int imageWidth = image.Width;

            // Calculating new width and height
            if (imageHeight > imageWidth)
            {
                imageWidth = (imageWidth * maxHeight) / imageHeight;
                imageHeight = maxHeight;
            }
            else
            {
                imageHeight = (imageHeight * maxWidth) / imageWidth;
                imageWidth = maxWidth;
            }

            // Resizing...
            Bitmap bitmap = new Bitmap(image, imageWidth, imageHeight);

            // Saving...
            bitmap.Save(filePath, System.Drawing.Imaging.ImageFormat.Jpeg);
        }

    }
}
