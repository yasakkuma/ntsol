/* TwitterBotクラスファイル */

using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;
using CoreTweet;

/* TwitterBotLibパッケージ名前空間 */
namespace ntsol.Tools.TwitterBotLib
{
    /// <summary>
    /// TwitterBotクラス
    /// </summary>
    public class TwitterBot : BaseTwitterBot
    {
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
            ReplyPostExecute(replyStack, replySettingFile);
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
            ReplyPostExecute(tlReplyStack, tlReplySettingFile);
        }

        /// <summary>
        /// リプライを実施し、リプライIDを記録する。
        /// </summary>
        /// <param name="replyStack">リプライのスタック</param>
        /// <param name="filepath">リプライIDを記録するファイルパス</param>
        /// <remarks>
        /// <para>リプライを実施する。</para>
        /// <para>同じリプライに反応しないようにリプライIDを記録する。</para>
        /// </remarks>
        private void ReplyPostExecute(Stack<ReplyTweetData> replyStack, string filepath)
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
