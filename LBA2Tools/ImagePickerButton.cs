using System;
using System.Drawing;
using System.Windows.Forms;
using System.ComponentModel;
using System.Windows.Forms.VisualStyles;

public class ImagePickerButton : Control {
	private ImageList _imageList;
	private Image _selectedImage = null;
	private int _imageIndex = -1;

	private bool _isMouseOver = false;
	private bool _isMouseDown = false;

	[Category("Data")]
	[Description("The ImageList component that contains the images for the palette.")]
	public ImageList ImageList {
		get { return _imageList; }
		set {
			_imageList = value;
			if(_imageList == null || _imageIndex >= _imageList.Images.Count) {
				_imageIndex = -1;
				SelectedImage = null;
			}
			else {
				ApplyImageIndex();
			}
			Invalidate();
		}
	}

	[Category("Appearance")]
	[TypeConverter(typeof(ImageIndexConverter))]
	[Editor("System.Windows.Forms.Design.ImageIndexEditor, System.Design", typeof(System.Drawing.Design.UITypeEditor))]
	[DefaultValue(-1)]
	[Description("The index of the image in the ImageList to display.")]
	public int ImageIndex {
		get { return _imageIndex; }
		set {
			if(value < -1) value = -1;
			if(_imageIndex != value) {
				_imageIndex = value;
				ApplyImageIndex();
				Invalidate();
			}
		}
	}

	[Category("Appearance")]
	[Browsable(false)]
	public Image SelectedImage {
		get { return _selectedImage; }
		set {
			if(_selectedImage != value) {
				_selectedImage = value;
				Invalidate();
				OnSelectedImageChanged(EventArgs.Empty);
			}
		}
	}

	public event EventHandler SelectedImageChanged;
	public event EventHandler ImageIndexChanged;

	public ImagePickerButton() {
		Size = new Size(80, 80);
		BackColor = SystemColors.Control;
		Cursor = Cursors.Hand;
		DoubleBuffered = true;
	}

	private void ApplyImageIndex() {
		if(_imageList != null && _imageIndex >= 0 && _imageIndex < _imageList.Images.Count) {
			SelectedImage = _imageList.Images[_imageIndex];
		}
		else {
			SelectedImage = null;
		}
		OnImageIndexChanged(EventArgs.Empty);
	}

	protected virtual void OnSelectedImageChanged(EventArgs e) => SelectedImageChanged?.Invoke(this, e);
	protected virtual void OnImageIndexChanged(EventArgs e) => ImageIndexChanged?.Invoke(this, e);

	protected override void OnMouseEnter(EventArgs e) { base.OnMouseEnter(e); _isMouseOver = true; Invalidate(); }
	protected override void OnMouseLeave(EventArgs e) { base.OnMouseLeave(e); _isMouseOver = false; _isMouseDown = false; Invalidate(); }
	protected override void OnMouseDown(MouseEventArgs e) {
		base.OnMouseDown(e);
		if(e.Button == MouseButtons.Left) { _isMouseDown = true; Invalidate(); }
	}
	protected override void OnMouseUp(MouseEventArgs e) { base.OnMouseUp(e); _isMouseDown = false; Invalidate(); }

	protected override void OnPaint(PaintEventArgs e) {
		base.OnPaint(e);

		PushButtonState state;
		if(!Enabled) state = PushButtonState.Disabled;
		else if(_isMouseDown) state = PushButtonState.Pressed;
		else if(_isMouseOver) state = PushButtonState.Hot;
		else state = PushButtonState.Normal;

		ButtonRenderer.DrawButton(e.Graphics, ClientRectangle, state);

		if(_selectedImage != null) {
			int padding = 4;
			Rectangle imgRect = new Rectangle(padding, padding, Width - (padding * 2), Height - (padding * 2));
			e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
			e.Graphics.DrawImage(_selectedImage, imgRect);
		}
		else {
			TextRenderer.DrawText(e.Graphics, "Pick", Font, ClientRectangle, SystemColors.GrayText, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
		}
	}

	protected override void OnClick(EventArgs e) {
		base.OnClick(e);
		ShowImagePalette();
	}

	private void ShowImagePalette() {
		if(_imageList == null || _imageList.Images.Count == 0) return;

		ToolStripDropDown dropDown = new ToolStripDropDown();
		PickerPanel scrollPanel = new PickerPanel();
		scrollPanel.AutoScroll = true;
		scrollPanel.BackColor = SystemColors.Window;
		scrollPanel.Padding = new Padding(4);

		int columns = 3;
		int thumbSize = 48;
		int padding = 4;
		int contentWidth = (columns * thumbSize) + ((columns + 1) * padding);
		int contentHeight = (3 * thumbSize) + (4 * padding);

		scrollPanel.Width = contentWidth + SystemInformation.VerticalScrollBarWidth;
		scrollPanel.Height = contentHeight;

		for(int i = 0; i < _imageList.Images.Count; i++) {
			// FIX: Create a local copy of 'i'. 
			// If we use 'i' directly in the lambdas below, they will all use the final value of 'i' (count).
			int currentIndex = i;

			Image img = _imageList.Images[i];
			PictureBox pic = new PictureBox();
			pic.Image = img;
			pic.Size = new Size(thumbSize, thumbSize);
			pic.SizeMode = PictureBoxSizeMode.Zoom;
			pic.Cursor = Cursors.Hand;
			pic.Margin = Padding.Empty;
			pic.BackColor = Color.Transparent;

			int col = currentIndex % columns;
			int row = currentIndex / columns;
			pic.Location = new Point(padding + (col * (thumbSize + padding)), padding + (row * (thumbSize + padding)));

			// Use the Tuple to store both index and hover state
			pic.Tag = new Tuple<int, bool>(currentIndex, false);

			pic.Paint += Pic_Paint;

			pic.MouseEnter += (s, ev) => {
				pic.Tag = new Tuple<int, bool>(currentIndex, true);
				pic.Invalidate();
			};

			pic.MouseLeave += (s, ev) => {
				pic.Tag = new Tuple<int, bool>(currentIndex, false);
				pic.Invalidate();
			};

			pic.Click += (s, ev) => {
				// Now this correctly uses the specific index for this button
				ImageIndex = currentIndex;
				dropDown.Close();
			};

			scrollPanel.Controls.Add(pic);
		}

		ToolStripControlHost host = new ToolStripControlHost(scrollPanel);
		host.AutoSize = false;
		host.Size = scrollPanel.Size;

		dropDown.Items.Add(host);
		dropDown.Show(this, new Point(0, Height));
		scrollPanel.Focus();
	}

	private void Pic_Paint(object sender, PaintEventArgs e) {
		PictureBox pic = sender as PictureBox;
		if(pic.Tag is Tuple<int, bool> state) {
			if(state.Item2) // Is Hovered?
			{
				Rectangle rect = new Rectangle(0, 0, pic.Width - 1, pic.Height - 1);
				e.Graphics.DrawRectangle(new Pen(SystemColors.Highlight), rect);
			}
		}
	}

	private class PickerPanel : Panel {
		public PickerPanel() {
			DoubleBuffered = true;
			SetStyle(ControlStyles.Selectable, true);
			TabStop = false;
		}
	}
}