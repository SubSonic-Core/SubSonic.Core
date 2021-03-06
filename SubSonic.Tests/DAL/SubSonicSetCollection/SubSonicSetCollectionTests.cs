﻿using FluentAssertions;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace SubSonic.Tests.DAL.DbSetCollection
{
    using Data.Caching;
    using Data.DynamicProxies;
    using Extensions.Test;
    using Extensions.Test.Models;
    
    using SUT;

    [TestFixture]
    public class SubSonicSetCollectionTests
        : BaseTestFixture
    {
        public override void SetupTestFixture()
        {
            base.SetupTestFixture();

            string
                properties_all = @"SELECT [T1].[ID], [T1].[StatusID], [T1].[HasParallelPowerGeneration]
FROM [dbo].[RealEstateProperty] AS [T1]",
                units =
@"SELECT [{0}].[ID], [{0}].[Bedrooms] AS [NumberOfBedrooms], [{0}].[StatusID], [{0}].[RealEstatePropertyID]
FROM [dbo].[Unit] AS [{0}]
WHERE ([{0}].[RealEstatePropertyID] = @realestatepropertyid_1)",
                status =
@"SELECT [{0}].[ID], [{0}].[name] AS [Name], [{0}].[IsAvailableStatus]
FROM [dbo].[Status] AS [{0}]
WHERE ([{0}].[ID] = @id_1)",
                statuses =
@"SELECT [{0}].[ID], [{0}].[name] AS [Name], [{0}].[IsAvailableStatus]
FROM [dbo].[Status] AS [{0}]",
                kara =
@"SELECT [{0}].[ID], [{0}].[FirstName], [{0}].[MiddleInitial], [{0}].[FamilyName], [{0}].[FullName]
FROM [dbo].[Person] AS [{0}]
WHERE ([{0}].[ID] = @id_1)";

            Context.Database.Instance.AddCommandBehavior(properties_all, cmd => RealEstateProperties.ToDataTable());
            Context.Database.Instance.AddCommandBehavior(units.Format("T1"), cmd => Units.Where(x => x.RealEstatePropertyID == cmd.Parameters["@realestatepropertyid_1"].GetValue<int>()).ToDataTable());
            Context.Database.Instance.AddCommandBehavior(status.Format("T1"), cmd => Statuses.Where(x => x.ID == cmd.Parameters["@id_1"].GetValue<int>()).ToDataTable());
            Context.Database.Instance.AddCommandBehavior(statuses.Format("T1"), Statuses);
            Context.Database.Instance.AddCommandBehavior(kara.Format("T1"), cmd => People.Where(x => x.ID == cmd.Parameters["@id_1"].GetValue<int>()).ToDataTable());
        }

        [Test]
        public void CanAddNewInstanceToCollection()
        {
            Status status = Context.Statuses.Single(x => x.ID == 1);

            RealEstateProperty property = new RealEstateProperty()
            {
                HasParallelPowerGeneration = true,
                StatusID = status.ID,
                Status = status
            };

            Context.RealEstateProperties.Add(property);

            IEntityProxy<RealEstateProperty> proxy = Context.ChangeTracking.GetCacheFor<RealEstateProperty>().Single(x => x.IsNew);

            proxy.Data.Should().BeEquivalentTo(property);
        }

        [Test]
        public void CanEnumerateCacheObject()
        {
            Context.ChangeTracking.Add(typeof(RealEstateProperty), new Entity<RealEstateProperty>(new RealEstateProperty() { ID = -1, StatusID = 1 }));

            Context.ChangeTracking.Add(typeof(RealEstateProperty), DynamicProxy.MapInstanceOf(Context, new Entity<RealEstateProperty>(new RealEstateProperty() { ID = -2, StatusID = 1 })));

            Context.ChangeTracking.Add(typeof(Status), DynamicProxy.MapInstanceOf(Context, new Entity<Status>(new Status() { ID = -1, Name = "None", IsAvailableStatus = false })));

            foreach (var item in Context.ChangeTracking)
            {
                foreach (IEntityProxy proxy in item.Value)
                {
                    object value = null;

                    if (proxy is Entity entity)
                    {
                        value = entity.Data;
                    }

                    if ((value ?? proxy) is RealEstateProperty property)
                    {
                        property.Should().NotBeNull();
                    }
                    else if ((value ?? proxy) is Status status)
                    {
                        status.Should().NotBeNull();
                    }
                }
            }
        }

        [Test]
        public void CanQueryFromCacheObject()
        {
            Expression expression = Context.Statuses.Where(x => x.ID == 1).Expression;

            Status
                status_ctrl = Context.Statuses.Where(x => x.ID == 1).Single(),
                status_cache = Context.ChangeTracking.Where<IEnumerable<Status>>(typeof(Status), Context.Statuses.Provider, expression).Single();

            status_ctrl.Should().BeSameAs(status_cache);
        }

        [Test]
        public void CanPullFromCacheInsteadOfDatabase()
        {
            List<Status> statuses = Context.Statuses.ToList();

            foreach (Status status in statuses)
            {
                status.Should().NotBeNull();
            }

            Status _status = Context.Statuses.Single(x => x.ID == 2);

            _status.Should().NotBeNull();
            _status.ID.Should().Be(2);
        }
    }
}
