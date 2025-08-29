// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.

namespace Microsoft.SCIM.WebHostSample.Provider
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.IO;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using Microsoft.SCIM;

    public class InMemoryStorage
    {
        internal readonly IDictionary<string, Core2Group> Groups;
        internal readonly IDictionary<string, Core2EnterpriseUser> Users;

        private InMemoryStorage()
        {
            this.Groups = new Dictionary<string, Core2Group>();
            this.Users = new Dictionary<string, Core2EnterpriseUser>();
        }

        private static readonly Lazy<InMemoryStorage> InstanceValue =
                                new Lazy<InMemoryStorage>(
                                        () =>
                                            new InMemoryStorage());

        public static InMemoryStorage Instance => InMemoryStorage.InstanceValue.Value;

        private JArray LoadFromFile(string contentRootPath,string resourceSuffix)
        {
            if (contentRootPath == null) { return null; }
            string combinedPath = contentRootPath + resourceSuffix;
            if (!File.Exists(combinedPath)) { return null; }
            using (StreamReader sr = File.OpenText(combinedPath))
            {
                using (JsonTextReader reader = new JsonTextReader(sr))
                {
                    JArray ja = (JArray)JToken.ReadFrom(reader);
                    return ja;
                }
            }
        }

        public void LoadUsers(string contentRootPath) {
            JArray ja = LoadFromFile(contentRootPath, "\\users.json");
            if (ja == null) { return;  }
            foreach (JObject item in ja)
            {
                string itemId = item.GetValue("id").ToString();
                Core2EnterpriseUser u = new Core2EnterpriseUser();
                u.Identifier = itemId;
                if (item.GetValue("externalId") != null) u.ExternalIdentifier = item.GetValue("externalId").ToString();
                if (item.GetValue("displayName") != null) u.DisplayName = item.GetValue("displayName").ToString();
                if (item.GetValue("userName") != null) u.UserName = item.GetValue("userName").ToString();
                Users[itemId] = u;

            }


        }
        public void LoadGroups(string contentRootPath) {
            JArray ja = LoadFromFile(contentRootPath, "\\groups.json");
            if (ja == null) { return; }
            foreach (JObject item in ja)
            {
                string itemId = item.GetValue("id").ToString();
                Core2Group g = new Core2Group();
                g.Identifier = itemId;
                if (item.GetValue("externalId") != null) g.ExternalIdentifier = item.GetValue("externalId").ToString();
                if (item.GetValue("displayName") != null) g.DisplayName = item.GetValue("displayName").ToString();
                Groups[itemId] = g;
            }
        }
    }

    /// <summary>
    /// Enables the efficient, dynamic composition of query predicates.
    /// Source: C# 9.0 in a Nutshell http://www.albahari.com/nutshell/predicatebuilder.aspx
    /// </summary>
    public static class PredicateBuilder
    {
        /// <summary>
        /// Creates a predicate that evaluates to true.
        /// </summary>
        public static Expression<Func<T, bool>> True<T>() { return f => true; }
        /// <summary>
        /// Creates a predicate that evaluates to false.
        /// </summary>
        public static Expression<Func<T, bool>> False<T>() { return f => false; }

        /// <summary>
        /// Combines the first predicate with the second using the logical "or".
        /// </summary>
        public static Expression<Func<T, bool>> Or<T>(this Expression<Func<T, bool>> expr1,
                                                            Expression<Func<T, bool>> expr2)
        {
            var invokedExpr = Expression.Invoke(expr2, expr1.Parameters.Cast<Expression>());
            return Expression.Lambda<Func<T, bool>>
                  (Expression.OrElse(expr1.Body, invokedExpr), expr1.Parameters);
        }

        /// <summary>
        /// Combines the first predicate with the second using the logical "and".
        /// </summary>
        public static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>> expr1,
                                                             Expression<Func<T, bool>> expr2)
        {
            var invokedExpr = Expression.Invoke(expr2, expr1.Parameters.Cast<Expression>());
            return Expression.Lambda<Func<T, bool>>
                  (Expression.AndAlso(expr1.Body, invokedExpr), expr1.Parameters);
        }
    }

}
