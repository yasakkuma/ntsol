/* ReplyTableDataクラスファイル */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

/* TwitterBotパッケージ名前空間 */
namespace ntsol.Tools.TwitterBot
{
    [XmlRoot("ReplyTable")]
    public class ReplyTableData
    {
        [XmlElement("ReplyMessagePair")]
        public List<ReplyMessageData> ReplyMessageDataList { get; set; }
    }
}
