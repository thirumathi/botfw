﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace WebAndLoadTestProject1
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Microsoft.VisualStudio.TestTools.WebTesting;
    using Amido.PerformanceTests.Common;


    public class WebTestCoded1 : WebTest
    {

        public WebTestCoded1()
        {
            this.PreAuthenticate = true;
            this.Proxy = "default";
        }

        public override IEnumerator<WebTestRequest> GetRequestEnumerator()
        {
            WebTestRequest request1 = new WebTestRequest("https://directline.botframework.com/api/conversations");
            request1.Method = "POST";
            request1.Headers.Add(new WebTestRequestHeader("Authorization", "Bearer FTk1OxvYyCY.cwA.oQA.Cn9aCk2yZxsFzI2vkWccV66V3FsV2TsLRxfyISyIXe4"));
            FormPostHttpBody request1Body = new FormPostHttpBody();
            request1.Body = request1Body;
            WebTestRequest request1Dependent1 = new WebTestRequest(("https://directline.botframework.com/api/conversations/"
                            + (this.Context["ConvID"].ToString() + "/messages")));
            request1Dependent1.Method = "POST";
            request1Dependent1.Headers.Add(new WebTestRequestHeader("Authorization", "Bearer FTk1OxvYyCY.cwA.oQA.Cn9aCk2yZxsFzI2vkWccV66V3FsV2TsLRxfyISyIXe4"));
            FormPostHttpBody request1Dependent1Body = new FormPostHttpBody();
            request1Dependent1Body.FormPostParameters.Add("text", "hello");
            request1Dependent1Body.FormPostParameters.Add("from", "user1");
            request1Dependent1.Body = request1Dependent1Body;
            WebTestRequest request1Dependent1Dependent1 = new WebTestRequest(("https://directline.botframework.com/api/conversations/"
                            + (this.Context["ConvID"].ToString() + "/messages")));
            request1Dependent1Dependent1.Encoding = System.Text.Encoding.GetEncoding("utf-8");
            request1Dependent1Dependent1.Headers.Add(new WebTestRequestHeader("Authorization", "Bearer FTk1OxvYyCY.cwA.oQA.Cn9aCk2yZxsFzI2vkWccV66V3FsV2TsLRxfyISyIXe4"));
            request1Dependent1.DependentRequests.Add(request1Dependent1Dependent1);
            request1.DependentRequests.Add(request1Dependent1);
            JsonExtractionRule extractionRule1 = new JsonExtractionRule();
            extractionRule1.Name = "conversationId";
            extractionRule1.ContextParameterName = "ConvID";
            request1.ExtractValues += new EventHandler<ExtractionEventArgs>(extractionRule1.Extract);
            yield return request1;
            request1 = null;
        }
    }
}
