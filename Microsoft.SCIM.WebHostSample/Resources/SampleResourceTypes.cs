// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.

namespace Microsoft.SCIM.WebHostSample.Resources
{
    using System;

    public class SampleResourceTypes
    {
        public static Core2ResourceType UserResourceType
        {
            get
            {
                Core2ResourceType userResource = new Core2ResourceType
                {
                    Identifier = Types.User,
                    Endpoint = new Uri($"{SampleConstants.SampleScimEndpoint}/Users"),
                    Schema = SampleConstants.UserEnterpriseSchema
                };

                return userResource;
            }
        }

        public static Core2ResourceType GroupResourceType
        {
            get
            {
                Core2ResourceType groupResource = new Core2ResourceType
                {
                    Identifier = Types.Group,
                    Endpoint = new Uri($"{SampleConstants.SampleScimEndpoint}/Groups"),
                    Schema = $"{SampleConstants.Core2SchemaPrefix}{Types.Group}"
                };

                return groupResource;
            }
        }

        public static Core2ResourceType AgenticIdentityResourceType
        {
            get
            {
                Core2ResourceType agenticIdentityResource = new Core2ResourceType
                {
                    Identifier = AgenticIdentityTypes.AgenticIdentity,
                    Endpoint = new Uri($"{SampleConstants.SampleScimEndpoint}/AgenticIdentities"),
                    Schema = $"{SampleConstants.Core2SchemaPrefix}{AgenticIdentityTypes.AgenticIdentity}"
                };

                return agenticIdentityResource;
            }
        }
    }
}
