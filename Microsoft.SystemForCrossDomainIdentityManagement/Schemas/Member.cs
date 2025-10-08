//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.SCIM
{
    using System.Runtime.Serialization;

    [DataContract]
    public sealed class Member : MemberBase
    {

        public string GetDerivedTypeName()
        {
            if (TypeName != null) return TypeName;
            if (Reference == null) return null;
            // "https://example.com/v2/Users/2819c223-7f76-453a-919d-413861904646",
        
            int last = Reference.LastIndexOf('/');
            if (last < 1) return null;
            string sub1 = Reference.Substring(0, last);
            int first = sub1.LastIndexOf('/');
            if (first < 0) return null;
            return sub1.Substring(first + 1);
            
        }
    }
}