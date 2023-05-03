#region FileHeader
// Project: CoCa7
// Filename:   WriteJson.cs
// Last write: 3/9/2023 5:18:34 PM
// Creation:   3/5/2023 12:43:22 AM
// Copyright: E.H. Terwiel, 2021,2022, 2023, the Netherlands
// No part of this file may be copied in any form without written consent
// of the programmer, owner and/or copyrightholder.
#endregion FileHeader

using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Terwiel.Glaucon.Json
{
    public static partial class Json
    {
        public static string WriteJsonString<T>(T source)
        {
            Debug.WriteLine("Enter " + MethodBase.GetCurrentMethod().Name);

            var cc = Thread.CurrentThread.CurrentCulture;

            // Must set to en-US, to read in the configuration data file,
            // which must follow this culture in decimal formats etc.
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");

            try
            {

                return JsonConvert.SerializeObject((source,Formatting.Indented,new []{ new StringEnumConverter()}));
            }
            catch (JsonWriterException e)
            {
                e.Data.Add("EM_JsonError", e.Path);
                throw;
            }

            catch (Exception e)
            {
                e.Data.Add("EM_Serialize", "Calculation");
                throw;
            }
            finally
            {
                Thread.CurrentThread.CurrentCulture = cc;
                Debug.WriteLine("Exit " + MethodBase.GetCurrentMethod().Name);
            }
        }

        public static void WriteJsonFile<T>(string jsonFileName, T source)
        {
            Debug.WriteLine("Enter " + MethodBase.GetCurrentMethod().Name);

            var cc = Thread.CurrentThread.CurrentCulture;

            // Must set to en-US, to read in the configuration data file,
            // which must follow this culture in decimal formats etc.
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");

            try
            {
                using var file = File.CreateText(jsonFileName);
                var serializer = new JsonSerializer();
                serializer.Serialize(file, source);
            }
            catch (JsonWriterException e)
            {
                e.Data.Add("EM_JsonError", e.Path);
                throw;
            }

            catch (Exception e)
            {
                e.Data.Add("EM_Serialize", jsonFileName);
                throw;
            }
            finally
            {
                Thread.CurrentThread.CurrentCulture = cc;
                Debug.WriteLine("Exit " + MethodBase.GetCurrentMethod().Name);
            }
        }
    }
}
