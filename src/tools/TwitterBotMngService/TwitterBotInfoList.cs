/* TwitterBotInfoListクラスファイル */

using System.Collections.Generic;
using System.Xml.Serialization;

/* TwitterBotMngServiceパッケージ名前空間 */
namespace ntsol.Tools.TwitterBotMngService
{
    /// <summary>
    /// TwitterBot情報一覧格納クラス
    /// </summary>
    [XmlRoot("TwitterBotInfoList")]
    public class TwitterBotInfoList
    {
        #region プロパティ
        [XmlElement("TwitterBotInfo")]
        public List<TwitterBotInfo> TwitterBotList
        {
            get;
            set;
        }
        #endregion
    }
}
