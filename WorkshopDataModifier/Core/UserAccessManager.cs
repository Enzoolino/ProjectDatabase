using System;
using System.Collections.Generic;

namespace WorkshopDataModifier.Core
{
    /// <summary>
    /// Class that stores the access level of a current user
    /// </summary>
    public static class UserAccessManager
    {
        /// <summary>
        /// Current stored access level
        /// </summary>
        public static byte AccessLevel { get; private set; }

        /// <summary>
        /// Changes the current stored access level
        /// </summary>
        /// <param name="accessLevel">Access level that will be set</param>
        public static void SetAccessLevel(byte accessLevel)
        {
            AccessLevel = accessLevel;
        }
    }
}
