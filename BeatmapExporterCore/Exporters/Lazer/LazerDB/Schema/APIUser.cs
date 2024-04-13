// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.

#nullable disable

using Newtonsoft.Json;

namespace BeatmapExporterCore.Exporters.Lazer.LazerDB.Schema
{
    [JsonObject(MemberSerialization.OptIn)]
    public class APIUser 
    {   
        [JsonProperty(@"username")]
        public string Username { get; set; } = string.Empty;
    }
}