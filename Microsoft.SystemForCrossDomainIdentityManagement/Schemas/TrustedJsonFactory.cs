//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.SCIM
{
    using System.Collections.Generic;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    public class TrustedJsonFactory : JsonFactory
    {
        public override Dictionary<string, object> Create(string json)
        {
            object res = JsonConvert.DeserializeObject(
                    json);
            if (res == null) return null;
            if (res is JObject)
            {
                JObject jo = (JObject)res;
                return jo.ToObject<Dictionary<string,object>>();
            }
            Dictionary<string, object> result =
                (Dictionary<string, object>)res;
            return result;
        }

        public override string Create(string[] json)
        {
            string result = JsonConvert.SerializeObject(json);
            return result;
        }

        public override string Create(Dictionary<string, object> json)
        {
            string result = JsonConvert.SerializeObject(json);
            return result;
        }

        public override string Create(IDictionary<string, object> json)
        {
            string result = JsonConvert.SerializeObject(json);
            return result;
        }

        public override string Create(IReadOnlyDictionary<string, object> json)
        {
            string result = JsonConvert.SerializeObject(json);
            return result;
        }
    }
}
