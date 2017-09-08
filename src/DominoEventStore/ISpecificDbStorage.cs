﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DominoEventStore
{
    public interface ISpecificDbStorage : IStoreBatchProgress
    {
        /// <summary>
        /// Should calculate the version of the entity and use that to detect concurrency problems
        /// </summary>
        /// <param name="commit"></param>
        /// <param name="eventDeserializer">Used to </param>
        /// <exception cref="DuplicateCommitException"></exception>
        /// <exception cref="ConcurrencyException"></exception>
        /// <returns></returns>
        Task Append(UnversionedCommit commit, Func<Commit, IEnumerable<object>> eventDeserializer);

        /// <summary>
        /// Adds the commit as is. Duplicates should be ignored
        /// </summary>
        /// <param name="commit"></param>        
        /// <returns></returns>
        void Import(Commit commit);

        Task<Optional<EntityStreamData>> GetData(QueryConfig cfg, CancellationToken token);
        /// <summary>
        /// Creates the tables in the specified/default schema
        /// </summary>
        /// <param name="schema"></param>
        void InitStorage(string schema = null);
        void ResetStorage();
        void DeleteTenant(string tenantId);

        Task Store(Snapshot snapshot);
        /// <summary>
        /// Delete one or all stored snapshots
        /// </summary>
        /// <param name="entityId"></param>
        /// <param name="tenantId"></param>
        /// <param name="entityVersion">If missing, it deletes all stored snapshots</param>
        /// <returns></returns>
        Task DeleteSnapshot(Guid entityId, string tenantId, int? entityVersion=null);
    }
}