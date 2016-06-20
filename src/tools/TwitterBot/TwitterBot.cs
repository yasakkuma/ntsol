/* TwitterBotクラスファイル */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using CoreTweet;

/* TwitterBotパッケージ名前空間 */
namespace ntsol.Tools.TwitterBot
{
    /// <summary>
    /// TwitterBotクラス
    /// </summary>
    public class TwitterBot
    {
        /// <summary>
        /// Twitterアカウント認証用トークン。
        /// </summary>
        private Tokens token;

        /// <summary>
        /// ランダム辞書ファイル。
        /// </summary>
        private string randomDicFile = string.Empty;

        private string replyDicFile = string.Empty;

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
        /// Botの初期化処理を行う。
        /// </summary>
        /// <remarks>Botのプロパティの設定を行い、トークンを作成する。</remarks>
        /// <exception cref="InvalidOperationException">トークンの作成に必要な情報が未設定の場合。</exception>
        public void Initialize(string botName, string consumerKey,
            string consumerSecret, string accessToken, string accessTokenSecret)
        {
            // Botの初期化を行う。
            BotName = botName;
            ConsumerKey = consumerKey;
            ConsumerSecret = consumerSecret;
            AccessToken = accessToken;
            AccessTokenSecret = accessTokenSecret;

            // 辞書ファイルのファイル名を設定
            randomDicFile = "Random" + BotName + "Dic.txt";
            replyDicFile = "Reply" + BotName + "Dic.txt";

            // トークン生成
            token = Tokens.Create(this.ConsumerKey,
                this.ConsumerSecret,
                this.AccessToken,
                this.AccessTokenSecret);
        }

        /// <summary>
        /// パラメータに設定した内容でツイートする。
        /// </summary>
        /// <param name="tweet">ポストする文字列</param>
        /// <exception cref="InvalidOperationException">トークンが未作成の場合</exception>
        /// <exception cref="TwitterException">ツイートが重複した場合</exception>
        public void Post(string tweet)
        {
            // トークンの作成状態をチェック
            if (token == null)
            {
                throw new InvalidOperationException("トークンが生成されていません。\n" + 
                    "CreateToken()を使用してトークンを生成してください。");
            }

            // ツイート
            token.Statuses.Update(new {status = tweet});
        }

        /// <summary>
        /// ランダムツイートする。
        /// </summary>
        /// <remarks>辞書ファイルから取得した文字列をランダムにツイートする。</remarks>
        /// <exception cref="FileNotFoundException">定義したファイルが存在しない場合。</exception>
        /// <exception cref="ntsol.Tools.TwitterBot.Post"></exception>
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
            Post(randomPostList[random.Next(randomPostList.Count)]);
        }

        /// <summary>
        /// 特定のリプライに対して返信する。
        /// </summary>
        public void ReplyPost()
        {

        }

        /// <summary>
        /// 特定のツイートに対してリプライする。
        /// </summary>
        public void TLReplyPost()
        {
            List<Status> statusList = new List<Status>();
            // タイムラインを取得
            foreach(Status status in token.Statuses.HomeTimeline(count => 100))
            {
                if(status.User.ScreenName != token.ScreenName)
                {
                    statusList.Add(status);
                    Console.WriteLine(status.User.ScreenName + ":" +status.Text);
                }
            }
            //token. = "Update";
            token.Account.UpdateProfile("testName");
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
