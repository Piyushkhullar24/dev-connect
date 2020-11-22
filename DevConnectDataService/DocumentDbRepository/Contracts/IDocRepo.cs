using Microsoft.Azure.Documents;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DocumentDbRepository.Contracts
{
    /// <summary>
    /// Interface for DocRepo
    /// </summary>
    /// <typeparam name="TDoc"></typeparam>
    public interface IDocRepo<TDoc>
    {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="documentId"></param>
        /// <param name="partitionKey"></param>
        /// <param name="throwNotFoundErrors"></param>
        /// <returns></returns>
        Task<TDoc> ReadAsync(string documentId, object partitionKey = null, bool throwNotFoundErrors = false);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="document"></param>
        /// <returns></returns>
        Task<string> UpsertAsync(TDoc document);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="documentId"></param>
        /// <param name="partitionKey"></param>
        /// <returns></returns>
        Task<bool> DeleteAsync(string documentId, object partitionKey = null);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="enableCrossPartitionQuery">must be true to query across partitions</param>
        /// <returns></returns>
        Task<IEnumerable<TDoc>> QueryAsync(string sql, bool enableCrossPartitionQuery = false);

        /// <summary>
        /// Execute Linq returning collection of TDoc documents
        /// </summary>
        /// <param name="predicate"></param>
        /// <param name="enableCrossPartitionQuery">must be true to query across partitions</param>
        /// <returns></returns>
        Task<IEnumerable<TDoc>> QueryAsync(Expression<Func<TDoc, bool>> predicate, bool enableCrossPartitionQuery = false);

        /// <summary>
        /// Execute Linq returning collection of TDoc documents, with order by ASC (need a DSC version!)
        /// </summary>
        /// <typeparam name="R"></typeparam>
        /// <param name="predicate"></param>
        /// <param name="orderBy"></param>
        /// <param name="enableCrossPartitionQuery"></param>
        /// <param name="descending"></param>
        /// <returns></returns>
        Task<IEnumerable<TDoc>> QueryOrderByAsync<R>(Expression<Func<TDoc, bool>> predicate, Expression<Func<TDoc, R>> orderBy, bool enableCrossPartitionQuery = false, bool descending = false);

        /// <summary>
        /// Execute SQL returning strongly typed results
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="sql"></param>
        /// <param name="partitionKey"></param>
        /// <returns></returns>
        Task<IEnumerable<TResult>> QueryCustomResultAsync<TResult>(string sql, object partitionKey = null);

        /// <summary>
        /// Execute SQL returning strongly typed results
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="sqlSpec"></param>
        /// <param name="partitionKey"></param>
        /// <returns></returns>
        Task<IEnumerable<TResult>> QueryCustomResultAsync<TResult>(SqlQuerySpec sqlSpec, object partitionKey = null);

    }
}
