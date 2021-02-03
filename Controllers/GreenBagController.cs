using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Azure.Storage.Blobs;

namespace GreenBagImageManager2.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class GreenBagController : Controller
    {
        //public Settings Settings { get; set; }
        private Settings settings;

        public GreenBagController(Settings settings)
        {
            this.settings = settings;
        }

        [HttpGet]
        [Route("{tagNumber}")]
        [Route("{tagNumber}/{showMarkup:bool=true}")]
        public async Task<IActionResult> Get(string tagNumber, bool showMarkup = true)
        {
            BlobContainerClient container = new BlobContainerClient(settings.BlobStorage, "greenbagimage");
            BlobClient blob = container.GetBlobClient(tagNumber + ".jpg");

            var ms = new MemoryStream();
            var msResult = new MemoryStream();

            var resultStream = await blob.DownloadToAsync(ms)
                .ContinueWith(async t =>
                {
                    if (t.IsFaulted)
                        throw new Exception();

                    ms.Position = 0;

                    // No markup?  Done - return our memory stream
                    if (showMarkup == false)
                        return ms;

                    // Else, try & get the markup and apply to the image
                    Bitmap image = Bitmap.FromStream(ms) as Bitmap;
                    try
                    {
                        var props = await blob.GetPropertiesAsync();
                        string markup = null;
                        if (!props.Value?.Metadata?.TryGetValue("markup", out markup) ?? false)
                            throw new Exception();

                        var objects = Helpers.ParseDetectedObjects(markup);

                        using (var g = Graphics.FromImage(image))
                        {
                            var dotBrush = new SolidBrush(Color.HotPink);
                            var notCountedBrush = new SolidBrush(Color.Yellow);

                            foreach (var obj in objects)
                            {
                                if (obj.IsCounted)
                                    g.FillEllipse(dotBrush, (obj.X + obj.Width / 2), (obj.Y + obj.Height / 2), 20, 20);
                                else
                                    g.FillEllipse(notCountedBrush, (obj.X + obj.Width / 2), (obj.Y + obj.Height / 2), 20, 20);

                                var pen = new Pen(ClassToColor(obj.Class), 2);
                                g.DrawRectangle(pen, obj.X, obj.Y, obj.Width, obj.Height);
                            }
                        }
                    }
                    catch (Exception)
                    {

                    }

                    // Always exit out this way :)
                    image.Save(msResult, ImageFormat.Jpeg);
                    msResult.Position = 0;
                    return msResult;

                });

            return new FileStreamResult(await resultStream, "image/jpg");
        }

        [HttpGet]
        [Route("Markup/{tagNumber}")]
        public async Task<IEnumerable<DetectedObject>> Get(string tagNumber)
        {
            BlobContainerClient container = new BlobContainerClient(settings.BlobStorage, "greenbagimage");
            BlobClient blob = container.GetBlobClient(tagNumber + ".jpg");
            
            var props = await blob.GetPropertiesAsync();
            string markup = null;
            if (!props.Value?.Metadata?.TryGetValue("markup", out markup) ?? false)
                throw new Exception();

            /* 
             * Markup comes in as a single string with colon ':' separated entries of each object.  This is the snippet of an example:
             *
             *  belt 1225 391 53 306 False:belt 1238 81 40 306 False:belt 118 688 68 28 False:pet 161 441 117 62 True:pet 813 586 96 113 True:pet 885 527 103 136 True
             *
             * The "DetectedObject" class has a static method called "Parse" which will deserialize a DetectedObject from a string.  
             *
             */

            
            // TODO - break apart the "markup" string and parse each entry into a "DetectedObject".  Beware that some parsed objects are null - those should be excluded
            var objects = new List<DetectedObject>();

            return objects;
        }

        [HttpPost]
        [Route("Markup/{tagNumber=001-0004769377}")]
        public IActionResult Post(IEnumerable<DetectedObject> objects)
        {
            // Just an error for right now
            return new StatusCodeResult(500);
        }

        private static Color ClassToColor(string type)
        {
            switch (type.ToLower())
            {
                case "can":
                case "alum":
                case "c":
                    return System.Drawing.Color.DarkRed;

                case "pet":
                case "plastic":
                case "p":
                    return System.Drawing.Color.DarkGreen;

                case "glass":
                case "g":
                    return System.Drawing.Color.DarkBlue;

                case "refill":
                case "r":
                    return System.Drawing.Color.DarkOrange;


                default:
                    return System.Drawing.Color.LimeGreen;
            }
        }
    }
}
