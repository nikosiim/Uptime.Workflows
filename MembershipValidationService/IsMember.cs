using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using System.Collections.Specialized;
using System.Net;
using System.Web;

namespace MembershipValidationService
{
    public class IsMember
    {
        [Function("IsMember")]
        public static async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "membership/isMember")] HttpRequestData req)
        {
            NameValueCollection q = HttpUtility.ParseQueryString(req.Url.Query);
            string user = q["user"] ?? "";
            string group = q["group"] ?? "";

            // 🔧 quick hard-coded dictionary for testing
            var membership = new Dictionary<string, string[]>
            {
                ["Approvers"] = ["alice@contoso.com", "bob@contoso.com"],
                ["WF_Admins"] = ["carol@contoso.com"],
                ["Site Owners"] = []
            };

            bool isMember = membership.TryGetValue(group, out string[]? members) && members.Contains(user, StringComparer.OrdinalIgnoreCase);

            HttpResponseData resp = req.CreateResponse(HttpStatusCode.OK);
            await resp.WriteAsJsonAsync(new { isMember });
            return resp;
        }
    }
}