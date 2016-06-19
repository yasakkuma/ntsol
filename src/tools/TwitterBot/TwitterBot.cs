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
        /// ランダム辞書ファイルパスを取得・設定する。
        /// </summary>
        public string RandomDicFile
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
            ConsumerKey = string.Empty;
            ConsumerSecret = string.Empty;
            AccessToken = string.Empty;
            AccessTokenSecret = string.Empty;
            RandomDicFile = string.Empty;

        }

        /// <summary>
        /// トークンを生成する。
        /// </summary>
        /// <remarks>
        /// <para>コンシューマキー、コンシューマシークレット、</para>
        /// <para>アクセストークン、アクセストークンシークレット設定後呼び出す。</para>
        /// </remarks>
        public void CreateToken()
        {
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
            if (!File.Exists(RandomDicFile))
            {
                throw new FileNotFoundException(
                    string.Format("ファイル\"{0}\"が見つかりません。", RandomDicFile));
            }

            using (StreamReader reader = new StreamReader(RandomDicFile))
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

        public void ReplyPost()
        {

        }
    }
}
