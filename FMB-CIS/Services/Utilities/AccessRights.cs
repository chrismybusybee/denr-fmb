namespace Services.Utilities
{
    public static class AccessRightsUtilities
    {
        public static bool IsAccessRights(string accessRights, string accessRight)
        {
            var accessRightsList = accessRights.Split(",");
            return accessRightsList.FirstOrDefault(a => a == accessRight) != null;
        }
        public static bool IsInAccessRights(string accessRights, string userAccessRights)
        {
            var accessRightsList = accessRights.Split(",");
            var userAccessRightsList = userAccessRights.Split(",");
            var isInAccessRights = false;

            foreach(string accessRight in userAccessRightsList) 
            {
                if (accessRightsList.FirstOrDefault(a => a == accessRight) != null) {
                    isInAccessRights = true;
                }
            }
            return isInAccessRights;
        } 
    }
}