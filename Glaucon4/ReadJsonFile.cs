#region FileHeader
// Project: Glaucon4
// Filename:   ReadJsonFile.cs
// Last write: 4/30/2023 2:29:36 PM
// Creation:   4/24/2023 12:01:10 PM
// Copyright: E.H. Terwiel, 2021,2022, 2023, the Netherlands
// No part of this file may be copied in any form without written consent
// of the programmer, owner and/or copyrightholder.
// This is a C# rewrite of FRAME3DD by H. Gavin cs.
// See https://frame3dd.sourceforge.net/
#endregion FileHeader

using System;
using System.Collections;
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

namespace Terwiel.Glaucon
{
    public partial class Glaucon
    {
        private T ProcessJsonString<T>(string json, string jsonSchema)
        {
            Contract.Assert(jsonSchema != null, "Error: Json schema is null");
            var p = JToken.Parse(json, new JsonLoadSettings
            {
                CommentHandling = CommentHandling.Ignore,
                DuplicatePropertyNameHandling = DuplicatePropertyNameHandling.Ignore
            });


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

        private static T ReadJsonFile<T>(string jsonFileName, string jsonSchema)
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
                e.Data.Add("EM_ConfigError", jsonFileName);
                e.Data.Add("Line", e.LineNumber.ToString());
                e.Data.Add("Pos", e.LinePosition);
                throw;
            }

            catch (FileNotFoundException e)
            {
                e.Data.Add("EM_FileNotFound", e.FileName);
                throw;
            }
            catch (Exception e)
            {
                e.Data.Add("EM_Deserialize", jsonFileName);
                throw;
            }
        }

         /// <summary>
        /// Entering this method, the MainWindowViewModel already exists,
        /// as does the Configuration (in .Config),  but not the Supports.
        /// </summary>
        /// <param name="target"></param>
        private void ReadDefaultInput(Glaucon glaucon, string defInput)
        {
            var schema = JSchema.Parse(UnpackStringResource("DefaultInputSchema.json"));

            var json = File.ReadAllText(defInput);

            var model = JObject.Parse(json);
            var valid = model.IsValid(schema, out IList<string> messages); // properly validates
            if (!valid)
            {
                var e = new JsonReaderException();
                for (var i = 0; i < messages.Count; i++)
                {
                    e.Data.Add($"json{i}", messages[i]);
                    e.Data.Add($"file{i}", "Resources");
                }

                throw e;
            }

            JsonConvert.PopulateObject(json, glaucon);
        }

        private string UnpackStringResource(string fileName)
        {
            Assembly? ass = Assembly.GetExecutingAssembly();

            //Debug.Assert(  ass.GetManifestResourceNames()
                //.Any(x => x.EndsWith(fileName)),
                //$"Cannot find resource {fileName}");

            var resource = ((string[]) ass.GetManifestResourceNames())[0]; // .First(x => x.EndsWith(fileName));
            var stream = ass.GetManifestResourceStream(resource);
            Debug.Assert(stream != null, $"Cannot find resource {resource}");

            var reader = new StreamReader(stream);

            var rd = reader.ReadToEnd();
            return rd;
        }
    }
}
