// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.

namespace Microsoft.SCIM.WebHostSample.Resources
{
    public static class SampleAgenticIdentityAttributes
    {
        public static AttributeScheme DisplayNameAttributeScheme
        {
            get
            {
                AttributeScheme groupDisplayScheme = new AttributeScheme("displayName", AttributeDataType.@string, false)
                {
                    Description = SampleConstants.DescriptionGroupDisplayName,
                    Required = true,
                    Uniqueness = Uniqueness.server // XXX check this
                };

                return groupDisplayScheme;
            }
        }

        // XXX add other attributes

        public static AttributeScheme OwnersAttributeScheme
        {
            get
            {
                AttributeScheme membersScheme = new AttributeScheme("owners", AttributeDataType.complex, true)
                {
                    Description = "owners responsible for an agentic identity" // XXX  SampleConstants.DescriptionOwners
                };
                membersScheme.AddSubAttribute(SampleMultivaluedAttributes.ValueSubAttributeScheme);
                membersScheme.AddSubAttribute(SampleMultivaluedAttributes.TypeSubAttributeScheme);

                return membersScheme;
            }
        }

        public static AttributeScheme oAuthClientIdentifiersAttributeScheme
        {
            get
            {
                AttributeScheme membersScheme = new AttributeScheme("oAuthClientIdentifiers", AttributeDataType.complex, true)
                {
                    Description = "oAuth Client Identifiers of an agentic identity" // XXX  SampleConstants.DescriptionOwners
                };
                // XXX add sub attributes

                return membersScheme;
            }
        }
    }
}
