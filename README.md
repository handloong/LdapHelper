## 多系统痛点(老项目改造)
1. 我们日常的办公系统是不是有多个？

2. 每个系统之间是不是都有独立的账号密码？

3. 密码多了，有时候半天想不起来哪个密码对应哪个系统？

4. 每次新项目的开发，都需要重新开发和维护一套用户密码？

5. 维护多套系统的用户是不是非常头疼？


## 如何接入:
利用LDAP可以快速实现统一登录认证,本方法简单粗暴,提供一个验证用户的方法,方便快速接入现有系统

在用户表上新增一个字段 AuthType ,类型是LDAP的就走LDAP逻辑,类型数DataBase的就走之前的逻辑

```csharp
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
```
配置好相关信息,直接通过CheckUser获取用户信息

```csharp
 public class LadpUserInfo
 {
     public bool ValidUser { get; set; }
     public bool ValidPassword { get; set; }

     public string UserDN { get; set; }
     public Dictionary<string, string> Attributes { get; set; }
     public string Message { get; set; }
 }

```

