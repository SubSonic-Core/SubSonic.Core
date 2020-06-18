﻿using NUnit.Framework;
using SubSonic.Tests.DAL.SUT;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SubSonic.Infrastructure.Logging;
using Models = SubSonic.Extensions.Test.Models;

namespace SubSonic.Tests.DAL.UserDefinedTable
{
    using FluentAssertions;
    using Infrastructure;
    using SubSonic.Data.Caching;
    using SubSonic.Extensions.Test;
    using SubSonic.Infrastructure.Schema;
    using SubSonic.Linq.Expressions;
    using System.Data;
    using System.Reflection;

    [TestFixture]
    public partial class UserDefinedTableTests
        : BaseTestFixture
    {
        public override void SetupTestFixture()
        {
            base.SetupTestFixture();

            Logger = DbContext.Instance.GetService<ISubSonicLogger<DbUserDefinedTableBuilder>>();
        }
        [Test]
        [TestCase(typeof(Models.RealEstateProperty))]
        [TestCase(typeof(Models.Unit))]
        public void CanGenerateUserDefinedTableForModel(Type modelType)
        {
            IEnumerable data = null;

            if(modelType == typeof(Models.RealEstateProperty))
            {
                data = RealEstateProperties;
            }
            else if (modelType == typeof(Models.Unit))
            {
                data = Units;
            }


            DbUserDefinedTableBuilder builder = new DbUserDefinedTableBuilder(
                DbContext.Model.GetEntityModel(modelType),
                data);

            string sql = null;

            using (var perf = Logger.Start("Generate SQL"))
            {
                 sql = builder.GenerateSql();
            }

            DataTable table = null;

            using (var perf = Logger.Start("Generate Data"))
            {
                table = builder.GenerateTable();
            }

            table.Dispose();

            Logger.LogInformation($"\n{sql}");
        }

        [Test]
        [TestCase(typeof(Models.RealEstateProperty), DbQueryType.Insert)]
        public void CanGenerateInsertStoredProcedureSqlFor(Type modelType, DbQueryType queryType)
        {
            IEnumerable data = null;

            string expected =
@"DECLARE @Result [Int];
EXEC @Result = [dbo].[InsertRealEstateProperty] @Entities = @Entities, @Error = @Error out";

            if (modelType == typeof(Models.RealEstateProperty))
            {
                data = RealEstateProperties.Select(x => new Entity<Models.RealEstateProperty>(x));
            }
            else if (modelType == typeof(Models.Unit))
            {
                data = Units.Select(x => new Entity<Models.Unit>(x));
            }
            else if (modelType == typeof(Models.Renter))
            {
                data = Renters.Select(x => new Entity<Models.Renter>(x));
            }

            Type StoredProcedureType = modelType.GetCustomAttributes<DbCommandQueryAttribute>().Single(x => x.QueryType == queryType).StoredProcedureType;

            object procedure = Activator.CreateInstance(StoredProcedureType, data);

            DbStoredProcedure proc = DbStoredProcedureParser.ParseStoredProcedure(procedure);

            proc.Sql.Should().Be(expected);
            
            proc.Name.Should().Be("[dbo].[InsertRealEstateProperty]");

            proc.Parameters.Count().Should().Be(3);

            IDbDataParameter parameter = proc.Parameters.First();

            parameter.ParameterName.Should().Be("@Entities");
            parameter.Value.Should().BeOfType<DataTable>();
            parameter.DbType.Should().Be(DbType.Object);
            ((System.Data.SqlClient.SqlParameter)parameter).SqlDbType.Should().Be(SqlDbType.Structured);
        }
    }
}
