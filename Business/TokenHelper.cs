using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace QiYeWeiXinNotify
{
    public class TokenHelper : Singleton<TokenHelper>
    {
        #region 属性

        private const int Timeout = 2000;
        private const string GET = "GET";
        private string urlToken = "https://qyapi.weixin.qq.com/cgi-bin/gettoken?";
        private string corpid = "wwaa9cec8328775e88";
        private string corpsecret = "goY7qN86enRPbtZNZQMyThB3RP_h9yJmJldDOp6N6y0";
        public StringBuilder token = new StringBuilder();

        #endregion 属性

        public override void Init()
        {
            base.Init();
            SetUrlToken(corpid, corpsecret);
            //token.Append("bctPX8pFs7YZQV80sYKkXoe4Bj-_MSnwD5uUqwuOBXoP9uoiYGZEJ3W-Dglflp8q_6Iqr5GWJIfY8DJX9FysA3D-V0Gvb9AvtNqFRMz467nF8WL-fq4U1ufO_ZP8r_P7z6kUFzqWffbithRvMTHMeTik8kbc7RzFSRodIV1pYKNnhjNuJ-Wxf4c-_f7FJvebzWaGgNfe1-eK2XJPtJ3I7Q");
        }

        public void SetUrlToken(string corpid, string corpsecret)
        {
            urlToken = urlToken + "corpid=" + corpid + "&corpsecret=" + corpsecret;
        }

        public async Task<bool> GetToken()
        {
            try
            {
                string data = string.Empty;
                HttpWebRequest httpWebRequest = (HttpWebRequest)HttpWebRequest.Create(urlToken);
                httpWebRequest.Method = GET;
                httpWebRequest.Timeout = Timeout;
                HttpWebResponse response = await httpWebRequest.GetResponseAsync() as HttpWebResponse;
                using (StreamReader streamReader = new StreamReader(response.GetResponseStream()))
                {
                    data = streamReader.ReadToEnd();
                }
                response.Close();
                httpWebRequest.Abort();
                if (!string.IsNullOrEmpty(data))
                {
                    TokenData tokenData = JsonConvert.DeserializeObject<TokenData>(data);
                    //获取Token成功
                    if (tokenData.errcode == 0)
                    {
                        token.Clear();
                        token.Append(tokenData.access_token);
                    }
                    else
                    {
                        Console.WriteLine("返回Token请求失败 错误码：" + tokenData.errcode);
                        return false;
                    }
                }
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Token请求失败 ：" + e.Message);
                return false;
            }
        }
    }

}
