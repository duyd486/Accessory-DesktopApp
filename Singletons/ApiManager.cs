using Accessory_DesktopApp.Models;
using Accessory_DesktopApp.Models.Response;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Windows;

namespace Accessory_DesktopApp.Singletons
{
    internal class ApiManager
    {
        public static ApiManager? Instance { get; private set; }

        private const string baseUrl = "http://localhost:8000/api/";

        private readonly HttpClient client = new();

        private User? currentUser;

        public static ApiManager GetInstance()
        {
            if (Instance == null)
            {
                Instance = new ApiManager();
            }

            return Instance;
        }

        public string GetOrigin()
        {
            return new Uri(baseUrl, UriKind.Absolute).GetLeftPart(UriPartial.Authority);
        }

        public string? ToAbsoluteUrl(string? urlOrPath)
        {
            if (string.IsNullOrWhiteSpace(urlOrPath))
                return null;

            urlOrPath = urlOrPath.Trim();

            if (Uri.TryCreate(urlOrPath, UriKind.Absolute, out _))
                return urlOrPath;

            var origin = GetOrigin();

            if (urlOrPath.StartsWith("/", StringComparison.Ordinal))
                return origin + urlOrPath;

            return origin + "/" + urlOrPath;
        }

        #region Login

        public async Task<bool> LoginAsync(string email, string password)
        {
            try
            {
                var payload = new { email = email, password = password };
                string json = JsonSerializer.Serialize(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PostAsync(baseUrl + "login", content);

                string result = await response.Content.ReadAsStringAsync();

                ResponseBase<UserResponse>? res = JsonSerializer.Deserialize<ResponseBase<UserResponse>>(result);

                if (res?.status == true)
                {
                    currentUser = res?.data?.user;
                    MessageBox.Show("Đăng nhập thành công, xin chào " + res?.data?.user?.name);
                    client.DefaultRequestHeaders.Add("Authorization", "Bearer " + res?.data?.token);
                    return true;
                }
                else
                {
                    MessageBox.Show("Đăng nhập thất bại");
                    return false;
                }
            } catch(Exception ex)
            {
                MessageBox.Show("Đã có lỗi xảy ra: " + ex.Message);
                return false;
            }
        }

        #endregion

        public async Task<T> HttpGetAsync<T>(string url)
        {
            try
            {
                HttpResponseMessage response = await client.GetAsync(baseUrl + url).ConfigureAwait(false);

                response.EnsureSuccessStatusCode();

                string result = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                ResponseBase<T>? res = JsonSerializer.Deserialize<ResponseBase<T>>(result);

                return res!.data;
            }
            catch (Exception ex)
            {
                if (Application.Current?.Dispatcher?.CheckAccess() == true)
                {
                    MessageBox.Show(ex.Message);
                }
                else
                {
                    Application.Current?.Dispatcher?.Invoke(() => MessageBox.Show(ex.Message));
                }
                return default!;
            }
        }

        public async Task HttpGetNoDataAsync(string url)
        {
            HttpResponseMessage response = await client.GetAsync(baseUrl + url);
            response.EnsureSuccessStatusCode();
        }

        public async Task HttpPostNoDataAsync(string url, object? payload = null)
        {
            try
            {
                var json = JsonSerializer.Serialize(payload ?? new { });
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(baseUrl + url, content);
                response.EnsureSuccessStatusCode();
            } catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public async Task<T> HttpPostFormAsync<T>(string url, object? payload = null)
        {
            try
            {
                var json = JsonSerializer.Serialize(payload ?? new { });
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(baseUrl + url, content).ConfigureAwait(false);
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                var res = JsonSerializer.Deserialize<ResponseBase<T>>(result);
                return res!.data;
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() => MessageBox.Show(ex.Message));
                return default!;
            }
        }

        public void SetCurrentUser(User? user)
        {
            if (user is null)
                return;

            var token = currentUser?.token;
            currentUser = user;

            if (string.IsNullOrWhiteSpace(currentUser.token) && !string.IsNullOrWhiteSpace(token))
                currentUser.token = token;
        }

        public User? GetCurrentUser()
        {
            return currentUser;
        }
    }

    public class ResponseBase<T>
    {
        public bool status { get; set; }
        public T data { get; set; } = default!;
        public string? message { get; set; }
    }
}