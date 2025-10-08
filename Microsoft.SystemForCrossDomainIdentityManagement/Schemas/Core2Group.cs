//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.SCIM
{
    using System.Runtime.Serialization;

    [DataContract]
    public sealed class Core2Group : Core2GroupBase
    {
        public Core2Group()
            : base()
        {
        }

        public bool ContainsMemberById(string id)
        {
            if (Members == null) return false;
            foreach (Member m in Members)
            {
                if (m.Value == id) return true;
            }
            return false;
        }
    }
}