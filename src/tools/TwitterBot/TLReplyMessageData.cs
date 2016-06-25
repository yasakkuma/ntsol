/* TLReplyMessageDataクラスファイル */
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
    /// タイムラインに対する返信データクラス
    /// </summary>
    public class TLReplyMessageData
    {
        /// <summary>
        /// タイムラインリプライトリガー
        /// </summary>
        [XmlAttribute]
        public string TLReplyTrigger
        {
            get;
            set;
        }

        /// <summary>
        /// タイムライン返信用メッセージ
        /// </summary>
        [XmlElement]
        public string TLReplyMessage
        {
            get;
            set;
        }
    }
}
