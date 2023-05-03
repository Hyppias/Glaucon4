#region FileHeader
// Project: CoCa7
// Filename:   ReadJsonFile.cs
// Last write: 3/9/2023 5:18:34 PM
// Creation:   3/5/2023 12:43:22 AM
// Copyright: E.H. Terwiel, 2021,2022, 2023, the Netherlands
// No part of this file may be copied in any form without written consent
// of the programmer, owner and/or copyrightholder.
#endregion FileHeader

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.IO;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;

namespace Terwiel.Glaucon.Json
{
    public static partial class Json
    {
        public static T ProcessJsonString<T>(string json, string jsonSchema)
        {
            Contract.Assert(jsonSchema != null, "Error: Json schema is null");
            var p = JToken.Parse(json, new JsonLoadSettings
            {
                CommentHandling = CommentHandling.Ignore,
                DuplicatePropertyNameHandling = DuplicatePropertyNameHandling.Ignore
            });

            //IList<ValidationError> messageList;
            var schema = JSchema.Parse(jsonSchema);

            if (!p.IsValid(schema, out IList<ValidationError> messageList))
            {
                var ex = new JsonException("EM_JsonValidation");
                var i = 0;
                foreach (var ms in messageList)
                {
                    ex.Data.Add($"{i}", $"{ms.Message}, Line {ms.LineNumber}, Position {ms.LinePosition}");
                    i++;
                }

                throw ex;
            }

            var serializer = new JsonSerializer
            {
                MaxDepth = 4,
                Culture = new CultureInfo("en-US")
            };

           
            return serializer.Deserialize<T>(p.CreateReader());
        }

        public static T ReadJsonFile<T>(string jsonFileName, string jsonSchema)
        {
            Debug.WriteLine($"Enter {MethodBase.GetCurrentMethod()?.Name ?? string.Empty}");
            try
            {
                var json = File.ReadAllText(jsonFileName);
                //var id = new InputData();
                return JsonConvert.DeserializeObject<T>(json, new StringEnumConverter());
            }
            catch (JsonReaderException e)
            {
                e.Data.Add("Config", jsonFileName);
                e.Data.Add("Line", e.LineNumber.ToString());
                e.Data.Add("Pos", e.LinePosition);
                throw;
            }
          
            catch (FileNotFoundException e)
            {
                e.Data.Add("File",$"{e.FileName} not found");
                throw;
            }
            catch (Exception e)
            {
                e.Data.Add("JSON",$"{jsonFileName}: cannot deserialize.");
                throw;
            }
        }
    }
}
