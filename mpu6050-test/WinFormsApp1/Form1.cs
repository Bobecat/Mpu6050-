using System.Globalization;
using System.IO.Ports;
using System.Threading;

namespace WinFormsApp1
{
    public partial class Form1 : Form
    {
        private SerialPort _serial;
        private double _currentRollDeg = double.NaN;
        private double _currentPitchDeg = double.NaN;

        public Form1()
        {
            InitializeComponent();
            _serial = new SerialPort();
            _serial.DataReceived += Serial_DataReceived;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // populate available COM ports
            try
            {
                var ports = SerialPort.GetPortNames();
                cmbPorts.Items.Clear();
                cmbPorts.Items.AddRange(ports);
                if (ports.Length > 0) cmbPorts.SelectedIndex = 0;
            }
            catch
            {
                // ignore
            }
        }

        private void btnOpenClose_Click(object sender, EventArgs e)
        {
            if (_serial.IsOpen)
            {
                try
                {
                    _serial.Close();
                    btnOpenClose.Text = "Open";
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to close port: " + ex.Message);
                }
                return;
            }

            if (cmbPorts.SelectedItem == null)
            {
                MessageBox.Show("Please select a COM port.");
                return;
            }

            _serial.PortName = cmbPorts.SelectedItem.ToString();
            _serial.BaudRate = 115200; // adjust if your device uses different baud
            _serial.NewLine = "\n";
            try
            {
                _serial.Open();
                btnOpenClose.Text = "Close";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to open port: " + ex.Message);
            }
        }

        private void Serial_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                var line = _serial.ReadLine();
                if (string.IsNullOrWhiteSpace(line)) return;
                var text = line.Trim();

                // expected format: Acc[X, Y, Z] | Gyro[X, Y, Z] | Temp[T]
                double accX = double.NaN, accY = double.NaN, accZ = double.NaN;
                double gyroX = double.NaN, gyroY = double.NaN, gyroZ = double.NaN;

                var parts = text.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var p in parts)
                {
                    var seg = p.Trim();
                    try
                    {
                        string key;
                        string inside;
                        var idx = seg.IndexOf('[');
                        var idx2 = seg.IndexOf(']');
                        if (idx >= 0 && idx2 > idx)
                        {
                            key = seg.Substring(0, idx).Trim().ToLowerInvariant();
                            inside = seg.Substring(idx + 1, idx2 - idx - 1);
                        }
                        else
                        {
                            var colon = seg.IndexOf(':');
                            if (colon >= 0)
                            {
                                key = seg.Substring(0, colon).Trim().ToLowerInvariant();
                                inside = seg.Substring(colon + 1).Trim();
                                // remove surrounding brackets if present
                                if (inside.StartsWith("[") && inside.EndsWith("]"))
                                {
                                    inside = inside.Substring(1, inside.Length - 2).Trim();
                                }
                            }
                            else
                            {
                                continue; // unknown segment
                            }
                        }

                        var nums = inside.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        if (key.StartsWith("acc"))
                        {
                            if (nums.Length >= 3)
                            {
                                double.TryParse(nums[0], NumberStyles.Float, CultureInfo.InvariantCulture, out accX);
                                double.TryParse(nums[1], NumberStyles.Float, CultureInfo.InvariantCulture, out accY);
                                double.TryParse(nums[2], NumberStyles.Float, CultureInfo.InvariantCulture, out accZ);
                            }
                        }
                        else if (key.StartsWith("gyro"))
                        {
                            if (nums.Length >= 3)
                            {
                                double.TryParse(nums[0], NumberStyles.Float, CultureInfo.InvariantCulture, out gyroX);
                                double.TryParse(nums[1], NumberStyles.Float, CultureInfo.InvariantCulture, out gyroY);
                                double.TryParse(nums[2], NumberStyles.Float, CultureInfo.InvariantCulture, out gyroZ);
                            }
                        }
                    }
                    catch
                    {
                        // ignore per-segment parse errors
                    }
                }

