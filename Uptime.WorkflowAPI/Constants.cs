using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Uptime.WorkflowAPI;

internal static class Constants
{
    internal static class AuthSchemes
    {
        public const string Combined = "NegotiateOrBearer";
        public const string Bearer = JwtBearerDefaults.AuthenticationScheme;
        public const string Negotiate = "Negotiate";
    }

    internal static class AuthenticationScopes
    {
        public const string Start = "Workflow.Start";
        public const string Modify = "Workflow.Modify";
        public const string Cancel = "Workflow.Cancel";
    }

    internal static class ConfigurationKeys
    {
        public const string ApiPrefix = "Api:";
        public const string AADSection = $"{ApiPrefix}AAD";
    }

    internal static class ClaimNames
    {
        public const string Upn  = System.Security.Claims.ClaimTypes.Upn;
        public const string Sid  = System.Security.Claims.ClaimTypes.Sid;
        public const string Oid  = "oid";
        public const string PreferredUserName = "preferred_username";
    }

    internal static class Policies
    {
        public const string Admin = "ApiAdminAccess";
        public const string Start = "ApiStartAccess";
        public const string Modify = "ApiModifyAccess";
    }
}