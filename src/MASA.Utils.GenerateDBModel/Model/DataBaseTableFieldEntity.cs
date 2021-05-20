namespace MASA.Utils.GenerateDBModel.Model
{
    /// <summary>
    /// Copyright (c) 杭州数闪科技有限公司
    /// 创建人：ypj
    /// 日 期：2021.05.17
    /// 描 述：数据库表字段
    /// </summary>
    public class DataBaseTableFieldEntity
    {
        /// <summary>
        /// 字段名
        /// </summary>
        public string ColumnName { get; set; }

        /// <summary>
        /// 是否允许为空
        /// </summary>
        public string IsNullable { get; set; }

        /// <summary>
        /// 数据类型
        /// </summary>
        public string DataType { get; set; }

        /// <summary>
        /// 字段类型
        /// </summary>
        public string ColumnType { get; set; }

        /// <summary>
        /// 字段主外键信息
        /// </summary>
        public string ColumnKey { get; set; }

        /// <summary>
        /// 字段备注说明
        /// </summary>
        public string ColumnComment { get; set; }

        /// <summary>
        /// 字段默认值
        /// </summary>
        public string ColumnDefault { get; set; }

        /// <summary>
        /// 其他信息，主键是否是自增的等
        /// </summary>
        public string Extra { get; set; }
    }
}
