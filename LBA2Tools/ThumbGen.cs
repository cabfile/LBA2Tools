using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace LBA2Tools {
	internal class ThumbGen {
		public static void Generate(string path) {
			if(!Directory.Exists(path)) {
				MessageBox.Show("The specified directory does not exist.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			string[] pngFiles = Directory.GetFiles(path, "*.png");
			List<string> failed = new List<string>();
			foreach(var pngFile in pngFiles) {
				if(Path.GetFileName(pngFile).StartsWith("thumb_")) continue;
				try {
					using(var image = Image.FromFile(pngFile)) {
						int newWidth = 32;
						int newHeight = (int)(image.Height * (32.0 / image.Width));

						if(newHeight > 32) {
							newHeight = 32;
							newWidth = (int)(image.Width * (32.0 / image.Height));
						}

						using(var thumbnail = new Bitmap(32, 32)) {
							using(var graphics = Graphics.FromImage(thumbnail)) {
								graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
								graphics.DrawImage(image, 16 - newWidth / 2, 16 - newHeight / 2, newWidth, newHeight);
							}
							string thumbnailPath = Path.Combine(path, $"thumb_{Path.GetFileName(pngFile)}");
							thumbnail.Save(thumbnailPath, System.Drawing.Imaging.ImageFormat.Png);
						}
					}
				}
				catch {
					failed.Add(Path.GetFileName(pngFile));
				}
			}
			if(failed.Count != pngFiles.Length) {
				string msg = "Thumbnail generation complete.";
				if(failed.Count > 0) msg += "\nThumbnails could not be generated for: " + string.Join(", ", failed);
				MessageBox.Show(msg, "Finished", MessageBoxButtons.OK, MessageBoxIcon.Information);
			} else MessageBox.Show("Failed to generate even a single thumbnail.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
		}
	}
}
