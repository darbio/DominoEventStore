﻿ 
using FluentAssertions;
using Xunit;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Data.SQLite;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using CavemanTools.Logging;
using DominoEventStore;
using Ploeh.AutoFixture;
using SqlFu;
using SqlFu.Providers.Sqlite;


namespace Tests
{
    public class IntegrationTests:IDisposable
    {
        private const int MaxEvents = 30;
        private IStoreEvents _dest;
        private IStoreEvents _src;
        Fixture _fixture=new Fixture();
        private List<object> _events=new List<object>();
        private List<Guid> _entities;


        public IntegrationTests()
        {
            LogManager.OutputTo(s=>Trace.WriteLine(s));
            _dest = EventStore.Build(c =>
                c.UseMSSql(SqlServerTests.ConnectionString, SqlClientFactory.Instance.CreateConnection));
            _src = EventStore.Build(c =>
                c.UseSqlite(SqliteTests.ConnectionString, SQLiteFactory.Instance.CreateConnection));
        }

        async Task SetupSrc()
        {
            _entities=new List<Guid>();
            for (var i = 0; i < MaxEvents;i++)
            {
                var id = Guid.NewGuid();
                _entities.Add(id);
                var @event = i%2==0?(object)_fixture.Create<Event1>():_fixture.Create<Event2>();
                _events.Add(@event);
                await _src.Append(id, Guid.NewGuid(),@event);
            }
            
        }


        

      [Fact]
        public async Task migrate_from_sqlite_to_sqlserver()
        {
            await SetupSrc();
            var i = 10;

            _src.Advanced.MigrateEventsTo(_dest, "bubu");
            var evs = await _dest.GetEvents(_entities[i]);
            evs.Value.Count.Should().Be(1);
            var ev = evs.Value.First() as Event1;
            _events[i].CastAs<Event1>().ShouldBeEquivalentTo(ev);

        }


        public void Dispose()
        {
            _src.Advanced.ResetStorage();
            _dest.Advanced.ResetStorage();
        }
    }
} 
