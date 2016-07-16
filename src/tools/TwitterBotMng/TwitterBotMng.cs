/* TwitterBotMngクラスファイル */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;
using ntsol.Tools.TwitterBotLib;

/* TwitterBotMngパッケージ名前空間 */
namespace ntsol.Tools.TwitterBotMng
{
    /// <summary>
    /// TwitterBot管理クラス
    /// </summary>
    public class TwitterBotMng
    {
        /// <summary>
        /// botのディクショナリ
        /// </summary>
        Dictionary<string, TwitterBot> botDic;

        /// <summary>
        /// ランダムタイマーのディクショナリ
        /// </summary>
        Dictionary<string, System.Threading.Timer> randomTimerDic;

        /// <summary>
        /// リプライタイマーのディクショナリ
        /// </summary>
        Dictionary<string, System.Threading.Timer> replyTimerDic;

        /// <summary>
        /// TwitterBotMngクラスのメインメソッド
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            TwitterBotMng twitterBotMng = new TwitterBotMng();
            twitterBotMng.Initialize();
            
            // 無限にタイマー処理を実施する。
            while(true)
            {
            }
        }

        private void Initialize()
        {
            // メンバ変数の初期化
            botDic = new Dictionary<string, TwitterBot>();
            randomTimerDic = new Dictionary<string, System.Threading.Timer>();
            replyTimerDic = new Dictionary<string, System.Threading.Timer>();

            // 設定ファイルからbot情報一覧を抽出
            TwitterBotInfoList xmlData = new TwitterBotInfoList();
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
                    TimerCallback randomCallback = new TimerCallback(RandomExecute);
                    randomTimerDic.Add(bot.BotName,
                        new System.Threading.Timer(randomCallback, bot, 0, bot.RandomTimer * 1000 * 60));

                    // リプライタイマー生成
                    TimerCallback replyCallback = new TimerCallback(ReplyExecute);
                    replyTimerDic.Add(bot.BotName,
                        new System.Threading.Timer(replyCallback, bot, 0, bot.ReplyTimer * 1000 * 60));

                }
            }
            catch
            {
                // ボットの初期処理に失敗した場合はアプリケーションを落とす。
                MessageBox.Show("botの初期化に失敗しました。",
                    "エラー",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                Application.Exit();
            }
        }

        /// <summary>
        /// ランダムポストを実行
        /// </summary>
        /// <param name="sender"></param>
        /// <remarks>ランダムポスト時間になった際、呼び出される。</remarks>
        private void RandomExecute(object sender)
        {
            try
            {
                ((TwitterBot)sender).RandomPost();
            }
            catch
            {
            }
        }

        /// <summary>
        /// リプライポストを実行
        /// </summary>
        /// <param name="sender"></param>
        /// <remarks>リプライポスト時間になった際、呼び出される。</remarks>
        private void ReplyExecute(object sender)
        {
            try
            {
                ((TwitterBot)sender).ReplyPost();
                ((TwitterBot)sender).TLReplyPost();

                // リプライのついでにフォロバも実行
                ((TwitterBot)sender).FollowedBack();
            }
            catch
            {
            }
        }
    }
}
