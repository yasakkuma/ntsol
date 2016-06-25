/* ReplyTweetDataクラスファイル */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/* TwitterBotLibパッケージ名前空間 */
namespace ntsol.Tools.TwitterBotLib
{
    /// <summary>
    /// リプライツイートデータ
    /// </summary>
    /// <remarks>リプライするメッセージとどのリプライに対する返信かを保持するクラス</remarks>
    public class ReplyTweetData
    {
        /// <summary>
        /// リプライメッセージ
        /// </summary>
        public string ReplyMessage
        {
            get;
            set;
        }

        /// <summary>
        /// リプライID
        /// </summary>
        public long ReplyId
        {
            get;
            set;
        }
    }
}
