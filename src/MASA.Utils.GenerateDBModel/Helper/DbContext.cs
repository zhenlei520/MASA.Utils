﻿using Dapper;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace MASA.Utils.GenerateDBModel.Helper
{
    public static class DbContext
    {
        private static IDbConnection Db;

        public static IDbConnection GetDbConnection(string connStr)
        {
            DapperHelper.GetInstance(connStr);
            Db = DapperHelper.OpenCurrentDbConnection();
            return Db;
        }


        public static T QueryFirstOrDefault<T>(string sql, object param = null)
        {
            return Db.QueryFirstOrDefault<T>(sql, param);
        }

        public static Task<T> QueryFirstOrDefaultAsync<T>(string sql, object param = null)
        {
            return Db.QueryFirstOrDefaultAsync<T>(sql, param);
        }

        public static IEnumerable<T> Query<T>(string sql, object param = null, IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = null, CommandType? commandType = null)
        {
            return Db.Query<T>(sql, param, transaction, buffered, commandTimeout, commandType);
        }

        public static Task<IEnumerable<T>> QueryAsync<T>(string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return Db.QueryAsync<T>(sql, param, transaction, commandTimeout, commandType);
        }

        public static int Execute(string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return Db.Execute(sql, param, transaction, commandTimeout, commandType);
        }

        public static Task<int> ExecuteAsync(string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return Db.ExecuteAsync(sql, param, transaction, commandTimeout, commandType);
        }

        public static T ExecuteScalar<T>(string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return Db.ExecuteScalar<T>(sql, param, transaction, commandTimeout, commandType);
        }

        public static Task<T> ExecuteScalarAsync<T>(string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return Db.ExecuteScalarAsync<T>(sql, param, transaction, commandTimeout, commandType);
        }

        public static SqlMapper.GridReader QueryMultiple(string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return Db.QueryMultiple(sql, param, transaction, commandTimeout, commandType);
        }
        public static Task<SqlMapper.GridReader> QueryMultipleAsync(string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return Db.QueryMultipleAsync(sql, param, transaction, commandTimeout, commandType);
        }
    }
}