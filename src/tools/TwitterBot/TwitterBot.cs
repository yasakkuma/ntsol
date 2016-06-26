/* TwitterBotクラスファイル */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using ntsol.Tools.XmlReader;
using System.Text.RegularExpressions;
using CoreTweet;

/* TwitterBotLibパッケージ名前空間 */
namespace ntsol.Tools.TwitterBotLib
{
    /// <summary>
    /// TwitterBotクラス
    /// </summary>
    public class TwitterBot
    {
        /// <summary>
        /// リトライ回数
        /// </summary>
        private const short retryNum = 10;

        /// <summary>
        /// 最後のリプライID
        /// </summary>
        private long lastReplyId = 0;

        /// <summary>
        /// Twitterアカウント認証用トークン。
        /// </summary>
        private Tokens token;

        /// <summary>
        /// ランダム辞書ファイル。
        /// </summary>
        private string randomDicFile = string.Empty;

        /// <summary>
        /// リプライ辞書ファイル。
        /// </summary>
        private string replyDicFile = string.Empty;

        /// <summary>
        /// リプライ設定ファイル。
        /// </summary>
        private string replySettingFile = string.Empty;

        /// <summary>
        /// TLリプライ設定ファイル。
        /// </summary>
        private string tlReplySettingFile = string.Empty;

        /// <summary>
        /// 設定ファイル格納ディレクトリ
        /// </summary>
        private const string settingDir = @"Setting\";

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
        /// TwitterBotクラスのコンストラクタ。
        /// </summary>
        /// <remarks>Botのメンバを初期化する。</remarks>
        public TwitterBot()
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
            } catch
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
        private void ReplyPost(string tweet, long replyId)
        {
            try
            {
                // ツイート
                token.Statuses.Update(tweet, replyId);
                lastReplyId = replyId;
            } catch(TwitterException)
            {
                // 重複ツイートは握り潰す。
            }
        }

        /// <summary>
        /// ランダムツイートする。
        /// </summary>
        /// <remarks>辞書ファイルから取得した文字列をランダムにツイートする。</remarks>
        /// <exception cref="FileNotFoundException">定義したファイルが存在しない場合。</exception>
        /// <exception cref="ntsol.Tools.TwitterBotLib.Post"></exception>
        public void RandomPost()
        {
            List<string> randomPostList = new List<string>();

            // ファイルの存在チェック
            if (!File.Exists(randomDicFile))
            {
                throw new FileNotFoundException(
                    string.Format("ファイル\"{0}\"が見つかりません。", randomDicFile));
            }

            using (StreamReader reader = new StreamReader(randomDicFile))
            {
                string readLine;
                
                // 辞書ファイルからポストする一覧を取得する。
                while ((readLine = reader.ReadLine()) != null)
                {
                    // コメント行は無視
                    if (readLine[0] == '#')
                    {
                        continue;
                    }
                    randomPostList.Add(readLine);
                }
            }

            // 乱数値を生成し、ツイート
            Random random = new Random();

            // 重複ツイートの場合はリトライする。
            for (int i = 0; i < retryNum; i++)
            {
                try
                {
                    Post(randomPostList[random.Next(randomPostList.Count)]);
                    return;
                }
                catch (TwitterException)
                {
                    // 重複ツイートは握り潰す。
                }
            }
        }

        /// <summary>
        /// 特定のリプライに対して返信する。
        /// </summary>
        /// <exception cref="InvalidOperationException">辞書ファイルに重複トリガーが設定されていた場合</exception>
        /// <exception cref="TwitterException">トークンが存在しない場合</exception>
        public void ReplyPost()
        {
            Dictionary<string, string> replyTriggerDic = new Dictionary<string, string>();
            Stack<ReplyTweetData> replyStack = new Stack<ReplyTweetData>();

            try
            {
                // 辞書ファイルからリプライトリガーメッセージとリプライメッセージを取り出す。
                ReplyTableData xmlData = new ReplyTableData();
                XmlReader.XmlReader.GetInstance.Read(replyDicFile, ref xmlData);

                foreach (ReplyMessageData message in xmlData.ReplyMessageDataList)
                {
                    replyTriggerDic.Add(message.ReplyTrigger, message.ReplyMessage);
                }
            } catch
            {
                throw new InvalidOperationException("リプライ辞書ファイルに重複トリガーが設定されています。");
            }

            // ファイルの存在チェック
            if (!File.Exists(replySettingFile))
            {
                using (StreamWriter writer = new StreamWriter(File.Create(replySettingFile)))
                {
                    writer.WriteLine("0");
                }
            }

            //　最後のリプライIDを取得
            using (StreamReader reader = new StreamReader(replySettingFile))
            {
                if (!Int64.TryParse(reader.ReadLine(), out lastReplyId))
                {
                    throw new InvalidOperationException("設定ファイル:" + replySettingFile + "が不正です。");
                }
            }

            // メンションを取得
            foreach (Status status in token.Statuses.MentionsTimeline(count => 50, trim_user => false))
            {
                // リプライ済みのツイートは読み飛ばす。
                if (status.Id <= lastReplyId)
                {
                    continue;
                }

                // メンションの宛先部分を削除して比較
                string compStr = status.Text.Substring(status.Text.IndexOf(" ") + 1);
                if(replyTriggerDic.ContainsKey(compStr))
                {
                    // リプライデータを生成
                    // (古いツイートから取得できない為、一時的にスタックにためる。)
                    ReplyTweetData replyTweet = new ReplyTweetData();
                    replyTweet.ReplyMessage = "@" + status.User.ScreenName + " " + replyTriggerDic[compStr];
                    replyTweet.ReplyId = status.Id;
                    replyStack.Push(replyTweet);
                }
            }

            // リプライを実施し、リプライIDを記録する。
            ReplyPostFinalize(replyStack, replySettingFile);
        }

