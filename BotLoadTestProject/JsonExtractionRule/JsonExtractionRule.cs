using System;
using System.ComponentModel;
using System.Globalization;
using Microsoft.VisualStudio.TestTools.WebTesting;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace Amido.PerformanceTests.Common
{
    [DisplayName("JSON Extraction Rule")]
    [Description("Extracts the specified JSON value from an object.")]
    public class JsonExtractionRule : ExtractionRule
    {
        public string Name { get; set; }

        public override void Extract(object sender, ExtractionEventArgs e)
        {
            if (e.Response.BodyString != null)
            {
                var json = e.Response.BodyString;
                if (!string.IsNullOrEmpty(json))
                {
                    var data = JObject.Parse(json);

                    if (data != null)
                    {
                        e.WebTest.Context.Add(this.ContextParameterName, data.SelectToken(Name));
                        e.Success = true;
                        return;
                    }
                }
            }

            e.Success = false;
            e.Message = String.Format(CultureInfo.CurrentCulture, "Not Found: {0}", Name);
        }
    }

    [DisplayName("Json Message Id Validation Rule")]
    [Description("Extracts the specified JSON value from an object.")]
    public class JsonMessageIdValidationRule : ValidationRule
    {
        public string Name { get; set; }

        public string ContextVariableToValidate { get; set; }

        public override void Validate(object sender, ValidationEventArgs e)
        {
            if (e.Response.BodyString != null)
            {
                var json = e.Response.BodyString;

                var data = JObject.Parse(json);
                Dictionary<string, object> dataDictionary = data.ToObject<Dictionary<string, object>>();

                List<string> messageIds = new List<string>();
                Dictionary<string, dynamic> activityDetails = new Dictionary<string, dynamic>();
                if (dataDictionary != null && dataDictionary.ContainsKey("activities"))
                {
                    List<object> activities = ((JArray)dataDictionary["activities"]).ToList<object>();
                    
                    if (activities.Count > 0)
                    {
                        foreach (var item in activities)
                        {
                            JObject activityInfo = item as JObject;
                            if (activityInfo != null)
                            {
                                string messageId = activityInfo[Name].ToString();
                                messageIds.Add(messageId);
                                activityDetails.Add(messageId, new { id = activityInfo[Name].ToString(), fromId = activityInfo["from"]["id"] , fromName = activityInfo["from"]["name"] });
                            }
                        }

                        string botResponseLastMessageId = messageIds[messageIds.Count - 1];
                        string requestMessageIdOnContext = e.WebTest.Context[ContextVariableToValidate].ToString();
                        var tempTokens1 = requestMessageIdOnContext.Split(new char[] { '|' });
                        var tempTokens2 = botResponseLastMessageId.Split(new char[] { '|' });
                        if (tempTokens1.Length == tempTokens2.Length && tempTokens1.Length == 2)
                        {
                            if (tempTokens1[0] == tempTokens2[0])
                            {
                                int id1 = int.Parse(tempTokens1[1]);
                                int id2 = int.Parse(tempTokens2[1]);

                                e.IsValid = (id2 > id1);
                                e.Message = $"Request [{requestMessageIdOnContext}] and response [{botResponseLastMessageId}] validation " + (e.IsValid ? "succeeded" : "failed");

                                if (e.IsValid)
                                {
                                    string userFromId = activityDetails[requestMessageIdOnContext].fromId;
                                    string botFromId = activityDetails[botResponseLastMessageId].fromId;
                                    e.IsValid = !userFromId.Equals(botFromId, StringComparison.OrdinalIgnoreCase);
                                    e.Message += string.Format(", Response bot id validation {0}", e.IsValid ? "succeeded" : "failed");
                                }

                                return;
                            }
                        }
                    }
                }
            }

            e.IsValid = false;
            e.Message = String.Format(CultureInfo.CurrentCulture, "Not Found: {0}", Name);
        }
    }
}