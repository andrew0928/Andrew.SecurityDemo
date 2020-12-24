using System;
using System.Collections.Generic;
using System.Linq;

namespace SecurityDemo
{
    class Program
    {
        static Dictionary<(string name, int shop), User> users = null;

        static Dictionary<(string name, int shop), Group> groups = null;

        // domain service: features 與 action 的對應
        //static Dictionary<string, string> features = null;

        
        static HashSet<string> actions = null;

        static Dictionary<(string name, int shop), Policy> policies = null;

        static void Init()
        {
            // 以下是 domain service 註冊時就要產生的資料


            //features = new Dictionary<string, string>()
            //{
            //    { "bbs::view", "bbs::list.boards" },
            //    { "bbs::admin", "bbs::manage.boards" }
            //};

            actions = new HashSet<string>()
            {
                "bbs::list.boards",
                "bbs::manage.boards",

                "bbs::boards/tags/host",    // can admin boards
                "bbs::boards/tags/user",    // can read and post
                "bbs::boards/tags/viewer",  // can read
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
                    } },
                    { "bbs::manage.boards", new Policy.Item()
                    {
                        Allow_Roles = new HashSet<string>() { "administrators" },
                    } },
                    { "bbs::boards/tags/host", new Policy.Item()
                    {
                        Allow_Roles = new HashSet<string>() { "administrators" },
                    } },
                    { "bbs::boards/tags/user", new Policy.Item()
                    {
                        Allow_Roles = new HashSet<string>() { "users" },
                    } },
                    { "bbs::boards/tags/viewer", new Policy.Item()
                    {
                        Allow_Roles = new HashSet<string>() { "users" },
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
                        //Deny_Users = new HashSet<string>() { "boris" }
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





        static void Main(string[] args)
        {
            Init();

            //Demo1();  // basic PBAC test
            //Demo2();  // full managed resource PBAC test
            Demo3();    // basic managed resource ABAC test
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

        static void Demo2()
        {
            // init boards permissions
            policies[("bbs::default", 9527)].PolicyActionItems["bbs::boards/1/host"] = new Policy.Item()
            {
                Allow_Roles = new HashSet<string>() { "administrators" },
                Allow_Users = new HashSet<string>() { "andrew" }
            };
            policies[("bbs::default", 9527)].PolicyActionItems["bbs::boards/2/host"] = new Policy.Item()
            {
                Allow_Roles = new HashSet<string>() { "administrators" },
                Allow_Users = new HashSet<string>() { "rick" }
            };
            policies[("bbs::default", 9527)].PolicyActionItems["bbs::boards/3/host"] = new Policy.Item()
            {
                Allow_Roles = new HashSet<string>() { "administrators" },
                Allow_Users = new HashSet<string>() { "boris" }
            };


            var user = users[("boris", 9527)];
            SessionToken me = new SessionToken()
            {
                ShopID = user.ShopID,
                UserID = user.UserID,
                Roles = user.Roles
            };

            Console.WriteLine($"* {user.UserID} try to access: [bbs::boards/1/host] => ..." + HasPermission(me, "bbs::boards", 1, "host"));
            Console.WriteLine($"* {user.UserID} try to access: [bbs::boards/2/host] => ..." + HasPermission(me, "bbs::boards", 2, "host"));
            Console.WriteLine($"* {user.UserID} try to access: [bbs::boards/3/host] => ..." + HasPermission(me, "bbs::boards", 3, "host"));
        }



        static void Demo3()
        {
            // init boards permissions
            BBS_Board[] boards = new BBS_Board[]
            {
                new BBS_Board()
                {
                    Id = 1, Name = "UPD: Host talk",
                    ActionTags = new HashSet<string>() { "host" },
                },
                new BBS_Board()
                {
                    Id = 2, Name = "UPD: User talk",
                    ActionTags = new HashSet<string>() { "user" },
                },
                new BBS_Board()
                {
                    Id = 3, Name = "UPD: Viewer talk",
                    ActionTags = new HashSet<string>() { "viewer" },
                },
                new BBS_Board()
                {
                    Id = 4, Name = "UPD: Manager talk",
                    ActionTags = new HashSet<string>() { },
                },
                new BBS_Board()
                {
                    Id = 5, Name = "UPD: VP talk",
                    ActionTags = new HashSet<string>() { },
                },
            };

            var user = users[("boris", 9527)];
            SessionToken me = new SessionToken()
            {
                ShopID = user.ShopID,
                UserID = user.UserID,
                Roles = user.Roles
            };

            var tags = new HashSet<string>(GetGrantedResourceActionTags(me, "bbs::boards"));

            Console.WriteLine($"{me.UserID} can visit those boards:");
            foreach(var board in (from b in boards where b.ActionTags.Overlaps(tags) select b))
            {
                Console.WriteLine($"* board: {board.Id} / {board.Name}, tags: {board.ActionTags}");
            }
        }






        // text formatting, text parser, helper function ...



        static bool HasPermission(SessionToken who, string domain_action)
        {

            // search sequence:
            // 1. shop customized policy
            // 2. domain service default (built-in)

            (string domain, string action) = _name_parser(domain_action);


            //bool deny = false;
            bool allow = false;

            if (policies.ContainsKey(($"{domain}::default", who.ShopID)))
            {
                var items = policies[($"{domain}::default", who.ShopID)].PolicyActionItems;
                if (items.ContainsKey(domain_action))
                {
                    if (items[domain_action].Deny_Users != null && items[domain_action].Deny_Users.Contains(who.UserID)) return false; //deny = true;
                    if (items[domain_action].Deny_Roles != null && items[domain_action].Deny_Roles.Overlaps(who.Roles)) return false;  //deny = true;
                    if (items[domain_action].Allow_Users != null && items[domain_action].Allow_Users.Contains(who.UserID)) allow = true;
                    if (items[domain_action].Allow_Roles != null && items[domain_action].Allow_Roles.Overlaps(who.Roles)) allow = true;
                }
            }

            if (policies.ContainsKey(($"{domain}::default", 0)))
            {
                var items = policies[($"{domain}::default", 0)].PolicyActionItems;
                if (items.ContainsKey(domain_action))
                {
                    if (items[domain_action].Deny_Users != null && items[domain_action].Deny_Users.Contains(who.UserID)) return false;  //deny = true;
                    if (items[domain_action].Deny_Roles != null && items[domain_action].Deny_Roles.Overlaps(who.Roles)) return false;  //deny = true;
                    if (items[domain_action].Allow_Users != null && items[domain_action].Allow_Users.Contains(who.UserID)) allow = true;
                    if (items[domain_action].Allow_Roles != null && items[domain_action].Allow_Roles.Overlaps(who.Roles)) allow = true;
                }
            }

            return (allow == true);
        }

        static bool HasPermission(SessionToken who, string domain_resource, int resourceId, string resourceAction)
        {
            //  {domain service}::{resource name}/{resource id}/{resource action} - 綁定 full managed resource 用的 action
            (string domain, string resource) = _name_parser(domain_resource);

            // TODO: check resourceId exist

            return HasPermission(who, $"{domain}::{resource}/{resourceId}/{resourceAction}");
        }

        static IEnumerable<string> GetGrantedResourceActionTags(SessionToken who, string domain_resource)
        {
            //  {domain service}::{resource name}/tags/{action tags} - 對應的 resource 支援的 action tags (static, 開發時可以決定)
            (string domain, string resource) = _name_parser(domain_resource);

            foreach (var action in actions)
            {
                if (action.StartsWith($"{domain}::{resource}/tags/") == false) continue;    // action not match, bypass
                if (HasPermission(who, action) == false) continue;                          // action not granted, bypass

                yield return action.Substring($"{domain}::{resource}/tags/".Length);        // return granted action tags
            }
        }








        private static (string domain, string action) _name_parser(string name)
        {
            string[] segments = name.Split("::", 2);

            return (segments[0], segments[1]);
        }

        

        // policy (key: id + shop)
        //  {domain service}::default   - 預設 policy name
        //  policy search 順序: *[portal global] -> *[shop: global] -> [domain service default] -> [shop: domain service]

        // action:
        //  {domain service}::{action}  - 基本 basic action name (static, 開發時可以決定)
        //  {domain service}::{resource name}/tags/{action tags} - 對應的 resource 支援的 action tags (static, 開發時可以決定)
        //  {domain service}::{resource name}/{resource id}/{resource action} - 綁定 full managed resource 用的 action
    }














    public class BBS_Board
    {
        public int Id;

        public string Name;

        public HashSet<string> ActionTags;
    }
    
}
