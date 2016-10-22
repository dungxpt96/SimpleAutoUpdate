using System;
using System.Xml.Linq;
using System.Linq;
using System.IO;

namespace RedCell.Diagnostics.Update
{
    internal class Manifest
    {
        #region Fields
        private string _data;
        #endregion

        #region Inittialization
        /// <summary>
        /// Initializes a new instance of the <see cref="Manifest"/> class.
        /// </summary>
        /// <param name="data"></param>
        public Manifest(string data)
        {
            Load(data);
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the version.
        /// </summary>
        /// <value>The version.</value>
        public int Version { get; private set; }

        /// <summary>
        /// Gets the check interval.
        /// </summary>
        /// <value>The check interval.</value>
        public int CheckInterval { get; private set; }

        /// <summary>
        /// Gets the remote configuration URI.
        /// </summary>
        /// <value>The remote configuration URI.</value>
        public string RemoteConfigUri { get; private set; }

        /// <summary>
        /// Gets the security token.
        /// </summary>
        /// <value>The security token.</value>
        public string SecurityToken { get; private set; }

        /// <summary>
        /// Gets the base URI.
        /// </summary>
        /// <value>The base URI.</value>
        public string BaseUri { get; private set; }

        /// <summary>
        /// Gets the payload.
        /// </summary>
        /// <value>The payload.</value>
        public string[] Payloads { get; private set; }
        #endregion

        #region Methods
        /// <summary>
        /// Loads the specified data
        /// </summary>
        /// <param name="data"></param>
        private void Load(string data)
        {
            _data = data;
            try
            {
                // Tải về cấu hình từ tệp tin XML

                // Tạo một XDocument từ chuỗi data
                var xml = XDocument.Parse(data);
                // Kiểm tra tên cục bộ của tệp tin XML
                if (xml.Root.Name.LocalName != "Manifest")
                {
                    Log.Write("Root XML element {0} is not recognized, stopping.", xml.Root.Name);
                    return;
                }

                // Cài đặt các thuộc tính của Class Manifest
                // Lấy về số phiên bản trong thuộc tính version của file XML
                Version = int.Parse(xml.Root.Attribute("version").Value);
                // Lấy về nội dung của phần tử CheckInterval của file XML
                CheckInterval = int.Parse(xml.Root.Element("CheckInterval").Value);
                // Lấy về nội dung của phần tử RemoteConfigUri của file XML
                RemoteConfigUri = xml.Root.Element("RemoteConfigUri").Value;
                // Lấy về nội dung của phần tử SecurityToken của file XML
                SecurityToken = xml.Root.Element("SecurityToken").Value;
                // Lấy về nội dung của phần tử BaseUri của file XML
                BaseUri = xml.Root.Element("BaseUri").Value;
                // Lấy về nội dung của phần tử PayLoad của file XML
                Payloads = xml.Root.Elements("Payload").Select(x => x.Value).ToArray();

            }
            catch (Exception ex)
            {
                // Thông báo xảy ra lỗi
                Console.Write("Error: {0}", ex.Message);
                return;
            }
        }

        /// <summary>
        /// Writes the specified path.
        /// </summary>
        /// <param name="path"></param>
        public void Write(string path)
        {
            // Tạo một file mới có đường dẫn là Path, nội dung của file là _data
            File.WriteAllText(path, _data);
        }
        #endregion

    }

}