using System;
using System.IO;
using System.Collections.Generic;
using System.ServiceProcess;
using System.Timers;
using ntsol.Tools.XmlReader;
using ntsol.Tools.TwitterBotLib;

/* TwitterBotMngServiceパッケージ名前空間 */
namespace ntsol.Tools.TwitterBotMngService
{
    /// <summary>
    /// TwitterBot管理クラス
    /// </summary>
    public partial class TwitterBotMng : ServiceBase
    {
        /// <summary>
        /// botのディクショナリ
        /// </summary>
        Dictionary<string, TwitterBot> botDic;

        /// <summary>
        /// ランダムタイマーのディクショナリ
        /// </summary>
        Dictionary<string, Timer> randomTimerDic;

        /// <summary>
        /// リプライタイマーのディクショナリ
        /// </summary>
        Dictionary<string, Timer> replyTimerDic;

        // ポーリング間隔
        private readonly TimeSpan pollingInterval = new TimeSpan(0, 0, 30);

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public TwitterBotMng()
        {
            InitializeComponent();
        }

        /// <summary>
        /// サービスの開始処理。
        /// </summary>
        /// <param name="args"></param>
        /// <remarks><para>サービスの初期処理を行う。</para></remarks>
        protected override void OnStart(string[] args)
        {
            Initialize();
        }

        /// <summary>
        /// サービスの終了処理。
        /// </summary>
        /// <remarks><para>タイマーのリソースを解放する。</para></remarks>
        protected override void OnStop()
        {
            foreach(Timer randomTimer in randomTimerDic.Values)
            {
                randomTimer.Dispose();
            }

            foreach(Timer replyTimer in replyTimerDic.Values)
            {
                replyTimer.Dispose();
            }
        }

        /// <summary>
        /// botの初期化処理
        /// </summary>
        /// <remarks>各botのタイマーを設定する。</remarks>
        private void Initialize()
        {
            // メンバ変数の初期化
            botDic = new Dictionary<string, TwitterBot>();
            randomTimerDic = new Dictionary<string, Timer>();
            replyTimerDic = new Dictionary<string, Timer>();

            // 設定ファイルからbot情報一覧を抽出
            TwitterBotInfoList xmlData = new TwitterBotInfoList();
            string currPath = Directory.GetParent(System.Reflection.Assembly.GetEntryAssembly().Location).ToString();
            System.Environment.CurrentDirectory = currPath;
            XmlReader.XmlReader.GetInstance.Read(@"Setting\TwitterBotSetting.xml", ref xmlData);

            try
            {
                foreach (TwitterBotInfo twitterBot in xmlData.TwitterBotList)
                {
                    // bot情報からTwitterBotインスタンスを生成
                    TwitterBot bot = TwitterBot.CreateTwitterBot(twitterBot.BotName,
                        twitterBot.ConsumerKey,
                        twitterBot.ConsumerSecret,
                        twitterBot.AccessToken,
                        twitterBot.AccessTokenSecret,
                        Int32.Parse(twitterBot.RandomTimer),
                        Int32.Parse(twitterBot.ReplyTimer));
                    botDic.Add(twitterBot.BotName, bot);

                    // ランダムポストのタイマー生成
                    Timer randomTimer = new Timer();
                    randomTimer.Elapsed += RandomExecute;
                    randomTimer.Enabled = true;
                    randomTimer.AutoReset = true;
                    randomTimer.Interval = bot.RandomTimer * 1000 * 60;
                    randomTimerDic.Add(bot.BotName, randomTimer);

                    // リプライタイマー生成
                    Timer replyTimer = new Timer();
                    replyTimer.Elapsed += ReplyExecute;
                    replyTimer.Enabled = true;
                    replyTimer.AutoReset = true;
                    replyTimer.Interval = bot.ReplyTimer * 1000 * 60;
                    replyTimerDic.Add(bot.BotName, replyTimer);

                }
            }
            catch
            {
                // ボットの初期処理に失敗した場合はアプリケーションを落とす。
                OnStop();
            }
        }

        /// <summary>
        /// ランダムポストを実行
        /// </summary>
        /// <param name="sender">タイマーオブジェクト</param>
        /// <param name="e">イベントパラメータ</param>
        /// <remarks>ランダムポスト時間になった際、呼び出される。</remarks>
        private void RandomExecute(object sender, ElapsedEventArgs e)
        {
            try
            {
                int index = 0;
                bool hitflg = false;
                
                // タイマーオブジェクトからどのbotかを判断する。
                foreach(Timer timer in randomTimerDic.Values)
                {
                    if((Timer)sender == timer)
                    {
                        hitflg = true;
                        break;
                    }
                    index++;
                }

                // ランダムポスト
                if (hitflg)
                {
                    TwitterBot[] botArray = new TwitterBot[botDic.Values.Count];
                    botDic.Values.CopyTo(botArray, 0);
                    botArray[index].RandomPost();
                }
            }
            catch
            {
            }
        }

        /// <summary>
        /// リプライポストを実行
        /// </summary>
        /// <param name="sender">タイマーオブジェクト</param>
        /// <param name="e">イベントパラメータ</param>
        /// <remarks>リプライポスト時間になった際、呼び出される。</remarks>
        private void ReplyExecute(object sender, ElapsedEventArgs e)
        {
            try
            {
                int index = 0;
                bool hitflg = false;

                // タイマーオブジェクトからどのbotかを判断する。
                foreach (Timer timer in replyTimerDic.Values)
                {
                    if ((Timer)sender == timer)
                    {
                        hitflg = true;
                        break;
                    }
                    index++;
                }

                if (hitflg)
                {
                    // リプライとTLリプライを実行
                    TwitterBot[] botArray = new TwitterBot[botDic.Values.Count];
                    botDic.Values.CopyTo(botArray, 0);
                    botArray[index].ReplyPost();
                    botArray[index].TLReplyPost();

                    // フォロバも実行
                    botArray[index].FollowedBack();
                }
            }
            catch
            {
            }
        }
    }
}
