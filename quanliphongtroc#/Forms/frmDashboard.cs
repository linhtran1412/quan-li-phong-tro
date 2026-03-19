using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using quanliphongtro.DAL;

namespace quanliphongtro.Forms
{
    public class frmDashboard : Form
    {
        private readonly Color BG = Color.FromArgb(15, 15, 30);
        private readonly Color CARD = Color.FromArgb(24, 24, 46);
        private readonly Color ACCENT = Color.FromArgb(99, 102, 241);
        private readonly Color TEXT = Color.FromArgb(230, 230, 255);
        private readonly Color SEC = Color.FromArgb(160, 160, 200);
        private readonly Color GREEN = Color.FromArgb(16, 185, 129);
        private readonly Color YELLOW = Color.FromArgb(234, 179, 8);
        private readonly Color RED = Color.FromArgb(220, 38, 38);
        private readonly Color BLUE = Color.FromArgb(59, 130, 246);

        private readonly TaiKhoan _user;
        private Panel pnlCanhBao;

        public frmDashboard(TaiKhoan user)
        {
            _user = user;
            this.BackColor = BG;
            this.Font = new Font("Segoe UI", 9.5f);
            this.Load += (s, e) => BuildUI();
        }

        private void BuildUI()
        {
            this.Controls.Clear();
            int W = this.Width;

            // ── Tiêu đề ──────────────────────────────────────────────
            var lblTitle = new Label
            {
                Text = $"Chào, {_user.TenDangNhap}!   📅  {DateTime.Now:dddd, dd/MM/yyyy}",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = TEXT,
                AutoSize = true,
                Location = new Point(20, 20)
            };
            this.Controls.Add(lblTitle);

            // ── KPI Cards ─────────────────────────────────────────────
            try
            {
                var baoDao = new BaoCaoDAL();
                var phongDal = new PhongDAL();
                var hopDongDal = new HopDongDAL();

                var tinhTrang = baoDao.TinhTrangPhong();
                int tongPhong = 0, phongTrong = 0, dangThue = 0, baoTri = 0;
                foreach (DataRow r in tinhTrang.Rows)
                {
                    int sl = Convert.ToInt32(r["SoPhong"]);
                    tongPhong += sl;
                    string tt = r["TrangThai"].ToString();
                    if (tt == "Trống") phongTrong = sl;
                    else if (tt == "Đang thuê") dangThue = sl;
                    else baoTri = sl;
                }

                var dtLN = baoDao.LoiNhuan(DateTime.Now.Month, DateTime.Now.Year);
                decimal doanhThu = 0, chiPhi = 0;
                if (dtLN.Rows.Count > 0)
                {
                    doanhThu = Convert.ToDecimal(dtLN.Rows[0]["DoanhThu"]);
                    chiPhi = Convert.ToDecimal(dtLN.Rows[0]["ChiPhi"]);
                }

                int sapHetHan = new HopDongDAL().GetSapHetHan(30).Rows.Count;

                // 4 cards hàng đầu
                int cx = 20, cy = 65, cw = (W - 60) / 4, ch = 110;
                AddCard("🚪  Tổng Phòng", tongPhong.ToString(), ACCENT, new Point(cx, cy), cw, ch);
                AddCard("✅  Đang Thuê", dangThue.ToString(), GREEN, new Point(cx + cw + 13, cy), cw, ch);
                AddCard("⬜  Còn Trống", phongTrong.ToString(), BLUE, new Point(cx + (cw + 13) * 2, cy), cw, ch);
                AddCard("⚠  Sắp Hết Hạn", sapHetHan.ToString(), sapHetHan > 0 ? RED : GREEN,
                                                                              new Point(cx + (cw + 13) * 3, cy), cw, ch);

                // 2 cards doanh thu
                cy += ch + 18;
                int cw2 = (W - 50) / 2;
                AddCard($"💰  Doanh Thu Tháng {DateTime.Now.Month}",
                        doanhThu.ToString("N0") + " ₫", GREEN, new Point(cx, cy), cw2, ch);
                AddCard($"💸  Chi Phí Tháng {DateTime.Now.Month}",
                        chiPhi.ToString("N0") + " ₫", RED, new Point(cx + cw2 + 10, cy), cw2, ch);

                // ── Cảnh báo HĐ sắp hết hạn ─────────────────────────
                cy += ch + 18;
                if (sapHetHan > 0)
                {
                    var dtCB = new HopDongDAL().GetSapHetHan(30);
                    AddCanhBao(dtCB, new Point(cx, cy), W - 40);
                }
            }
            catch (Exception ex)
            {
                var lbl = new Label
                {
                    Text = "⚠  Không thể tải dữ liệu: " + ex.Message,
                    ForeColor = YELLOW,
                    AutoSize = true,
                    Location = new Point(20, 80)
                };
                this.Controls.Add(lbl);
            }
        }

        private void AddCard(string title, string value, Color accent, Point loc, int w, int h)
        {
            var pnl = new Panel
            {
                Location = loc,
                Size = new Size(w, h),
                BackColor = CARD
            };

            // Thanh màu bên trái
            var bar = new Panel
            {
                Size = new Size(5, h),
                Location = new Point(0, 0),
                BackColor = accent
            };

            var lblTitle = new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 9f),
                ForeColor = SEC,
                AutoSize = true,
                Location = new Point(18, 18)
            };

            var lblVal = new Label
            {
                Text = value,
                Font = new Font("Segoe UI", 26, FontStyle.Bold),
                ForeColor = accent,
                AutoSize = true,
                Location = new Point(18, 45)
            };

            pnl.Controls.AddRange(new Control[] { bar, lblTitle, lblVal });
            this.Controls.Add(pnl);
        }

        private void AddCanhBao(DataTable dt, Point loc, int w)
        {
            var pnl = new Panel
            {
                Location = loc,
                Size = new Size(w, 200),
                BackColor = CARD
            };

            var lblH = new Label
            {
                Text = "⚠  Hợp Đồng Sắp Hết Hạn (30 ngày tới)",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = YELLOW,
                AutoSize = true,
                Location = new Point(15, 12)
            };

            var grid = new DataGridView
            {
                Location = new Point(10, 44),
                Size = new Size(w - 20, 145),
                BackgroundColor = CARD,
                BorderStyle = BorderStyle.None,
                ReadOnly = true,
                RowHeadersVisible = false,
                AllowUserToAddRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                EnableHeadersVisualStyles = false,
                Font = new Font("Segoe UI", 9f)
            };
            grid.DefaultCellStyle.BackColor = CARD;
            grid.DefaultCellStyle.ForeColor = TEXT;
            grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(35, 35, 60);
            grid.ColumnHeadersDefaultCellStyle.ForeColor = SEC;
            grid.ColumnHeadersHeight = 32;
            grid.RowTemplate.Height = 30;
            grid.DataSource = dt;

            pnl.Controls.AddRange(new Control[] { lblH, grid });
            this.Controls.Add(pnl);
        }
    }
}