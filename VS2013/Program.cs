using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using NextionEditor.Properties;
using System.Globalization;

namespace NextionEditor
{
	internal static class Program
	{
		[STAThread]
		private static void Main()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(true);

			Utility.TranslateInit();

			HmiOptions.Language = 1;
			HmiOptions.Encoding = Encoding.GetEncoding("utf-8");
			HmiOptions.SoftName = "Nextion Editor";
			HmiOptions.Icon = Resources.ITeadIco;

			Utility.SetAppPaths();
			Utility.DeleteFiles(HmiOptions.AppDataPath, "*.ca");

			#region Check Firmware version
			string fwc_path = Path.Combine(Application.StartupPath, "fwc.bin");
			InfoFirmware fwInfo;
			using (StreamReader reader = new StreamReader(fwc_path))
			{
				byte[] buffer = new byte[HmiOptions.InfoFirmwareSize];

				reader.BaseStream.Position = HmiOptions.FlashInfoFwBegin;
				reader.BaseStream.Read(buffer, 0, buffer.Length);
				fwInfo = Utility.ToStruct<InfoFirmware>(buffer);
				reader.BaseStream.Close();
				reader.Close();
			}

			byte fwMajor = (byte)(fwInfo.Version / 0x100);
			byte fwMinor = (byte)(fwInfo.Version % 0x100);

			if (fwMajor != HmiOptions.FirmwareMajor || fwMinor != HmiOptions.FirmwareMinor)
			{
				MessageBox.Show(string.Concat(
					"fwc.bin file is not matched with program.".Translate(),
					fwMajor, ".", fwMinor,
					"-", HmiOptions.FirmwareMajor, ".", HmiOptions.FirmwareMinor
					)
				);
				Environment.Exit(0);
				return;
			}
			#endregion

			Application.Run(new main());
		}
	}
}
