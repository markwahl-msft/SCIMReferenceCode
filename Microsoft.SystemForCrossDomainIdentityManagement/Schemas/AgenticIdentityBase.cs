//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.SCIM
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    [DataContract]
    public abstract class AgenticIdentityBase : Resource
    {
        [DataMember(Name = AttributeNames.DisplayName)]
        public virtual string DisplayName
        {
            get;
            set;
        }

        [DataMember(Name = AttributeNames.Active)]
        public virtual bool Active
        {
            get;
            set;
        }


        [DataMember(Name = AttributeNames.Description)]
        public virtual string Description
        {
            get;
            set;
        }


        [DataMember(Name = AgenticIdentityAttributeNames.AgenticApplicationId)]
        public virtual string AgenticApplicationId
        {
            get;
            set;
        }

        [DataMember(Name = AgenticIdentityAttributeNames.Owners, IsRequired = false, EmitDefaultValue = false)]
        public virtual IEnumerable<AgenticIdentityOwner> Owners
        {
            get;
            set;
        }

        [DataMember(Name = AgenticIdentityAttributeNames.OAuthClientIdentifiers, IsRequired = false, EmitDefaultValue = false)]
        public virtual IEnumerable<AgenticIdentityOAuthClientIdentifier> OAuthClientIdentifiers
        {
            get;
            set;
        }

        [DataMember(Name = AttributeNames.Metadata)]
        public virtual Core2Metadata Metadata
        {
            get;
            set;
        }

        [DataMember(Name = AttributeNames.Roles, IsRequired = false, EmitDefaultValue = false)]
        public virtual IEnumerable<Role> Roles
        {
            get;
            set;
        }

        [DataMember(Name = AttributeNames.Entitlements, IsRequired = false, EmitDefaultValue = false)]
        public virtual IEnumerable<Entitlement> Entitlements
        {
            get;
            set;
        }

        [DataMember(Name = AttributeNames.Groups, IsRequired = false, EmitDefaultValue = false)]
        public virtual IEnumerable<GroupAttributeValue> Groups
        {
            get;
            set;
        }

    }
}


