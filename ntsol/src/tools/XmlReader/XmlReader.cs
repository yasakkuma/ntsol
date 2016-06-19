/* XmlReaderクラスファイル */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml.Serialization;

/* XmlReaderパッケージ名前空間 */
namespace ntsol.Tools.XmlReader
{
    /// <summary>
    /// XML読み込みクラス
    /// </summary>
    public class XmlReader
    {
        ///<value>クラスインスタンス</value>
        private static XmlReader instance = null;

        /// <summary>
        /// インスタンスを取得するプロパティ。
        /// </summary>
        public static XmlReader GetInstance
        {
            get
            {
                // シングルトンでインスタンス生成
                if (instance == null)
                {
                    instance = new XmlReader();
                }

                return instance;
            }
        }

        /// <summary>
        /// XmlReaderクラスのコンストラクタ。
        /// </summary>
        /// <remarks>外部クラスからインスタンスの生成を抑制する。</remarks>
        private XmlReader()
        {
        }

        /// <summary>
        /// XMLファイル読み込みメソッド
        /// </summary>
        /// <param name="filePath">XMLファイルパス</param>
        /// <param name="xmlData">XMLデータ格納先のデータ型</param>
        /// <exception cref="FileNotFoundException">読み込むXMLファイルが見つからない場合</exception>
        /// <exception cref="InvalidOperationException">
        /// <para>XMLファイルがUTF-8でない場合</para>
        /// <para>XMLの要素がデータクラスと一致しない場合</para>
        /// </exception>
        public void Read<T>(String filePath, ref T xmlData)
        {

            // ファイルの存在チェック
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException(string.Format("ファイル\"{0}\"が見つかりません。", filePath));
            }

            try
            {
                // XMLファイル読み込み
                using (FileStream fileStream = new FileStream(filePath, FileMode.Open))
                {
                    // デシリアライズ
                    XmlSerializer serializer = new XmlSerializer(xmlData.GetType());
                    xmlData = (T)serializer.Deserialize(fileStream);
                }
            } catch (InvalidOperationException)
            {
                throw new InvalidOperationException("データクラスの格納に失敗しました。");
            }
        } 
    }
}
