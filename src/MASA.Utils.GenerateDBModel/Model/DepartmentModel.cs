using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace MASA.Utils.GenerateDBModel.Model
{
    /// <summary>
    /// 树控件操作实体类
    /// </summary>
    public class DepartmentModel : INotifyPropertyChanged
    {
        public List<DepartmentModel> Nodes { get; set; }

        /// <summary>
        /// 主节点的父id默认为0
        /// </summary>
        public DepartmentModel()
        {
            this.Nodes = new List<DepartmentModel>();
            this.ParentId = 0;    
        }

        /// <summary>
        /// 父节点
        /// </summary>
        public DepartmentModel Parent { get; set; }

        /// <summary>
        /// 节点标识
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 显示名
        /// </summary>
        public string DeptName { get; set; }

        /// <summary>
        /// 父类 Id
        /// </summary>
        public int ParentId { get; set; }

        /// <summary>
        /// 是否选中
        /// </summary>
        private bool isChecked = false;

        public bool IsChecked
        {
            get
            {
                return this.isChecked;
            }
            set
            {
                this.isChecked = value;
                NotifyPropertyChanged("IsChecked");
            }
        }

        /// <summary>  
        /// 设置所有子项的选中状态  
        /// </summary>  
        /// <param name="isChecked"></param>  
        public void SetChildrenChecked(bool isChecked)
        {
            foreach (DepartmentModel child in Nodes)
            {
                child.IsChecked = isChecked;
                child.SetChildrenChecked(isChecked);
            }
        }

        /// <summary>  
        /// 设置所属父节点的选中状态  
        /// </summary>  
        /// <param name="isChecked"></param>  
        public void SetParentChecked(bool isChecked)
        {
            if (Parent != null)
            {
                if (isChecked && !Parent.IsChecked)
                {
                    Parent.IsChecked = IsChecked;
                    Parent.SetParentChecked(isChecked);
                }
                else
                {
                    if (!isChecked && !Parent.GetChildrenChecked())
                    {
                        Parent.IsChecked = IsChecked;
                        Parent.SetParentChecked(isChecked);
                    }
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String info)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
        }

        /// <summary>  
        /// 获取所有子项的选中状态  
        /// </summary>  
        /// <return value>true表示有选中的，false表示没有选中的</teturn>  
        public bool GetChildrenChecked()
        {
            bool value = false;
            foreach (DepartmentModel child in Nodes)
            {
                if (child.IsChecked)
                {
                    value = true;
                    break;
                }
            }
            return value;
        }
    }

    public class SCTable
    {
        public string SchemaName { get; set; }

        public string TableName { get; set; }
    }
}
