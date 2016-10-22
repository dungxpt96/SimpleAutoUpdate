using System;
using System.Net;
using System.IO;
using System.Threading;
using System.Text;

namespace RedCell.Net
{
    public class Fetch
    {
        #region Initialization
        /// <summary>
        /// Initializes a new instance of the <see cref="Fetch"/> class
        /// </summary>
        public Fetch()
        {
            Headers = new WebHeaderCollection();
            Retries = 5;
            Timeout = 6000;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the headers
        /// </summary>
        /// <value>The header</value>
        public WebHeaderCollection Headers { get; private set; }

        /// <summary>
        /// Gets the response
        /// </summary>
        /// <value>The response</value>
        public HttpWebResponse Response { get; private set; }

        /// <summary>
        /// Gets or Sets the Credential
        /// </summary>
        /// <value>The credential</value>
        public NetworkCredential Credential { get; set; }

        /// <summary>
        /// Gets the response data
        /// </summary>
        /// <value>The response data</value>
        public byte[] ResponseData { get; private set; }

        /// <summary>
        /// Gets or Sets the retries
        /// </summary>
        /// <value>The retries</value>
        public int Retries { get; set; }

        /// <summary>
        /// Gets or Sets the time out
        /// </summary>
        /// <value>The timeout</value>
        public int Timeout { get; set; }

        /// <summary>
        /// Gets or Sets retry sleep in milliseconds
        /// </summary>
        /// <value>The retry sleep</value>
        public int RetrySleep { get; set; }

        /// <summary>
        /// Gets a value indicating whether this <see cref="Fetch"/> is success.
        /// </summary>
        /// <value><c>true</c> if success; otherwise, <c>false</c></value>
        public bool Success { get; private set; }
        #endregion

        #region Methods
        public void Load(string url)
        {
            for (int retry = 0; retry < Retries; retry++)
            {
                try
                {
                    // Tạo một HttpWebRequest đến URL được đề cập
                    var req = HttpWebRequest.Create(url) as HttpWebRequest;
                    // Cho phép req có thể chuyển hướng đến một vị trí mới
                    req.AllowAutoRedirect = true;
                    // Gọi đến chứng chỉ máy chỉ
                    ServicePointManager.ServerCertificateValidationCallback = (a, b, c, d) => true;

                    // 
                    if (Credential != null)
                        req.Credentials = Credential;
                    req.Headers = Headers;
                    req.Timeout = Timeout;

                    // Nhận những phản hồi trả về từ tài nguyên trên web
                    Response = req.GetResponse() as HttpWebResponse;
                    // Kiểm tra trạng thái của Response
                    switch (Response.StatusCode)
                    {
                        // Giá trị mã trạng thái cho HTTP không tìm thấy
                        case HttpStatusCode.Found:
                            {
                                // Đây là chuyển hướng đến một trang lỗi, bỏ qua...
                                Console.WriteLine("Found (302), ignoring...");
                                break;
                            }
                        // Giá trị mã trạng thái cho HTTP tìm thấy
                        case HttpStatusCode.OK:
                            {
                                // Đây là trang khả dụng

                                // Nhận các dòng tin từ máy chủ gửi về
                                using (var sr = Response.GetResponseStream())
                                using (var ms = new System.IO.MemoryStream())
                                {
                                    for (int b; (b = sr.ReadByte()) != -1;)
                                    {
                                        ms.WriteByte((byte)b);
                                    }
                                    // Lưu trữ những dòng tin được gửi về
                                    ResponseData = ms.ToArray();
                                }

                                break;
                            }
                        default:
                            {
                                // Xảy ra những tình huống không mong đợi
                                Console.WriteLine(Response.StatusCode);
                                break;
                            }
                    }

                    Success = true;
                    break;
                }
                catch (WebException we) // tạo yêu cầu không thành công
                {
                    Console.WriteLine(":Exception " + we.Message);

                    // Lấy những phản hồi trả về từ máy trạm từ xa
                    Response = we.Response as HttpWebResponse;

                    // Đảm bảo không có phản hồi nào được nhận khi hết phiên làm việc
                    if (we.Status == WebExceptionStatus.Timeout)
                    {
                        // Ngủ trong khoảng thời gian
                        Thread.Sleep(RetrySleep);
                        continue;
                    }
                    break;
                }
            }
        }

        public static byte[] Get(string url)
        {
            // Khởi tạo đối tượng f kiểu Fetch
            var f = new Fetch();
            // Lấy về dữ liệu phản hồi từ HTTP đến URL 
            f.Load(url);
            return f.ResponseData;
        }

        public string GetString()
        {
            // Cài đặt font chữ cho tài liệu
            var encoder = string.IsNullOrEmpty(Response.ContentEncoding) ?
                Encoding.UTF8 : Encoding.GetEncoding(Response.ContentEncoding);

            if (ResponseData == null) // Không có dữ liệu phản hồi về
                return string.Empty;
            return encoder.GetString(ResponseData);
        }
        #endregion
    }
}
