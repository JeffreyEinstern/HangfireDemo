using System.Collections.Generic;
using System.Threading.Tasks;

namespace QiYeWeiXinNotify
{
    public class NorifyMgr : Singleton<NorifyMgr>
    {
        private Queue<BillMessage> notifyMessages = new Queue<BillMessage>();
        private readonly object obj = new object();

        public override void Init()
        {
            lock (obj)
            {
                base.Init();
                Polling();
            }
        }

        public void Send(BillMessage message)
        {
            lock (obj)
            {
                notifyMessages.Enqueue(message);
            }
        }

        private async void Polling()
        {
            while (true)
            {
                if (notifyMessages.Count > 0)
                {
                    BillMessage message = notifyMessages.Dequeue();
                    NotifyMessage notifyMessage = new NotifyMessage();
                    await notifyMessage.SendMessage(message);
                }
                await Task.Delay(500);
            }
        }
    }
}
