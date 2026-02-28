using System.Linq;
using Newtonsoft.Json;
using LZStringCSharp;

public class LBA2Level {
	public LevelData Level { get; set; }

	public LBA2Level(string compressedString = null) {
		Level = new LevelData {
			entranceX = 96,
			entranceY = 1184,
			paused = false,
			props = new LevelProperties {
				waterheight = 0,
				gravity = 100,
				theme = 0,
				grabbing = 0,
				danec = 0,
				walljump = 1,
				doublejump = 1,
				startas = 1,
				shader = 0
			},
			spritedata = string.Empty,
			logic = new LogicPiece[0]
		};

		if(!string.IsNullOrEmpty(compressedString)) Level = JsonConvert.DeserializeObject<LevelData>(compressedString);
	}

	public void Add(params object[] data) {
		if(data == null || data.Length == 0) return;
		string str = "";
		if(!string.IsNullOrEmpty(Level.spritedata)) str += "|";
		Level.spritedata += str + string.Join(",", data);
	}

	public void Remove(int index) {
		var objs = Level.spritedata.Split('|').ToList();
		if(index >= 0 && index < objs.Count) {
			objs.RemoveAt(index);
			Level.spritedata = string.Join("|", objs);
		}
	}

	public override string ToString() {
		return LZString.CompressToBase64(JsonConvert.SerializeObject(Level));
	}

	public class LevelData {
		public double entranceX { get; set; }
		public double entranceY { get; set; }
		public bool paused { get; set; }
		public LevelProperties props { get; set; }
		public string spritedata { get; set; }
		public LogicPiece[] logic { get; set; }
	}

	public class LevelProperties {
		public double waterheight { get; set; }
		public double gravity { get; set; }
		public byte theme { get; set; }
		public byte grabbing { get; set; }
		public byte danec { get; set; }
		public byte walljump { get; set; }
		public byte doublejump { get; set; }
		public byte startas { get; set; }
		public byte shader { get; set; }
	}
	public struct LogicPiece {
		public string type { get; set; }
		public double x { get; set; }
		public double y { get; set; }
		public bool vis { get; set; }
		public object[] inputs { get; set; }
		public object[] connections { get; set; }
		public object[] props { get; set; }
	}
	public class Helpers {
		public static void AddBox(ref LBA2Level level, double x, double y, double angle = 0, double width = 32, double height = 32, double friction = 0.8, double elasticity = 0, int animationFrame = 5, bool immovable = false, double velocityX = 0, double velocityY = 0, double velocityAng = 0, string animationName = "Default", bool visible = true, bool connected = false, double connectX = 0, double connectY = 0, int colorR = 255, int colorG = 255, int colorB = 255, int connectIID = -1, bool platform = false, double connectAnchorX = 0, double connectAnchorY = 0, double connectOtherAnchorX = 0, double connectOtherAnchorY = 0) {
			level.Add("box", x, y, angle, width, height, friction, elasticity, 0, animationFrame, immovable ? 1 : 0, velocityX, velocityY, velocityAng, "", animationName, visible ? 1 : 0, connected ? 1 : 0, connectX, connectY, colorR + "." + colorG + "." + colorB, connectIID, platform ? 1 : 0, connectAnchorX, connectAnchorY, connectOtherAnchorX, connectOtherAnchorY);
		}
	}
}