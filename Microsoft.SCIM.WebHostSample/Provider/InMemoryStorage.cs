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
        internal readonly IDictionary<string, AgenticIdentity> AgenticIdentities;

        private string _contentRootPath;

        private InMemoryStorage()
        {
            this.Groups = new Dictionary<string, Core2Group>();
            this.Users = new Dictionary<string, Core2EnterpriseUser>();
            this.AgenticIdentities = new Dictionary<string, AgenticIdentity>();
        }

        private static readonly Lazy<InMemoryStorage> InstanceValue =
                                new Lazy<InMemoryStorage>(
                                        () =>
                                            new InMemoryStorage());

        public static InMemoryStorage Instance => InMemoryStorage.InstanceValue.Value;

        private JArray LoadFromFile(string contentRootPath,string resourceSuffix)
        {
            if (contentRootPath == null) { return null; }
            _contentRootPath = contentRootPath;
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



        public void SaveAgenticIdentities()
        {
            AgenticIdentity[] arr = AgenticIdentities.Values.ToArray();
            string result = JsonConvert.SerializeObject(arr);
            string combinedPath = _contentRootPath + "\\agenticidentities-written.json";
            using (StreamWriter sw = new StreamWriter(combinedPath))
            {
                sw.WriteLine(result);
                sw.Close();
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
                if (item.GetValue("meta") != null) { SetMetadata(u.Metadata, item.GetValue("meta")); }
                if (item.GetValue("roles") != null) { u.Roles = ParseRoles(item.GetValue("roles")); }
                //if (item.GetValue("entitlements") != null) { u.Entitlements = ParseEntitlements(item.GetValue("entitlements")); }
                //if (item.GetValue("groups") != null) { u.Groups = ParseGroups(item.GetValue("groups")); }
                // name, nickName, profileUrl, emails, addresses, phoneNumbers, ims, photos, x509Certificates
                if (item.GetValue("userType") != null) u.UserType = item.GetValue("userType").ToString();
                if (item.GetValue("active") != null) { u.Active = Boolean.Parse(item.GetValue("active").ToString()); }
                // title preferredLanguage locale, timezone, 
                if (item.GetValue("urn:ietf:params:scim:schemas:extension:enterprise:2.0:User ") != null)
                {
                    JObject c2u = (JObject)item.GetValue("urn:ietf:params:scim:schemas:extension:enterprise:2.0:User ");
                    // employeeNumber costCenter organization division department manager
                }
                
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
                if (item.GetValue("meta") != null) { SetMetadata(g.Metadata, item.GetValue("meta")); }
                if (item.GetValue("members") != null) { g.Members = ParseMembers(item.GetValue("members")); }
                Groups[itemId] = g;
            }
        }

        public void LoadAgenticIdentities(string contentRootPath)
        {
            JArray ja = LoadFromFile(contentRootPath, "\\agenticidentities.json");
            if (ja == null) { return; }
            foreach (JObject item in ja)
            {
                string itemId = item.GetValue("id").ToString();
                AgenticIdentity g = new AgenticIdentity();
                g.Identifier = itemId;
                if (item.GetValue("externalId") != null) g.ExternalIdentifier = item.GetValue("externalId").ToString();
                if (item.GetValue("displayName") != null) g.DisplayName = item.GetValue("displayName").ToString();
                if (item.GetValue("description") != null) g.Description = item.GetValue("description").ToString();
                if (item.GetValue("agenticApplicationId") != null) g.AgenticApplicationId = item.GetValue("agenticApplicationId").ToString();
                if (item.GetValue("active") != null) { g.Active = Boolean.Parse(item.GetValue("active").ToString()); }
                if (item.GetValue("owners") != null) { g.Owners = ParseOwners(item.GetValue("owners")); }
                if (item.GetValue("oAuthClientIdentifiers") != null) { SetOAuthClientIdentifiers(g, item.GetValue("oAuthClientIdentifiers")); }
                if (item.GetValue("meta") != null) { SetMetadata(g.Metadata, item.GetValue("meta")); }
                if (item.GetValue("roles") != null) { g.Roles = ParseRoles(item.GetValue("roles")); }
                if (item.GetValue("entitlements") != null) { g.Entitlements = ParseEntitlements(item.GetValue("entitlements")); }
                if (item.GetValue("groups") != null) { g.Groups = ParseGroups(item.GetValue("groups")); }
                // XXX custom extensions
                AgenticIdentities[itemId] = g;
            }
        }

        private List<AgenticIdentityOwner> ParseOwners(JToken jt)
        {
            List<AgenticIdentityOwner> ra = new List<AgenticIdentityOwner>();
            JArray ja = (JArray)jt;
            foreach (JObject item in ja)
            {
                AgenticIdentityOwner nr = new AgenticIdentityOwner();
                if (item.GetValue("value") != null) nr.Value = item.GetValue("value").ToString();
                
                // XXX parse JObject  for typename
                ra.Add(nr);
            }
            return ra;
        }

        private List<Member> ParseMembers(JToken jt)
        {
            List<Member> ra = new List<Member>();
            JArray ja = (JArray)jt;
            foreach (JObject item in ja)
            {
                Member nr = new Member();
                if (item.GetValue("value") != null) nr.Value = item.GetValue("value").ToString();
                if (item.GetValue("$ref") != null) ; // XXX
                ra.Add(nr);
            }
            return ra;
        }

        private List<Role> ParseRoles(JToken jt)
        {
            List<Role> ra = new List<Role>();
            JArray ja = (JArray)jt;
            foreach (JObject item in ja)
            {
                Role nr = new Role();
                // XXX parse JObject
                ra.Add(nr);
            }
            return ra;
        }

        private List<Entitlement> ParseEntitlements(JToken jt)
        {
            List<Entitlement> ra = new List<Entitlement>();
            JArray ja = (JArray)jt;
            foreach (JObject item in ja)
            {
                Entitlement nr = new Entitlement();
                // XXX parse JObject
                ra.Add(nr);
            }
            return ra;
        }
        private List<GroupAttributeValue> ParseGroups(JToken jt)
        {
            List<GroupAttributeValue> ra = new List<GroupAttributeValue>();
            JArray ja = (JArray)jt;
            foreach (JObject item in ja)
            {
                GroupAttributeValue nr = new GroupAttributeValue();
                if (item.GetValue("value") != null) nr.Value = item.GetValue("value").ToString();
                if (item.GetValue("$ref") != null) ; // XXX
                if (item.GetValue("display") != null) nr.Display = item.GetValue("display").ToString(); 
                ra.Add(nr);
            }
            return ra;
        }
        private void SetOAuthClientIdentifiers(AgenticIdentity g,JToken jt)
        {
            JArray ja = (JArray)jt;
            List<AgenticIdentityOAuthClientIdentifier> l = new List<AgenticIdentityOAuthClientIdentifier>();
            foreach (JObject item in ja)
            {
                AgenticIdentityOAuthClientIdentifier aic = new AgenticIdentityOAuthClientIdentifier();
                if (item.GetValue("clientId") != null)
                {
                    aic.ClientId = item.GetValue("clientId").ToString();
                }
                if (item.GetValue("description") != null)
                {
                    aic.Description = item.GetValue("description").ToString();
                }
                if (item.GetValue("issuer") != null)
                {
                    aic.Issuer = item.GetValue("issuer").ToString();
                }
                if (item.GetValue("name") != null)
                {
                    aic.Name = item.GetValue("name").ToString();
                }

                if (item.GetValue("subject") != null)
                {
                    aic.Subject = item.GetValue("subject").ToString();
                }

                if (item.GetValue("audiences") != null)
                {
                    List<string> naud = new List<string>();
                    JArray jaud = (JArray)item.GetValue("audiences");
                    foreach (JValue aud1 in jaud)
                    {
                        naud.Add(aud1.ToString());
                    }
                    aic.Audiences = naud;
                }
 
                l.Add(aic);
            }
            g.OAuthClientIdentifiers = l;
        }

        private void SetMetadata(Core2Metadata m,JToken jt)
        {
            JObject jo = (JObject)jt;

            if (jo.GetValue("created") != null)
            {
                string created = jo.GetValue("created").ToString();
                m.Created = DateTime.Parse(created);
            }

            if (jo.GetValue("lastModified") != null)
            {
                string lastModified = jo.GetValue("lastModified").ToString();
                m.LastModified = DateTime.Parse(lastModified);
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
