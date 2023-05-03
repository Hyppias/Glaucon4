#region FileHeader
// Project: CoCa7
// Filename:   ReadDefaultInput.cs
// Last write: 3/9/2023 5:18:34 PM
// Creation:   3/5/2023 12:43:22 AM
// Copyright: E.H. Terwiel, 2021,2022, 2023, the Netherlands
// No part of this file may be copied in any form without written consent
// of the programmer, owner and/or copyrightholder.
#endregion FileHeader

using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;

namespace Terwiel.Glaucon.Json
{
    public static partial class Json
    {
        /// <summary>
        /// Entering this method, the MainWViewModel already exists,
        /// as does the Configuration (in .Config),  but not the Supports.
        /// </summary>
        /// <Param name="target"></Param>
        public static void ReadJsonFile(Glaucon glaucon, string defInput,string schemaResource)
        {
            var schema = JSchema.Parse(schemaResource);

            var json = File.ReadAllText(defInput);

            var model = JObject.Parse(json);
            var valid = model.IsValid(schema, out IList<string> messages); // properly validates
            if (!valid)
            {
                var e = new JsonReaderException();
                for (var i = 0; i < messages.Count; i++)
                {
                    e.Data.Add($"json{i}", messages[i]);
                    e.Data.Add($"file{i}", defInput);
                }

                throw e;
            }

            
            JsonConvert.PopulateObject(json, glaucon);
        }

    }
}
