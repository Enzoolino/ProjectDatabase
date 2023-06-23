using System;
using System.Collections.Generic;

namespace WorkshopDataModifier.Core
{
    /// <summary>
    /// Class that stores the access level of a current user
    /// </summary>
    public static class UserAccessManager
    {
        public static byte AccessLevel { get; private set; }

        public static void SetAccessLevel(byte accessLevel)
        {
            AccessLevel = accessLevel;
        }
    }
}
