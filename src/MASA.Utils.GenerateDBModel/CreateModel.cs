using MASA.Utils.GenerateDBModel.Helper;
using MASA.Utils.GenerateDBModel.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace MASA.Utils.GenerateDBModel
{
    /// <summary>
    /// 根据数据库表创建模型
    /// </summary>
    public class CreateModel
    {
        #region 属性和字段

        /// <summary>
        /// 命名空间名
        /// </summary>
        protected string NamespaceName { get; set; }

        protected string _filePath;

        /// <summary>
        /// 生成文件路径
        /// </summary>
        public string FilePath
        {
            get
            {
                return _filePath;
            }
            set
            {
                _filePath = value;
                if (!Directory.Exists(_filePath))
                    Directory.CreateDirectory(_filePath);
                #region EFMapping
                //string mapDir = Path.Combine(_filePath, "EFMapping");
                //if (!Directory.Exists(mapDir))
                //    Directory.CreateDirectory(mapDir);
                #endregion
            }
        }

        /// <summary>
        /// 枚举程序集全名
        /// </summary>
        public string EnumAssemblyFullName { get; set; }

        /// <summary>
        /// 模板名称
        /// </summary>
        protected string TemplateName { get; set; }

        /// <summary>
        /// 当前时间字符串格式
        /// </summary>
        protected string CurrentTime { get; set; }

        /// <summary>
        /// 年份
        /// </summary>
        protected string Year { get; set; }

        protected string BaseEntityName { get; set; } = "EntityBase";

        #endregion

        /// <summary>
        /// 创建数据库表模型
        /// </summary>
        /// <param name="nameSpaceName">命名空间</param>
        /// <param name="filePath">文件生成路径（不传，默认放在“我的文档”文件夹下）</param>
        /// <param name="enumAsmFullName">程序集全名</param>
        /// <param name="templateName">模板名称</param>
        public CreateModel(string nameSpaceName, string filePath, string enumAsmFullName, string templateName)
        {
            NamespaceName = nameSpaceName;
            if (string.IsNullOrEmpty(filePath) || filePath == "请选择路径")
                this.FilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "TempEntityCode");
            else
                this.FilePath = filePath;
            this.EnumAssemblyFullName = enumAsmFullName;
            this.TemplateName = templateName;
            this.CurrentTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            this.Year = DateTime.Now.Year.ToString();
        }

        /// <summary>
        /// 根据数据库名生成实体类
        /// </summary>
        public void CreateModeBySchemaName(List<string> tableName, string schemaName)
        {
            string strSQL = "SELECT TABLE_NAME as TableName,TABLE_COMMENT as TableComment FROM `TABLES` WHERE `TABLE_SCHEMA` = @schemaName";
            var args = new { schemaName };
            List<DataBaseTableEntity> tables = DbContext.Query<DataBaseTableEntity>(strSQL, args).ToList();

            strSQL = @"
SELECT
    COLUMN_NAME AS ColumnName,
	IS_NULLABLE AS IsNullable,
	DATA_TYPE AS DataType,
	COLUMN_TYPE AS ColumnType,
	COLUMN_KEY AS ColumnKey,
	COLUMN_COMMENT AS ColumnComment,
    COLUMN_DEFAULT AS ColumnDefault,
    EXTRA AS Extra
FROM
	`COLUMNS` WHERE `TABLE_SCHEMA` = @schemaName AND `TABLE_NAME` = @tableName";

            foreach (var table in tables)
            {
                if (tableName.Contains(table.TableName))
                {
                    var args_t = new { schemaName, tableName = table.TableName };

                    List<DataBaseTableFieldEntity> tempField = DbContext.Query<DataBaseTableFieldEntity>(strSQL, args_t).ToList();
                    CreateEntity(table, tempField);
                }
            }
        }

        /// <summary>
        /// 生成 CS 文件
        /// </summary>
        private void CreateEntity(DataBaseTableEntity table, List<DataBaseTableFieldEntity> fields)
        {
            TemplateName = "02_DotNetCoreEntity";

            string templateDir = Path.Combine(Environment.CurrentDirectory, "Template");
            string codeStr = File.ReadAllText(Path.Combine(templateDir, TemplateName + ".template"));

            string baseEntityStr = File.ReadAllText(Path.Combine(templateDir, "00_EntityBase.template"));

            // 组合字段
            string fieldStr = @"
        /// <summary>
        /// [ColumnComment]
        /// </summary>
        [ColumnName]
        public [DataType] [FieldName] { get; set; }
";
            List<string> fieldStrs = new();
            List<string> PRIStrs = new();
            string TKey = null;

            Assembly asm = Assembly.LoadFile(EnumAssemblyFullName);
            string enumNamespace = null;

            foreach (var item in fields)
            {
                string dataType = GetDataType(item);
                string columnName = ToTitleCase(item.ColumnName);

                #region 主键处理

                if (item.ColumnKey == "PRI")
                {
                    PRIStrs.Add(string.Format("t.{0}", columnName));

                    //Guid的处理
                    if (item.ColumnType == "char(36)" || item.DataType == "uniqueidentifier")
                        TKey = "Guid";
                    else
                        TKey = item.DataType;

                }

                #endregion

                //通用字段过滤
                if (item.ColumnName == "CreateTime" || item.ColumnName == "CreateUserId" ||
                    item.ColumnName == "UpdateTime" || item.ColumnName == "UpdateUserId" ||
                    item.ColumnName == "IsDeleted" || item.ColumnName == "Id")
                {
                    continue;
                }

                //枚举字段处理
                foreach (var type in asm?.GetTypes())
                {
                    if (string.IsNullOrEmpty(enumNamespace))
                        enumNamespace = type.Namespace;

                    //规则 表明+复数(字段名)
                    string pluralColumnName = StringUtil.ToPlural(item.ColumnName);
                    string enumName = table.TableName + pluralColumnName;
                    if (enumName.ToLower() == type.Name.ToLower())
                    {
                        dataType = type.Name;
                        break;
                    }
                }

                fieldStrs.Add(fieldStr.Replace("[ColumnComment]", item.ColumnComment)
                    .Replace("[ColumnName]", @"[Column(""" + item.ColumnName + @""")]")
                    .Replace("[DataType]", dataType)
                    .Replace("[FieldName]", columnName));
            }

            //生成BaseEntity
            string baseEntityCode = baseEntityStr.Replace("[NamespaceName]", this.NamespaceName);
            string fileAllPath = Path.Combine(_filePath, $"{BaseEntityName}.cs");
            File.WriteAllText(fileAllPath, baseEntityCode);

            // 实体类文件字段替换
            string entityClassName = ToTitleCase(table.TableName);
            string entityCode = codeStr.Replace("[NamespaceName]", this.NamespaceName)
            .Replace("[TKey]", TKey)
            .Replace("[EnumNamespace]", string.IsNullOrEmpty(enumNamespace) ? null : $"using {enumNamespace};")
            .Replace("[TableName]", $"[Table(\"{table.TableName}\")]")
            .Replace("[CreateTime]", CurrentTime)
            .Replace("[TableComment]", table.TableComment)
            .Replace("[EntityClassName]", entityClassName)
            .Replace("[Fiels]", string.Join("", fieldStrs))
            .Replace("[Year]", Year);
            fileAllPath = Path.Combine(_filePath, entityClassName + ".cs");
            File.WriteAllText(fileAllPath, entityCode);

            #region 生成Mapping文件

            // 实体数据库实体映射 字段替换
            /*
            codeStr = File.ReadAllText(Path.Combine(templateDir, TemplateName + "Map.template"));
            string mapClassName = ToTitleCase(table.TableName) + "Map";
            string mapCode = codeStr.Replace("[EntityNamespaceName]", this.NamespaceName)
                .Replace("[NamespaceName]", this.NamespaceName.Replace(this.NamespaceName.Split('.').Last(), "EFMapping"))
                .Replace("[CreateTime]", CurrentTime)
                .Replace("[TableComment]", table.TableComment)
                .Replace("[MapClassName]", mapClassName)
                .Replace("[EntityClassName]", entityClassName)
                .Replace("[TableName]", table.TableName)
                .Replace("[PRIStrs]", string.Join(",", PRIStrs))
                .Replace("[Year]", Year);
            fileAllPath = Path.Combine(_filePath, "EFMapping", mapClassName + ".cs");
            File.WriteAllText(fileAllPath, mapCode);
            */

            #endregion
        }

        /// <summary>
        /// 转换数据类型
        /// </summary>
        /// <param name="field"></param>
        /// <returns></returns>
        private string GetDataType(DataBaseTableFieldEntity field)
        {
            string type = "string";

            switch (field.DataType)
            {
                case "char":
                    if (field.ColumnType == "char(36)")
                    {
                        type = "Guid";
                        if (field.IsNullable == "YES") type = "Guid?";
                    }
                    break;
                case "varchar":
                case "text":
                case "nchar":
                case "ntext":
                case "nvarchar":
                    break;
                case "smallint":
                    type = "short";
                    if (field.IsNullable == "YES") type = "short?";
                    break;
                case "int":
                case "mediumint":
                    type = "int";
                    if (field.IsNullable == "YES") type = type = "int?";
                    break;
                case "bigint":
                    type = "long";
                    if (field.IsNullable == "YES") type = "long?";
                    break;
                case "binary":
                case "image":
                case "varbinary":
                    type = "byte[]";
                    break;
                case "bit":
                    type = "bool";
                    if (field.IsNullable == "YES") type = "bool?";
                    break;
                case "datetime":
                case "smalldatetime":
                case "timestamp":
                case "date":
                    type = "DateTime";
                    if (field.IsNullable == "YES") type = "DateTime?";
                    break;
                case "decimal":
                case "money":
                case "smallmoney":
                case "numeric":
                    type = "decimal";
                    if (field.IsNullable == "YES") type = "decimal?";
                    break;
                case "real":
                case "double":
                    type = "double";
                    if (field.IsNullable == "YES") type = "double?";
                    break;
                case "float":
                    type = "float";
                    if (field.IsNullable == "YES") type = "float?";
                    break;
                case "tinyint":
                    type = "short";
                    if (field.IsNullable == "YES")
                    {
                        type = "short?";
                    }
                    if (this.TemplateName != "DotNetCoreEntity")
                    {
                        if (field.ColumnType.Contains("(1)"))
                        {
                            type = "bool";
                            if (field.ColumnType.Contains("(1)") && field.IsNullable == "YES") type = "bool?";
                        }
                    }
                    break;
                case "uniqueidentifier":
                    type = "Guid";
                    break;
                case "Variant":
                    type = "Object";
                    break;
                default:
                    break;
            }

            return type;
        }

        /// <summary>
        /// 表名、字段名映射规则（驼峰规则）
        /// </summary>
        /// <param name="fromStr"></param>
        private static string ToTitleCase(string fromStr)
        {
            string[] strs = Regex.Replace(fromStr, @"\W", "").Split('_');

            string toStr = "";
            for (int i = 0; i < strs.Length; i++)
            {
                string str = strs[i];
                int str_length = str.Length;
                if (str_length > 1)
                {
                    toStr += (str.Substring(0, 1).ToUpper() + str.Substring(1, str_length - 1));
                }
                else
                    toStr += str.ToUpper();
            }

            return toStr;
        }
    }
}
