using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.DirectoryServices;
using System.Security.Principal;
using RTXSAPILib;
using System.Runtime.InteropServices;

namespace RTXLDAP
{
    public partial class frmAD : Form
    {

        private List<AdModel> list;

        Config config = new Config();

        RTXSAPILib.RTXSAPIRootObj RootObj;                  //rtx root obj
        RTXSAPILib.RTXSAPIDeptManager DeptManagerObj;       //rtx department obj
        RTXSAPILib.RTXSAPIUserManager UserManagerObj;       //rtx user obj
        RTXSAPILib.RTXSAPIUserAuthObj UserAuthObj;          //rtx auth obj

        public delegate void OnLogin(string name,string pwd);
        OnLogin onLogin;

        public frmAD()
        {
            InitializeComponent();
            RootObj = new RTXSAPIRootObj();     
            DeptManagerObj = RootObj.DeptManager;    
            UserManagerObj = RootObj.UserManager;   
            UserAuthObj = RootObj.UserAuthObj;
            UserAuthObj.OnRecvUserAuthRequest += new _IRTXSAPIUserAuthObjEvents_OnRecvUserAuthRequestEventHandler(UserAuthObj_OnRecvUserAuthRequest); //user auth event
            RootObj.ServerIP = config.RTXIP;        //rtx ip
            RootObj.ServerPort = config.RTXPort;    //rtx port
            UserAuthObj.AppGUID = config.AppGUID;   //app guid
            UserAuthObj.AppName = config.AppName;   //app name
            try
            {
                UserAuthObj.RegisterApp();      //register app
                UserAuthObj.StartApp("", 8);    //start app
            }
            catch (COMException ex)
            {
                MessageBox.Show(ex.Message);
                Application.Exit();
            }
            onLogin = new OnLogin(onUserLogin);
        }

        private void frmAD_Load(object sender, EventArgs e)
        {
            txtDomainName.Text = config.DomainName;
            txtUserName.Text = config.DomainUser;
            txtPwd.Text = config.DomainPwd;
        }

        private void ok_Click(object sender, EventArgs e)
        {
            treeView1.Nodes.Clear();
            list = new List<AdModel>();
            DirectoryEntry domain;

            if (LDAP.connect(txtDomainName.Text.Trim(), txtUserName.Text.Trim(), txtPwd.Text.Trim(), out domain))
            {
                buildTree(domain);
            }
            else
            {
                MessageBox.Show("can't connect domain", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            MessageBox.Show("read success!");
        }

        public void buildTree(DirectoryEntry entry)
        {
            LDAP.fillList(entry,null,list);

            foreach (var item in list)
            {
                TreeNode node = new TreeNode();
                node.Text = item.Name;
                node.Name = item.Id;
                node.Tag = item.TypeId;
                if (item.ParentId != "0")
                {
                    TreeNode[] ret = treeView1.Nodes.Find(item.ParentId,true);
                    ret[0].Nodes.Add(node);
                }
                else
                {
                    treeView1.Nodes.Add(node);
                }
            }
        }

        private void treeView1_AfterCheck(object sender, TreeViewEventArgs e)
        {
            bool is_checked = e.Node.Checked;
            foreach (TreeNode node in e.Node.Nodes) {
                node.Checked = is_checked;
            }
        }


        private void button1_Click(object sender, EventArgs e)
        {
            sync(treeView1.Nodes);
            MessageBox.Show("sync success!");
        }

        private void sync(TreeNodeCollection nodes) 
        {
            foreach (TreeNode n in nodes) 
            {
                if (n.Nodes.Count > 0)
                {
                    sync(n.Nodes);
                }
                else 
                {
                    if (n.Checked == true && (TypeEnum)n.Tag == TypeEnum.USER)
                    {
                        List<TreeNode> ns = new List<TreeNode>();
                        TreeNode curr = n;
                        while (curr.Parent != null)
                        {
                            ns.Add(curr.Parent);
                            curr = curr.Parent;
                        }
                        ns.RemoveAt(ns.Count - 1); // remove root
                        StringBuilder parentDept = new StringBuilder();
                        for (int i = ns.Count - 1; i >= 0; i--) {
                            try
                            {
                                DeptManagerObj.AddDept(ns[i].Text, parentDept.ToString());
                            }
                            catch 
                            {
                                //MessageBox.Show(ex.Message);
                            }
                            finally 
                            {
                                if (parentDept.ToString() == "")
                                {
                                    parentDept.Append(ns[i].Text);
                                }
                                else 
                                {
                                    parentDept.Append(@"\").Append(ns[i].Text);
                                }
                            }
                        }
                        try
                        {
                            UserManagerObj.AddUser(n.Text, 1);
                        }
                        catch 
                        {
                            //MessageBox.Show(ex.Message);
                        }
                        try
                        {
                            DeptManagerObj.AddUserToDept(n.Text, null, parentDept.ToString(), false);
                        }
                        catch 
                        {
                            //MessageBox.Show(ex.Message);
                        }
                        
                    }
                }
            }
        }

        public void UserAuthObj_OnRecvUserAuthRequest(string name, string pwd, out RTXSAPI_USERAUTH_RESULT pResult)
        {
            
            listBox1.BeginInvoke(onLogin, name, pwd);

            DirectoryEntry entry = new DirectoryEntry();
            int nRet;
            try
            {
                entry.Path = string.Format("LDAP://{0}", txtDomainName.Text.Trim());
                entry.Username = name;
                entry.Password = pwd;
                entry.AuthenticationType = AuthenticationTypes.Secure;
                entry.RefreshCache();

                DirectorySearcher search = new DirectorySearcher(entry);
                search.Filter = "(SAMAccountName=" + name + ")";
                search.PropertiesToLoad.Add("cn");
                SearchResult result = search.FindOne();

                if (null == result)
                {
                    nRet = 100;
                }
                else
                {
                    nRet = 0;
                }
            }
            catch 
            {
                nRet = 2;
            }

            if (nRet == 0)
                pResult = RTXSAPI_USERAUTH_RESULT.RTXSAPI_USERAUTH_RESULT_OK;
            else if (nRet == 1)
                pResult = RTXSAPI_USERAUTH_RESULT.RTXSAPI_USERAUTH_RESULT_FAIL;
            else if (nRet == 2)
                pResult = RTXSAPI_USERAUTH_RESULT.RTXSAPI_USERAUTH_RESULT_ERRPWD;
            else
                pResult = RTXSAPI_USERAUTH_RESULT.RTXSAPI_USERAUTH_RESULT_ERRNOUSER;

        }

        public void onUserLogin(string name, string pwd)
        {
            string strAuthContent = "UserName: " + name + " Pwd:" + "*******";
            listBox1.Items.Add(strAuthContent); 
        }

    }


    public enum TypeEnum : int
    {
        OU = 1,
        USER = 2
    }


    public class AdModel
    {
        public AdModel(string id, string name, int typeId, string parentId)
        {
            Id = id;
            Name = name;
            TypeId = typeId;
            ParentId = parentId;
        }

        public string Id { get; set; }

        public string Name { get; set; }

        public int TypeId { get; set; }

        public string ParentId { get; set; }
    }

}
