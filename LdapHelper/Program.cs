namespace LdapHelper
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var ldapSetting = new LdapSetting
            {
                Server = "192.168.1.1",
                Port = 389,
                BindingDN = "CN=jxldap,OU=Service Accounts,OU=Special Accounts,DC=luxshare,DC=com,DC=cn",
                BindingPassword = "",
                UserOU = "OU=xx集团,DC=luxshare,DC=com,DC=cn"
            };

            LdapHelper ldapHelper = new(ldapSetting);
            var userInfo = await ldapHelper.CheckUser("12727xxx","");
            Console.WriteLine(userInfo.UserDN);
        }
    }
}
