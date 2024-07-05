using Novell.Directory.Ldap;

namespace LdapHelper
{
    public class LdapHelper
    {
        private readonly LdapSetting _ldapSetting;

        /// <summary>
        /// LDAP初始化配置
        /// </summary>
        /// <param name="ldapSetting"></param>
        public LdapHelper(LdapSetting ldapSetting)
        {
            _ldapSetting = ldapSetting;
        }

        /// <summary>
        /// 查询用户信息
        /// </summary>
        /// <param name="account">账号,例如:12727182</param>
        /// <param name="password">账号密码,如果只获取用户信息密码传空字符串或者null</param>
        /// <param name="filter">用户过滤器,{0} 表示占位符</param>
        /// <param name="attributeList">要获取的标签,为空获取全部</param>
        /// <returns></returns>
        public async Task<LadpUserInfo> CheckUser(string account,
            string password,
            string filter = "(&(objectClass=user)(sAMAccountName={0}))",
            params string[] attributeList)
        {
            LadpUserInfo ladpUser = new LadpUserInfo { ValidUser = false, ValidPassword = false };
            filter = string.Format(filter, account);

            using (var ldap = new LdapConnection())
            {
                try
                {
                    //连接
                    await ldap.ConnectAsync(_ldapSetting.Server, _ldapSetting.Port);

                    // 绑定到LDAP服务器 
                    await ldap.BindAsync(_ldapSetting.BindingDN, _ldapSetting.BindingPassword);

                    foreach (var userOU in _ldapSetting.UserOU.Split('|'))
                    {
                        // 创建搜索请求
                        SearchOptions options = new SearchOptions(userOU, LdapConnection.ScopeSub, filter, attributeList);
                        var entities = await ldap.SearchUsingSimplePagingAsync(options, 1);
                        var attribute = new Dictionary<string, string>();

                        if (entities.Count == 0)
                        {
                            ladpUser.Message = "没有找到匹配的用户";
                            return ladpUser;
                        }
                        var ldapEntry = entities.First();

                        foreach (var item in ldapEntry.GetAttributeSet())
                        {
                            attribute.Add(item.Name, item.StringValue);
                        }
                        ladpUser.ValidUser = true;
                        ladpUser.UserDN = ldapEntry.Dn;
                        ladpUser.Attributes = attribute;
                        try
                        {
                            if (!string.IsNullOrWhiteSpace(password))
                            {
                                //重新连接并重新绑定
                                await ldap.ConnectAsync(_ldapSetting.Server, _ldapSetting.Port);
                                await ldap.BindAsync(ladpUser.UserDN, password);
                                ladpUser.ValidPassword = true;
                                ladpUser.Message = "密码验证正确";
                            }
                        }
                        catch (Exception ex)
                        {
                            ladpUser.Message = ex.Message;
                        }

                        //支持多个OU查询,如果查询到了就直接return
                        if (ladpUser.ValidUser == true)
                        {
                            return ladpUser;
                        }
                        else
                        {
                            ladpUser.Message = "没有找到匹配的用户";
                        }
                    }
                }
                catch (LdapException ex)
                {
                    ladpUser.Message = ex.Message;
                }
                finally
                {
                    if (ldap.Connected)
                        ldap.Disconnect();
                    ldap.Dispose();
                }
            }

            return ladpUser;
        }
    }

    public class LadpUserInfo
    {
        public bool ValidUser { get; set; }
        public bool ValidPassword { get; set; }

        public string UserDN { get; set; }
        public Dictionary<string, string> Attributes { get; set; }
        public string Message { get; set; }
    }

    public class LdapSetting
    {
        public string Server { get; set; }
        public int Port { get; set; }
        public string BindingDN { get; set; }
        public string BindingPassword { get; set; }
        public string UserOU { get; set; }
    }
}
