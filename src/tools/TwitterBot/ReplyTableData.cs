/* ReplyTableDataクラスファイル */
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
    /// リプライに対する返信の対応表データクラス。
    /// </summary>
    [XmlRoot("ReplyTable")]
    public class ReplyTableData
    {
        #region プロパティ
        /// <summary>
        /// リプライメッセージリスト
        /// </summary>
        [XmlElement("ReplyMessagePair")]
        public List<ReplyMessageData> ReplyMessageDataList
        {
            get;
            set;
        }

        /// <summary>
        /// TLリプライメッセージリスト
        /// </summary>
        [XmlElement("TLReplyMessagePair")]
        public List<TLReplyMessageData> TLReplyMessageDataList
        {
            get;
            set;
        }
        #endregion
    }
}
