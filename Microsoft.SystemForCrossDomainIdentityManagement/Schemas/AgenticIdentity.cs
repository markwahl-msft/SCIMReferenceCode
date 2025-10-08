//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.SCIM
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Runtime.Serialization;

    [DataContract]
    public sealed class AgenticIdentity : AgenticIdentityBase
    {
        private IDictionary<string, IDictionary<string, object>> customExtension;

        public AgenticIdentity()
            : base()
        {
            this.AddSchema(AgenticIdentitySchemaIdentifiers.AgenticIdentity);
            this.Metadata =
                new Core2Metadata()
                {
                    ResourceType = AgenticIdentityTypes.AgenticIdentity
                };
            this.OnInitialization();
        }


        public IReadOnlyDictionary<string, IDictionary<string, object>> CustomExtension
        {
            get
            {
                return new ReadOnlyDictionary<string, IDictionary<string, object>>(this.customExtension);
            }
        }


        public void AddCustomAttribute(string key, object value)
        {
            if
            (
                    key != null
                && key.StartsWith(SchemaIdentifiers.PrefixExtension, StringComparison.OrdinalIgnoreCase)
                && value is Dictionary<string, object> nestedObject
            )
            {
                this.customExtension.Add(key, nestedObject);
            }
        }

        [OnDeserializing]
        private void OnDeserializing(StreamingContext context)
        {
            this.OnInitialization();
        }

        private void OnInitialization()
        {
            this.customExtension = new Dictionary<string, IDictionary<string, object>>();
        }

        public override Dictionary<string, object> ToJson()
        {
            Dictionary<string, object> result = base.ToJson();

            foreach (KeyValuePair<string, IDictionary<string, object>> entry in this.CustomExtension)
            {
                result.Add(entry.Key, entry.Value);
            }

            return result;
        }

        public void UpdateReferencesToGroup(Core2Group group, bool isAdd)
        {
            if (Groups == null && isAdd == false) { return; }
            if (Groups == null)
            {
                Groups = new List<GroupAttributeValue>();
            }
            if (isAdd == false)
            {
                List<GroupAttributeValue> newGroups = new List<GroupAttributeValue>();
                foreach (GroupAttributeValue gav in Groups)
                {
                    if (gav.Value == group.Identifier)
                    {
                        // value to remove
                    } else
                    {
                        newGroups.Add(gav);
                    }
                }
                Groups = newGroups;
            } else
            {
                foreach (GroupAttributeValue gav in Groups)
                {
                    if (gav.Value == group.Identifier)
                    {
                        // already present
                        return;
                    }
                }
                GroupAttributeValue nv = new GroupAttributeValue();
                nv.Value = group.Identifier;
                nv.Display = group.DisplayName;
                Groups = Groups.Append(nv);
            }

        }
    }
}
