using ColdDrinkWeixin.Controllers;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
//using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace ColdDrinkWeixin.Common
{
    public class AccessToken
    {
        private IMemoryCache memoryCache;
        private string access_token = "";
        private WeixinSetting weixinSetting;
        public AccessToken(IOptions<WeixinSetting> weixinSetting,IMemoryCache memoryCache)
        {
            this.weixinSetting = weixinSetting.Value;
            this.memoryCache = memoryCache;
        }

        public void Update()
        {
            string key = "access_token";
            if (!memoryCache.TryGetValue(key, out this.access_token))
            {
                //正确{"access_token":"ACCESS_TOKEN","expires_in":7200}
                //错误{"errcode":40013,"errmsg":"invalid appid"}
                string url = string.Format("https://api.weixin.qq.com/cgi-bin/token?grant_type=client_credential&appid={0}&secret={1}", weixinSetting.AppId, weixinSetting.AppSecret);
                using (var client = new HttpClient())
                {
                    var json = client.GetStringAsync(url).Result;
                    AccessTokenResult m = JsonConvert.DeserializeObject<AccessTokenResult>(json);
                    if (m.errcode == 0)
                    {
                        this.access_token = m.access_token;
                        var cacheEntryOptions = new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromSeconds(m.expires_in));
                        memoryCache.Set(key, m.access_token, cacheEntryOptions);
                    }
                    else
                    {
                        throw new WeixinException(m.errcode, m.errmsg);
                    }
                }


            }
        }
    }
}
