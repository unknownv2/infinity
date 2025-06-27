using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using NoDev.Infinity.Network;
using NoDev.Infinity.Network.Api;

namespace NoDev.Infinity.User
{
    internal static class Me
    {
        internal static event EventHandler OnLogin;
        internal static event EventHandler OnLogout;

        internal static bool IsLoggedIn
        {
            get { return ID != 0; }
        }

        internal static bool IsDiamond
        {
            get { return AccessLevel == AccessLevel.Private || AccessLevel >= AccessLevel.Diamond; }
        }

        internal static int ID { get; private set; }
        internal static AccessLevel AccessLevel { get; private set; }
        internal static string Username { get; private set; }

        internal static bool HasAccess(AccessLevel accessLevel)
        {
            return accessLevel == AccessLevel.Private || accessLevel >= AccessLevel;
        }

        internal static async Task<bool> LoginAsync(string username, string password)
        {
            var req = ApiRequestFactory.Create(Method.POST, "/users/me");
            
            var response = await req.SendUrlEncodedFormAsync(new Dictionary<string, string>
            {
                { "username", username },
                { "password", password }
            });

            switch (response.StatusCode)
            {
                case HttpStatusCode.OK:
                    await ReadValuesAsync(response);
                    if (OnLogin != null)
                    {
                        OnLogin(null, EventArgs.Empty);
                    }
                    return true;
                case HttpStatusCode.Unauthorized:
                    return false;
                default:
                    throw new Exception("Unknown server response.");
            }
        }

        internal static async Task RefreshAsync()
        {
            if (!IsLoggedIn)
            {
                return;
            }

            await ReadValuesAsync(await ApiRequestFactory.Create(Method.GET, "/users/me").SendAsync());
        }

        private static async Task ReadValuesAsync(HttpWebResponse response)
        {
            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception("Invalid HTTP status code returned from /users/me.");
            }

            var data = await response.ToObjectAsync<Dictionary<string, object>>();

            ID = (int)data["id"];
            AccessLevel = (AccessLevel)data["access_level"];
            Username = (string)data["username"];
        }

        internal static async Task LogoutAsync()
        {
            var response = await ApiRequestFactory.Create(Method.DELETE, "/users/me").SendAsync();

            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception("Failed to log out.");
            }

            ID = 0;
            AccessLevel = AccessLevel.Everyone;
            Username = null;

            if (OnLogout != null)
            {
                OnLogout(null, EventArgs.Empty);
            }
        }
    }
}
