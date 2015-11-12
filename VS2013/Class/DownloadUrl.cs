using System;
using System.IO;
using System.Net;
using System.Threading;

namespace NextionEditor
{
    public class DownloadUrl
    {
        public int Retries = 3;
        public bool DownloadOK = false;
        public string Error = string.Empty;
		public int FileLength = 0;
		public string FilePath = string.Empty;
		public string Url = string.Empty;

		private StreamWriter streamWriter;
		private Stream stream;
		private Thread download_thread;

        private void closeResources()
        {
			try
			{
				streamWriter.Close();
				streamWriter.Dispose();
			}
			catch { }
			try
			{
				stream.Close();
				stream.Dispose();
			}
			catch { }
        }

		public void DownloadStop()
		{
			download_thread.Abort();
			closeResources();
		}

		public void DownloadStart()
		{
			download_thread = new Thread(new ThreadStart(downloadFile_start));
			download_thread.Start();
		}

		private void downloadFile_start()
        {
            int retry = 0;
            while (retry <= Retries)
            {
                try
                {
                    WebClient client = new WebClient();
                    if (DownloadOK)
                        DownloadOK = false;
                    if (Error != "")
                        Error = "";

                    if (!Utility.DeleteFileWait(FilePath))
                    {
                        Error = "file error" + FilePath;
                    }
                    else
                    {
                        streamWriter = new StreamWriter(FilePath);
                        stream = client.OpenRead(Url);
                        byte[] buffer = new byte[0x10000];
                        FileLength = 0;
                        int count = 1;
                        while (count > 0)
                        {
                            count = stream.Read(buffer, 0, buffer.Length);
                            streamWriter.BaseStream.Write(buffer, 0, count);
                            FileLength += count;
                        }
                        streamWriter.Close();
                        streamWriter.Dispose();
                        stream.Close();
                        stream.Dispose();
                        DownloadOK = true;
                    }
                    break;
                }
                catch (Exception ex)
                {
                    closeResources();
                    retry++;
                    if (retry > Retries)
                    {
                        Error = ex.Message + "\r\n";
                    }
                }
            }
        }
    }
}
