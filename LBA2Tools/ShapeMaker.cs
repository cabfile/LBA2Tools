using System;
using System.Drawing;
using System.IO;
using System.Collections.Generic;
using System.Windows.Forms;

namespace LBA2Tools {
	internal class ShapeMaker {
		private const string TemplatePath = "templates";
		private static Size templateSize;

		public static void Generate(string folderPath) {
			var templates = LoadTemplates();
			if(templates.Count == 0) {
				MessageBox.Show("The template directory is missing or empty.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}
			if(!Directory.Exists(folderPath)) {
				MessageBox.Show("The specified directory does not exist.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			templateSize = templates[0].Bitmap.Size;
			string[] images = Directory.GetFiles(folderPath, "*.png");
			List<string> failed = new List<string>();
			foreach(var imagePath in images) {
				try {
					using(var image = new Bitmap(imagePath)) {
						if(image.Size != templateSize) throw new InvalidOperationException("Invalid size");

						foreach(var template in templates) {
							var outputImage = ApplyTemplate(image, template);
							var outputDarkImage = DarkenImage(outputImage, 0.25f);

							// Save the images
							string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(imagePath);
							outputImage.Save(Path.Combine(folderPath, $"{fileNameWithoutExtension}_{template.Tag}.png"));
							outputDarkImage.Save(Path.Combine(folderPath, $"{fileNameWithoutExtension}_{template.Tag}_dark.png"));
						}
					}
				} catch {
					failed.Add(Path.GetFileName(imagePath));
				}
			}
			if(failed.Count != images.Length) {
				string msg = "Shape generation complete.";
				if(failed.Count > 0) msg += "\nShapes could not be generated for: " + string.Join(", ", failed);
				MessageBox.Show(msg, "Finished", MessageBoxButtons.OK, MessageBoxIcon.Information);
			}
			else MessageBox.Show("Failed to generate even a single shape.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
		}

		private static List<Template> LoadTemplates() {
			var templates = new List<Template>();
			if(!Directory.Exists(TemplatePath)) return templates;
			foreach(var templatePath in Directory.GetFiles(TemplatePath, "*.png")) {
				var template = new Bitmap(templatePath);
				templates.Add(new Template(template, Path.GetFileNameWithoutExtension(templatePath)));
			}
			return templates;
		}

		private static Bitmap ApplyTemplate(Bitmap image, Template template) {
			var outputImage = new Bitmap(image.Width, image.Height);
			for(int x = 0; x < image.Width; x++) {
				for(int y = 0; y < image.Height; y++) {
					Color templateColor = template.Bitmap.GetPixel(x, y);
					if(templateColor.R == 0 && templateColor.G == 0 && templateColor.B == 0)
					{
						outputImage.SetPixel(x, y, Color.Transparent);
					}
					else {
						outputImage.SetPixel(x, y, image.GetPixel(x, y));
					}
				}
			}
			return outputImage;
		}

		private static Bitmap DarkenImage(Bitmap image, float factor) {
			var darkenedImage = new Bitmap(image.Width, image.Height);
			for(int x = 0; x < image.Width; x++) {
				for(int y = 0; y < image.Height; y++) {
					Color originalColor = image.GetPixel(x, y);
					Color darkenedColor = Color.FromArgb(
						originalColor.A,
						(int)(originalColor.R * (1 - factor)),
						(int)(originalColor.G * (1 - factor)),
						(int)(originalColor.B * (1 - factor))
					);
					darkenedImage.SetPixel(x, y, darkenedColor);
				}
			}
			return darkenedImage;
		}

		private class Template {
			public Bitmap Bitmap { get; }
			public string Tag { get; }

			public Template(Bitmap bitmap, string tag) {
				Bitmap = bitmap;
				Tag = tag;
			}
		}
	}
}
