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
        [XmlAttribute]
        public string BotName
        {
            get;
            set;
        }

        [XmlElement]
        public string ConsumerKey
        {
            get;
            set;
        }

        [XmlElement]
        public string ConsumerSecret
        {
            get;
            set;
        }

        [XmlElement]
        public string AccessToken
        {
            get;
            set;
        }

        [XmlElement]
        public string AccessTokenSecret
        {
            get;
            set;
        }

        [XmlElement]
        public string RandomTimer
        {
            get;
            set;
        }

        [XmlElement]
        public string ReplyTimer
        {
            get;
            set;
        }
    }
}
