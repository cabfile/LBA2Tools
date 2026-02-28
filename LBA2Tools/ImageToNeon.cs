using System.Drawing;
using System.IO;
using System.Collections.Generic;
using System.Windows.Forms;

namespace LBA2Tools {
	internal class ImageToNeon {
		public static void Generate(string path, int boxWidth, int boxHeight, string lvl) {
			if(!File.Exists(path)) {
				MessageBox.Show("The specified image does not exist.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}
			LBA2Level level;
			if(lvl == "") level = new LBA2Level();
			else {
				try {
					level = new LBA2Level(lvl);
				}
				catch {
					MessageBox.Show("The specified level is invalid.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
					return;
				}
			}
			Bitmap img = new Bitmap(path);
			Square[] squares = new BitmapProcessor().ProcessBitmap(img);
			uint count = 0;
			foreach(Square square in squares) {
				if(square.Color.A == 255) {
					LBA2Level.Helpers.AddBox(ref level,
						((double)square.X + (double)square.Width / 2) * boxWidth,
						64 + ((double)square.Y + (double)square.Height / 2) * boxHeight,
						0,
						square.Width * boxWidth,
						square.Height * boxHeight,
						animationFrame: 17,
						immovable: true,
						colorR: square.Color.R,
						colorG: square.Color.G,
						colorB: square.Color.B);
					count++;
				}
			}
			Clipboard.SetText(level.ToString());
			string msg = "A level with " + count + (lvl != "" ? " extra" : "") + " Box(es) is in your clipboard now.";
			if(count > 1000) msg += "\nConsider using a simpler image.";
			if(count > 2000) msg += " The level will take a long time to load.";
			MessageBox.Show(msg, "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
		}

		public struct Square {
			public float X { get; set; }
			public float Y { get; set; }
			public int Width { get; set; }
			public int Height { get; set; }
			public Color Color { get; set; }
		}

		public class BitmapProcessor {
			public Square[] ProcessBitmap(Bitmap bitmap) {
				List<Square> squares = new List<Square>();
				bool[,] processed = new bool[bitmap.Height, bitmap.Width];

				for(int y = 0; y < bitmap.Height; y++) {
					for(int x = 0; x < bitmap.Width;) {
						if(!processed[y, x]) {
							Color startColor = bitmap.GetPixel(x, y);
							int maxWidth = GetMaxWidth(bitmap, processed, x, y, startColor);
							int maxHeight = GetMaxHeight(bitmap, processed, x, y, maxWidth, startColor);

							// Shrink rectangle until it fits in unprocessed area
							while(true) {
								bool valid = IsRectangleUnprocessed(processed, x, y, maxWidth, maxHeight);
								if(valid) break;

								if(maxHeight > 1) maxHeight--;
								else if(maxWidth > 1) maxWidth--;
								else break;
							}

							if(maxWidth > 0 && maxHeight > 0) {
								squares.Add(new Square {
									X = x,
									Y = y,
									Width = maxWidth,
									Height = maxHeight,
									Color = startColor
								});

								MarkProcessed(processed, x, y, maxWidth, maxHeight);
								x += maxWidth;
							}
							else {
								x++;
							}
						}
						else {
							x++;
						}
					}
				}
				return squares.ToArray();
			}

			private Color SimplifyAlpha(Color col) {
				var c = col;
				if(c.A >= 127) c = Color.FromArgb(255, c);
				else c = Color.Transparent;
				return c;
			}

			private int GetMaxWidth(Bitmap bitmap, bool[,] processed, int x, int y, Color targetColor) {
				int width = 0;
				int targetArgb = targetColor.ToArgb();
				while(x + width < bitmap.Width &&
					   !processed[y, x + width] &&
					   SimplifyAlpha(bitmap.GetPixel(x + width, y)).ToArgb() == targetArgb) {
					width++;
				}
				return width;
			}

			private int GetMaxHeight(Bitmap bitmap, bool[,] processed, int x, int y, int width, Color targetColor) {
				int height = 1;
				int targetArgb = targetColor.ToArgb();

				for(int currentY = y + 1; currentY < bitmap.Height; currentY++) {
					for(int dx = 0; dx < width; dx++) {
						int currentX = x + dx;
						if(currentX >= bitmap.Width ||
							processed[currentY, currentX] ||
							SimplifyAlpha(bitmap.GetPixel(currentX, currentY)).ToArgb() != targetArgb) {
							return height;
						}
					}
					height++;
				}
				return height;
			}

			private bool IsRectangleUnprocessed(bool[,] processed, int x, int y, int width, int height) {
				for(int dy = 0; dy < height; dy++) {
					for(int dx = 0; dx < width; dx++) {
						if(y + dy >= processed.GetLength(0) ||
							x + dx >= processed.GetLength(1) ||
							processed[y + dy, x + dx]) {
							return false;
						}
					}
				}
				return true;
			}

			private void MarkProcessed(bool[,] processed, int x, int y, int width, int height) {
				for(int dy = 0; dy < height; dy++) {
					for(int dx = 0; dx < width; dx++) {
						if(y + dy < processed.GetLength(0) && x + dx < processed.GetLength(1)) {
							processed[y + dy, x + dx] = true;
						}
					}
				}
			}
		}
	}
}
