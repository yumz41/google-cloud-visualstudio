﻿using GoogleCloudExtension.OAuth.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace GoogleCloudExtension.OAuth
{
    public static class OAuthManager
    {
        const string OAuthRefreshUrl = "https://www.googleapis.com/oauth2/v3/token";
        const string OAuthRedirectUrl = "urn:ietf:wg:oauth:2.0:oob";

        /// <summary>
        /// Given an app credentials and the refresh token it will create an access token by accessing the
        /// Google OAUTH end point.
        /// </summary>
        /// <param name="credentials"></param>
        /// <param name="refreshToken"></param>
        /// <returns></returns>
        public static async Task<AccessToken> RefreshAccessTokenAsync(OAuthCredentials credentials, string refreshToken)
        {
            WebClient client = new WebClient();
            var form = new NameValueCollection();
            form.Add("client_id", credentials.ClientId);
            form.Add("client_secret", credentials.ClientSecret);
            form.Add("refresh_token", refreshToken);
            form.Add("grant_type", "refresh_token");

            try
            {
                var result = await client.UploadValuesTaskAsync(OAuthRefreshUrl, form);
                var decoded = Encoding.UTF8.GetString(result);
                var model = JsonConvert.DeserializeObject<AccessTokenModel>(decoded);
                return new AccessToken(model);
            }
            catch (WebException ex)
            {
                Debug.WriteLine($"Failed to get refresh token: {ex.Message}");
                throw new OAuthException(ex.Message, ex);
            }
            catch (JsonException ex)
            {
                Debug.WriteLine($"Failed to parse result: {ex.Message}");
                throw new OAuthException(ex.Message, ex);
            }
        }

        /// <summary>
        /// Returns the URL to use to start the OAUTH login flow.
        /// </summary>
        /// <param name="credentials"></param>
        /// <param name="scopes"></param>
        /// <returns></returns>
        public static string GetOAuthBeginFlowUrl(OAuthCredentials credentials, IEnumerable<string> scopes)
        {
            var form = new Dictionary<string, string>
            {
                ["response_type"] = "code",
                ["client_id"] = credentials.ClientId,
                ["client_secret"] = credentials.ClientSecret,
                ["redirect_uri"] = OAuthRedirectUrl,
                ["scope"] = String.Join(" ", scopes),
            };
            return $"https://accounts.google.com/o/oauth2/auth?{ToQueryString(form)}";
        }

        private static string ToQueryString(IDictionary<string, string> form)
        {
            return String.Join(
                "&",
                form.Select(x => $"{x.Key}={Uri.EscapeUriString(x.Value)}"));
        }

        public static Task<OAuthLoginResult> EndOAuthFlow(string accessCode)
        {
            throw new NotImplementedException();
        }
    }
}
