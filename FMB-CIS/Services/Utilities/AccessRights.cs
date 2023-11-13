namespace Services.Utilities
{
    public static class AccessRightsUtilities
    {
        public static bool IsAccessRights(string accessRights, string accessRight)
        {
            var accessRightsList = accessRights.Split(",");
            return accessRightsList.FirstOrDefault(a => a == accessRight) != null;
        }
    }
}