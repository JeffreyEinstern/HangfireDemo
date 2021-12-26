using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace QiYeWeiXinNotify
{
    #region 数据类型

    public class Singleton<T> where T : class, new()
    {
        private static T instance;

        public static T Ins
        {
            get
            {
                if (instance == null)
                {
                    instance = new T();
                }
                return instance;
            }
        }

        protected Singleton()
        {
            Init();
        }

        public void Dispose()
        {
            instance = null;
        }

        public virtual void Init()
        {
        }
    }

    public class SendResult
    {
        /// <summary>
        ///
        /// </summary>
        public int errcode { get; set; }

        /// <summary>
        ///
        /// </summary>
        public string errmsg { get; set; }

        /// <summary>
        ///
        /// </summary>
        public string msgid { get; set; }
    }

    public class TokenData
    {
        /// <summary>
        ///
        /// </summary>
        public int errcode { get; set; }

        /// <summary>
        ///
        /// </summary>
        public string errmsg { get; set; }

        /// <summary>
        ///
        /// </summary>
        public string access_token { get; set; }

        /// <summary>
        ///
        /// </summary>
        public int expires_in { get; set; }
    }

    public class TextData
    {
        public string content { get; set; }
    }

    public class MessageData
    {
        /// <summary>
        ///
        /// </summary>
        public string touser { get; set; }

        /// <summary>
        ///
        /// </summary>
        public string toparty { get; set; }

        /// <summary>
        ///
        /// </summary>
        public string totag { get; set; }

        /// <summary>
        ///
        /// </summary>
        public string msgtype = "text";

        /// <summary>
        ///
        /// </summary>
        public int agentid = 1000002;

        /// <summary>
        ///
        /// </summary>
        public TextData text { get; set; }

        /// <summary>
        ///
        /// </summary>
        public int safe { get; set; }

        /// <summary>
        ///
        /// </summary>
        public int enable_id_trans { get; set; }

        /// <summary>
        ///
        /// </summary>
        public int enable_duplicate_check { get; set; }

        /// <summary>
        ///
        /// </summary>
        public int duplicate_check_interval { get; set; }
    }

    public class SendMsgHistory
    {
        public long Id { get; set; }

        public string BILL_TEMPLATE_NAME { get; set; }
        public string BILL_NO { get; set; }
        public string BILL_SUBMIT_USER_NAME { get; set; }
        public string ReceivedUserId { get; set; }
        public string WxUser { get; set; }
        public string CreateTime { get; set; }
        
    }
    public class BillMessage
    {
        public string[] User { get; set; }
        public string BILL_SUBMIT_USER_NAME { get; set; }
        public string BILL_TEMPLATE_NAME { get; set; }
        public string BILL_NO { get; set; }
        public string BILL_AMOUNT { get; set; }
        public string BILL_SUBMIT_DATETIME { get; set; }
        public string URL { get; set; }
    }

    #endregion 数据类型

    public class NotifyMessage
    {
        #region 属性

        private const string GET = "GET";
        private const string POST = "POST";
        private const int Timeout = 2000;
        private string urlSend = "https://qyapi.weixin.qq.com/cgi-bin/message/send?access_token=";
        private BillMessage mould;
        private int count = 0;
        private int limitCount = 10;

        #endregion 属性

        public NotifyMessage()
        {
        }

        public async Task<bool> SendMessage(BillMessage mould)
        {
            if (this.mould == null)
            {
                this.mould = mould;
            }
            string url = urlSend + TokenHelper.Ins.token.ToString();
            HttpWebRequest httpWebRequest = (HttpWebRequest)HttpWebRequest.Create(url);
            MessageData messageData = new MessageData();
            messageData.touser = GetToUser();

            TextData textData = new TextData();
            textData.content = GetTextContent();
            messageData.text = textData;
            string data = JsonConvert.SerializeObject(messageData);
            //字符串转换为字节码
            byte[] bs = Encoding.UTF8.GetBytes(data);
            httpWebRequest.ContentType = "application/json";
            //参数数据长度
            httpWebRequest.ContentLength = bs.Length;
            //设置请求类型
            httpWebRequest.Method = POST;
            //设置超时时间
            httpWebRequest.Timeout = Timeout;
            //将参数写入请求地址中
            httpWebRequest.GetRequestStream().Write(bs, 0, bs.Length);
            HttpWebResponse httpWebResponse = await httpWebRequest.GetResponseAsync() as HttpWebResponse;
            //读取返回数据
            StreamReader streamReader = new StreamReader(httpWebResponse.GetResponseStream(), Encoding.UTF8);
            string json = streamReader.ReadToEnd();
            streamReader.Close();
            httpWebResponse.Close();
            httpWebRequest.Abort();
            SendResult sendResult = JsonConvert.DeserializeObject<SendResult>(json);
            if (sendResult.errcode == 0)
            {
                return true;
            }
            else if (sendResult.errcode == 40014)
            {
                if (count >= limitCount)
                {
                    return false;
                }
                Console.WriteLine("Token 无效重新请求");
                await TokenHelper.Ins.GetToken();
                await SendMessage(this.mould);
                count += 1;
            }
            return true;
        }

        public string GetToUser()
        {
            if (mould.User == null || mould == null)
            {
                return string.Empty;
            }
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < mould.User.Length; i++)
            {
                builder.Append(mould.User[i]);
                builder.Append("|");
            }
            return builder.ToString();
        }

        private string GetLink()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("<a href=\"link\"> 去审批 </a>");
            string link = builder.ToString().Replace("link", mould.URL);
            return link;
        }

        private string GetApplicant()
        {
            return "申请人:" + mould.BILL_SUBMIT_USER_NAME;
        }

        private string GetDocumentType()
        {
            return "单据类型:" + mould.BILL_TEMPLATE_NAME;
        }

        private string GetDocumentNumber()
        {
            return "单据号:" + mould.BILL_NO;
        }

        private string GetAmount()
        {
            return "金额:" + mould.BILL_AMOUNT;
        }

        private string GetBlDate()
        {
            return "提单日期:" + mould.BILL_SUBMIT_DATETIME;
        }

        public string GetTextContent()
        {
            StringBuilder builder = new StringBuilder();

            builder.Append("您有待审批的信息").AppendLine();
            builder.Append(GetApplicant()).AppendLine();
            builder.Append(GetDocumentType()).AppendLine();
            builder.Append(GetDocumentNumber()).AppendLine();
            builder.Append(GetAmount()).AppendLine();
            builder.Append(GetBlDate()).AppendLine();
            builder.Append(GetLink()).AppendLine();
            return builder.ToString();
        }
    }
}
