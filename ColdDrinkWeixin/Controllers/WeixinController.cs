using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using ColdDrinkWeixin.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace ColdDrinkWeixin.Controllers
{
    [Produces("application/json")]
    [Route("api/Weixin")]
    public class WeixinController : Controller
    {
        private WeixinSetting weixinSetting;
        //private IAccessToken accessToken;
        //private WeixinMenu weixinMenu;
        private IMemoryCache memoryCache;
        private string AccessToken
        {
            get { return memoryCache.Get<string>(Common.AccessToken.key); }
        }
        public WeixinController(IOptions<WeixinSetting> weixinSetting, IMemoryCache memoryCache)
        {
            new AccessToken(weixinSetting, memoryCache);
            this.weixinSetting = weixinSetting.Value;
            //this.weixinMenu = weixinMenu.Value;
            //this.accessToken = accessToken;
            this.memoryCache = memoryCache;
        }
        // GET: api/Weixin
        [HttpGet]
        public ActionResult Get(string signature, string timestamp, string nonce, string echostr)
        {
            //接入微信
            //1）将token、timestamp、nonce三个参数进行字典序排序 
            //2）将三个参数字符串拼接成一个字符串进行sha1加密 
            //3）开发者获得加密后的字符串可与signature对比，标识该请求来源于微信
            if (!auth(signature, timestamp, nonce))
                throw new Exception("认证失败");
            return Content(echostr);
        }
        private bool auth(string signature, string timestamp, string nonce)
        {
            if (string.IsNullOrEmpty(signature) || string.IsNullOrEmpty(timestamp) || string.IsNullOrEmpty(nonce))
            {
                throw new ArgumentNullException("signature timestamp nonce echostr");
            }
            string token = this.weixinSetting.Token;
            var str = string.Join("", new[] { token, timestamp, nonce }.OrderBy(o => o));
            var sha1 = SHA1.Create();
            var dataHashed = sha1.ComputeHash(Encoding.UTF8.GetBytes(str));
            string hash = BitConverter.ToString(dataHashed).Replace("-", "").ToLower();
            return hash == signature;
        }
        // GET: api/Weixin/5
        [HttpGet("{id}", Name = "Get")]
        public string Get(int id)
        {
            return "value";
        }
        
        // POST: api/Weixin
        [HttpPost]
        public ActionResult Post(string signature, string timestamp, string nonce)//[FromBody]string value)
        {
            if (!auth(signature, timestamp, nonce))
                throw new Exception("认证失败");
            //<xml><ToUserName><![CDATA[gh_16554ff5447a]]></ToUserName>
            //< FromUserName >< ![CDATA[oElrWjsMqwMYryic9pFuVZr40oak]] ></ FromUserName >
            //< CreateTime > 1524296205 </ CreateTime >
            //< MsgType >< ![CDATA[event]]></MsgType>
            //<Event><![CDATA[subscribe]]></Event>
            //<EventKey><![CDATA[]]></EventKey>
            //</xml>

            //微信服务器在五秒内收不到响应会断掉连接，并且重新发起请求，总共重试三次。
            //关于重试的消息排重，推荐使用FromUserName + CreateTime 排重。
            XDocument body = XDocument.Load(Request.Body);
            string msgType = body.Root.Element("MsgType").Value;
            string fromUserName = body.Root.Element("FromUserName").Value;//openid
            string toUserName = body.Root.Element("ToUserName").Value;
            string responseMsg = "";
            DateTime startTime = new DateTime(1970, 1, 1);
            long timeStamp = (long)(DateTime.Now - startTime).TotalSeconds; // 相差秒数
            
            switch (msgType)
            {
                case "event":
                    string eventName = body.Root.Element("Event").Value;
                    switch(eventName)
                    {
                        case "subscribe"://关注
                            responseMsg = string.Format("<xml><ToUserName><![CDATA[{0}]]></ToUserName><FromUserName><![CDATA[{1}]]></FromUserName><CreateTime>{2}</CreateTime><MsgType><![CDATA[text]]></MsgType><Content><![CDATA[你好]]></Content></xml>", fromUserName, toUserName, timeStamp);
                            UserInfo ui = getUserInfo(fromUserName);
                            break;
                        case "unsubscribe"://取消关注
                            break;
                    }
                    break;
                default:
                    responseMsg = "success";
                    break;
            }
            //responseMsg = (responseMsg ?? "").Replace("\r\n", "\n");

            //var bytes = Encoding.UTF8.GetBytes(responseMsg);
            //Response.Body.WriteAsync(bytes, 0, bytes.Length);
            return Content(responseMsg);
        }

        private UserInfo getUserInfo(string openid)
        {
            string url = string.Format("https://api.weixin.qq.com/cgi-bin/user/info?access_token={0}&openid={1}&lang=zh_CN",this.AccessToken, openid);
            using (var client = new HttpClient())
            {
                var json = client.GetStringAsync(url).Result;
                UserInfo m = JsonConvert.DeserializeObject<UserInfo>(json);
                if (m.errcode == 0)
                {
                    return m;
                }
                throw new WeixinException(m.errcode, m.errmsg);
            }
        }
        // PUT: api/Weixin/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }
        
        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
