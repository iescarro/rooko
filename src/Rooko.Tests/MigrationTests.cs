﻿//	<file>
//		<license></license>
//		<owner name="Ian Escarro" email="ian.escarro@gmail.com"/>
//	</file>

using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rooko.Core;

namespace Rooko.Tests
{
	[TestFixture]
	public class MigrationTests
	{
		CreateUsers m1;
		AddUsernameToUsers m2;
		
		[SetUpAttribute]
		public void Setup()
		{
			m1 = new CreateUsers();
			m1.Migrating += delegate(object sender, MigrationEventArgs e) {
				Console.WriteLine(e.Message);
			};
			
			m2 = new AddUsernameToUsers();
			m2.Migrating += delegate(object sender, MigrationEventArgs e) {
				Console.WriteLine(e.Message);
			};
		}
		
		[Test]
		public void TestCreateItemsMigrate()
		{
			m1.Migrate();
		}
		
		[Test]
		public void TestCreateItemsRollback()
		{
			m1.Rollback();
		}
		
		[Test]
		public void TestAddPriceToItemsMigrate()
		{
			m2.Migrate();
		}
		
		[Test]
		public void TestAddPriceToItemsRollback()
		{
			m2.Rollback();
		}
	}
	
	public class CreateUsers : Migration
	{
		public CreateUsers() : base("E128A916-A06E-4142-B73D-DD0E6811D618")
		{
		}
		
		public override void Migrate()
		{
			CreateTable(
				"users",
				new Column("id", "integer", true, true, true),
				new Column("name"),
				new Column("password")
			);
		}
		
		public override void Rollback()
		{
			DropTable("users");
		}
	}
	
	public class AddUsernameToUsers : Migration
	{
		public AddUsernameToUsers() : base("DA8965AD-31E3-4085-8125-8396758C4A82")
		{
		}
		
		public override void Migrate()
		{
			AddColumn("users", new Column("username"));
		}
		
		public override void Rollback()
		{
			RemoveColumn("users", "username");
		}
	}
	
	public class InsertRootUser : Migration
	{
		public InsertRootUser() : base("116AA4B6-5DEE-4621-8D39-DB0FC5D9110E")
		{
		}
		
		public override void Migrate()
		{
			Insert(
				"users",
				new[] {
					new KeyValuePair<string, object>("username", "admin"),
					new KeyValuePair<string, object>("password", "root")
				}
			);
		}
		
		public override void Rollback()
		{
			Delete(
				"users",
				new[] {
					new KeyValuePair<string, object>("username", "admin"),
					new KeyValuePair<string, object>("password", "root")
				}
			);
		}
	}
}
