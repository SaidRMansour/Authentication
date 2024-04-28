using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenID_Exercise.Models;
using static System.Net.WebRequestMethods;

namespace OpenID_Exercise.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private static readonly Dictionary<string, string> _cache = new();
    private readonly KeycloakConfig _keycloakConfig;

    public HomeController(ILogger<HomeController> logger, IOptions<KeycloakConfig> keycloakConfig)
    {
        _logger = logger;
        _keycloakConfig = keycloakConfig.Value;

    }

    // GET: /
    public IActionResult Index()
    {
        return View();
    }

    // GET: /login
    [HttpGet("/login")]
    public IActionResult Login()
    {
        var clientId = "said-client";
        var redirectUri = "http://localhost:5000/callback";
        var scope = "openid email phone address profile";
        var state = RandomString();
        var codeVerifier = RandomString();
        _cache.Add(state, codeVerifier);
        Console.WriteLine(_cache.First());
        // HttpContext.Session.SetString(state, codeVerifier);
        var codeChallenge = CreateCodeChallenge(codeVerifier);

        var authorizationUri = $"{_keycloakConfig.AuthorizationEndpoint}" +
                    $"?client_id={clientId}" +
                    $"&scope={scope}" +
                    $"&response_type=code" +
                    $"&redirect_uri={redirectUri}" +
                    $"&prompt=login" +
                    $"&state={state}" +
                    $"&code_challenge_method=plain" + // evt. "plain" el. "S256"
                    $"&code_challenge={codeChallenge}";
        
        
        return Redirect(authorizationUri);
    }

    // GET: /callback

    [HttpGet("/callback")]
    public async Task<IActionResult> Callback(string state, string code)
    {
        var fullRequestUrl = $"{Request.Scheme}://{Request.Host}{Request.Path}{Request.QueryString}";
        _logger.LogInformation("Received callback with URL: {Url}", fullRequestUrl);

        
        if (!_cache.TryGetValue(state, out var codeVerifier))
        {
            _logger.LogWarning("No verifier found in cache for state: {State}", state);
            return BadRequest("Session expired or invalid state.");
        }

        var clientId = "said-client";
        var clientSecret = "nnfBfwyU8PRPEer2uIM9RBWiWqxpm5T6";
        var redirectUri = "http://localhost:5000/callback";

        string codeChallenge = CreateCodeChallenge(codeVerifier);
        
        var parameters = new Dictionary<string, string>
        {
            { "grant_type", "authorization_code" },
            { "code", code },
            { "redirect_uri", redirectUri },
            { "code_verifier", codeChallenge },
            { "client_id", clientId },
            { "client_secret", clientSecret }
        };

        using (var client = new HttpClient())
        {
            var response = await client.PostAsync(_keycloakConfig.TokenEndpoint, new FormUrlEncodedContent(parameters));
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Token request failed: {Error}", errorContent);
                return BadRequest("Error while fetching tokens. Details: " + errorContent);
            }

            var responseString = await response.Content.ReadAsStringAsync();
            var tokenResponse = JsonConvert.DeserializeObject<TokenResponse>(responseString); // Deserialisér til din TokenResponse klasse

            bool isValid = await ValidateIdTokenAsync(tokenResponse.IdToken,clientId);
            if (!isValid)
            {
                return BadRequest("Invalid ID token.");
            }

            // Gem ID token til senere validering (valgfrit)
            HttpContext.Session.SetString("id_token", tokenResponse.IdToken);
            HttpContext.Session.SetString("access_token", tokenResponse.AccessToken);
            HttpContext.Session.SetString("refresh_token", tokenResponse.RefreshToken);


            // Næste skridt: Validér ID-token og hent brugeroplysninger
            return RedirectToAction("FetchUserInfo");
        }
    }

    public async Task<IActionResult> FetchUserInfo()
    {
        var accessToken = HttpContext.Session.GetString("access_token"); 
        if (string.IsNullOrEmpty(accessToken))
        {
            return BadRequest("Access token is missing or expired.");
        }


        var http = new HttpClient
        {
            DefaultRequestHeaders =
                {
                    { "Authorization", "Bearer " + accessToken }
                }
        };

        var response = await http.GetAsync(_keycloakConfig.UserInfoEndpoint);
        if (!response.IsSuccessStatusCode)
        {
            return BadRequest("Failed to fetch user information.");
        }

        var contentString = await response.Content.ReadAsStringAsync();
        var content = JObject.Parse(contentString); // Parse content to JObject
        return View("UserInfo", content);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    // Private methods:
    private static string RandomString()
    {
        using (RandomNumberGenerator rng = new RNGCryptoServiceProvider())
        {
            byte[] tokenData = new byte[32];
            rng.GetBytes(tokenData);

            string token = Convert.ToBase64String(tokenData)
                .Replace('+', '-') // Erstat '+' med '-'
                .Replace('/', '_'); // Erstat '/' med '_'
            return token;
        }
    }

    private static string CreateCodeChallenge(string codeVerifier)
    {
        using (var sha256 = SHA256.Create())
        {
            var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(codeVerifier));
            var base64Url = Convert.ToBase64String(hash)
                .TrimEnd('=') // Fjern padding
                .Replace('+', '-') // 62nd char of encoding
                .Replace('/', '_'); // 63rd char of encoding
            return base64Url;
        }
    }

    private async Task<bool> ValidateIdTokenAsync(string idToken, string clientId)
    {
        var response = await new HttpClient().GetAsync(_keycloakConfig.CertsEndpoint);
        var keys = await response.Content.ReadAsStringAsync();
        var jwks = JsonWebKeySet.Create(keys);
        jwks.SkipUnresolvedJsonWebKeys = false;


        // TODO
        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKeys = jwks.GetSigningKeys(),  
            ValidateIssuer = true,
            ValidIssuer = _keycloakConfig.Authority,  // Erstat med den faktiske issuer fra din config
            ValidateAudience = true,
            ValidAudience = clientId,  
            ValidateLifetime = true,  
        };

        var tokenHandler = new JwtSecurityTokenHandler();

        try
        {
            // Token validation
            var principal = tokenHandler.ValidateToken(idToken, validationParameters, out var validatedToken);
            return validatedToken != null;

        }catch(Exception ex)
        {
            _logger.LogError($"Token validation failed: { ex.Message}");
            return false;  // Token validation failed
        }


        
    }

}