                BeginInvoke(() => UpdateSensorDisplay(accX, accY, accZ, gyroX, gyroY, gyroZ, text));
            }
            catch
            {
                // ignore parse errors
            }
        }

        private void UpdateSensorDisplay(double accX, double accY, double accZ, double gyroX, double gyroY, double gyroZ, string raw)
        {
            if (!double.IsNaN(accX) && !double.IsNaN(accY) && !double.IsNaN(accZ))
                lblAcc.Text = $"Acc: X:{accX:F2} Y:{accY:F2} Z:{accZ:F2}";
            else
                lblAcc.Text = "Acc: ---";

            if (!double.IsNaN(gyroX) && !double.IsNaN(gyroY) && !double.IsNaN(gyroZ))
                lblGyro.Text = $"Gyro: X:{gyroX:F2} Y:{gyroY:F2} Z:{gyroZ:F2}";
            else
                lblGyro.Text = "Gyro: ---";

            this.Tag = raw;
            // compute roll/pitch from accel for visualization
            if (!double.IsNaN(accX) && !double.IsNaN(accY) && !double.IsNaN(accZ))
            {
                // roll = atan2(accY, accZ); pitch = atan2(-accX, sqrt(accY^2+accZ^2))
                var roll = Math.Atan2(accY, accZ) * (180.0 / Math.PI);
                var pitch = Math.Atan2(-accX, Math.Sqrt(accY * accY + accZ * accZ)) * (180.0 / Math.PI);
                _currentRollDeg = roll;
                _currentPitchDeg = pitch;
            }

            // request redraw
            try
            {
                pbDisplay.Invalidate();
            }
            catch
            {
                // ignore if control not yet ready
            }
        }

        private void pbDisplay_Paint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            var w = pbDisplay.ClientSize.Width;
            var h = pbDisplay.ClientSize.Height;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            // background
            g.Clear(System.Drawing.Color.Black);

            // center
            var cx = w / 2.0f;
            var cy = h / 2.0f;

            // draw outer circle representing attitude indicator
            var radius = Math.Min(w, h) * 0.45f;
            var rect = new System.Drawing.RectangleF(cx - (float)radius, cy - (float)radius, (float)radius * 2, (float)radius * 2);
            using (var penOuter = new System.Drawing.Pen(System.Drawing.Color.White, 2))
            {
                g.DrawEllipse(penOuter, rect);
            }

            if (double.IsNaN(_currentRollDeg) || double.IsNaN(_currentPitchDeg))
            {
                // no data
                using (var sf = new System.Drawing.StringFormat { Alignment = System.Drawing.StringAlignment.Center, LineAlignment = System.Drawing.StringAlignment.Center })
                using (var f = new System.Drawing.Font("Segoe UI", 10))
                {
                    g.DrawString("No accel data", f, System.Drawing.Brushes.White, new System.Drawing.PointF(cx, cy), sf);
                }
                return;
            }

            // compute horizon line endpoints using roll (angle) and pitch (vertical offset)
            var rollRad = _currentRollDeg * Math.PI / 180.0;
            var pitchPixels = (float)(_currentPitchDeg * 2.0); // scale: degrees -> pixels

            // compute y at x=0 and x=w
            float y0 = cy + pitchPixels + (float)(Math.Tan(rollRad) * (0 - cx));
            float yw = cy + pitchPixels + (float)(Math.Tan(rollRad) * (w - cx));

            // validate and clamp
            bool IsFinite(float v) => !float.IsNaN(v) && !float.IsInfinity(v);
            if (!IsFinite(y0) || !IsFinite(yw))
            {
                // invalid geometry, skip drawing horizon
                using (var sf = new System.Drawing.StringFormat { Alignment = System.Drawing.StringAlignment.Center, LineAlignment = System.Drawing.StringAlignment.Center })
                using (var f = new System.Drawing.Font("Segoe UI", 10))
                {
                    g.DrawString("Invalid attitude data", f, System.Drawing.Brushes.White, new System.Drawing.PointF(cx, cy), sf);
                }
                return;
            }

            float ClampY(float yy) => Math.Max(-10000f, Math.Min(h + 10000f, yy));

            var skyPts = new System.Drawing.PointF[] {
                new System.Drawing.PointF(0,0),
                new System.Drawing.PointF(w,0),
                new System.Drawing.PointF(w,ClampY(yw)),
                new System.Drawing.PointF(0,ClampY(y0))
            };

            var groundPts = new System.Drawing.PointF[] {
                new System.Drawing.PointF(0,ClampY(y0)),
                new System.Drawing.PointF(w,ClampY(yw)),
                new System.Drawing.PointF(w,h),
                new System.Drawing.PointF(0,h)
            };

            try
            {
                using (var skyBrush = new System.Drawing.SolidBrush(System.Drawing.Color.SkyBlue))
                using (var groundBrush = new System.Drawing.SolidBrush(System.Drawing.Color.SaddleBrown))
                {
                    g.FillPolygon(skyBrush, skyPts);
                    g.FillPolygon(groundBrush, groundPts);
                }

                // draw horizon line
                using (var pen = new System.Drawing.Pen(System.Drawing.Color.Yellow, 3))
                {
                    g.DrawLine(pen, 0, y0, w, yw);
                }

                // draw center aircraft symbol
                using (var pen = new System.Drawing.Pen(System.Drawing.Color.White, 2))
                {
                    var wingLen = radius * 0.4f;
                    g.DrawLine(pen, cx - wingLen, cy, cx + wingLen, cy);
                    g.DrawLine(pen, cx, cy - 10, cx, cy + 10);
                }

                // draw text with angles
                using (var sf = new System.Drawing.StringFormat { Alignment = System.Drawing.StringAlignment.Near, LineAlignment = System.Drawing.StringAlignment.Near })
                using (var f = new System.Drawing.Font("Segoe UI", 9))
                {
                    var txt = $"Roll: {_currentRollDeg:F1}°\nPitch: {_currentPitchDeg:F1}°";
                    g.DrawString(txt, f, System.Drawing.Brushes.White, new System.Drawing.PointF(8, 8), sf);
                }
            }
            catch (Exception ex)
            {
                // drawing failed — show message
                using (var sf = new System.Drawing.StringFormat { Alignment = System.Drawing.StringAlignment.Center, LineAlignment = System.Drawing.StringAlignment.Center })
                using (var f = new System.Drawing.Font("Segoe UI", 10))
                {
                    g.DrawString("Draw error: " + ex.Message, f, System.Drawing.Brushes.White, new System.Drawing.PointF(cx, cy), sf);
                }
            }
        }

        private void pbDisplay_Click(object sender, EventArgs e)
        {

        }
    }
}
