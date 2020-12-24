using System;
using System.Collections.Generic;
using System.Text;

namespace SecurityDemo
{

    public class SessionToken
    {
        public int ShopID;
        public string UserID;
        public HashSet<string> Roles;

        // extensions: login ip, expire time .... etc
    }

    public class User
    {
        public int ShopID;
        public string UserID;
        public HashSet<string> Roles;
    }

    public class Group
    {
        public int ShopID;
        public string Name;
    }

    public class Policy
    {
        public string PolicyID;
        public PolicyTypeEnum Type;
        public int ShopID;

        public Dictionary<string, Item> PolicyActionItems;

        public class Item
        {
            public HashSet<string> Allow_Users;
            public HashSet<string> Allow_Roles;

            public HashSet<string> Deny_Users;
            public HashSet<string> Deny_Roles;
        }
    }

    public enum PolicyTypeEnum
    {
        DS_BUILT_IN = 1,
        DS_SHOP = 2,
    }
}
