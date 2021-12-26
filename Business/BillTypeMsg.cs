using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QiYeWeiXinNotify
{
    /// <summary>
    /// 单据消息
    /// </summary>
    public class BillTypeMsg
    {
        public string[] User { get; set; }

        /// <summary>
        /// 申请人
        /// </summary>
        public string BILL_SUBMIT_USER_NAME { get; set; }
        /// <summary>
        /// 单据类型
        /// </summary>
        public string BILL_TEMPLATE_NAME { get; set; }
        /// <summary>
        /// 单据号
        /// </summary>
        public string BILL_NO { get; set; }
        /// <summary>
        /// 金额
        /// </summary>
        public string BILL_AMOUNT { get; set; }
        /// <summary>
        /// 提单日期
        /// </summary>
        public string BILL_SUBMIT_DATETIME { get; set; }

        /// <summary>
        /// 去审批
        /// </summary>
        public string URL { get; set; }

        /// <summary>
        /// 微信号
        /// </summary>
        public string USER_OA_LOGIN { get; set; }

        public string TASK_USER { get; set; }

        public string TASK_HEADSHIP { get; set; }
    }



    public class HangFireState
    {
        public long Id { get; set; }
        public long StateId { get; set; }

        public string StateName { get; set; }

        public string Arguments { get; set; }

        public DateTime CreatedAt { get; set; }

        public string InvocationData { get;set; }

    }

    public interface ICompanyRepository
    {
        public Task<IEnumerable<HangFireState>> GetHangFireStates();

        public Task<IEnumerable<BillTypeMsg>> SendMessage();
    }

    public class CompanyRepository : ICompanyRepository
    {
        private readonly DapperContext _context;

        public CompanyRepository(DapperContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<HangFireState>> GetHangFireStates()
        {
            var query = "SELECT * FROM [HangFire].[State]";
            using (var connection = _context.CreateConnection())
            {
                var companies = await connection.QueryAsync<HangFireState>(query);
                if (companies == null) { return new List<HangFireState>(); }    
                foreach (var item in companies.ToList())
                {
                    Console.WriteLine("msg {0} Recurring job completed successfully!msg:Name:{1},Reason:{2}", DateTime.Now.ToString(),item.StateName,item.CreatedAt);
                }
                await TokenHelper.Ins.GetToken();
                BillMessage mould = new BillMessage();
                mould.User = new string[] { "TianYongHeng", "NieXiaoCun", "xiaopei.nie" };
                // mould.users = new string[] { "TianYongHeng" };
                mould.BILL_SUBMIT_USER_NAME = "NXC";
                mould.BILL_TEMPLATE_NAME = "测试类型";
                mould.BILL_NO = "23213132131";
                mould.BILL_AMOUNT = "8563.24元";
                mould.BILL_SUBMIT_DATETIME = "2021-12-23";
                mould.URL = "https://www.baidu.com/";
                //await notifyMessage.SendMessage(mould);
                NorifyMgr.Ins.Send(mould);
                return companies.ToList();
            }
        }

        public async Task<IEnumerable<BillTypeMsg>> SendMessage()
        {
            // 1.查询当前哪些单据需要发布消息
            // 2.判断该单据对应人今天是否已经发过,目前一天发只一次
            // 3.调用企业微信接口，发消息
            var query = @"select B.BILL_NO,B.BILL_TEMPLATE_NAME,B.BILL_AMOUNT,B.BILL_SUBMIT_DATETIME,T.USER_NAME+'('+T.USER_CODE+')' AS BILL_SUBMIT_USER_NAME,TASK_USER,TASK_HEADSHIP,  TASK_FORM_ID,TASK_TYPE_NAME,TASK_STATUS,TASK_USER,TASK_LAST_USER_NAME,TASK_RESULT,TASK_LAST_DATETIME,TASK_LAST_HEADSHIP_NAME,TASK_KIND_NAME,TASK_CREATE_DATETIME   
                            from VBILL_TASK_LIST inner join dbo.TBILL B on B.BILL_ID = VBILL_TASK_LIST.TASK_FORM_ID
                            INNER JOIN dbo.TUSER T ON B.BILL_SUBMIT_USER = T.USER_ID
                            WHERE TASK_STATUS = 'N' and(TASK_STATUS <> 'I')
                            AND TASK_PROCESS_TYPE = 'B'   and B.BILL_FLAG <> 'DFT'   AND B.BILL_FLAG IN('ACC','ADT','APP') 
                            and TASK_KIND_NAME in('财务审核', '财务审批', '业务审批')";
            var sql_one= @"Select USER_OA_LOGIN,USER_NAME+'('+USER_CODE+')' AS UserName,*  From TUSER where USER_ID ='{0}'";
            using (var connection = _context.CreateConnection())
            {
                var billsList = await connection.QueryAsync<BillTypeMsg>(query);
                if (billsList == null) { return new List<BillTypeMsg>(); }
                foreach (var item in billsList.ToList())
                {
                    var billInfo = new BillTypeMsg();
                    if (item.TASK_HEADSHIP != "") 
                    {   
                        var heads_ip = item.TASK_HEADSHIP.Replace("▓▓", ",").Replace("▓", "");
                        billInfo = await connection.QueryFirstOrDefaultAsync<BillTypeMsg>(string.Format(sql_one, heads_ip));
                    }
                    else {
                        var heads_ip = item.TASK_USER.Replace("▓▓", ",").Replace("▓", "");
                        billInfo = await connection.QueryFirstOrDefaultAsync<BillTypeMsg>(string.Format(sql_one, heads_ip));
                       
                    }
                    if (billInfo != null)
                    {
                        item.USER_OA_LOGIN = billInfo.USER_OA_LOGIN;
                        if (item.User == null)
                        {
                            item.User = new string[] { };
                        }
                        item.User.Append(billInfo.USER_OA_LOGIN);
                    }
                    await TokenHelper.Ins.GetToken();
                    BillMessage mould = new BillMessage();
                    mould.User = new string[] { "TianYongHeng", "NieXiaoCun", "xiaopei.nie" };
                    mould.BILL_SUBMIT_USER_NAME = item.BILL_SUBMIT_USER_NAME;
                    mould.BILL_TEMPLATE_NAME = item.BILL_TEMPLATE_NAME;
                    mould.BILL_NO = item.BILL_NO;
                    mould.BILL_AMOUNT = item.BILL_AMOUNT;
                    mould.BILL_SUBMIT_DATETIME = item.BILL_SUBMIT_DATETIME;
                    mould.URL = "https://www.baidu.com/";
                    NorifyMgr.Ins.Send(mould);
                    Console.WriteLine("msg {0} Recurring job completed successfully!msg:BILL_NO:{1},Reason:{2}", DateTime.Now.ToString(), item.BILL_NO, item.BILL_TEMPLATE_NAME);
                }
                return billsList.ToList();
            }
        }
    }


}
