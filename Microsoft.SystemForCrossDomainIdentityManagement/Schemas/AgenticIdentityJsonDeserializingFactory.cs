//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.SCIM
{
    using System;
    using System.Collections.Generic;

    public sealed class AgenticIdentityJsonDeserializingFactory : JsonDeserializingFactory<AgenticIdentity>
    {
        public override AgenticIdentity Create(IReadOnlyDictionary<string, object> json)
        {
            if (null == json)
            {
                throw new ArgumentNullException(nameof(json));
            }

            AgenticIdentity result = base.Create(json);

            foreach (KeyValuePair<string, object> entry in json)
            {
                
                if
                (
                        entry.Key.StartsWith(SchemaIdentifiers.PrefixExtension, StringComparison.OrdinalIgnoreCase)
                    && entry.Value is Dictionary<string, object> nestedObject
                )
                {
                    result.AddCustomAttribute(entry.Key, nestedObject);
                }
                
            }

            return result;
        }
    }
}