//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.SCIM
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    [DataContract]
    public abstract class AgenticIdentityOAuthClientIdentifierBase
    {
        internal AgenticIdentityOAuthClientIdentifierBase()
        {
        }

        [DataMember(Name = "clientId", IsRequired = false, EmitDefaultValue = false)]
        public string ClientId
        {
            get;
            set;
        }

        [DataMember(Name = "description", IsRequired = false, EmitDefaultValue = false)]
        public string Description
        {
            get;
            set;
        }

        [DataMember(Name = "issuer", IsRequired = false, EmitDefaultValue = false)]
        public string Issuer
        {
            get;
            set;
        }
        [DataMember(Name = "name", IsRequired = false, EmitDefaultValue = false)]
        public string Name
        {
            get;
            set;
        }
        [DataMember(Name = "subject", IsRequired = false, EmitDefaultValue = false)]
        public string Subject
        {
            get;
            set;
        }

        [DataMember(Name = "audiences", IsRequired = false, EmitDefaultValue = false)]
        public virtual IEnumerable<string> Audiences
        {
            get;
            set;
        }

    }
}