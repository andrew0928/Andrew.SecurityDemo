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
                        Allow_Roles = new HashSet<string>() { "users" },
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
                { ("boris", 9527), new User() { ShopID = 9527, UserID = "boris", Roles = new HashSet<string>() { "users" } } },
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
                        Deny_Users = new HashSet<string>() { "boris" }
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


        static bool HasPermission(SessionToken who, string domain_action)
        {

            // search sequence:
            // 1. shop customized policy
            // 2. domain service default (built-in)

            (string domain, string action) = _name_parser(domain_action);


            bool deny = false;
            bool allow = false;

            if (policies.ContainsKey(($"{domain}::default", who.ShopID)))
            {
                var items = policies[($"{domain}::default", who.ShopID)].PolicyActionItems;
                if (items.ContainsKey(domain_action))
                {
                    if (items[domain_action].Deny_Users != null && items[domain_action].Deny_Users.Contains(who.UserID)) deny = true;
                    if (items[domain_action].Deny_Roles != null && items[domain_action].Deny_Roles.Overlaps(who.Roles)) deny = true;
                    if (items[domain_action].Allow_Users != null && items[domain_action].Allow_Users.Contains(who.UserID)) allow = true;
                    if (items[domain_action].Allow_Roles != null && items[domain_action].Allow_Roles.Overlaps(who.Roles)) allow = true;
                }
            }

            if (policies.ContainsKey(($"{domain}::default", 0)))
            {
                var items = policies[($"{domain}::default", 0)].PolicyActionItems;
                if (items.ContainsKey(domain_action))
                {
                    if (items[domain_action].Deny_Users != null && items[domain_action].Deny_Users.Contains(who.UserID)) deny = true;
                    if (items[domain_action].Deny_Roles != null && items[domain_action].Deny_Roles.Overlaps(who.Roles)) deny = true;
                    if (items[domain_action].Allow_Users != null && items[domain_action].Allow_Users.Contains(who.UserID)) allow = true;
                    if (items[domain_action].Allow_Roles != null && items[domain_action].Allow_Roles.Overlaps(who.Roles)) allow = true;
                }
            }

            return (allow == true && deny == false);
        }

        static bool HasPermission(SessionToken who, string resource, string action)
        {
            throw new NotImplementedException();
        }

        private static (string domain, string action) _name_parser(string name)
        {
            string[] segments = name.Split("::", 2);

            return (segments[0], segments[1]);
        }


        static void Main(string[] args)
        {
            Init();

            Demo1();
        }

        // 基本展示: 存取 BBS feature(s)
        static void Demo1()
        {
            var user = users[("boris", 9527)];
            SessionToken me = new SessionToken()
            {
                ShopID = user.ShopID,
                UserID = user.UserID,
                Roles = user.Roles
            };

            Console.WriteLine($"* {user.UserID} try to access: [bbs::list.boards] => ..." + HasPermission(me, "bbs::list.boards"));
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
