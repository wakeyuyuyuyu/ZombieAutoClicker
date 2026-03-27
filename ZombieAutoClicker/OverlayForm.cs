using System.Drawing;
using System.Windows.Forms;
using ZombieAutoClicker.Core.Interfaces;

namespace ZombieAutoClicker
{
    public partial class OverlayForm : Form
    {
        private VisionOCRResult _ocrResult;

        public OverlayForm()
        {
            InitializeComponent();
            this.FormBorderStyle = FormBorderStyle.None;
            this.WindowState = FormWindowState.Maximized;
            this.TopMost = true;
            this.BackColor = Color.Magenta;
            this.TransparencyKey = Color.Magenta;
            this.DoubleBuffered = true;
        }

        public void UpdateOcrResult(VisionOCRResult ocrResult)
        {
            _ocrResult = ocrResult;
            this.Invalidate(); // Triggers the Paint event
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.Clear(this.BackColor);

            if (_ocrResult == null || _ocrResult.TextBlocks == null) return;

            Graphics g = e.Graphics;

            using (Pen redPen = new Pen(Color.Red, 2))
            using (Font drawFont = new Font("Arial", 12))
            using (SolidBrush drawBrush = new SolidBrush(Color.Red))
            {
                foreach (var block in _ocrResult.TextBlocks)
                {
                    if (block.BoxPoints != null && block.BoxPoints.Length >= 4)
                    {
                        // Draw bounding box
                        Point[] points = new Point[4];
                        for (int i = 0; i < 4; i++)
                        {
                            points[i] = new Point(block.BoxPoints[i].X, block.BoxPoints[i].Y);
                        }
                        g.DrawPolygon(redPen, points);

                        // Draw text label
                        string displayText = block.Text;
                        if (!string.IsNullOrEmpty(displayText))
                        {
                            Point textPoint = new Point(points[0].X, points[0].Y - 20);
                            g.DrawString(displayText, drawFont, drawBrush, textPoint);
                        }
                    }
                }
            }
        }
    }
}