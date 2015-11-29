using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;
using Frapid.ApplicationState.Cache;
using Frapid.Authentication.DTO;
using Frapid.Messaging;

namespace Frapid.Authentication.Messaging
{
    public class ResetEmail
    {
        private readonly Reset _resetDetails;

        public ResetEmail(Reset reset)
        {
            _resetDetails = reset;
        }

        private string GetTemplate()
        {
            string path = "~/Catalogs/{catalog}/Areas/Frapid.Authentication/EmailTemplates/password-reset.html";
            path = path.Replace("{catalog}", AppUsers.GetCatalog());
            path = HostingEnvironment.MapPath(path);

            if (!File.Exists(path))
            {
                return string.Empty;
            }

            return path != null ? File.ReadAllText(path, Encoding.UTF8) : string.Empty;
        }

        private string ParseTemplate(string template)
        {
            string siteUrl = HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority);
            string link = siteUrl + "/account/reset/confirm?token=" + _resetDetails.RequestId;

            string parsed = template.Replace("{{Name}}", _resetDetails.Name);
            parsed = parsed.Replace("{{EmailAddress}}", _resetDetails.Email);
            parsed = parsed.Replace("{{ResetLink}}", link);
            parsed = parsed.Replace("{{SiteUrl}}", siteUrl);

            return parsed;
        }

        public async Task SendAsync()
        {
            string template = GetTemplate();
            string parsed = ParseTemplate(template);
            string subject = "Your Password Reset Link for " + HttpContext.Current.Request.Url.Authority;

            MailQueueManager queue = new MailQueueManager(AppUsers.GetCatalog(), parsed, "", _resetDetails.Email,
                subject);
            queue.Add();
            await queue.ProcessMailQueueAsync();
        }
    }
}