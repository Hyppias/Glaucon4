#region FileHeader
// Project: Glaucon4
// Filename:   WriteJson.cs
// Last write: 4/30/2023 2:29:51 PM
// Creation:   4/24/2023 12:01:10 PM
// Copyright: E.H. Terwiel, 2021,2022, 2023, the Netherlands
// No part of this file may be copied in any form without written consent
// of the programmer, owner and/or copyrightholder.
// This is a C# rewrite of FRAME3DD by H. Gavin cs.
// See https://frame3dd.sourceforge.net/
#endregion FileHeader

using System.IO;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Threading;
using System;

namespace Terwiel.Glaucon
{
    public partial class Glaucon
    {
        private string WriteJsonString<T>(T source)
        {
            Debug.WriteLine("Enter " + MethodBase.GetCurrentMethod().Name);

            var cc = Thread.CurrentThread.CurrentCulture;

            // Must set to en-US, to read in the configuration data file,
            // which must follow this culture in decimal formats etc.
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");

            try
            {

                return JsonConvert.SerializeObject((source, Formatting.Indented, new[] { new StringEnumConverter() }));
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

        private void WriteJsonFile<T>(string jsonFileName, T source)
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

