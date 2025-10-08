// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.

namespace Microsoft.SCIM.WebHostSample.Provider
{
    using System;
    using System.Text;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Net;
    using System.Threading.Tasks;
    using System.Web.Http;
    using Microsoft.SCIM;

    public class InMemoryGroupProvider : ProviderBase
    {
        private readonly InMemoryStorage storage;
        private InMemoryAgenticIdentityProvider inMemoryAgenticIdentityProvider;
        

        public InMemoryGroupProvider(string contentRootPath, InMemoryAgenticIdentityProvider imaip)
        {
            this.storage = InMemoryStorage.Instance;
            this.storage.LoadGroups(contentRootPath);
            inMemoryAgenticIdentityProvider = imaip;
        }

        public String ToHtml()
        {
            StringBuilder sb = new StringBuilder();
            IEnumerable<Core2Group> gl = this.storage.Groups.Values;
            foreach (var ai in gl)
            {
                sb.Append(ai.ToString());
                sb.AppendLine("");
            }
            return sb.ToString();
        }

        public override Task<Resource> CreateAsync(Resource resource, string correlationIdentifier)
        {
            if (resource.Identifier != null)
            {
                throw new HttpResponseException(HttpStatusCode.BadRequest);
            }

            Core2Group group = resource as Core2Group;

            if (string.IsNullOrWhiteSpace(group.DisplayName))
            {
                throw new HttpResponseException(HttpStatusCode.BadRequest);
            }

            IEnumerable<Core2Group> exisitingGroups = this.storage.Groups.Values;
            if
            (
                exisitingGroups.Any(
                    (Core2Group exisitingGroup) =>
                        string.Equals(exisitingGroup.DisplayName, group.DisplayName, StringComparison.Ordinal))
            )
            {
                throw new HttpResponseException(HttpStatusCode.Conflict);
            }
            //Update Metadata
            DateTime created = DateTime.UtcNow;
            group.Metadata.Created = created;
            group.Metadata.LastModified = created;

            string resourceIdentifier = Guid.NewGuid().ToString();
            resource.Identifier = resourceIdentifier;
            this.storage.Groups.Add(resourceIdentifier, group);

            this.storage.SaveGroups();

            updateReferenced(group);

            return Task.FromResult(resource);
        }

        public override Task DeleteAsync(IResourceIdentifier resourceIdentifier, string correlationIdentifier)
        {
            if (string.IsNullOrWhiteSpace(resourceIdentifier?.Identifier))
            {
                throw new HttpResponseException(HttpStatusCode.BadRequest);
            }

            string identifier = resourceIdentifier.Identifier;

            if (this.storage.Groups.ContainsKey(identifier))
            {
                if (this.storage.Groups.TryGetValue(identifier, out Core2Group groupToBeDeleted))
                {
                    removeReferenced(groupToBeDeleted);
                }
                this.storage.Groups.Remove(identifier);
            }

            this.storage.SaveGroups();

            return Task.CompletedTask;
        }

        public Core2Group GetByName(string name)
        {
            foreach (Core2Group g in storage.Groups.Values)
            {
                if (g.DisplayName.Equals(name)) { return g; }
            }
            return null;
        }

