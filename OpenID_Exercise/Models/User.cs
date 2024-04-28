using System;
using Newtonsoft.Json;

namespace OpenID_Exercise.Models
{
    // {"sub":"21a87cfb-b3e9-4ee9-b885-7cfd8423ec4c","address":{},"email_verified":false,"preferred_username":"admin"}

    public class User
    {
        [JsonProperty("sub")]
        public string Sub { get; set; }

        [JsonProperty("address")]
        public int Address { get; set; }

        [JsonProperty("email_verified")]
        public string EmailVerified { get; set; }

        [JsonProperty("preferred_username")]
        public string PreferredUsername { get; set; }

      
    }
}

