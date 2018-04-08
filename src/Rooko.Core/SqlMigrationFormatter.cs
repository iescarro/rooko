﻿//	<file>
//		<license></license>
//		<owner name="Ian Escarro" email="ian.escarro@gmail.com"/>
//	</file>

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace Rooko.Core
{
	// Example: migrate "..\src\Rooko.Tests\bin\Debug\Rooko.Tests.dll" "Server=.;Database=test;Trusted_Connection=True;" "System.Data.SqlClient"
	public class SqlMigrationFormatter : IMigrationFormatter
	{
		string connectionString;
		string database;
		
		public SqlMigrationFormatter(string connectionString)
		{
			this.connectionString = connectionString;
		}
		
		public IDbConnection CreateConnection()
		{
			var connection = new SqlConnection(connectionString);
			this.database = connection.Database;
			return connection;
		}
		
		public string CreateTable(Table table)
		{
			string cols = "";
			int i = 0;
			foreach (var c in table.Columns) {
				cols += c.Name + " " + c.Type;
				cols += c.IsPrimaryKey ? " primary key" : "";
				cols += c.NotNull ? " not null" : "";
				cols += c.AutoIncrement ? " identity" : "";
				cols += i++ < table.Columns.Count - 1 ? ", " : "";
			}
			return string.Format(@"CREATE TABLE {0}(
{1}
);", table.Name, cols);
		}
		
		public string DropTable(string tableName)
		{
			return string.Format("DROP TABLE {0};", tableName);
		}
		
		public string AddColumn(string tableName, params Column[] columns)
		{
			string cols = "";
			int i = 0;
			foreach (var c in columns) {
				cols += c.Name + " " + c.Type;
				cols += i++ < columns.Length - 1 ? ", " : "";
			}
			return string.Format("ALTER TABLE {0} ADD {1}", tableName, cols);
		}
		
		public string AddColumn(string tableName, params string[] columns)
		{
			var cols = new List<Column>();
			foreach (var c in columns) {
				cols.Add(new Column(c));
			}
			return AddColumn(tableName, cols.ToArray());
		}
		
		public string DropColumn(string tableName, params string[] columns)
		{
			string cols = "";
			int i = 0;
			foreach (var c in columns) {
				cols += c;
				cols += i++ < columns.Length - 1 ? ", " : "";
			}
			return string.Format("ALTER TABLE {0} DROP COLUMN {1};", tableName, cols);
		}
		
		public string Insert(string tableName, params Column[] columns)
		{
			string cols = "", vals = "";
			int i = 0;
			foreach (var c in columns) {
				cols += c.Name;
				vals += "'" + c.Value + "'";
				cols += i < columns.Length - 1 ? ", " : "";
				vals += i < columns.Length - 1 ? ", " : "";
				i++;
			}
			return string.Format("insert into {0}({1}) values({2})", tableName, cols, vals);
		}
		
		public string GetDelete(string tableName, params Column[] columns)
		{
			string @where = "";
			int i = 0;
			if (columns.Length > 0) {
				@where = " WHERE ";
				foreach (var c in columns) {
					@where += c.Name + " = '" + c.Value + "'";
					@where += i++ < columns.Length - 1 ? " AND " : "";
				}
			}
			return string.Format("DELETE FROM {0} {1};", tableName, @where);
		}
		
		public string Insert(string tableName, ICollection<KeyValuePair<string, object>> values)
		{
			string cols = "", vals = "";
			int i = 1;
			foreach (var v in values) {
				cols += v.Key;
				vals += "'" + v.Value + "'";
				cols += i < values.Count ? ", " : "";
				vals += i < values.Count ? ", " : "";
				i++;
			}
			return string.Format("insert into {0}({1}) values({2})", tableName, cols, vals);
		}
		
		public string Delete(string tableName, ICollection<KeyValuePair<string, object>> @where)
		{
			string wher = "";
			int i = 1;
			foreach (var w in @where) {
				wher += w.Key + " = '" + w.Value + "'";
				wher += i++ < @where.Count ? " and " : "";
			}
			return string.Format("delete from {0} where {1}", tableName, wher);
		}
		
		public string Update(string tableName, ICollection<KeyValuePair<string, object>> values, ICollection<KeyValuePair<string, object>> @where)
		{
			string vals = "", wher = "";
			int i = 1;
			foreach (var v in values) {
				vals += v.Key + " = '" + v.Value + "'";
				vals += i++ < values.Count ? " and " : ""; 
			}
			foreach (var w in @where) {
				wher += w.Key + " = '" + w.Value + "'";
				wher += i++ < @where.Count ? " and " : "";
			}
			return string.Format("update {0} set {1} where {2}", tableName, vals, wher);
		}
		
		public string CreateSchema()
		{
			return string.Format("create table schema_migrations(id integer not null primary key identity, version varchar(255))");
		}
		
		public string CheckSchema()
		{
			return string.Format("select 1 from {0}.information_schema.tables where table_name = 'schema_migrations'", database);
		}
	}
}