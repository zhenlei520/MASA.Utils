using MASA.Framework.Configuration;
using MASA.Utils.GenerateDBModel.Helper;
using MASA.Utils.GenerateDBModel.Model;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media;

namespace MASA.Utils.GenerateDBModel
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public bool IsConn = false;
        protected List<DepartmentModel> treeNodes;
        protected string connStr;
        protected const string dbServerSettingRootKey = "DBServer";

        public MainWindow()
        {
            InitializeComponent();
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            treeNodes = new List<DepartmentModel>();
            AppSettings.Initialize();

            #region 先指定Template

            // 查找模板
            /*string templateDir = System.IO.Path.Combine(Environment.CurrentDirectory, "Template");
            string[] files = Directory.GetFiles(templateDir, "*Entity.template");
            foreach (string file in files)
            {
                string name = file.Split('\\').Last().Split('.')[0];
                this.templateComboBox.Items.Add(name);
            }
            if (this.templateComboBox.Items.Count > 0)
            {
                this.templateComboBox.SelectedIndex = 0;
            }*/
            #endregion

            //初始化服务器地址配置
            var serverSettings = AppSettings.GetModel<ExpandoObject>(dbServerSettingRootKey)?.ToList();

            foreach (var item in serverSettings)
            {
                this.serverSettingsComboBox.Items.Add(item.Key);
            }
            if (this.serverSettingsComboBox.Items.Count > 0)
            {
                this.serverSettingsComboBox.SelectedIndex = 0;
            }
        }

        /// <summary>
        /// 测试按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Btn_test_Click(object sender, RoutedEventArgs e)
        {
            ConnToDB(true);
        }

        /// <summary>
        /// 下一步：读取数据库表
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Btn_db_next_Click(object sender, RoutedEventArgs e)
        {
            // 测试连接
            if (IsConn == false)
            {
                ConnToDB(false);
            }
            //根据连接结果显示
            if (IsConn)
            {
                this.tab1.IsSelected = false;
                this.tab2.IsSelected = true;
                this.Height = 600;
                GetTree();
            }
        }

        /// <summary>
        /// 链接数据库
        /// </summary>
        /// <param name="isMess"></param>
        protected void ConnToDB(bool isMess)
        {
            connStr = "Data Source={0};port={1};Initial Catalog=information_schema;user id={2};password={3};charset=utf8";
            connStr = string.Format(connStr, dbText.Text, txtPort.Text, userText.Text, pasText.Text);

            try
            {
                DbContext.OpenDbConnection(connStr);
                IsConn = true;

                if (isMess)
                    System.Windows.MessageBox.Show("连接成功！");
            }
            catch (Exception)
            {
                IsConn = false;
                System.Windows.MessageBox.Show("连接失败！");
            }
        }

        /// <summary>
        /// 生成库表结构树
        /// </summary>
        protected void GetTree()
        {
            string sqlStr = "SELECT DISTINCT(TABLE_SCHEMA) AS schemaName FROM `TABLES`";

            List<string> schema = DbContext.Query<string>(sqlStr).ToList();

            List<DepartmentModel> treeData = new List<DepartmentModel>();
            int i = 1;
            foreach (var item in schema)
            {
                DepartmentModel temp = new DepartmentModel
                {
                    Id = i,
                    DeptName = item,
                    ParentId = 0,
                    IsChecked = false
                };
                treeData.Add(temp);
                i++;
            }

            sqlStr = "SELECT TABLE_SCHEMA AS SchemaName,TABLE_NAME AS TableName FROM `TABLES`";
            List<SCTable> tables = DbContext.Query<SCTable>(sqlStr).ToList();

            foreach (var item in tables)
            {
                DepartmentModel temp = new DepartmentModel
                {
                    Id = i,
                    DeptName = item.TableName,
                    ParentId = treeData.FirstOrDefault(t => t.DeptName == item.SchemaName).Id,
                    IsChecked = false
                };
                treeData.Add(temp);
                i++;
            }

            treeNodes = GetTrees(0, treeData);
            this.departmentTree.ItemsSource = treeNodes;//数据绑定
        }

        /// <summary>
        /// 递归生成树形数据
        /// </summary>
        /// <param name="delst"></param>
        /// <returns></returns>
        public List<DepartmentModel> GetTrees(int parentid, List<DepartmentModel> nodes)
        {
            List<DepartmentModel> mainNodes = nodes.Where(x => x.ParentId == parentid).ToList<DepartmentModel>();
            List<DepartmentModel> otherNodes = nodes.Where(x => x.ParentId != parentid).ToList<DepartmentModel>();
            foreach (DepartmentModel dpt in mainNodes)
            {
                List<DepartmentModel> childList = GetTrees(dpt.Id, otherNodes);
                foreach (var item in childList)
                {
                    item.Parent = dpt;
                }

                dpt.Nodes = childList;
            }
            return mainNodes;
        }

        /// <summary>
        /// 树结构节点点击事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TreeNode_Click(object sender, RoutedEventArgs e)
        {
            TreeViewItem item = VisualUpwardSearch(e.OriginalSource as DependencyObject);
            if (item != null)
            {
                item.Focus();
                EachCheckedNode(item);
                e.Handled = true;
            }
        }
        private static TreeViewItem VisualUpwardSearch(DependencyObject source)
        {
            while (source != null && !(source is TreeViewItem))
                source = VisualTreeHelper.GetParent(source);

            return source as TreeViewItem;
        }
        private void EachCheckedNode(TreeViewItem chktree)
        {
            if (departmentTree.SelectedItem != null)
            {
                DepartmentModel tree = (DepartmentModel)departmentTree.SelectedItem;
                tree.SetChildrenChecked(tree.IsChecked);
                tree.SetParentChecked(tree.IsChecked);
            }
        }

        /// <summary>
        /// 选择文件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Btn_checkEnumFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new();

            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)//注意，此处一定要手动引入System.Window.Forms空间，否则你如果使用默认的DialogResult会发现没有OK属性
            {
                txt_enumAssemblyPath.Text = openFileDialog.FileName;
            }
        }

        /// <summary>
        /// 选择文件路径
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Btn_checkFile_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog openFolderDialog = new();
            DialogResult result = openFolderDialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.Cancel)
            {
                return;
            }
            else if (result == System.Windows.Forms.DialogResult.OK)
            {
                txt_filePath.Text = openFolderDialog.SelectedPath;
            }
        }

        /// <summary>
        /// 生成模型实体事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Btn_create_Click(object sender, RoutedEventArgs e)
        {
            if (txt_filePath.Text == "请选择目录")
            {
                System.Windows.MessageBox.Show("请选择目录！");
                return;
            }

            CreateModel createModel = new(txt_nameSpace.Text, txt_filePath.Text, txt_enumAssemblyPath.Text, "02_DotNetCoreEntity");
            // 获取选择的表
            foreach (var item in treeNodes)
            {
                if (item.IsChecked)
                {
                    List<string> nodeNames = new();
                    foreach (var node in item.Nodes)
                    {
                        if (node.IsChecked)
                            nodeNames.Add(node.DeptName);
                    }
                    createModel.CreateModeBySchemaName(nodeNames, item.DeptName);
                }
            }

            System.Windows.MessageBox.Show("模型生成完成！");
        }

        /// <summary>
        /// 返回上一步
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Btn_back_Click(object sender, RoutedEventArgs e)
        {
            this.tab1.IsSelected = true;
            this.tab2.IsSelected = false;
            this.Height = 360;
        }

        /// <summary>
        /// 配置文件ComboBox改变事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ServerSettingsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var dbServerSettingKey = this.serverSettingsComboBox.SelectedItem;
            string connstr = AppSettings.GetModel<string>($"{dbServerSettingRootKey}:{dbServerSettingKey}");
            string[] connStrSplit = connstr.Split(';');
            this.dbText.Text = connStrSplit?.FirstOrDefault(c => c.ToUpper().Contains("DATA SOURCE"))?.Split('=')[1];
            this.txtPort.Text = connStrSplit?.FirstOrDefault(c => c.ToUpper().Contains("PORT"))?.Split('=')[1];
            this.userText.Text = connStrSplit?.FirstOrDefault(c => c.ToUpper().Contains("USER ID"))?.Split('=')[1];
            this.pasText.Text = connStrSplit?.FirstOrDefault(c => c.ToUpper().Contains("PASSWORD"))?.Split('=')[1];
        }
    }
}