        public override Task<Resource[]> QueryAsync(IQueryParameters parameters, string correlationIdentifier)
        {
            if (parameters == null)
            {
                throw new ArgumentNullException(nameof(parameters));
            }

            if (string.IsNullOrWhiteSpace(correlationIdentifier))
            {
                throw new ArgumentNullException(nameof(correlationIdentifier));
            }

            if (null == parameters.AlternateFilters)
            {
                throw new ArgumentException(SystemForCrossDomainIdentityManagementServiceResources.ExceptionInvalidParameters);
            }

            if (string.IsNullOrWhiteSpace(parameters.SchemaIdentifier))
            {
                throw new ArgumentException(SystemForCrossDomainIdentityManagementServiceResources.ExceptionInvalidParameters);
            }

            IEnumerable<Resource> results;
            IFilter queryFilter = parameters.AlternateFilters.SingleOrDefault();

            var predicate = PredicateBuilder.False<Core2Group>();
            Expression<Func<Core2Group, bool>> predicateAnd;
            predicateAnd = PredicateBuilder.True<Core2Group>();

            if (queryFilter == null)
            {
                results = this.storage.Groups.Values.Select(
                    (Core2Group user) => user as Resource);
            }
            else
            {
                if (string.IsNullOrWhiteSpace(queryFilter.AttributePath))
                {
                    throw new ArgumentException(SystemForCrossDomainIdentityManagementServiceResources.ExceptionInvalidParameters);
                }

                if (string.IsNullOrWhiteSpace(queryFilter.ComparisonValue))
                {
                    throw new ArgumentException(SystemForCrossDomainIdentityManagementServiceResources.ExceptionInvalidParameters);
                }

                if (queryFilter.FilterOperator != ComparisonOperator.Equals)
                {
                    throw new NotSupportedException(string.Format(SystemForCrossDomainIdentityManagementServiceResources.ExceptionFilterOperatorNotSupportedTemplate, queryFilter.FilterOperator));
                }


                if (queryFilter.AttributePath.Equals(AttributeNames.DisplayName))
                {
                    
                    string displayName = queryFilter.ComparisonValue;
                    predicateAnd = predicateAnd.And(p => string.Equals(p.DisplayName, displayName, StringComparison.OrdinalIgnoreCase));
                  
                }
                else
                {
                    throw new NotSupportedException(string.Format(SystemForCrossDomainIdentityManagementServiceResources.ExceptionFilterAttributePathNotSupportedTemplate, queryFilter.AttributePath));
                }
            }
            
            predicate = predicate.Or(predicateAnd);
            results = this.storage.Groups.Values.Where(predicate.Compile());

            return Task.FromResult(results.ToArray());
        }

        public override Task<Resource> ReplaceAsync(Resource resource, string correlationIdentifier)
        {
            if (resource.Identifier == null)
            {
                throw new HttpResponseException(HttpStatusCode.BadRequest);
            }

            Core2Group group = resource as Core2Group;

            if (string.IsNullOrWhiteSpace(group.DisplayName))
            {
                throw new HttpResponseException(HttpStatusCode.BadRequest);
            }

            Core2Group exisitingGroups = resource as Core2Group;
            if
            (
                this.storage.Groups.Values.Any(
                    (Core2Group exisitingUser) =>
                        string.Equals(exisitingUser.DisplayName, group.DisplayName, StringComparison.Ordinal) &&
                        !string.Equals(exisitingUser.Identifier, group.Identifier, StringComparison.OrdinalIgnoreCase))
            )
            {
                throw new HttpResponseException(HttpStatusCode.Conflict);
            }

            if (!this.storage.Groups.TryGetValue(group.Identifier, out Core2Group _))
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            if (this.storage.Groups.TryGetValue(group.Identifier, out Core2Group groupToBeReplaced))
            {
                removeReferenced(groupToBeReplaced);
            }

            // Update metadata
            group.Metadata.Created = exisitingGroups.Metadata.Created;
            group.Metadata.LastModified = DateTime.UtcNow;

            this.storage.Groups[group.Identifier] = group;
            Resource result = group as Resource;

            this.storage.SaveGroups();

            updateReferenced(group);


            return Task.FromResult(result);
        }

        public override Task<Resource> RetrieveAsync(IResourceRetrievalParameters parameters, string correlationIdentifier)
        {
            if (parameters == null)
            {
                throw new ArgumentNullException(nameof(parameters));
            }

            if (string.IsNullOrWhiteSpace(correlationIdentifier))
            {
                throw new ArgumentNullException(nameof(correlationIdentifier));
            }

            if (string.IsNullOrEmpty(parameters?.ResourceIdentifier?.Identifier))
            {
                throw new ArgumentNullException(nameof(parameters));
            }

            string identifier = parameters.ResourceIdentifier.Identifier;

            if (this.storage.Groups.ContainsKey(identifier))
            {
                if (this.storage.Groups.TryGetValue(identifier, out Core2Group group))
                {
                    Resource result = group as Resource;
                    return Task.FromResult(result);
                }
            }

            throw new HttpResponseException(HttpStatusCode.NotFound);
        }

