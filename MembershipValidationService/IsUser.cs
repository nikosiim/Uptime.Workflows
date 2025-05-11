using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using System.Collections.Specialized;
using System.Net;
using System.Web;

namespace MembershipValidationService
{
    public class IsUser
    {
        [Function("IsUser")]
        public static async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "get", Route = "membership/isUser")] HttpRequestData req)
        {
            NameValueCollection q = HttpUtility.ParseQueryString(req.Url.Query);
            string user = q["user"] ?? "";
            string target = q["target"] ?? "";

            bool isSameOrSubstitute = user.Equals(target, StringComparison.OrdinalIgnoreCase) || (user, target) switch   // 🔧 fake vacation list
            {
                ("dave@contoso.com", "bob@contoso.com") => true,
                _ => false
            };

            HttpResponseData resp = req.CreateResponse(HttpStatusCode.OK);
            await resp.WriteAsJsonAsync(new { isMember = isSameOrSubstitute });
            return resp;
        }
    }
}