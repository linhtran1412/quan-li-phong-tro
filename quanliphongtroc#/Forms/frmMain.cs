using quanliphongtro.DAL;
using quanliphongtro.Forms;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace quanliphongtro.Forms
{
    public class frmMain : Form
    {
        private readonly Color BG_DARK = Color.FromArgb(15, 15, 30);
        private readonly Color SIDEBAR = Color.FromArgb(20, 20, 42);
        private readonly Color ACCENT = Color.FromArgb(99, 102, 241);
        private readonly Color TEXT_PRI = Color.FromArgb(230, 230, 255);
        private readonly Color TEXT_SEC = Color.FromArgb(160, 160, 200);
        private readonly Color HOVER = Color.FromArgb(40, 40, 70);

        private Panel pnlContent;
        private Label lblClock, lblUser;
        private Button activeBtn;
        private readonly TaiKhoan _user;

        public frmMain(TaiKhoan user)
        {
            _user = user;
            this.Text = "Quản Lý Phòng Trọ";
            this.Size = new Size(1280, 780);
            this.MinimumSize = new Size(1100, 680);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = BG_DARK;
            this.Font = new Font("Segoe UI", 9.5f);
            BuildUI();
        }

        private void BuildUI()
        {
            bool isAdmin = _user.MaVaiTro == 1;

            // ── Header ────────────────────────────────────────────────
            var pnlHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 58,
                BackColor = Color.FromArgb(18, 18, 38)
            };

            var lblApp = new Label
            {
                Text = "🏠  Quản Lý Phòng Trọ",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = TEXT_PRI,
                AutoSize = true,
                Location = new Point(220 + 20, 14)
            };

            lblClock = new Label
            {
                Font = new Font("Segoe UI", 10),
                ForeColor = TEXT_SEC,
                AutoSize = true,
                Location = new Point(900, 20)
            };

            lblUser = new Label
            {
                Text = $"👤  {_user.TenDangNhap}  |  {_user.TenVaiTro}",
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                ForeColor = ACCENT,
                AutoSize = true,
                Location = new Point(1050, 20)
            };

            var btnLogout = new Button
            {
                Text = "⏻  Thoát",
                Location = new Point(1160, 13),
                Size = new Size(90, 32),
                BackColor = Color.FromArgb(60, 30, 30),
                ForeColor = Color.FromArgb(220, 80, 80),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnLogout.FlatAppearance.BorderSize = 0;
            btnLogout.Click += (s, e) => {
                if (MessageBox.Show("Đăng xuất?", "Xác nhận", MessageBoxButtons.YesNo) == DialogResult.Yes)
                { Application.Restart(); }
            };

            pnlHeader.Controls.AddRange(new Control[] { lblApp, lblClock, lblUser, btnLogout });

            // Timer đồng hồ
            var timer = new Timer { Interval = 1000, Enabled = true };
            timer.Tick += (s, e) => lblClock.Text = DateTime.Now.ToString("HH:mm:ss  |  dd/MM/yyyy");
            timer.Tick += null; lblClock.Text = DateTime.Now.ToString("HH:mm:ss  |  dd/MM/yyyy");

            // ── Sidebar ───────────────────────────────────────────────
            var pnlSide = new Panel
            {
                Dock = DockStyle.Left,
                Width = 220,
                BackColor = SIDEBAR
            };

            var lblMenu = new Label
            {
                Text = "MENU CHÍNH",
                Font = new Font("Segoe UI", 7.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(90, 90, 130),
                AutoSize = true,
                Location = new Point(16, 68)
            };

            int y = 90;
            Button btnFirst = null;

            Button AddMenu(string icon, string label, bool adminOnly = false)
            {
                if (adminOnly && !isAdmin) return null;
                var btn = new Button
                {
                    Text = $"  {icon}  {label}",
                    TextAlign = ContentAlignment.MiddleLeft,
                    Size = new Size(220, 48),
                    Location = new Point(0, y),
                    BackColor = Color.Transparent,
                    ForeColor = TEXT_SEC,
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 10f),
                    Cursor = Cursors.Hand,
                    Tag = label
                };
                btn.FlatAppearance.BorderSize = 0;
                btn.MouseEnter += (s, e) => { if (btn != activeBtn) btn.BackColor = HOVER; };
                btn.MouseLeave += (s, e) => { if (btn != activeBtn) btn.BackColor = Color.Transparent; };
                pnlSide.Controls.Add(btn);
                if (btnFirst == null) btnFirst = btn;
                y += 49;
                return btn;
            }

            void AddSep()
            {
                var sep = new Label
                {
                    Size = new Size(180, 1),
                    Location = new Point(20, y + 5),
                    BackColor = Color.FromArgb(40, 40, 65)
                };
                pnlSide.Controls.Add(sep);
                y += 16;
            }

            var btnDashboard = AddMenu("📊", "Tổng Quan");
            AddSep();
            var btnKhuTro = AddMenu("🏘️", "Khu Trọ", true);
            var btnPhong = AddMenu("🚪", "Phòng Trọ");
            var btnThietBi = AddMenu("🔧", "Thiết Bị", true);
            AddSep();
            var btnKhach = AddMenu("👥", "Khách Thuê");
            var btnHopDong = AddMenu("📋", "Hợp Đồng");
            AddSep();
            var btnChiSo = AddMenu("⚡", "Điện - Nước");
            var btnDichVu = AddMenu("🛎️", "Dịch Vụ", true);
            var btnHoaDon = AddMenu("🧾", "Hóa Đơn");
            AddSep();
            var btnChiPhi = AddMenu("💸", "Chi Phí", true);
            var btnBaoCao = AddMenu("📈", "Báo Cáo", true);
            AddSep();
            var btnTaiKhoan = AddMenu("🔑", "Tài Khoản", true);

            pnlSide.Controls.Add(lblMenu);

            // Phiên bản ở đáy sidebar
            var lblVer = new Label
            {
                Text = "v1.0 | HCMUNRE 2025",
                Font = new Font("Segoe UI", 7.5f),
                ForeColor = Color.FromArgb(60, 60, 90),
                AutoSize = true,
                Location = new Point(12, 700)
            };
            pnlSide.Controls.Add(lblVer);

            // ── Content area ──────────────────────────────────────────
            pnlContent = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = BG_DARK,
                Padding = new Padding(0)
            };

            // ── Wire up buttons ───────────────────────────────────────
            void SetActive(Button btn, Form child)
            {
                if (activeBtn != null) { activeBtn.BackColor = Color.Transparent; activeBtn.ForeColor = TEXT_SEC; }
                btn.BackColor = ACCENT; btn.ForeColor = Color.White;
                activeBtn = btn;

                pnlContent.Controls.Clear();
                if (child != null)
                {
                    child.TopLevel = false;
                    child.FormBorderStyle = FormBorderStyle.None;
                    child.Dock = DockStyle.Fill;
                    pnlContent.Controls.Add(child);
                    child.Show();
                }
            }

            if (btnDashboard != null) btnDashboard.Click += (s, e) => SetActive(btnDashboard, new frmDashboard(_user));
            if (btnKhuTro != null) btnKhuTro.Click += (s, e) => SetActive(btnKhuTro, new frmKhuTro());
            if (btnPhong != null) btnPhong.Click += (s, e) => SetActive(btnPhong, new frmPhongTro());
            if (btnThietBi != null) btnThietBi.Click += (s, e) => SetActive(btnThietBi, new frmThietBi());
            if (btnKhach != null) btnKhach.Click += (s, e) => SetActive(btnKhach, new frmKhachHang());
            if (btnHopDong != null) btnHopDong.Click += (s, e) => SetActive(btnHopDong, new frmHopDong());
            if (btnChiSo != null) btnChiSo.Click += (s, e) => SetActive(btnChiSo, new frmChiSoDienNuoc(_user));
            if (btnDichVu != null) btnDichVu.Click += (s, e) => SetActive(btnDichVu, new frmDichVu());
            if (btnHoaDon != null) btnHoaDon.Click += (s, e) => SetActive(btnHoaDon, new frmHoaDon());
            if (btnChiPhi != null) btnChiPhi.Click += (s, e) => SetActive(btnChiPhi, new frmChiPhi());
            if (btnBaoCao != null) btnBaoCao.Click += (s, e) => SetActive(btnBaoCao, new frmBaoCao());
            if (btnTaiKhoan != null) btnTaiKhoan.Click += (s, e) => SetActive(btnTaiKhoan, new frmTaiKhoan(_user));

            this.Controls.AddRange(new Control[] { pnlContent, pnlSide, pnlHeader });

            // Load dashboard mặc định
            this.Shown += (s, e) =>
            {
                var startBtn = isAdmin ? btnDashboard : btnChiSo;
                startBtn?.PerformClick();
            };
        }
    }
}