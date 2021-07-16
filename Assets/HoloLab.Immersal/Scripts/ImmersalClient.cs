// Copyright (c) 2021 HoloLab Inc. All rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace HoloLab.Immersal
{
    public class ImmersalClient
    {
        private readonly HttpClient httpClient = new HttpClient();

        public string Token { set; get; }

        public string ApiBaseUri { set; get; } = "https://api.immersal.com";

        public async Task<LocalizeResult> Localize(IEnumerable<int> mapIds, byte[] imageData,
            float fx, float fy, float ox, float oy)
        {
            try
            {
                var b64 = Convert.ToBase64String(imageData);

                var sdkMapIds = mapIds.Select(x => new SDKMapId()
                {
                    id = x
                }).ToArray();

                var request = new SDKLocalizeRequest()
                {
                    mapIds = sdkMapIds,
                    b64 = b64,
                    ox = ox,
                    oy = oy,
                    fx = fx,
                    fy = fy,
                    token = Token
                };

                var json = JsonUtility.ToJson(request);

                var uri = $"{ApiBaseUri}/localizeb64";
                var response = await PostAsync(uri, json);
                var content = await response.Content.ReadAsStringAsync();

                var localizeResult = JsonUtility.FromJson<SDKLocalizeResult>(content);
                return localizeResult.ToLocalizeResult();
            }
            catch (Exception e)
            {
                Debug.LogWarning(e);

                return new LocalizeResult()
                {
                    Success = false
                };
            }
        }

        private async Task<HttpResponseMessage> PostAsync(string uri, string json)
        {
            var stringContent = new StringContent(json, Encoding.UTF8, "application/json");
            var result = await httpClient.PostAsync(uri, stringContent);
            return result;
        }
    }
}