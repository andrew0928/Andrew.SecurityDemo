using System;
using System.Collections.Generic;

namespace SecurityDemo
{
    class Program
    {
        static Dictionary<(string name, int shop), User> users = null;

        static Dictionary<(string name, int shop), Group> groups = null;

        // domain service: features 與 action 的對應
        static Dictionary<string, string> features = null;

        
        static HashSet<string> actions = null;

        static Dictionary<(string name, int shop), Policy> policies = null;

        static void Init()
        {
            // 以下是 domain service 註冊時就要產生的資料

            features = new Dictionary<string, string>()
            {
                { "bbs::view", "bbs::list.boards" },
                { "bbs::admin", "bbs::manage.boards" }
            };

            actions = new HashSet<string>()
            {
                "bbs::list.boards",
                "bbs::manage.boards",

                "bbs::resources/host",
                "bbs::resources/user",
                "bbs::resources/viewer"
            };

            policies = new Dictionary<(string name, int shop), Policy>();

            policies.Add(("bbs::default", 0), new Policy()
            {
                Type = PolicyTypeEnum.DS_BUILT_IN,
                PolicyID = $"bbs::default",
                ShopID = 0,
                PolicyActionItems = new Dictionary<string, Policy.Item>()
                {
                    { "bbs::list.boards", new Policy.Item()
                    {
                        Allow_Roles = new string[] { "users" },
                        //Deny_Roles = null,
                        //Allow_Users = null,
                        //Deny_Users = null,
                    } },
                },
            });



            // 以下是開設新的商店時，或是註冊 domain service 時要替既有每個 shop 產生的資料
            users = new Dictionary<(string name, int shop), User>()
            {
                { ("rick", 8), new User() { ShopID = 8, UserID = "rick", Roles = new HashSet<string>() { "users" } } },
                { ("andrew", 9527), new User() { ShopID = 9527, UserID = "andrew", Roles = new HashSet<string>() { "users", "power_users" } } },
            };

            groups = new Dictionary<(string name, int shop), Group>()
            {
                { ("users", 9527), new Group() { ShopID = 9527, Name = "Users" } },
                { ("power_users", 9527), new Group() { ShopID = 9527, Name = "Power Users" } },
                { ("administrators", 9527), new Group() { ShopID = 9527, Name = "Administrators" } },

                { ("users", 8), new Group() { ShopID = 8, Name = "Users" } },
                { ("power_users", 8), new Group() { ShopID = 8, Name = "Power Users" } },
                { ("administrators", 8), new Group() { ShopID = 8, Name = "Administrators" } },
            };

            policies.Add(("bbs::default", 9527), new Policy()
            {
                PolicyID = "bbs::default",
                ShopID = 9527,
                Type = PolicyTypeEnum.DS_SHOP,
                PolicyActionItems = new Dictionary<string, Policy.Item>()
                {
                    { "bbs::list.boards", new Policy.Item()
                    {
                        Deny_Users = new string[] { "rick" }
                    } }
                }
            });

            /*
            groups.Add(("bbs::global.hosts", 9527), new Group() { Name = "bbs::global.hosts", ShopID = 9527 });

            policies.Add(($"bbs::resources/board1", 9527), new Policy()
            {
                Type = PolicyTypeEnum.DS_SHOP,
                PolicyID = "bbs::resources/board1",
                ShopID = 9527,
                PolicyActionItems = new Dictionary<string, Policy.Item>()
                {
                    { "host", new Policy.Item()
                    {
                        Allow_Roles = new string[] { "administrators" },
                        Allow_Users = new string[] { "andrew" }
                    } }
                }
            });

            policies.Add(($"bbs::resources/board2", 9527), new Policy()
            {
                Type = PolicyTypeEnum.DS_SHOP,
                PolicyID = "bbs::resources/board2",
                ShopID = 9527,
                PolicyActionItems = new Dictionary<string, Policy.Item>()
                {
                    { "host", new Policy.Item()
                    {
                        Allow_Roles = new string[] { "administrators" },
                        Allow_Users = new string[] { "rick" }
                    } }
                }
            });
            */
        }


        static bool HasPermission(SessionToken who, string action)
        {

            // search sequence:
            // 1. shop customized policy
            // 2. domain service default (built-in)

            string[] segments = action.Split("::");

            string policyname = $"{segments[0]}::default";
            if (policies.ContainsKey((policyname, who.ShopID)))
            {
                if (policies[(policyname, who.ShopID)].PolicyActionItems.ContainsKey(action))
                {
                    
                }
            }



            throw new NotImplementedException();
        }

        static bool HasPermission(SessionToken who, string resource, string action)
        {
            throw new NotImplementedException();
        }


        static void Main(string[] args)
        {
            Init();

            Demo1();
        }

        // 基本展示: 存取 BBS feature(s)
        static void Demo1()
        {

        }
    }









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
            public string[] Allow_Users;
            public string[] Allow_Roles;

            public string[] Deny_Users;
            public string[] Deny_Roles;
        }
    }

    public enum PolicyTypeEnum
    {
        DS_BUILT_IN = 1,
        DS_SHOP = 2,
    }
}
