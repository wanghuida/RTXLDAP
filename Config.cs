using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RTXLDAP
{
    class Config
    {
        public string DomainName { get; set; }

        public string DomainUser { get; set; }

        public string DomainPwd { get; set; }

        public string DomainRootOU { get; set; }



        public string RTXIP { get; set; }

        public short RTXPort { get; set; }



        public string AppGUID { get; set; }

        public string AppName { get; set; }

        public Config()
        {
            DomainName = System.Configuration.ConfigurationSettings.AppSettings["DomainName"];
            DomainUser = System.Configuration.ConfigurationSettings.AppSettings["DomainUser"];
            DomainPwd = System.Configuration.ConfigurationSettings.AppSettings["DomainPwd"];
            DomainRootOU = System.Configuration.ConfigurationSettings.AppSettings["DomainRootOU"];

            RTXIP = System.Configuration.ConfigurationSettings.AppSettings["RTXIP"];
            RTXPort = Convert.ToInt16(System.Configuration.ConfigurationSettings.AppSettings["RTXPort"]);

            AppGUID = System.Configuration.ConfigurationSettings.AppSettings["AppGUID"];
            AppName = System.Configuration.ConfigurationSettings.AppSettings["AppName"];
        }
    }
}
