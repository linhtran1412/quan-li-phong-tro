using quanliphongtro.DAL;
using quanliphongtro.Forms;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace quanliphongtro.Forms
{
    public class frmDangNhap : Form
    {
        private readonly Color BG = Color.FromArgb(15, 15, 30);
        private readonly Color PANEL = Color.FromArgb(20, 20, 42);
        private readonly Color ACCENT = Color.FromArgb(99, 102, 241);
        private readonly Color TEXT = Color.FromArgb(230, 230, 255);
        private readonly Color TEXT_SEC = Color.FromArgb(160, 160, 200);
        private readonly Color SUCCESS = Color.FromArgb(16, 185, 129);
        private readonly Color DANGER = Color.FromArgb(220, 38, 38);

        private TextBox txtUser, txtPass;
        private Label lblError;
        private Button btnLogin;
        private CheckBox chkShowPass;

        public frmDangNhap()
        {
            this.Text = "Đăng Nhập - Quản Lý Phòng Trọ";
            this.Size = new Size(900, 560);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.BackColor = BG;

            BuildUI();
        }

        private void BuildUI()
        {
            // ── Branding panel (trái) ──────────────────────────────────
            var pnlLeft = new Panel
            {
                Size = new Size(380, this.Height),
                Location = new Point(0, 0),
                BackColor = PANEL
            };

            var lblApp = new Label
            {
                Text = "🏠",
                Font = new Font("Segoe UI", 52),
                ForeColor = ACCENT,
                AutoSize = true
            };
            lblApp.Location = new Point((380 - lblApp.PreferredWidth) / 2, 130);

            var lblTitle = new Label
            {
                Text = "QUẢN LÝ\nPHÒNG TRỌ",
                Font = new Font("Segoe UI", 22, FontStyle.Bold),
                ForeColor = TEXT,
                TextAlign = ContentAlignment.MiddleCenter,
                Size = new Size(340, 80),
                Location = new Point(20, 240)
            };

            var lblSub = new Label
            {
                Text = "Hệ thống quản lý phòng trọ\nchuyên nghiệp & hiệu quả",
                Font = new Font("Segoe UI", 10),
                ForeColor = TEXT_SEC,
                TextAlign = ContentAlignment.MiddleCenter,
                Size = new Size(340, 50),
                Location = new Point(20, 330)
            };

            // Dots decoration
            var lblDots = new Label
            {
                Text = "● ● ●",
                Font = new Font("Segoe UI", 14),
                ForeColor = ACCENT,
                AutoSize = true
            };
            lblDots.Location = new Point((380 - lblDots.PreferredWidth) / 2, 400);

            var lblVersion = new Label
            {
                Text = "v1.0.0 | HCMUNRE 2025",
                Font = new Font("Segoe UI", 8),
                ForeColor = Color.FromArgb(80, 80, 120),
                AutoSize = true,
                Location = new Point(20, 490)
            };

            pnlLeft.Controls.AddRange(new Control[] { lblApp, lblTitle, lblSub, lblDots, lblVersion });

            // ── Login panel (phải) ────────────────────────────────────
            int lx = 420;

            var lblWelcome = new Label
            {
                Text = "Chào mừng trở lại!",
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                ForeColor = TEXT,
                AutoSize = true,
                Location = new Point(lx, 100)
            };

            var lblWelcomeSub = new Label
            {
                Text = "Đăng nhập để tiếp tục",
                Font = new Font("Segoe UI", 10),
                ForeColor = TEXT_SEC,
                AutoSize = true,
                Location = new Point(lx, 135)
            };

            // Tên đăng nhập
            AddLabel("👤  Tên đăng nhập", new Point(lx, 190));
            txtUser = CreateInput(new Point(lx, 213), 400);
            txtUser.Text = "admin";

            // Mật khẩu
            AddLabel("🔒  Mật khẩu", new Point(lx, 265));
            txtPass = CreateInput(new Point(lx, 288), 400);
            txtPass.PasswordChar = '●';
            txtPass.Text = "admin123";
            txtPass.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) DoLogin(); };

            chkShowPass = new CheckBox
            {
                Text = "  Hiển thị mật khẩu",
                Location = new Point(lx, 325),
                ForeColor = TEXT_SEC,
                AutoSize = true,
                Font = new Font("Segoe UI", 9f)
            };
            chkShowPass.CheckedChanged += (s, e) =>
                txtPass.PasswordChar = chkShowPass.Checked ? '\0' : '●';

            lblError = new Label
            {
                Text = "",
                ForeColor = DANGER,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                Size = new Size(400, 24),
                Location = new Point(lx, 355)
            };

            btnLogin = new Button
            {
                Text = "ĐĂNG NHẬP",
                Size = new Size(400, 46),
                Location = new Point(lx, 388),
                BackColor = ACCENT,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnLogin.FlatAppearance.BorderSize = 0;
            btnLogin.Click += (s, e) => DoLogin();

            var lblHint = new Label
            {
                Text = "Mặc định: admin / admin123",
                Font = new Font("Segoe UI", 8.5f),
                ForeColor = Color.FromArgb(80, 80, 110),
                AutoSize = true,
                Location = new Point(lx, 445)
            };

            this.Controls.AddRange(new Control[] {
                pnlLeft, lblWelcome, lblWelcomeSub,
                txtUser, txtPass, chkShowPass, lblError, btnLogin, lblHint
            });

            // Thêm labels trực tiếp (AddLabel gọi Controls.Add nội bộ)
            this.Controls.Add(MakeLabel("👤  Tên đăng nhập", new Point(lx, 190)));
            this.Controls.Add(MakeLabel("🔒  Mật khẩu", new Point(lx, 265)));
        }

        private void DoLogin()
        {
            lblError.Text = "";
            string user = txtUser.Text.Trim();
            string pass = txtPass.Text;

            if (string.IsNullOrWhiteSpace(user) || string.IsNullOrWhiteSpace(pass))
            {
                lblError.Text = "⚠  Vui lòng nhập đầy đủ thông tin!";
                return;
            }

            try
            {
                btnLogin.Enabled = false;
                btnLogin.Text = "Đang xử lý...";

                var dal = new TaiKhoanDAL();
                var tk = dal.DangNhap(user, pass);

                if (tk == null)
                {
                    lblError.Text = "⚠  Tên đăng nhập hoặc mật khẩu không đúng!";
                    txtPass.Focus(); txtPass.SelectAll();
                    return;
                }

                var main = new frmMain(tk);
                main.Show();
                this.Hide();
                main.FormClosed += (s, e) => this.Close();
            }
            catch (Exception ex)
            {
                lblError.Text = "Lỗi kết nối CSDL: " + ex.Message;
            }
            finally
            {
                btnLogin.Enabled = true;
                btnLogin.Text = "ĐĂNG NHẬP";
            }
        }

        private void AddLabel(string text, Point loc)
        {
            // placeholder, labels added separately below
        }

        private Label MakeLabel(string text, Point loc)
            => new Label
            {
                Text = text,
                AutoSize = true,
                Location = loc,
                ForeColor = TEXT_SEC,
                Font = new Font("Segoe UI", 8.5f)
            };

        private TextBox CreateInput(Point loc, int w)
            => new TextBox
            {
                Location = loc,
                Size = new Size(w, 34),
                BackColor = Color.FromArgb(28, 28, 50),
                ForeColor = Color.FromArgb(230, 230, 255),
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 11f)
            };
    }
}