using ColdDrinkWeixin.Controllers;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace ColdDrinkWeixin.Models
{
    public class AccessToken
    {
        public const string key = "access_token";
        private IMemoryCache memoryCache;
        private WeixinSetting weixinSetting;
        public AccessToken(IOptions<WeixinSetting> weixinSetting, IMemoryCache memoryCache)
        {
            this.weixinSetting = weixinSetting.Value;
            this.memoryCache = memoryCache;

            string access_token = "";
            if (!memoryCache.TryGetValue(key, out access_token))
            {
                //正确{"access_token":"ACCESS_TOKEN","expires_in":7200}
                //错误{"errcode":40013,"errmsg":"invalid appid"}
                AccessTokenResult m = getAccessToken();
                access_token = m.access_token;
                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromSeconds(m.expires_in));
                    //.RegisterPostEvictionCallback(PostEvictionDelegate);
                memoryCache.Set(key, m.access_token, cacheEntryOptions);
            }

        }
        private AccessTokenResult getAccessToken()
        {
            string url = string.Format("https://api.weixin.qq.com/cgi-bin/token?grant_type=client_credential&appid={0}&secret={1}", weixinSetting.AppId, weixinSetting.AppSecret);
            using (var client = new HttpClient())
            {
                var json = client.GetStringAsync(url).Result;
                AccessTokenResult m = JsonConvert.DeserializeObject<AccessTokenResult>(json);
                if (m.errcode == 0)
                {
                    return m;
                }
                else
                {
                    throw new WeixinException(m.errcode, m.errmsg);
                }
            }
        }
        void PostEvictionDelegate(object key, object value, EvictionReason reason, object state)
        {
            AccessTokenResult m = getAccessToken();
            ((AccessToken)state).memoryCache.Set(AccessToken.key, m.access_token);
        }
    }
}
