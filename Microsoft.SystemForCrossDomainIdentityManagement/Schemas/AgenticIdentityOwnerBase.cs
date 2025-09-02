﻿//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.SCIM
{
    using System.Runtime.Serialization;

    [DataContract]
    public abstract class AgenticIdentityOwnerBase
    {
        internal AgenticIdentityOwnerBase()
        {
        }

        [DataMember(Name = AttributeNames.Type, IsRequired = false, EmitDefaultValue = false)]
        public string TypeName
        {
            get;
            set;
        }

        [DataMember(Name = AttributeNames.Value)]
        public string Value
        {
            get;
            set;
        }
    }
}