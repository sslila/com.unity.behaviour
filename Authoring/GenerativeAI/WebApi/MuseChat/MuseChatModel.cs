#if ENABLE_MUSE_BEHAVIOR
using System;
using System.Collections.Generic;
using System.Threading;
using Newtonsoft.Json;
using Unity.Behavior.WebApi.Api;
using Unity.Behavior.WebApi.Client;
using Unity.Behavior.WebApi.Model;
#if UNITY_EDITOR
using Unity.Muse.Common.Account;
using UnityEditor;
#endif
using UnityEngine;

namespace Unity.Behavior.GenerativeAI
{
    internal class MuseChatModel : ILanguageModel
    {
        private const string MuseEndpoint = "https://rest-api.prd.azure.muse.unity.com";
        private const string DefaultApiAccessToken = "f5vv0wIGD-iJB3ulhlr5knNaywrccISH_PTOLopnjUk";
#if UNITY_EDITOR
        static Configuration CreateConfig()
        {
            Configuration config = new()
            {
                BasePath = MuseEndpoint
            };

            config.ApiKey.Add("access_token", DefaultApiAccessToken);
            config.DefaultHeaders.Add("Authorization", $"Bearer {CloudProjectSettings.accessToken}");
            return config;
        }
#endif

        public void Chat(string input, Action<(string output, string conversationId)> onResponseReceived,
            string origin,
            string conversationId = null)
        {
#if UNITY_EDITOR
            LLMCompletionRequest options = new(
                prompt: input,
                streamResponse: false,
                organizationId: AccountInfo.Instance.Organization.Id,
                tags: new List<string> { "muse-behavior", origin }
            );

            var api = new DefaultApi(CreateConfig());
            
            var request = api.CompletionV1MuseCompletionPostAsync(options, CancellationToken.None);
            var progress = 0.0f;
            
            //this is the only way to get the conversation id from the response
            api.ApiClient.OnResponseIntercepted += (webRequest, path, ops, config, o) =>
            {
                var conversationId = webRequest.GetResponseHeader("x-muse-conversation-id");
                var response = webRequest.downloadHandler.text;
                ResponseWrapper resp = JsonConvert.DeserializeObject<ResponseWrapper>(response);
                onResponseReceived?.Invoke((resp.response, conversationId));
            };

            void WaitForResponse()
            {
                if (!request.IsCompleted)
                {
                    EditorUtility.DisplayProgressBar("Muse Behavior", "Generating...", progress);
                    progress += 0.01f * Time.deltaTime;
                    return;
                }

                EditorUtility.ClearProgressBar();
                EditorApplication.update -= WaitForResponse;
            }

            EditorApplication.update += WaitForResponse;
#endif
        }

        [Serializable]
        internal struct ResponseWrapper
        {
            public string response;
        }
    }
}
#endif // ENABLE_MUSE_BEHAVIOR