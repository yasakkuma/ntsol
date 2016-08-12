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
        #region メンバ変数
        /// <summary>
        /// bot一覧
        /// </summary>
        List<TwitterBot> botList;

        /// <summary>
        /// ランダムタイマー一覧
        /// </summary>
        List<Timer> randomTimerList;

        /// <summary>
        /// リプライタイマー一覧
        /// </summary>
        List<Timer> replyTimerList;

        #endregion

        #region コンストラクタ
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public TwitterBotMng()
        {
            InitializeComponent();
        }
        #endregion

        #region メソッド
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
            foreach(Timer randomTimer in randomTimerList)
            {
                randomTimer.Dispose();
            }

            foreach(Timer replyTimer in replyTimerList)
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
            botList = new List<TwitterBot>();
            randomTimerList = new List<Timer>();
            replyTimerList = new List<Timer>();

            /* 
            サービスの場合はカレントディレクトリがプログラムの配置場所と異なる。
            設定ファイルをカレントパスで指定できるようにプログラム配置場所を
            カレントディレクトリに設定する。
            */
                string currPath = 
                Directory.GetParent(System.Reflection.Assembly.GetEntryAssembly().Location).ToString();

            // 設定ファイルからbot情報一覧を抽出
            TwitterBotInfoList xmlData = new TwitterBotInfoList();
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
                    botList.Add(bot);

                    // ランダムポストのタイマー生成
                    Timer randomTimer = new Timer();
                    randomTimer.Elapsed += RandomExecute;
                    randomTimer.Enabled = true;
                    randomTimer.AutoReset = true;
                    randomTimer.Interval = bot.RandomTimer * 1000 * 60;
                    randomTimerList.Add(randomTimer);

                    // リプライタイマー生成
                    Timer replyTimer = new Timer();
                    replyTimer.Elapsed += ReplyExecute;
                    replyTimer.Enabled = true;
                    replyTimer.AutoReset = true;
                    replyTimer.Interval = bot.ReplyTimer * 1000 * 60;
                    replyTimerList.Add(replyTimer);

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
                foreach(Timer timer in randomTimerList)
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
                    botList[index].RandomPost();
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
                foreach (Timer timer in replyTimerList)
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
                    botList[index].ReplyPost();
                    botList[index].TLReplyPost();

                    // フォロバも実行
                    botList[index].FollowedBack();
                }
            }
            catch
            {
            }
        }
        #endregion
    }
}
