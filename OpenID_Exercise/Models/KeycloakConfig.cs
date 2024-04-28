using System;
namespace OpenID_Exercise.Models
{
    public class KeycloakConfig
    {
        public string Authority { get; set; }
        public string AuthorizationEndpoint { get; set; }
        public string TokenEndpoint { get; set; }
        public string UserInfoEndpoint { get; set; }
        public string CertsEndpoint { get; set; }
    }

}

