using CRM;
using Plugins;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

namespace LoginPlugin
{
    /// <summary>
    /// This is a sample plugin that demonstrates how you can use prompts to gather custom properties
    /// such as username and password and then use those for authentication.
    /// </summary>
    public class LoginWithPrompts : IPluginAuth
    {
        /// <summary>
        /// Properties returned by the plugin.
        /// The minimum properties that should be returned are:
        /// Author, ContainsSensitiveData, Name, Type, Version
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, object> Properties() =>
            new Dictionary<string, object> {
                { "Id", new Guid("a5a8354d-f5c6-435e-90e6-2bf86f0b4d35") },
                { "Author", "Brad Wickett" },
                { "ContainsSensitiveData", false },
                { "Name", "Plugin - Login with Prompts" },
                { "Prompts", new List<PluginPrompt> 
                    {
                        new PluginPrompt { Name = "Username", Type = PluginPromptType.Text },
                        new PluginPrompt { Name = "Password", Type = PluginPromptType.Password },
                    }
                },
                { "Type", "Auth" },
                { "Version", "1.0.0" },
                { "LimitToTenants", new List<Guid> { new Guid("00000000-0000-0000-0000-000000000001") }},
                { "ButtonText", "Plugin - Login with Prompts Example" },
                { "ButtonClass", "btn btn-primary" },
                { "ButtonIcon", "<i class=\"icon fa-solid fa-sign-in-alt\"></i>" },
            };

        public async Task<(bool Result, List<string>? Messages, IEnumerable<object>? Objects)> Login(
            DataAccess da,
            Plugins.Plugin plugin,
            string url,
            Guid tenantId,
            Microsoft.AspNetCore.Http.HttpContext httpContext
        )
        {
            var messages = new List<string>();
            bool result = false;

            DataObjects.User output = new DataObjects.User();

            try {
                if (tenantId != Guid.Empty) {
                    if (!String.IsNullOrWhiteSpace(url)) {
                        string username = String.Empty;
                        string password = String.Empty;

                        if (plugin.Prompts != null && plugin.Prompts.Count > 0 && plugin.PromptValues != null) {
                            foreach (var prompt in plugin.Prompts) {
                                var promptValue = plugin.PromptValues.FirstOrDefault(x => x.Name.ToLower() == prompt.Name.ToLower());
                                if (promptValue != null && promptValue.Values != null && promptValue.Values.Length > 0) {
                                    if (prompt.Name.ToLower() == "username") {
                                        username += promptValue.Values[0];
                                    } else if (prompt.Name.ToLower() == "password") {
                                        password += promptValue.Values[0];
                                    }
                                }
                            }
                        }

                        if (String.IsNullOrWhiteSpace(username)) {
                            messages.Add("Missing Username");
                        }

                        if (String.IsNullOrWhiteSpace(password)) {
                            messages.Add("Missing Password");
                        }

                        if (messages.Count == 0) {
                            messages.Add("Calling Authenticate");

                            string fingerprint = da.Request("Fingerprint");

                            // Only continue if we have a valid username and password.
                            // For sample purposes only this code is going to call the Authenticate
                            // method in the DataAccess library. You would replace this with your own
                            // code to authenticate a user.
                            // This could perhaps be validating the credentials against some
                            // database or API endpoint. Or perhaps you would redirect to another
                            // site for authentication then validate that authentication when
                            // you get returned to this site.
                            var user = await da.Authenticate(new CRM.DataObjects.Authenticate {
                                TenantId = tenantId,
                                Username = username,
                                Password = password,
                            }, fingerprint);

                            if (user != null) {
                                if (user.ActionResponse.Result) {
                                    result = true;
                                    output = user;
                                } else {
                                    if (user.ActionResponse.Messages.Count > 0) {
                                        messages = user.ActionResponse.Messages;
                                    } else {
                                        messages.Add("Authentication Failure");
                                    }
                                }
                            } else {
                                messages.Add("Authentication Failed");
                            }
                        }
                    } else {
                        messages.Add("Missing the URL Parameter");
                    }
                }
            } catch(Exception ex) {
                messages.Add("Error in Login: " + ex.Message);
            }

            return (Result: result, Messages: messages, Objects: [output]);
        }

        public async Task<(bool Result, List<string>? Messages, IEnumerable<object>? Objects)> Logout(
            DataAccess da,
            Plugins.Plugin plugin,
            string url,
            Guid tenantId,
            Microsoft.AspNetCore.Http.HttpContext httpContext
        )
        {
            await Task.Delay(0); // Simulate a delay since this method has to be async. This can be removed once you implement your await logic.

            var output = new List<object>();
            var messages = new List<string>();
            var result = false;

            try {
                if (da != null) {
                    // If you need to redirect to any external system to remove auth properties or cookies do that here.
                    // You can use the url as a redirect parameter to get back to the root of the application.
                    da.Redirect(url);

                    result = true;
                } else {
                    messages.Add("Missing DataAccess Library Reference");
                }
            } catch(Exception ex) {
                messages.Add("Error in Logout: " + ex.Message);
            }

            return (Result: result, Messages: messages, Objects: output);
        }

        //private T? GetOrPost<T>(string url, object? post = null)
        //{
        //    var output = typeof(T) == typeof(System.String) ? default(T) : (T)Activator.CreateInstance(typeof(T));

        //    var client = new HttpClient();
        //    client.DefaultRequestHeaders.Accept.Clear();
        //    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        //    HttpResponseMessage? response = null;

        //    try {
        //        if (post != null) {
        //            response = client.PostAsync(url, new StringContent(System.Text.Json.JsonSerializer.Serialize(post), System.Text.Encoding.UTF8, "application/json")).Result;
        //        } else {
        //            response = client.GetAsync(url).Result;
        //        }

        //        if (response.IsSuccessStatusCode) {
        //            var content = response.Content.ReadAsStringAsync().Result;
        //            if (!String.IsNullOrWhiteSpace(content)) {
        //                output = System.Text.Json.JsonSerializer.Deserialize<T>(content);
        //            }
        //        }
        //    } catch { }

        //    return output;
        //}

        public class OktaAuthentication
        {
            public bool Authenticated { get; set; }
            public string? FirstName { get; set; }
            public string? LastName { get; set; }
            public string? MiddleName { get; set; }
            public string? Name { get; set; }
            public string? Email { get; set; }
            public string? PreferredEmail { get; set; }
            public string? WSUID { get; set; }
            public string? NID { get; set; }
            public string? Department { get; set; }
            public string? Title { get; set; }
            public List<string>? Groups { get; set; } = new List<string>();
            public DateTime? Expires { get; set; }
        }
    }
}