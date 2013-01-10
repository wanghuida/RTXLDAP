using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.DirectoryServices;

namespace RTXLDAP
{
    class LDAP
    {
        public static bool connect(string domainName, string userName, string userPwd, out DirectoryEntry domain)
        {
            domain = new DirectoryEntry();
            try
            {
                domain.Path = string.Format("LDAP://{0}", domainName);
                domain.Username = userName;
                domain.Password = userPwd;
                domain.AuthenticationType = AuthenticationTypes.Secure;
                domain.RefreshCache();

                return true;
            }
            catch
            {
                return false;
            }
        }

        public static void fillList(DirectoryEntry entry, string parentId, List<AdModel> list)
        {
            if (parentId == null)
            {
                string rootOuName = entry.Name;
                byte[] bGUID = entry.Properties["objectGUID"][0] as byte[];
                string id = BitConverter.ToString(bGUID);
                list.Add(new AdModel(id, rootOuName, (int)TypeEnum.OU, "0"));
                LDAP.fillList(entry, id, list);
            }
            else 
            {
                foreach (DirectoryEntry subEntry in entry.Children)
                {
                    string entrySchemaClsName = subEntry.SchemaClassName;

                    string[] arr = subEntry.Name.Split('=');
                    string categoryStr = arr[0];
                    string nameStr = arr[1];
                    string id = string.Empty;

                    if (subEntry.Properties.Contains("objectGUID"))   
                    {
                        byte[] bGUID = subEntry.Properties["objectGUID"][0] as byte[];

                        id = BitConverter.ToString(bGUID);
                    }

                    bool isExist = list.Exists(d => d.Id == id);

                    switch (entrySchemaClsName)
                    {
                        case "organizationalUnit":

                            if (!isExist)
                            {
                                list.Add(new AdModel(id, nameStr, (int)TypeEnum.OU, parentId));
                            }

                            LDAP.fillList(subEntry, id,list);
                            break;
                        case "user":
                            string accountName = string.Empty;

                            if (subEntry.Properties.Contains("samaccountName"))
                            {
                                accountName = subEntry.Properties["samaccountName"][0].ToString();
                            }

                            if (!isExist)
                            {
                                list.Add(new AdModel(id, accountName, (int)TypeEnum.USER, parentId));
                            }
                            break;
                    }
                }
            }
        }
    }
}
