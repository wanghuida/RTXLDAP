RTXLDAP
=======

### 让RTX同步LDAP用户，并且实现RTX与LDAP单点登陆

+ RTX:
+ LDAP:

+ 使用流程

1. git clone git@github.com:wanghuida/RTXLDAP.git
2. 建议使用vs 2008打开
3. 修改App.config配置文件 

```
<!-- 域 -->
<add key="DomainName" value="192.168.1.100/DC=home,DC=net" />
<!-- 域用户,用来读取所有域用户信息 -->
<add key="DomainUser" value="wanghuida" />
<!-- 域用户密码 -->
<add key="DomainPwd" value="123456" />

<!-- RTX服务IP，建议在同一台,可以免去其他配置 -->
<add key="RTXIP" value="127.0.0.1"/>
<!-- RTX服务端口 -->
<add key="RTXPort"  value="8006"/>

<!-- 标识，随意修改，不重复即可 -->
<add key="AppGUID" value="{193947E5-E990-4af8-A954-D358B385F099}"/>
<!-- 标识，随意修改，不重复即可 -->
<add key="AppName" value="WanghuidaRtxLdap"/>
```

4. vs调试运行
