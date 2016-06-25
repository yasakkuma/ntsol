/* ReplyMessageDataクラスファイル */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

/* TwitterBotパッケージ名前空間 */
namespace ntsol.Tools.TwitterBot
{
    public class ReplyMessageData
    {
        [XmlAttribute]
        public string ReplyTrigger
        {
            get;
            set;
        }

        [XmlElement]
        public string ReplyMessage
        {
            get;
            set;
        }
    }
}
