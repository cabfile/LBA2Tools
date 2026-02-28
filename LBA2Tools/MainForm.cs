using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace LBA2Tools {
	public partial class MainForm : Form {
		public MainForm() {
			InitializeComponent();
#if !DEBUG
			tabControl1.TabPages.Remove(tabPage2);
			tabPage2.Dispose();
#endif
			openFileDialog1.Filter = Program.fileFilter;
		}

		private void MainForm_Load(object sender, EventArgs e) {
		}

		private void button1_Click(object sender, EventArgs e) {
			var res = openFileDialog1.ShowDialog();
			if(res == DialogResult.OK) textBox1.Text = openFileDialog1.FileName;
		}

		private void button2_Click(object sender, EventArgs e) {
			ImageToNeon.Generate(textBox1.Text, (int)numericUpDown1.Value, (int)numericUpDown2.Value, !radioButton1.Checked ? textBox2.Text : "");
		}

		private void button3_Click(object sender, EventArgs e) {
			textBox2.Text = Clipboard.GetText();
			radioButton2.Checked = true;
		}

		private void radioButton1_CheckedChanged(object sender, EventArgs e) {
			textBox2.ReadOnly = true;
		}

		private void radioButton2_CheckedChanged(object sender, EventArgs e) {
			textBox2.ReadOnly = false;
		}

		private void button4_Click(object sender, EventArgs e) {
			var res = folderBrowserDialog1.ShowDialog();
			if(res == DialogResult.OK) textBox3.Text = folderBrowserDialog1.SelectedPath;
		}

		private void button5_Click(object sender, EventArgs e) {
			if(radioButton3.Checked) {
				ThumbGen.Generate(textBox3.Text);
			} else {
				ShapeMaker.Generate(textBox3.Text);
			}
		}

		private readonly Dictionary<byte, Color> colors = new Dictionary<byte, Color> {
			{ 0, Color.FromArgb(0,0,0) },
			{ 1, Color.FromArgb(0,0,127) },
			{ 2, Color.FromArgb(0,127,0) },
			{ 3, Color.FromArgb(0,127,127) },
			{ 4, Color.FromArgb(127,0,0) },
			{ 5, Color.FromArgb(127,0,127) },
			{ 6, Color.FromArgb(127,127,0) },
			{ 7, Color.FromArgb(127,127,127) },
			{ 8, Color.FromArgb(191,191,191) },
			{ 9, Color.FromArgb(0,0,255) },
			{ 10, Color.FromArgb(0,255,0) },
			{ 11, Color.FromArgb(0,255,255) },
			{ 12, Color.FromArgb(255,0,0) },
			{ 13, Color.FromArgb(255,0,255) },
			{ 14, Color.FromArgb(255,255,0) },
			{ 15, Color.FromArgb(255,255,255) }
		};
		private void button6_Click(object sender, EventArgs e) {
			if(tabControl2.SelectedIndex == 0) {
				var level = new LBA2Level();
				uint count = 0;
				using(var stream = System.IO.File.OpenRead(textBox5.Text))
				using(var reader = new System.IO.BinaryReader(stream)) {
					level.Level.entranceX = reader.ReadSingle();
					level.Level.entranceY = reader.ReadSingle();
					if(checkBox3.Checked) LBA2Level.Helpers.AddBox(ref level, 5120, 640, 0, 1280, immovable: true);
					count = reader.ReadUInt32();
					for(uint i = 0; i < count; i++) {
						float x = reader.ReadSingle();
						float y = reader.ReadSingle();
						float angle = reader.ReadSingle();
						float width = reader.ReadSingle();
						float height = reader.ReadSingle();
						bool immovable = reader.ReadByte() != 0;
						byte col = reader.ReadByte();
						if(col >= 16) {
							Color color = colors[(byte)(col % 16)];
							LBA2Level.Helpers.AddBox(ref level, x, y, angle, width, height, immovable: immovable, friction: 0.8, elasticity: 0.4, animationFrame: 17, colorR: color.R, colorG: color.G, colorB: color.B);
						}
						else {
							int mat = 5;
							if(col >= 0 && col < 10) mat = 1;
							else if(col == 11) mat = 0;
							LBA2Level.Helpers.AddBox(ref level, x, y, angle, width, height, immovable: immovable, friction: 0.8, elasticity: 0.4, animationFrame: mat);
						}
					}
				}
				Clipboard.SetText(level.ToString());
				MessageBox.Show("The level has been converted.\n" + (count + (checkBox3.Checked ? 1 : 0)) + " objects, the Entrance is at X " + level.Level.entranceX + ", Y " + level.Level.entranceY + ".", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
			}
			else if(tabControl2.SelectedIndex == 1) {
				var level = new LBA2Level();
				double scale = checkBox2.Checked ? 2 / 1.5 : 1.0;
				int offsetY = (int)(1216 - 288 * scale);
				level.Level.entranceX = 80 * scale;
				if(checkBox1.Checked) {
					LBA2Level.Helpers.AddBox(ref level, 640 * scale, 640, 0, 0, 1280, immovable: true);
					LBA2Level.Helpers.AddBox(ref level, 320 * scale, offsetY, 0, 640 * scale, 0, immovable: true);
				}
				string[] objs = textBox4.Text.Split('|');
				uint count = 0;
				for(int i = 0; i < objs.Length; i++) {
					char type = objs[i][0];
					string[] data = objs[i].Substring(1).Split(',');
					count++;
					switch(type) {
						case 'b':
							int mat = 5;
							switch(Convert.ToInt32(data[2])) {
								case 1: mat = 10; break;
								case 2: mat = 1; break;
								case 3: mat = 4; break;
								case 4: mat = 12; break;
							}
							LBA2Level.Helpers.AddBox(ref level, Convert.ToDouble(data[0]) * scale, Convert.ToDouble(data[1]) * scale + offsetY, 0, 24 * scale, 24 * scale, immovable: true, animationFrame: mat);
							break;
						case 'm':
							string son = "85046_newgrounds_parago";
							int fr = 1;
							switch(Convert.ToInt32(data[2])) {
								case 1: son = "squsinsaw music"; fr = 4; break;
								case 2: son = "thief"; fr = 5; break;
								case 3: son = "fmsynthsong"; fr = 4; break;
								case 4: son = "fanfare"; fr = 4; break;
							}
							level.Add("music", Convert.ToDouble(data[0]) * scale, Convert.ToDouble(data[1]) * scale + offsetY, "Default", fr, 64, son, 1);
							break;
						default:
							if(count > 0) count--;
							break;
					}
				}
				Clipboard.SetText(level.ToString());
				MessageBox.Show("The level has been converted.\n" + (count + (checkBox1.Checked ? 2 : 0)) + " objects, the Entrance is at X " + level.Level.entranceX + ", Y " + level.Level.entranceY + ".", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
			}
			else if(tabControl2.SelectedIndex == 2) {
				BounceConv.Convert(textBox6.Text,
								   BounceConv.GetMaterial(imagePickerButton1.ImageIndex),
								   BounceConv.GetMaterial(imagePickerButton2.ImageIndex),
								   BounceConv.GetMaterial(imagePickerButton5.ImageIndex),
								   BounceConv.GetMaterial(imagePickerButton3.ImageIndex),
								   BounceConv.GetMaterial(imagePickerButton4.ImageIndex));
			}
		}

		private void button7_Click(object sender, EventArgs e) {
			var res = openFileDialog2.ShowDialog();
			if(res == DialogResult.OK) textBox5.Text = openFileDialog2.FileName;
		}

		private void button8_Click(object sender, EventArgs e) {
			var res = openFileDialog3.ShowDialog();
			if(res == DialogResult.OK) textBox6.Text = openFileDialog3.FileName;
		}

		private void exitToolStripMenuItem_Click(object sender, EventArgs e) {
			Close();
		}

		private void wikiToolStripMenuItem_Click(object sender, EventArgs e) {
			try {
				System.Diagnostics.Process.Start("https://littlebigawoglet.miraheze.org/wiki/LBA2Tools");
			}
			catch {
				MessageBox.Show("Couldn't open the URL. Do you not have a browser or something?", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		private void aboutToolStripMenuItem_Click(object sender, EventArgs e) {
			new AboutForm().ShowDialog();
		}

		private void howToUseToolStripMenuItem_Click(object sender, EventArgs e) {
			string instructions = "What?";
			switch(tabControl1.SelectedIndex) {
				case 0:
					instructions = "1. Browse for or enter a path to an image file of your choice.";
					instructions += "\n2. Set the width and height of each Box that makes up the image's pixels.";
					instructions += "\n3. Pick the mode. If you want to use the image in an existing level, choose \"Append to level\".";
					instructions += "\n4. Click \"Convert\". The result (a level string) will be stored in your clipboard.";
					instructions += "\n5. In Create Mode, open the Game Menu, and paste the string into \"String\" field, then click \"From String\".";
					instructions += "\n\nYou will find your image near the top left corner of the level.";
					break;
				case 1:
					instructions = "1. Choose what game or port you want to import levels from.";
					instructions += "\n2. Browse for or enter a path to a level file of your choice (Classic/Bounce), or enter a level string (J2ME).";
					instructions += "\n3. Change settings to your liking.";
					instructions += "\n4. Click \"Import\". The result (a level string) will be stored in your clipboard.";
					instructions += "\n5. In Create Mode, open the Game Menu, and paste the string into \"String\" field, then click \"From String\".";
					break;
			}
			MessageBox.Show(instructions, "Instructions", MessageBoxButtons.OK, MessageBoxIcon.Information);
		}
	}
}
