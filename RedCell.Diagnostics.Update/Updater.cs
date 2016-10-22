using Ionic.Zip;
using RedCell.Net;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace RedCell.Diagnostics.Update
{
    public class Updater
    {
        #region Constants
        /// <summary>
        /// The default check interval.
        /// </summary>
        public const int DefaultCheckInterval = 900; // 900s = 15m

        /// <summary>
        /// The first check delay.
        /// </summary>
        public const int FirstCheckDelay = 15;

        /// <summary>
        /// The default configuration file.
        /// </summary>
        public const string DefaultConfigFile = "update.xml";

        /// <summary>
        /// The name of the derectory
        /// </summary>
        public const string WorkPath = "Document";
        #endregion

        #region Fields
        private Timer _timer;
        private volatile bool _updating;
        private readonly Manifest _localConfig;
        private Manifest _remoteConfig;
        private readonly FileInfo _localConfigFile;
        #endregion

        #region Initialization
        /// <summary>
        /// Initializes a new instance of the <see cref="Update"/> class
        /// </summary>
        public Updater() : this(new FileInfo(DefaultConfigFile))
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Update"/> class
        /// </summary>
        /// <param name="configFile"></param>
        public Updater(FileInfo configFile)
        {
            Log.Debug = true;

            _localConfigFile = configFile;
            Log.Write("Loading...");
            Log.Write("Loaded.");
            Log.Write("Initializing using file '{0}'.", configFile.FullName);

            if (!configFile.Exists) // nếu configFile không tồn tại, thông báo và dừng lại
            {
                Log.Write("Config file {0} does not exist, stopping.", configFile.Name);
                return;
            }

            string data = File.ReadAllText(configFile.FullName);
            Console.WriteLine("Information of data:\n" + data);
            this._localConfig = new Manifest(data);
        }
        #endregion

        #region Methods
        /// <summary>
        /// Starting monitoring.
        /// </summary>
        public void StartMonitoring()
        {
            Log.Write("Starting monitoring in {0}s...", this._localConfig.CheckInterval);
            Log.Write("Please wait...");
            _timer = new Timer(Check, null, 5000, this._localConfig.CheckInterval * 1000);
            Log.Write("Already.");
        }
        /// <summary>
        /// Stopping monitoring.
        /// </summary>
        public void StopMonitoring()
        {
            Log.Write("Stopping monitoring.");

            if (_timer == null)
            {
                Log.Write("Monitoring is already stopped.");
                return;
            }
            _timer.Dispose();
        }

        /// <summary>
        /// Check Remote Configuration
        /// </summary>
        /// <param name="state"></param>
        private void Check(Object state)
        {
            Log.Write("Checking...");

            if (_updating) // nếu _updating != null, việc kiểm tra cập nhật đã hoàn thành.
            {
                Log.Write("Updater is already updating.");
                Log.Write("Check ending.");
            }

            // Lấy đường dẫn chứa file xml
            var remoteUri = new Uri(this._localConfig.RemoteConfigUri);
            // In ra đường dẫn phiên bản hiện tại đang sử dụng
            Log.Write("Fetching local file: '{0}'.", remoteUri.AbsoluteUri);

            // Khởi tạo một đối tượng http kiểu Fetch
            var http = new Fetch { Retries = 5, RetrySleep = 30000, Timeout = 30000 };
            // Tải về đường đẫn chính xác URI
            http.Load(remoteUri.AbsoluteUri);

            if (!http.Success) // nếu http khởi tạo không thành công, thông báo lỗi xảy ra
            {
                Log.Write("Fetch error: {0}", http.Response.StatusDescription);
                this._remoteConfig = null;
                return;
            }

            // Lấy thông tin dữ liệu phản hồi từ http và định dạng font chữ cho dữ liệu 
            string data = Encoding.UTF8.GetString(http.ResponseData);

            // Khởi tạo đối tượng kiểu Manifest, lưu trữ thông tin của file xml chuẩn bị cập nhật
            this._remoteConfig = new Manifest(data);

            // Kiểm tra các trường hợp 

            if (this._remoteConfig == null) // nếu dữ liệu KHÔNG được tạo thành công, thông báo
            {
                Log.Write("Data is not found!, stopping...");
                Log.Write("Check ending.");
                return;
            }

            // Nếu mã bảo mật của phiên bản mới không giống phiên bản đang sử dụng, thông báo
            if (this._localConfig.SecurityToken != this._remoteConfig.SecurityToken)
            {
                Log.Write("Security token mismatch, stopping...");
                Log.Write("Check ending.");
                return;
            }

            // In ra thông tin số phiên bản của các phiên bản cũ và mới
            Log.Write("Remote config is valid.");
            Log.Write("Local version is {0}", _localConfig.Version);
            Log.Write("Remote version is {0}", _remoteConfig.Version);

            // Kiểm tra số phiên bản hợp lệ không?
            // Phiên bản xem xét cập nhật có số phiên bản thấp hơn, kết thúc kiểm tra, KHÔNG cập nhật
            if (this._remoteConfig.Version < this._localConfig.Version)
            {
                Log.Write("Remote version is older. That's weird.");
                Log.Write("Check ending.");
                return;
            }
            // 2 số phiên bản là giống nhau, kết thúc kiểm tra, KHÔNG cập nhật
            if (this._remoteConfig.Version == this._localConfig.Version)
            {
                Log.Write("Versions are the same.");
                Log.Write("Check ending.");
                return;
            }
            // ngược lại nếu, phiên bản xem xét cập nhật là mới hơn, cho phép cập nhật
            Log.Write("Remote version is newer. Updating.");
            _updating = true;
            Update();

            // Kết thúc việc kiểm tra cập nhật, đã chấp nhận cập nhật
            _updating = false;
            Log.Write("Check ending.");
            Console.ReadKey();
        }

        /// <summary>
        /// Update program
        /// </summary>
        private void Update()
        {
            // Thông báo số file cần cập nhật và in ra tên file đó.
            Log.Write("Updating '{0}' files.", this._remoteConfig.Payloads.Length);
            foreach (string str in this._remoteConfig.Payloads)
            {
                Console.WriteLine("Remote Payloads: " + str);
            }

            // Xóa bỏ thư mục nếu nó đã tồn tại
            if (Directory.Exists(WorkPath)) // nếu tồn tại, cảnh báo và xóa nó đi.
            {
                Log.Write("WARNING: Work directory already exists.");

                try
                {
                    Directory.Delete(WorkPath, true);
                }
                catch (IOException) // nếu xóa không thành công, thông báo và kết thúc.
                {
                    Log.Write("Cannot delete open directory '{0}'.", WorkPath);
                    return;
                }
            }
            // Tạo thư mục với tên thư mục là 'Document'
            Directory.CreateDirectory(WorkPath);

            // Tải về các file từ Manifest
            foreach (string update in this._remoteConfig.Payloads)
            {
                // Thông báo những file được tải về
                Log.Write("Fetching '{0}'.", update);
                // Tạo đường dẫn đến file cần tải.
                var url = this._remoteConfig.BaseUri + update;
                // Lấy dữ liệu từ file cần tải về.
                var file = Fetch.Get(url);

                if (file == null) // nếu lấy dữ liệu không thành công, thông báo và kết thúc.
                {
                    Log.Write("Fetch failed.");
                    return;
                }

                // Tạo đường dẫn để lưu file khi tải file về
                var info = new FileInfo(Path.Combine(WorkPath, update));
                // In ra đường dẫn lưu file
                Console.WriteLine("File is saved in: {0}", info.FullName);

                // Tạo thư mục để lưu trữ file với đường dẫn đã có
                Directory.CreateDirectory(info.DirectoryName);
                File.WriteAllBytes(Path.Combine(WorkPath, update), file);

                // Nếu file có dạng .zip, không mở, tải về cả .zip
                if (Regex.IsMatch(update, @"\.zip"))
                {
                    try
                    {
                        var zipfile = Path.Combine(WorkPath, update);
                        using (var zip = ZipFile.Read(zipfile))
                            zip.ExtractAll(WorkPath, ExtractExistingFileAction.Throw);
                        File.Delete(zipfile);
                    }
                    catch (Exception ex)
                    {
                        Log.Write("Unpack failed: {0}", ex.Message);
                        return;
                    }
                }
            }


            // Thay đổi thực thi nếu nó cần được ghi đè
            Process thisprocess = Process.GetCurrentProcess();
            string me = thisprocess.MainModule.FileName;

            // In ra đường dẫn đến chương trình đang thực thi.
            Console.WriteLine("Path: " + me);
            string bak = me + ".bak"; // thêm đuôi .bak
            Log.Write("Renaming running process to '{0}'.", bak);

            if (File.Exists(bak)) // nếu đã tồn tại file.bak, xóa nó đi.
                File.Delete(bak);
            File.Move(me, bak);
            File.Copy(bak, me);

            // Ghi dữ liệu lên Manifest mới.
            _remoteConfig.Write(Path.Combine(WorkPath, _localConfigFile.Name));

            // Sao chép mọi thứ.
            // Tạo thư mục với tên 'Document'.
            var directory = new DirectoryInfo(WorkPath);
            // Lấy về các file.
            var files = directory.GetFiles("*.*", SearchOption.AllDirectories);
            foreach (FileInfo file in files)
            {
                // Tạo đích đến cho các file.
                string destination = file.FullName.Replace(directory.FullName + @"\", "");
                // In ra những file cần cài đặt.
                Log.Write("installing file '{0}'.", destination);
                // Tạo tất cả các đường dẫn với tên đã xác định.
                Directory.CreateDirectory(new FileInfo(destination).DirectoryName);
                file.CopyTo(destination, true);
            }

            // Xóa thư mục 'Document'
            Log.Write("Deleting work directory.");
            Directory.Delete(WorkPath, true);

            // Khởi động lại
            Log.Write("Spawning new process.");
            var spawn = Process.Start(me);

            Log.Write("New process ID is {0}", spawn.Id);
            Log.Write("Closing old running process {0}.", thisprocess.Id);
            Console.WriteLine("Update Success! Press a key to close program...");
            Console.ReadKey();

            thisprocess.CloseMainWindow();
            thisprocess.Close();
            thisprocess.Dispose();

        }
        #endregion
    }
}
