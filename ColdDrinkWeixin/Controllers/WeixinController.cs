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
        public WeixinController(IOptions<WeixinSetting> weixinSetting)
        {
            this.weixinSetting = weixinSetting.Value;
            
            getAccessToken();
        }
        // GET: api/Weixin
        [HttpGet]
        public ActionResult Get(string signature, string timestamp, string nonce, string echostr)
        {
            //接入微信
            //1）将token、timestamp、nonce三个参数进行字典序排序 
            //2）将三个参数字符串拼接成一个字符串进行sha1加密 
            //3）开发者获得加密后的字符串可与signature对比，标识该请求来源于微信
            string token = this.weixinSetting.Token;
            var str = string.Join("", new[] { token, timestamp, nonce }.OrderBy(o => o));
            var sha1 = SHA1.Create();
            var dataHashed = sha1.ComputeHash(Encoding.UTF8.GetBytes(str));
            string hash = BitConverter.ToString(dataHashed).Replace("-", "").ToLower();
            return Content(hash == signature ? echostr : "");
        }

        // GET: api/Weixin/5
        [HttpGet("{id}", Name = "Get")]
        public string Get(int id)
        {
            return "value";
        }
        
        // POST: api/Weixin
        [HttpPost]
        public string Post()//string timestamp, string nonce, string openid)//[FromBody]string value)
        {
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
            switch (msgType)
            {
                case "subscribe"://关注
                    break;
                case "unsubscribe"://取消关注
                    break;
            }
            return "success";
        }
        
        private void getUserInfo(string openid)
        {
            getAccessToken();
            string url = string.Format("https://api.weixin.qq.com/cgi-bin/user/info?access_token={0}&openid={1}&lang=zh_CN", this.access_token, openid);
            using (var client = new HttpClient())
            {
                var json = client.GetStringAsync(url).Result;
                UserInfo m = JsonConvert.DeserializeObject<UserInfo>(json);
                if (m.errcode == 0)
                {
                }
                else
                {
                    throw new WeixinException(m.errcode, m.errmsg);
                }
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
