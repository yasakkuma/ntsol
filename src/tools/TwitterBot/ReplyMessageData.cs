/* ReplyMessageDataクラスファイル */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

/* TwitterBotLibパッケージ名前空間 */
namespace ntsol.Tools.TwitterBotLib
{
    /// <summary>
    /// リプライに対する返信データクラス
    /// </summary>
    public class ReplyMessageData
    {
        #region プロパティ
        /// <summary>
        /// リプライトリガー
        /// </summary>
        [XmlAttribute]
        public string ReplyTrigger
        {
            get;
            set;
        }

        /// <summary>
        /// 返信用メッセージ
        /// </summary>
        [XmlElement]
        public string ReplyMessage
        {
            get;
            set;
        }
        #endregion
    }
}
