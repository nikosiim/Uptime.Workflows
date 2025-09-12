using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Uptime.Workflows.Api;

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
        public const string AdminAccess = "access_as_admin";
        public const string UsersAccess = "access_as_user";
    }

    internal static class ConfigurationKeys
    {
        public const string ApiPrefix = "Api:";
        public const string AADSection = $"{ApiPrefix}AAD";

        public const string SharePointOnPrem = "SharePoint";
        public const string SharePointOnline = "SharePointOnline";
        public const string MembershipService = "MembershipValidationService";
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
        public const string AdminPolicy = "AdminPolicy";
        public const string UsersPolicy = "UsersPolicy";
        public const string UserOrAdmin = "UserOrAdmin";
    }
}