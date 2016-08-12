/* TwitterBotInfoクラスファイル */

using System.Xml.Serialization;

/* TwitterBotMngServiceパッケージ名前空間 */
namespace ntsol.Tools.TwitterBotMngService
{
    /// <summary>
    /// TwitterBot情報格納クラス
    /// </summary>
    public class TwitterBotInfo
    {
        #region プロパティ
        /// <summary>
        /// ボット名称
        /// </summary>
        [XmlAttribute]
        public string BotName
        {
            get;
            set;
        }

        /// <summary>
        /// コンシューマキー
        /// </summary>
        [XmlElement]
        public string ConsumerKey
        {
            get;
            set;
        }

        /// <summary>
        /// コンシューマシークレット
        /// </summary>
        [XmlElement]
        public string ConsumerSecret
        {
            get;
            set;
        }

        /// <summary>
        /// アクセストークン
        /// </summary>
        [XmlElement]
        public string AccessToken
        {
            get;
            set;
        }

        /// <summary>
        /// アクセストークンシークレット
        /// </summary>
        [XmlElement]
        public string AccessTokenSecret
        {
            get;
            set;
        }

        /// <summary>
        /// ランダムツイートを制御するタイマー
        /// </summary>
        [XmlElement]
        public string RandomTimer
        {
            get;
            set;
        }

        /// <summary>
        /// リプライツイートを制御するタイマー
        /// </summary>
        [XmlElement]
        public string ReplyTimer
        {
            get;
            set;
        }
        #endregion
    }
}
