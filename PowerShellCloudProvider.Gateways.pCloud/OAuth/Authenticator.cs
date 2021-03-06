/*
The MIT License(MIT)

Copyright(c) 2015 IgorSoft

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using System;
using System.Globalization;
using System.Threading.Tasks;
using pCloud.NET;

namespace IgorSoft.PowerShellCloudProvider.Gateways.pCloud.OAuth
{
    internal static class Authenticator
    {
        private static string LoadRefreshToken(string account)
        {
            var refreshTokens = Settings.Default.RefreshTokens;
            if (refreshTokens != null)
                foreach (RefreshTokenSetting setting in refreshTokens)
                    if (setting.Account == account)
                        return setting.RefreshToken;

            return  null;
        }

        private static void SaveRefreshToken(string account, string refreshToken)
        {
            var refreshTokens = Settings.Default.RefreshTokens;
            if (refreshTokens != null) {
                foreach (RefreshTokenSetting setting in refreshTokens)
                    if (setting.Account == account) {
                        refreshTokens.Remove(setting);
                        break;
                    }
            } else {
                refreshTokens = Settings.Default.RefreshTokens = new System.Collections.ObjectModel.Collection<RefreshTokenSetting>();
            }

            refreshTokens.Insert(0, new RefreshTokenSetting() { Account = account, RefreshToken = refreshToken });

            Settings.Default.Save();
        }

        public static async Task<pCloudClient> Login(string account, string code)
        {
            if (string.IsNullOrEmpty(account))
                throw new ArgumentNullException(nameof(account));

            var client = default(pCloudClient);

            var refreshToken = LoadRefreshToken(account);

            if (refreshToken != null) {
                client = pCloudClient.FromAuthToken(refreshToken);
            } else {
                if (string.IsNullOrEmpty(code))
                    throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Resources.ProvideAuthenticationData, account));

                var parts = code.Split(new[] { ',' }, 2);
                if (parts.Length != 2)
                    throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Resources.ProvideAuthenticationData, account));

                client = await pCloudClient.CreateClientAsync(parts[0], parts[1]);
            }

            SaveRefreshToken(account, client.AuthToken);

            return client;
        }
    }
}