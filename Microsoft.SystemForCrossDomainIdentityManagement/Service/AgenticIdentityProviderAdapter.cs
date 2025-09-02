// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.

namespace Microsoft.SCIM
{
    internal class AgenticIdentityProviderAdapter : ProviderAdapterTemplate<AgenticIdentity>
    {
        public AgenticIdentityProviderAdapter(IProvider provider)
            : base(provider)
        {
        }

        public override string SchemaIdentifier
        {
            get
            {
                return AgenticIdentitySchemaIdentifiers.AgenticIdentity;
            }
        }
    }
}