        public override Task UpdateAsync(IPatch patch, string correlationIdentifier)
        {
            if (null == patch)
            {
                throw new ArgumentNullException(nameof(patch));
            }

            if (null == patch.ResourceIdentifier)
            {
                throw new ArgumentException(SystemForCrossDomainIdentityManagementServiceResources.ExceptionInvalidOperation);
            }

            if (string.IsNullOrWhiteSpace(patch.ResourceIdentifier.Identifier))
            {
                throw new ArgumentException(SystemForCrossDomainIdentityManagementServiceResources.ExceptionInvalidOperation);
            }

            if (null == patch.PatchRequest)
            {
                throw new ArgumentException(SystemForCrossDomainIdentityManagementServiceResources.ExceptionInvalidOperation);
            }

            PatchRequest2 patchRequest =
                patch.PatchRequest as PatchRequest2;

            if (null == patchRequest)
            {
                string unsupportedPatchTypeName = patch.GetType().FullName;
                throw new NotSupportedException(unsupportedPatchTypeName);
            }

            if (this.storage.Groups.TryGetValue(patch.ResourceIdentifier.Identifier, out Core2Group group))
            {
                removeReferenced(group);
                if (group.Apply(patchRequest,this) == false) {
                    throw new HttpResponseException(HttpStatusCode.NotAcceptable);
                }
                // Update metadata
                group.Metadata.LastModified = DateTime.UtcNow;
            }
            else
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            updateReferenced(group);

            this.storage.SaveGroups();

            return Task.CompletedTask;
        }

        private void addRemoveReferenced(Core2Group group,Member member,bool isAdd)
        {
            if (member == null)
            {
                if (isAdd) throw new HttpResponseException(HttpStatusCode.InternalServerError);
                return;
            }
            string typename = member.GetDerivedTypeName();
            if (typename == null)
            {
                //if (isAdd) throw new HttpResponseException(HttpStatusCode.InternalServerError);
                return;
            }

            if (member.Value == null || member.Value.Length == 0)
            {
                if (isAdd) throw new HttpResponseException(HttpStatusCode.InternalServerError);
                return;
            }
            if (typename.Equals(Types.Group, StringComparison.InvariantCultureIgnoreCase))
            {
                // XXX
            }
            else if (typename.Equals(Types.User, StringComparison.InvariantCultureIgnoreCase))
            {
                // NYI
            }
            else if (typename.Equals(AgenticIdentityTypes.AgenticIdentity, StringComparison.InvariantCultureIgnoreCase))
            {
                inMemoryAgenticIdentityProvider.AddRemoveReferenced(group, member.Value, isAdd);
            }
            else
            {
                throw new HttpResponseException(HttpStatusCode.InternalServerError);
            }
        }

        private void removeReferenced(Core2Group group)
        {
            if (group == null || group.Members == null)
            {
                return;
            }
            foreach (var member in group.Members)
            {
                addRemoveReferenced(group,member,false);
            }
        }

        private void updateReferenced(Core2Group group)
        {
            if (group == null || group.Members == null)
            {
                return;
            }
            foreach (var member in group.Members)
            {
                addRemoveReferenced(group,member,true);
            }
        }

        public bool CheckReferenceValidity(string id)
        {
            if (this.storage.Groups.TryGetValue(id, out Core2Group group))
            {
                return true;
            }
            return false;
        }

        public override bool CheckReferenceValidity(Member member)
        {
            string typename = member.GetDerivedTypeName();

            if (member == null || typename == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotAcceptable);
                // return false;
            }
            if (member.Value == null || member.Value.Length == 0)
            {
                throw new HttpResponseException(HttpStatusCode.NotAcceptable);
            }
            if (typename.Equals(Types.Group, StringComparison.InvariantCultureIgnoreCase))
            {
                return CheckReferenceValidity(member.Value);
            }
            else if (typename.Equals(Types.User, StringComparison.InvariantCultureIgnoreCase))
            {
                return true; // NYI
            }
            else if (typename.Equals(AgenticIdentityTypes.AgenticIdentity, StringComparison.InvariantCultureIgnoreCase))
            {
                return inMemoryAgenticIdentityProvider.CheckReferenceValidity(member.Value);
            }
            else
            {
                throw new HttpResponseException(HttpStatusCode.NotAcceptable);
                // return false; 
            }
        }
    }
}
