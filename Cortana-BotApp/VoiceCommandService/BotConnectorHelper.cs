using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Json;

namespace VoiceCommandService
{
    class BotConnectorHelper
    {
        static string botId = "thirubotapp2";
        static HttpClient client;
        static string convId;
        static HttpResponseMessage response;

        public static async Task<string> SendAndReceive(string input)
        {
            string output = string.Empty;
            try
            {
                StringBuilder sbOutput = new StringBuilder();

                JsonObject root = null;
                string responseString;

                if (client == null)
                {
                    client = new HttpClient { BaseAddress = new Uri("https://directline.botframework.com") };
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "BgxxmPYX45g.cwA.2wE.QFBmoKFHzHrY8AYv6ClLzhomVj1p9MthiNMyUFORyAg");

                    var content = new FormUrlEncodedContent(new[]
                    {
                    new KeyValuePair<string, string>("login", "login")
                    });

                    // make an initial request to ensure auth worked
                    // create a conversation
                    response = await client.PostAsync("/v3/directline/conversations", content).ConfigureAwait(false);
                    response.EnsureSuccessStatusCode();

                    // read the conversation and auth token data
                    responseString = await response.Content.ReadAsStringAsync();
                    root = JsonObject.Parse(responseString);
                    convId = root["conversationId"].ToString().Trim('\"');
                }

                string url = $"/v3/directline/conversations/{convId}/activities";
                string jsonInput = "{\"type\":\"message\",\"text\":\""+ input + "\", \"from\":{\"id\":\"test\",\"name\":\"test\"}}";
                StringContent strContent = new StringContent(jsonInput, Encoding.UTF8, "application/json");
                response = await client.PostAsync(url, strContent).ConfigureAwait(false);
                response.EnsureSuccessStatusCode();

                // read the conversation and auth token data
                responseString = await response.Content.ReadAsStringAsync();
                root = JsonObject.Parse(responseString);
                string messageId = root["id"].ToString().Trim('\"');

                var tempTokens1 = messageId.Split(new char[] { '|' });
                int msgId = 0;
                if (tempTokens1.Length == 2)
                {
                    msgId = int.Parse(tempTokens1[1]);
                }

                url = $"/v3/directline/conversations/{convId}/activities";
                response = await client.GetAsync(url).ConfigureAwait(false);
                response.EnsureSuccessStatusCode();
                responseString = await response.Content.ReadAsStringAsync();
                root = JsonObject.Parse(responseString);
                JsonArray activities = JsonArray.Parse(root["activities"].ToString());
                var msgId1 = 0;
                for (uint i = 0; i < activities.Count; i++)
                {
                    var activity = activities.GetObjectAt(i);
                    messageId = activity.GetNamedString("id");
                    tempTokens1 = messageId.Split(new char[] { '|' });
                    if (tempTokens1.Length == 2)
                    {
                        msgId1 = int.Parse(tempTokens1[1]);
                    }

                    if (msgId1 > msgId)
                    {
                        JsonObject fromObject = activity.GetNamedObject("from");
                        var fromId = fromObject.GetNamedString("id");
                        if (fromId.Equals(botId, StringComparison.OrdinalIgnoreCase))
                        {
                            string msgText = activity.GetNamedString("text");
                            sbOutput.AppendFormat("{0}{1}", sbOutput.Length > 0 ? "," : string.Empty, msgText);
                        }
                    }
                }

                client.Dispose();

                output = sbOutput.ToString();
            }
            catch (Exception ex)
            {
                output = ex.Message;
            }
            return output;
        }
    }
}
