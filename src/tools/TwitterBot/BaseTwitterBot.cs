/* BaseTwitterBotインターフェースファイル */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreTweet;

/* TwitterBotLibパッケージ名前空間 */
namespace ntsol.Tools.TwitterBotLib
{
    /// <summary>
    /// TwitterBotの基底クラス
    /// </summary>
    public abstract class BaseTwitterBot
    {
        /// <summary>
        /// リトライ回数
        /// </summary>
        internal const short retryNum = 10;

        /// <summary>
        /// 最後のリプライID
        /// </summary>
        internal long lastReplyId = 0;

        /// <summary>
        /// Twitterアカウント認証用トークン。
        /// </summary>
        internal Tokens token;

        /// <summary>
        /// ランダム辞書ファイル。
        /// </summary>
        internal string randomDicFile = string.Empty;

        /// <summary>
        /// リプライ辞書ファイル。
        /// </summary>
        internal string replyDicFile = string.Empty;

        /// <summary>
        /// リプライ設定ファイル。
        /// </summary>
        internal string replySettingFile = string.Empty;

        /// <summary>
        /// TLリプライ設定ファイル。
        /// </summary>
        internal string tlReplySettingFile = string.Empty;

        /// <summary>
        /// 設定ファイル格納ディレクトリ
        /// </summary>
        internal const string settingDir = @"Setting\";

        /// <summary>
        /// ボット名称
        /// </summary>
        public string BotName
        {
            get;
            set;
        }

        /// <summary>
        /// コンシューマキーを取得・設定する。
        /// </summary>
        public string ConsumerKey
        {
            get;
            set;
        }

        /// <summary>
        /// コンシューマシークレットを取得・設定する。
        /// </summary>
        public string ConsumerSecret
        {
            get;
            set;
        }

        /// <summary>
        /// アクセストークンを取得・設定する。
        /// </summary>
        public string AccessToken
        {
            get;
            set;
        }

        /// <summary>
        /// アクセストークンシークレットを取得・設定する。
        /// </summary>
        public string AccessTokenSecret
        {
            get;
            set;
        }

        /// <summary>
        /// コンストラクタ。
        /// </summary>
        /// <remarks>Botのメンバを初期化する。</remarks>
        public BaseTwitterBot()
        {
            BotName = string.Empty;
            ConsumerKey = string.Empty;
            ConsumerSecret = string.Empty;
            AccessToken = string.Empty;
            AccessTokenSecret = string.Empty;
        }

        /// <summary>
        /// Botの生成処理を行う。
        /// </summary>
        /// <param name="botName">Bot名称</param>
        /// <param name="consumerKey">コンシューマキー</param>
        /// <param name="consumerSecret">コンシューマシークレット</param>
        /// <param name="accessToken">アクセストークン</param>
        /// <param name="accessTokenSecret">アクセストークンシークレット</param>
        /// <returns>作成したTwitterBotインスタンス</returns>
        /// <remarks>Botのプロパティの設定を行い、トークンを作成する。</remarks>
        /// <exception cref="InvalidOperationException">トークンの作成に必要な情報に誤りがある場合。</exception>
        public static TwitterBot CreateTwitterBot(string botName, string consumerKey,
            string consumerSecret, string accessToken, string accessTokenSecret)
        {
            TwitterBot bot = new TwitterBot();

            // Botの初期化を行う。
            bot.BotName = botName;
            bot.ConsumerKey = consumerKey;
            bot.ConsumerSecret = consumerSecret;
            bot.AccessToken = accessToken;
            bot.AccessTokenSecret = accessTokenSecret;

            // 辞書ファイルのファイル名を設定
            bot.randomDicFile = settingDir + "Random" + bot.BotName + "Dic.txt";
            bot.replyDicFile = settingDir + "Reply" + bot.BotName + "Dic.xml";
            bot.replySettingFile = settingDir + "Reply" + bot.BotName;
            bot.tlReplySettingFile = settingDir + "TLReply" + bot.BotName;

            // トークン生成
            bot.token = Tokens.Create(bot.ConsumerKey,
                bot.ConsumerSecret,
                bot.AccessToken,
                bot.AccessTokenSecret);

            try
            {
                // 正常にアクセスできるか確認
                bot.token.Statuses.HomeTimeline(count => 0);
            }
            catch
            {
                throw new InvalidOperationException("トークンの生成に失敗しました。\n" +
                    "ConsumerKey,ConsumerSecret,AccessToken,AccessTokenSecretが正しいか確認してください。");
            }

            return bot;
        }

        /// <summary>
        /// パラメータに設定した内容でツイートする。
        /// </summary>
        /// <param name="tweet">ポストする文字列</param>
        /// <exception cref="TwitterException">ツイートが重複した場合</exception>
        public void Post(string tweet)
        {
            // ツイート
            token.Statuses.Update(new { status = tweet });
        }

        /// <summary>
        /// リプライをポストする。
        /// </summary>
        /// <param name="tweet">リプライするツイート。</param>
        /// <param name="replyId">返信するリプライID</param>
        internal void ReplyPost(string tweet, long replyId)
        {
            try
            {
                // ツイート
                token.Statuses.Update(tweet, replyId);
                lastReplyId = replyId;
            }
            catch (TwitterException)
            {
                // 重複ツイートは握り潰す。
            }
        }

        /// <summary>
        /// フォローバックを実施する。
        /// </summary>
        /// <remarks>
        /// <para>フォロワーにフォローしていないユーザーがいる場合はフォローバックを行う。</para>
        /// <para>既にリムーブされていてフォローしているユーザーにはリムーブを行う。</para>
        /// </remarks>
        public void FollowedBack()
        {
            List<User> followedList = token.Friends.List().ToList();
            List<User> followerList = token.Followers.List().ToList();
            Dictionary<long?, User> followedDic = new Dictionary<long?, User>();
            Dictionary<long?, User> followerDic = new Dictionary<long?, User>();

            // フォロー一覧をディクショナリに追加
            foreach (User user in followedList)
            {
                followedDic.Add(user.Id, user);
            }

            // フォローしていないフォロワーがいたばあいフォローする。
            foreach (User user in followerList)
            {
                if (!followedDic.ContainsKey(user.Id))
                {
                    token.Friendships.Create(user_id => user.Id);
                }
                followerDic.Add(user.Id, user);
            }

            // リムーブされていた場合はこちらもリムーブする。
            foreach (User user in followedList)
            {
                if (!followerDic.ContainsKey(user.Id))
                {
                    token.Friendships.Destroy(user_id => user.Id);
                }
            }
        }

        /// <summary>
        /// アカウントの表示名を変更する。
        /// </summary>
        /// <param name="newName">新しい表示名</param>
        public void UpdateName(string newName)
        {
            token.Account.UpdateProfile(newName);
        }
    }
}
