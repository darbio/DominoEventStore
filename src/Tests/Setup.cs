﻿using DominoEventStore;
using DominoEventStore.Providers;
using SqlFu;
using SqlFu.Providers.SqlServer;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using AutoFixture;
using CavemanTools.Logging;
using Serilog;
using Xunit;
using Xunit.Abstractions;
using Utils = DominoEventStore.Utils;
[assembly: CollectionBehavior(DisableTestParallelization = true, MaxParallelThreads = 1)]
namespace Tests
{
    public static class Setup
    {
        public static EventStoreSettings EventStoreSettings(ITestOutputHelper h)
        {
            Logger(h);
            
            var f = new EventStoreSettings();
            
            return f;
        }

        public static void Logger(ITestOutputHelper h)
        {
            EventStore.WithLogger(c => c.WriteTo.TestOutput(h));
        }

        public const string TestSchema = "";

       public static ISpecificDbStorage GetDbStorage(IDbFactory f,ITestOutputHelper t)
       {
           if (f==null) return new InMemory();
            var provs=new Dictionary<string,ISpecificDbStorage>()
            {
                {
                    SqlServer2012Provider.Id
                    ,new SqlServerProvider(f)                    
                },
                {
                    SqlFu.Providers.Sqlite.SqliteProvider.Id
                    ,new SqliteProvider(f)
                }
            };
            ProviderExtensions.RegisterSqlFuConfig(f.Configuration);
            
            f.Configuration.UseLogManager();
           Log.Logger=new LoggerConfiguration().MinimumLevel.Debug().WriteTo.TestOutput(t).CreateLogger();
           LogManager.OutputTo(t.WriteLine);
           return provs[f.Provider.ProviderId];
       }

        public static readonly bool IsAppVeyor = Environment.GetEnvironmentVariable("Appveyor")?.ToUpperInvariant() == "TRUE";

        public static readonly Guid EntityId = Guid.NewGuid();

        public static IEnumerable<Commit> Commits(int count) => Commits<SomeEvent, SomeEvent>(count);
        public static Commit Commit(params object[] events) => new Commit("_",Guid.NewGuid(), Utils.PackEvents(events),Guid.NewGuid(), DateTimeOffset.Now, 1);

        public static IEnumerable<Commit> Commits<T,V>(int count) where T : class, new() where V : class, new()
        {
            return Enumerable.Range(1, count)
                .Select(i => new Commit("_", Setup.EntityId, Utils.PackEvents(new Object[]{new T(), new V()}), Guid.NewGuid(), DateTimeOffset.Now, i));
        }

        public static CommittedEvents CommittedEvents<T, V>(int count) where T : class, new() where V : class, new()
            => new CommittedEvents(Setup.Commits<T, V>(count).ToArray());

        public static IEnumerable<object> GetEvents(this Commit commit, IReadOnlyDictionary<Type, IMapEventDataToObject> upc)
        {
            return Utils.UnpackEvents(commit.Timestamp, commit.EventData, upc);
        }

        public static UnversionedCommit UnversionedCommit(string tenantId = "_", Guid? guid = null)
            =>
                new UnversionedCommit(tenantId, guid ?? Guid.NewGuid(), Utils.PackEvents(Events(1)), Guid.NewGuid(),
                    DateTimeOffset.Now);
        

        public static List<object> Events(int count=4)
        {
            var f = new Fixture();
            return Enumerable.Range(1, count)
                .Select(i => i % 2 == 1 ? (object)f.Create<Event1>() : f.Create<Event2>())
                .ToList();          
        }

        public static Snapshot Snapshot(int ver,Guid entity,string tenant="_")
        => new Snapshot(ver,entity,tenant,Utils.PackSnapshot(new Fixture().Create<SomeMemento>()),DateTimeOffset.Now);

        public static Func<Commit, IEnumerable<object>> EventDeserializerWIthoutUpcasting()
        {
            return c=>c.GetEvents(ImmutableDictionary<Type, IMapEventDataToObject>.Empty);
        }
    }

  

}