// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using Newtonsoft.Json;

namespace Test
{
    [JsonObject(MemberSerialization.OptIn)]
    public class APIUser 
    {   
        [JsonProperty(@"username")]
        public string Username { get; set; } = string.Empty;
    }
}