using System;
using System.Globalization;
using System.Windows.Forms;
using System.Drawing.Imaging;

namespace LBA2Tools {
	internal class Program {
		public static string fileFilter = "Supported image formats|";
		[STAThread]
		static void Main(string[] args) {
			CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("en-US");
			var decoders = ImageCodecInfo.GetImageDecoders();
			string together = "";
			string separate = "";
			foreach(var decoder in decoders) {
				together += decoder.FilenameExtension + ";";
				separate += decoder.FormatDescription + "|" + decoder.FilenameExtension + "|";
			}
			fileFilter += together.Remove(together.Length - 1) + "|" + separate + "All files|*";

			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new MainForm());
		}
	}
}
