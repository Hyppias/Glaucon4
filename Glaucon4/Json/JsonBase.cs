#region FileHeader
// Project: CoCa7
// Filename:   JsonBase.cs
// Last write: 3/9/2023 5:18:34 PM
// Creation:   3/5/2023 8:25:17 PM
// Copyright: E.H. Terwiel, 2021,2022, 2023, the Netherlands
// No part of this file may be copied in any form without written consent
// of the programmer, owner and/or copyrightholder.
#endregion FileHeader

using Newtonsoft.Json;

namespace Terwiel.Glaucon.Json
{
    [JsonObject(MemberSerialization.OptIn)]
    public class JsonBase 
    {

        [JsonProperty("Id")]
        public string? Id { get; set; } // ISO 3166-1 alpha-2 code

        [JsonProperty("Description")]
        public string? Description { get; set; }

        [JsonProperty("IsSelected")]
        public bool IsSelected { get; set; }

        [JsonProperty("Display")]
        public bool Display { get; set; }
    }
}
