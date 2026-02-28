using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace LBA2Tools {

	public class BounceConv {
		// Constants mapped from JS
		private const bool IGNORE_SIZE = false;

		public static void Convert(string filePath, int wallMaterial, int bouncyMaterial, int waterMaterial, int endMaterial, int endMaterialBG) {
			if(!File.Exists(filePath)) {
				MessageBox.Show("The specified level file does not exist.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			BounceLevel bounce = ParseLevelBuffer(File.ReadAllBytes(filePath));
			if(bounce.TileMapHeight > 36) {
				MessageBox.Show("Level is too tall (" + bounce.TileMapHeight + " tiles)", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}
			else if(bounce.TileMapWidth > 320) {
				MessageBox.Show("Level is too tall (" + bounce.TileMapWidth + " tiles)", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}
			else Console.WriteLine("Processing a " + bounce.TileMapWidth + "x" + bounce.TileMapHeight + " level");
			int offsetY = 640 - bounce.TileMapHeight * 16;
			var level = new LBA2Level();
			level.Level.props.startas = 2;
			level.Level.props.theme = 2;
			level.Level.entranceX = bounce.StartCol * 32 + 16;
			level.Level.entranceY = bounce.StartRow * 32 + 16 + offsetY;

			int[] counts = new int[2];
			List<int> types = new List<int>();
			var processor = new SquareProcessor();
			int[][] tileMap = bounce.TileMap;
			int[][] gridWalls = tileMap.Select(row =>
				row.Select(r => {
					int val = r & 63;
					return (val == 1 || val == 2 || val == 9) ? val : 0;
				}).ToArray()
			).ToArray();
			int[][] gridWater = tileMap.Select(row =>
				row.Select(r => (r >= 64) ? 1 : 0).ToArray()
			).ToArray();

			var squares = processor.ProcessGrid(gridWalls);
			var squaresWater = processor.ProcessGrid(gridWater);
			int boxI = 0;
			int endI = -1;
			double endX = 0;
			double endY = 0;

			foreach(var square in squaresWater) {
				if(square[4] == 1) {
					level.Add("box",
						(square[0] + square[2] / 2.0) * 32,
						(square[1] + square[3] / 2.0) * 32 + offsetY,
						0,
						square[2] * 32,
						square[3] * 32,
						1, 0.5, 0, waterMaterial, 1, 0, 0, 0, "", "Defaultbg", 1, 0, -1, -1, "", -1, 0, -1, -1, -1, -1);

					if(endI == -1) boxI++;
					counts[0] += square[2] * square[3];
				}
			}
			foreach(var square in squares) {
				int type = square[4];

				if(type == 1)
				{
					level.Add("box",
						(square[0] + square[2] / 2.0) * 32,
						(square[1] + square[3] / 2.0) * 32 + offsetY,
						0,
						square[2] * 32,
						square[3] * 32,
						1, 0.5, 0, wallMaterial, 1, 0, 0, 0, "", "Default", 1, 0, -1, -1, "", -1, 0, -1, -1, -1, -1);

					if(endI == -1) boxI++;
					counts[0] += square[2] * square[3];
				}
				else if(type == 2)
				{
					level.Add("box",
						(square[0] + square[2] / 2.0) * 32,
						(square[1] + square[3] / 2.0) * 32 + offsetY,
						0,
						square[2] * 32,
						square[3] * 32,
						1, 1.25, 0, bouncyMaterial, 1, 0, 0, 0, "", "Default", 1, 0, -1, -1, "", -1, 0, -1, -1, -1, -1);

					if(endI == -1) boxI++;
					counts[0] += square[2] * square[3];
				}
				else if(type == 9)
				{
					if(endI == -1) {
						level.Add("box",
							(square[0] + square[2] / 2.0) * 32,
							(square[1] + square[3] / 2.0) * 32 + offsetY,
							0,
							square[2] * 32,
							square[3] * 32,
							1, 0.5, 0, endMaterialBG, 1, 0, 0, 0, "", "Defaultbg", 1, 0, -1, -1, "", -1, 0, -1, -1, -1, -1);
						level.Add("box",
							(square[0] + square[2] / 2.0) * 32,
							(square[1] + square[3] / 2.0) * 32 + offsetY,
							0,
							square[2] * 32,
							square[3] * 32,
							1, 0.5, 0, endMaterial, 1, 0, 0, 0, "", "Default", 1, 0, -1, -1, "", -1, 0, -1, -1, -1, -1);
						endI = boxI + 1;
						endX = (square[0] + square[2] / 2.0) * 32;
						endY = (square[1] + square[3] / 2.0) * 32 + offsetY;
						counts[0] += square[2] * square[3];
					}
				}
			}
			for(int y = 0; y < bounce.TileMapHeight; y++) {
				for(int x = 0; x < bounce.TileMapWidth; x++) {
					int tile = tileMap[y][x] & 63;
					if(tile == 0 || tile == 1 || tile == 2 || tile == 9 || tile == 10 || tile == 14 || tile == 16 || tile == 22 || tile == 24 || tile == 29)
						continue;
					counts[0]++;
					if(tile >= 3 && tile < 7)
					{
						level.Add("box", x * 32 + 16, y * 32 + 16 + offsetY, (tile - 3) * 90, 32, 32, 1, 0.5, 0, 8, 1, 0, 0, 0, "", "Triangle", 1, 0, -1, -1, "", -1, 0, -1, -1, -1, -1);
					}
					else if(tile == 7)
					{
						level.Add("cp", x * 32 + 16, y * 32 + 16 + offsetY);
					}
					else if(tile == 13)
					{
						level.Add("score", x * 32 + 16, y * 32 + 32 + offsetY, 0, 16, 56, 0, 0, 0, 0, 0, 1, 1, 0, 0, 0);
					}
					else if(tile == 15)
					{
						level.Add("score", x * 32 + 32, y * 32 + 16 + offsetY, 0, 56, 16, 0, 0, 0, 0, 0, 1, 1, 0, 0, 0);
					}
					else if(tile == 21)
					{
						level.Add("score", x * 32 + 16, y * 32 + 32 + offsetY, 0, 16, 64, 0, 0, 0, 0, 0, 1, 1, 0, 0, 0);
					}
					else if(tile == 23)
					{
						level.Add("score", x * 32 + 32, y * 32 + 16 + offsetY, 0, 64, 16, 0, 0, 0, 0, 0, 1, 1, 0, 0, 0);
					}
					else
					{
						counts[1]++;
						if(!types.Contains(tile)) types.Add(tile);
					}
				}
			}

			var logicList = new List<LBA2Level.LogicPiece>();
			for(int i = 0; i < bounce.TotalRings; i++) {
				logicList.Add(new LBA2Level.LogicPiece {
					type = "tag",
					x = i * 32 + 16,
					y = 16,
					vis = false,
					inputs = new object[] { null },
					connections = new object[0],
					props = new object[] { $"7:{i}", 0 }
				});
			}
			logicList.Add(new LBA2Level.LogicPiece {
				type = "tagsensor",
				x = 0 * 32 + 16,
				y = 48,
				vis = false,
				inputs = new object[0],
				connections = new object[] { new object[] { new object[] { bounce.TotalRings + 1, 0 } } },
				props = new object[] { "", 0, 10240, 0, 1 }
			});
			logicList.Add(new LBA2Level.LogicPiece {
				type = "destroy",
				x = 1 * 32 + 16,
				y = 48,
				vis = false,
				inputs = new object[] { new object[] { bounce.TotalRings, 0, 0 } },
				connections = new object[0],
				props = new object[] { $"0:{endI}" }
			});
			logicList.Add(new LBA2Level.LogicPiece {
				type = "playersensor",
				x = endX,
				y = endY,
				vis = false,
				inputs = new object[0],
				connections = new object[] { new object[] { new object[] { bounce.TotalRings + 3, 0 } } },
				props = new object[] { "", 32, 1, 0 }
			});
			logicList.Add(new LBA2Level.LogicPiece {
				type = "ender",
				x = endX,
				y = endY,
				vis = false,
				inputs = new object[] { new object[] { bounce.TotalRings + 2, 0, 0 }, null },
				connections = new object[0],
				props = new object[] { 0, 0 }
			});
			level.Level.logic = logicList.ToArray();

			Clipboard.SetText(level.ToString());
			MessageBox.Show("The level has been converted.\nProcessed " + counts[0] + " tiles, approximately missed " + (counts[1] > 0 ? $"{counts[1]} (types: {string.Join(", ", types)})" : "none")
							+ "\n" + (level.Level.spritedata.Split('|').Length + level.Level.logic.Length) + " objects, the Entrance is at X " + level.Level.entranceX + ", Y " + level.Level.entranceY + ".", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
		}

		public static int GetMaterial(int _i) {
			int i = _i;
			if(_i >= 7) i += 3;
			if(_i >= 10) i += 1;
			if(_i >= 12) i += 2;
			if(_i >= 14) i += 1;
			if(_i >= 27) i += 1;
			return i;
		}

		public static BounceLevel ParseLevelBuffer(byte[] buffer) {
			using(var ms = new MemoryStream(buffer))
			using(var reader = new BinaryReader(ms)) {
				var level = new BounceLevel {
					StartCol = reader.ReadByte(),
					StartRow = reader.ReadByte(),
					StartBallSize = reader.ReadByte() == 1 ? 16 : 0,
					ExitX = reader.ReadByte(),
					ExitY = reader.ReadByte(),
					TotalRings = reader.ReadByte(),
					TileMapWidth = reader.ReadByte(),
					TileMapHeight = reader.ReadByte()
				};

				level.TileMap = new int[level.TileMapHeight][];
				for(int y = 0; y < level.TileMapHeight; y++) {
					level.TileMap[y] = new int[level.TileMapWidth];
					for(int x = 0; x < level.TileMapWidth; x++) {
						level.TileMap[y][x] = reader.ReadByte();
					}
				}

				byte numMoveObj = reader.ReadByte();
				level.MovingObjects = new List<MovingObject>();
				for(int i = 0; i < numMoveObj; i++) {
					var obj = new MovingObject {
						TopLeft = new int[] { reader.ReadByte(), reader.ReadByte() },
						BotRight = new int[] { reader.ReadByte(), reader.ReadByte() },
						Direction = new int[] { reader.ReadByte(), reader.ReadByte() },
						Offset = new int[] { reader.ReadByte(), reader.ReadByte() }
					};
					level.MovingObjects.Add(obj);
				}
				return level;
			}
		}

		public class BounceLevel {
			public int Version { get; set; } = 1;
			public int StartCol { get; set; }
			public int StartRow { get; set; }
			public int StartBallSize { get; set; }
			public int ExitX { get; set; }
			public int ExitY { get; set; }
			public int TotalRings { get; set; }
			public int TileMapWidth { get; set; }
			public int TileMapHeight { get; set; }
			public int[][] TileMap { get; set; }
			public List<MovingObject> MovingObjects { get; set; }
		}

		public class MovingObject {
			public int[] TopLeft { get; set; }
			public int[] BotRight { get; set; }
			public int[] Direction { get; set; }
			public int[] Offset { get; set; }
		}
	}

	public class SquareProcessor {
		public List<int[]> ProcessGrid(int[][] grid) {
			var squares = new List<int[]>();
			if(grid == null || grid.Length == 0) return squares;

			int height = grid.Length;
			int width = grid[0].Length;

			bool[][] processed = new bool[height][];
			for(int i = 0; i < height; i++) processed[i] = new bool[width];

			for(int y = 0; y < height; y++) {
				for(int x = 0; x < width;) {
					if(!processed[y][x]) {
						int startColor = grid[y][x];
						int maxWidth = GetMaxWidth(grid, processed, x, y, startColor);
						int maxHeight = GetMaxHeight(grid, processed, x, y, maxWidth, startColor);

						while(true) {
							bool valid = IsRectangleUnprocessed(processed, x, y, maxWidth, maxHeight);
							if(valid) break;
							if(maxHeight > 1) maxHeight--;
							else if(maxWidth > 1) maxWidth--;
							else break;
						}

						if(maxWidth > 0 && maxHeight > 0) {
							squares.Add(new int[] { x, y, maxWidth, maxHeight, startColor });
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
			return squares;
		}

		private int GetMaxWidth(int[][] grid, bool[][] processed, int x, int y, int targetColor) {
			int width = 0;
			int rowWidth = grid[0].Length;
			while(x + width < rowWidth && !processed[y][x + width] && grid[y][x + width] == targetColor) {
				width++;
			}
			return width;
		}

		private int GetMaxHeight(int[][] grid, bool[][] processed, int x, int y, int width, int targetColor) {
			int height = 1;
			int gridHeight = grid.Length;
			int rowWidth = grid[0].Length;

			for(int currentY = y + 1; currentY < gridHeight; currentY++) {
				for(int dx = 0; dx < width; dx++) {
					int currentX = x + dx;
					if(currentX >= rowWidth || processed[currentY][currentX] || grid[currentY][currentX] != targetColor) {
						return height;
					}
				}
				height++;
			}
			return height;
		}

		private bool IsRectangleUnprocessed(bool[][] processed, int x, int y, int width, int height) {
			int procHeight = processed.Length;
			int procWidth = processed[0].Length;

			for(int dy = 0; dy < height; dy++) {
				for(int dx = 0; dx < width; dx++) {
					if(y + dy >= procHeight || x + dx >= procWidth || processed[y + dy][x + dx]) {
						return false;
					}
				}
			}
			return true;
		}

		private void MarkProcessed(bool[][] processed, int x, int y, int width, int height) {
			int procHeight = processed.Length;
			int procWidth = processed[0].Length;

			for(int dy = 0; dy < height; dy++) {
				for(int dx = 0; dx < width; dx++) {
					if(y + dy < procHeight && x + dx < procWidth) {
						processed[y + dy][x + dx] = true;
					}
				}
			}
		}
	}
}
