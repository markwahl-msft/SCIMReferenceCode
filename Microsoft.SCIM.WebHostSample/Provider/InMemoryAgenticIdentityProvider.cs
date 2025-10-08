// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.

namespace Microsoft.SCIM.WebHostSample.Provider
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Net;
    using System.Text;
    using System.Threading.Tasks;
    using System.Web.Http;
    using Microsoft.SCIM;

    public class InMemoryAgenticIdentityProvider : ProviderBase
    {
        private readonly InMemoryStorage storage;

        public InMemoryAgenticIdentityProvider(string contentRootPath)
        {
            this.storage = InMemoryStorage.Instance;
            this.storage.LoadAgenticIdentities(contentRootPath);
        }

        public String ToHtml()
        {
            StringBuilder sb = new StringBuilder();
            IEnumerable<AgenticIdentity> exisitingAgenticIdentities = this.storage.AgenticIdentities.Values;
            foreach (var ai in exisitingAgenticIdentities)
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

            AgenticIdentity agenticIdentity = resource as AgenticIdentity;

            if (string.IsNullOrWhiteSpace(agenticIdentity.DisplayName))  // NYI agent identities without display names
            {
                throw new HttpResponseException(HttpStatusCode.BadRequest);
            }

            IEnumerable<AgenticIdentity> exisitingAgenticIdentities = this.storage.AgenticIdentities.Values;
            if
            (
                exisitingAgenticIdentities.Any(
                    (AgenticIdentity exisitingAgenticIdentity) =>
                        string.Equals(exisitingAgenticIdentity.DisplayName, agenticIdentity.DisplayName, StringComparison.Ordinal)) //vuniqueness NYI
            )
            {
                throw new HttpResponseException(HttpStatusCode.Conflict);
            }
            //Update Metadata
            DateTime created = DateTime.UtcNow;
            agenticIdentity.Metadata.Created = created;
            agenticIdentity.Metadata.LastModified = created;

            // update schemas and active
            if (agenticIdentity.Schemas == null || agenticIdentity.Schemas.Count == 0)
            {
                agenticIdentity.AddSchema(AgenticIdentitySchemaIdentifiers.AgenticIdentity);

            }
            agenticIdentity.Active = true; // NYI for now assume created as active

            string resourceIdentifier = Guid.NewGuid().ToString();
            resource.Identifier = resourceIdentifier;
            this.storage.AgenticIdentities.Add(resourceIdentifier, agenticIdentity);

            this.storage.SaveAgenticIdentities();

            return Task.FromResult(resource);
        }

        public override Task DeleteAsync(IResourceIdentifier resourceIdentifier, string correlationIdentifier)
        {
            if (string.IsNullOrWhiteSpace(resourceIdentifier?.Identifier))
            {
                throw new HttpResponseException(HttpStatusCode.BadRequest);
            }

            string identifier = resourceIdentifier.Identifier;

            if (this.storage.AgenticIdentities.ContainsKey(identifier))
            {
                this.storage.AgenticIdentities.Remove(identifier);
            }

            this.storage.SaveAgenticIdentities();

            return Task.CompletedTask;
        }

        // currently allows only displayname search and not externalId (NYI)
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

            var predicate = PredicateBuilder.False<AgenticIdentity>();
            Expression<Func<AgenticIdentity, bool>> predicateAnd;
            predicateAnd = PredicateBuilder.True<AgenticIdentity>();

            if (queryFilter == null)
            {
                results = this.storage.AgenticIdentities.Values.Select(
                    (AgenticIdentity ai) => ai as Resource);
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
            results = this.storage.AgenticIdentities.Values.Where(predicate.Compile());

            return Task.FromResult(results.ToArray());
        }

        public override Task<Resource> ReplaceAsync(Resource resource, string correlationIdentifier)
        {
            if (resource.Identifier == null)
            {
                throw new HttpResponseException(HttpStatusCode.BadRequest);
            }

            AgenticIdentity agenticIdentity = resource as AgenticIdentity;

            if (string.IsNullOrWhiteSpace(agenticIdentity.DisplayName))
            {
                throw new HttpResponseException(HttpStatusCode.BadRequest);
            }

            AgenticIdentity exisitingAgenticIdentities = resource as AgenticIdentity;
            if
            (
                this.storage.AgenticIdentities.Values.Any(
                    (AgenticIdentity exisitingUser) =>
                        string.Equals(exisitingUser.DisplayName, agenticIdentity.DisplayName, StringComparison.Ordinal) &&
                        !string.Equals(exisitingUser.Identifier, agenticIdentity.Identifier, StringComparison.OrdinalIgnoreCase))
            )
            {
                throw new HttpResponseException(HttpStatusCode.Conflict);
            } // XXX display name uniqueness

            if (!this.storage.AgenticIdentities.TryGetValue(agenticIdentity.Identifier, out AgenticIdentity _))
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            // Update metadata
            agenticIdentity.Metadata.Created = exisitingAgenticIdentities.Metadata.Created;
            agenticIdentity.Metadata.LastModified = DateTime.UtcNow;

            this.storage.AgenticIdentities[agenticIdentity.Identifier] = agenticIdentity;

            this.storage.SaveAgenticIdentities();

            Resource result = agenticIdentity as Resource;
            return Task.FromResult(result);
        }

        public AgenticIdentity GetById(string identifier)
        {

            if (this.storage.AgenticIdentities.ContainsKey(identifier))
            {
                if (this.storage.AgenticIdentities.TryGetValue(identifier, out AgenticIdentity agenticIdentity))
                {
                    
                    return agenticIdentity;
                }
            }
            return null;
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

            if (this.storage.AgenticIdentities.ContainsKey(identifier))
            {
                if (this.storage.AgenticIdentities.TryGetValue(identifier, out AgenticIdentity agenticIdentity))
                {
                    Resource result = agenticIdentity as Resource;
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

            if (this.storage.AgenticIdentities.TryGetValue(patch.ResourceIdentifier.Identifier, out AgenticIdentity agenticIdentity))
            {

                // XXX not yet agenticIdentity.Apply(patchRequest);
                // Update metadata
                agenticIdentity.Metadata.LastModified = DateTime.UtcNow;
                throw new ArgumentException(SystemForCrossDomainIdentityManagementServiceResources.ExceptionInvalidOperation); // XXX see above
            }
            else
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            this.storage.SaveAgenticIdentities();

            return Task.CompletedTask;
        }

        public bool CheckReferenceValidity(string id)
        {
            if (this.storage.AgenticIdentities.TryGetValue(id, out AgenticIdentity agenticIdentity))
            {
                return true;
            }
            throw new HttpResponseException(HttpStatusCode.NotAcceptable);
            //return false;
        }

        public void AddRemoveReferenced(Core2Group group, String id, bool isAdd)
        {
            if (this.storage.AgenticIdentities.TryGetValue(id, out AgenticIdentity agenticIdentity))
            {
                agenticIdentity.UpdateReferencesToGroup(group, isAdd);
                agenticIdentity.Metadata.LastModified = DateTime.UtcNow;
                this.storage.SaveAgenticIdentities();
            } else if (isAdd)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }
        }
    }
}