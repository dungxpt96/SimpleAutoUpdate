using System;
using System.IO;
using System.Net;
using System.Text;

namespace RedCell.Net
{
    /// <summary>
    /// Fetches streamed content.
    /// </summary>
    public class FetchStream
    {
        #region Properties
        /// <summary>
        /// Gets the response.
        /// </summary>
        public HttpWebResponse Response { get; private set; }

        /// <summary>
        /// Gets the response data.
        /// </summary>
        public byte[] ResponseData { get; private set; }
        #endregion

        #region Methods
        /// <summary>
        /// Gets the specified URL.
        /// </summary>
        /// <param name="url"></param>
        public void Load(string url)
        {
            try
            {
                // Tạo một HttpWebRequest đến URL được đề cập.
                var req = HttpWebRequest.Create(url) as HttpWebRequest;
                // Cho phép req có thể chuyển hướng đến một vị trí mới.
                req.AllowAutoRedirect = false;

                // Nhận những phản hồi trả về từ tài nguyên trên web.
                Response = req.GetResponse() as HttpWebResponse;
                // Kiểm tra trạng thái của Response.
                switch (Response.StatusCode)
                {
                    // Giá trị mã trạng thái cho HTTP không tìm thấy.
                    case HttpStatusCode.Found:
                        {
                            // Đây là chuyển hướng đến một trang lỗi, bỏ qua...
                            Console.WriteLine("Found (302), ignoring ");
                            break;
                        }

                    // Giá trị mã trạng thái cho HTTP tìm thấy.
                    case HttpStatusCode.OK:
                        {
                            // Đây là trang khả dụng.

                            // Nhận các dòng tin từ máy chủ gửi về.
                            using (var sr = Response.GetResponseStream())
                            using (var ms = new MemoryStream())
                            {
                                for (int b; (b = sr.ReadByte()) != -1;)
                                    ms.WriteByte((byte)b);
                                // Lưu trữ những dòng tin được gửi về.
                                ResponseData = ms.ToArray();
                            }
                            break;
                        }

                    default:
                        {
                            // Trường hợp không mong đợi.
                            Console.WriteLine(Response.StatusCode);
                            break;
                        }
                }
            }
            catch (WebException ex)
            {
                Console.WriteLine(":Exception " + ex.Message);
                Response = ex.Response as HttpWebResponse;
            }
        }

        /// <summary>
        /// Gets the specified URL.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <returns></returns>
        public static byte[] Get(string url)
        {
            var f = new Fetch();
            f.Load(url);
            return f.ResponseData;
        }

        /// <summary>
        /// Gets the string.
        /// </summary>
        /// <returns></returns>
        public string GetString()
        {
            var encoder = string.IsNullOrEmpty(Response.ContentEncoding) ?
                Encoding.UTF8 : Encoding.GetEncoding(Response.ContentEncoding);
            if (ResponseData == null)
                return string.Empty;
            return encoder.GetString(ResponseData);
        }
        #endregion
    }
}
