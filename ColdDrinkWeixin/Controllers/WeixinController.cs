using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

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
        }
        // GET: api/Weixin
        [HttpGet]
        public string Get(string signature, string timestamp, string nonce, string echostr)
        {
            //1）将token、timestamp、nonce三个参数进行字典序排序 
            //2）将三个参数字符串拼接成一个字符串进行sha1加密 
            //3）开发者获得加密后的字符串可与signature对比，标识该请求来源于微信
            string token = this.weixinSetting.Token;
            var str = string.Join("", new[] { token, timestamp, nonce }.OrderBy(o => o));
            var sha1 = SHA1.Create();
            var dataHashed = sha1.ComputeHash(Encoding.UTF8.GetBytes(str));
            string hash = BitConverter.ToString(dataHashed).Replace("-", "");
            hash = hash.ToLower();
            return hash == signature ? echostr : "";
        }

        // GET: api/Weixin/5
        [HttpGet("{id}", Name = "Get")]
        public string Get(int id)
        {
            return "value";
        }
        
        // POST: api/Weixin
        [HttpPost]
        public void Post([FromBody]string value)
        {
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