        /// <summary>
        /// 特定のツイートに対してリプライする。
        /// </summary>
        /// /// <exception cref="InvalidOperationException">辞書ファイルに重複トリガーが設定されていた場合</exception>
        /// <exception cref="TwitterException">トークンが存在しない場合</exception>
        public void TLReplyPost()
        {
            Dictionary<string, string> tlReplyTriggerDic = new Dictionary<string, string>();
            Stack<ReplyTweetData> tlReplyStack = new Stack<ReplyTweetData>();

            try
            {
                // 辞書ファイルからTLリプライトリガーメッセージとTLリプライメッセージを取り出す。
                ReplyTableData xmlData = new ReplyTableData();
                XmlReader.XmlReader.GetInstance.Read(replyDicFile, ref xmlData);

                foreach (TLReplyMessageData message in xmlData.TLReplyMessageDataList)
                {
                    tlReplyTriggerDic.Add(message.TLReplyTrigger, message.TLReplyMessage);
                }
            }
            catch
            {
                throw new InvalidOperationException("リプライ辞書ファイルに重複トリガーが設定されています。");
            }

            // ファイルの存在チェック
            if (!File.Exists(tlReplySettingFile))
            {
                using (StreamWriter writer = new StreamWriter(File.Create(tlReplySettingFile)))
                {
                    writer.WriteLine("0");
                }
            }

            //　最後のリプライIDを取得
            using (StreamReader reader = new StreamReader(tlReplySettingFile))
            {
                if (!Int64.TryParse(reader.ReadLine(), out lastReplyId))
                {
                    throw new InvalidOperationException("設定ファイル:" + replySettingFile + "が不正です。");
                }
            }

            List<string> triggerArray = tlReplyTriggerDic.Keys.ToList();
            List<string> messageArray = tlReplyTriggerDic.Values.ToList();

            // タイムラインを取得
            foreach (Status status in token.Statuses.HomeTimeline(count => 100, exclude_replies => true))
            {
                // リプライ済みのツイートは読み飛ばす。
                if (status.Id <= lastReplyId)
                {
                    continue;
                }
                
                int index = 0;
                foreach (string trigger in triggerArray)
                {
                    // トリガーにマッチするデータがあるか検索
                    if (Regex.Match(@status.Text, @trigger, RegexOptions.None).Success)
                    {
                        // リプライデータを生成
                        // (古いツイートから取得できない為、一時的にスタックにためる。)
                        ReplyTweetData replyTweet = new ReplyTweetData();
                        replyTweet.ReplyMessage = "@" + status.User.ScreenName + " " + messageArray[index];
                        replyTweet.ReplyId = status.Id;
                        tlReplyStack.Push(replyTweet);
                        break;
                    }
                    index++;
                }
            }

            // リプライを実施し、リプライIDを記録する。
            ReplyPostFinalize(tlReplyStack, tlReplySettingFile);
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
                if(!followedDic.ContainsKey(user.Id))
                {
                    token.Friendships.Create(user_id => user.Id);
                }
                followerDic.Add(user.Id, user);
            }

            // リムーブされていた場合はこちらもリムーブする。
            foreach (User user in followedList)
            {
                if(!followerDic.ContainsKey(user.Id))
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

        /// <summary>
        /// リプライを実施し、リプライIDを記録する。
        /// </summary>
        /// <param name="replyStack">リプライのスタック</param>
        /// <param name="filepath">リプライIDを記録するファイルパス</param>
        private void ReplyPostFinalize (Stack<ReplyTweetData> replyStack, string filepath)
        {
            // まとめてリプライする。
            while(replyStack.Count > 0)
            {
                ReplyTweetData replyTweet = replyStack.Pop();
                ReplyPost(replyTweet.ReplyMessage, replyTweet.ReplyId);
            }

            // 最後のリプライを記録する。
            using (StreamWriter writer = new StreamWriter(filepath, false))
            {
                writer.WriteLine(lastReplyId);
            }
        }
    }
}
