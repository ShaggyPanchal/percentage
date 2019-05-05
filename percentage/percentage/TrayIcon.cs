using System;
using System.Drawing;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace percentage
{
	class TrayIcon
	{
		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		static extern bool DestroyIcon(IntPtr handle);

		private const string iconFont = "Segoe UI";

		private string batteryPercentage;
		private NotifyIcon notifyIcon;

		public TrayIcon()
		{
			ContextMenu contextMenu = new ContextMenu();
			MenuItem menuItem = new MenuItem();

			notifyIcon = new NotifyIcon();

			// initialize contextMenu
			contextMenu.MenuItems.AddRange(new MenuItem[] { menuItem });

			// initialize menuItem
			menuItem.Index = 0;
			menuItem.Text = "E&xit";
			menuItem.Click += new System.EventHandler(MenuItem_Click);

			notifyIcon.ContextMenu = contextMenu;

			batteryPercentage = "?";

			notifyIcon.Visible = true;

			Timer timer = new Timer();
			timer.Tick += new EventHandler(Timer_Tick);
			timer.Interval = 1000; // in miliseconds
			timer.Start();
		}

		private void Timer_Tick(object sender, EventArgs e)
		{
			try
			{
				PowerStatus powerStatus = SystemInformation.PowerStatus;
				batteryPercentage = (powerStatus.BatteryLifePercent * 100).ToString();

				UpdateNotifyIcon(ref notifyIcon, (int)(powerStatus.BatteryLifePercent * 100));
			}
			catch (Exception)
			{
			}
		}

		private void MenuItem_Click(object sender, EventArgs e)
		{
			notifyIcon.Visible = false;
			notifyIcon.Dispose();
			Application.Exit();
		}

		private Image DrawText(String text, Font font, Color textColor, Color backColor)
		{
			var textSize = GetImageSize(text, font);
			//Image image = new Bitmap((int)textSize.Width, (int)textSize.Height);
			Image image = new Bitmap(32, 32);
			using (Graphics graphics = Graphics.FromImage(image))
			{
				// paint the background
				graphics.Clear(backColor);

				// create a brush for the text
				using (Brush textBrush = new SolidBrush(textColor))
				{
					graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SingleBitPerPixelGridFit;
					graphics.DrawString(text, font, textBrush, 0, 0);
					graphics.Save();
				}
			}

			return image;
		}

		private static SizeF GetImageSize(string text, Font font)
		{
			using (Image image = new Bitmap(1, 1))
			using (Graphics graphics = Graphics.FromImage(image))
				return graphics.MeasureString(text, font);
		}


		static public void UpdateNotifyIcon(ref NotifyIcon notifyIcon, int percentage)
		{
			if (notifyIcon == null)
			{
				return;
			}

			string drawMe = percentage.ToString();
			Font fontToUse;
			Brush brushToUse = new SolidBrush(Color.White);
			Rectangle rect;
			Bitmap bitmapText;
			Graphics g;
			IntPtr hIcon = IntPtr.Zero;

			try
			{
				// draw correct icon size (prevents antialiasing due to dpi)
				int requestedSize = NativeMethods.GetSystemMetrics(NativeMethods.SystemMetric.SM_CXSMICON);

				if (requestedSize > 16)
				{
					//32x32

					if (percentage == 100)
					{
						// reduce size to fit "100"
						fontToUse = new Font(iconFont, 20, FontStyle.Regular, GraphicsUnit.Pixel);
					}
					else
					{
						fontToUse = new Font(iconFont, 24, FontStyle.Regular, GraphicsUnit.Pixel);
					}

					rect = new Rectangle(-6, 2, 42, 32);
					bitmapText = new Bitmap(32, 32);

				}
				else
				{
					//16x16

					if (percentage == 100)
					{
						// reduce size to fit "100"
						fontToUse = new Font(iconFont, 9, FontStyle.Regular, GraphicsUnit.Pixel);
					}
					else
					{
						fontToUse = new Font(iconFont, 12, FontStyle.Regular, GraphicsUnit.Pixel);
					}

					rect = new Rectangle(-2, 1, 20, 16);
					bitmapText = new Bitmap(16, 16);

				}

				g = Graphics.FromImage(bitmapText);
				g.Clear(Color.Transparent);
				g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SingleBitPerPixelGridFit;
				StringFormat sf = new StringFormat
				{
					Alignment = StringAlignment.Center,
					LineAlignment = StringAlignment.Center
				};

				g.DrawString(drawMe, fontToUse, brushToUse, rect, sf);

				hIcon = (bitmapText.GetHicon());

				notifyIcon.Icon = Icon.FromHandle(hIcon);
				notifyIcon.Text = percentage.ToString() + "%";

			}
			finally
			{
				DestroyIcon(hIcon);
			}
		}

	}
}
