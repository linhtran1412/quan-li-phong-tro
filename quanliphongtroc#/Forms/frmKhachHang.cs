using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using quanliphongtro.DAL;

namespace quanliphongtro.Forms
{
    public class frmKhachHang : BaseListForm
    {
        protected override string FormTitle => "👥  Quản Lý Khách Thuê";
        protected override int FormPanelWidth => 400;

        private TextBox txtHoTen, txtCCCD, txtDienThoai, txtPhuong;
        private CheckBox chkTamTru;
        private DateTimePicker dtpNgayTamTru;
        private Label lblNgayTamTru;
        private readonly KhachHangDAL _dal = new KhachHangDAL();

        public frmKhachHang()
        {
            InitBase();
            LoadData();
        }

        protected override DataGridView BuildGrid()
        {
            var g = MakeGrid();
            g.Columns.AddRange(
                Col("MaKhachHang", "Mã", 50),
                Col("HoTen", "Họ Tên"),
                Col("CCCD", "CCCD/CMND", 120),
                Col("DienThoai", "Điện Thoại", 110),
                Col("Phuong", "Phường/Xã", 120),
                Col("TamTru", "Tạm Trú", 80),
                Col("NgayTamTru", "Ngày ĐK", 90)
            );
            return g;
        }

        protected override Panel BuildFormPanel()
        {
            var pnl = new Panel { Dock = DockStyle.Fill, BackColor = CARD, Padding = new Padding(18) };
            int y = 18;

            pnl.Controls.Add(new Label
            {
                Text = "THÔNG TIN KHÁCH THUÊ",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = ACCENT,
                AutoSize = true,
                Location = new Point(18, y)
            });
            y += 34;

            void AddRow(string label, Control ctrl, int h = 28)
            {
                pnl.Controls.Add(Lbl(label, new Point(18, y)));
                y += 20;
                ctrl.Location = new Point(18, y);
                ctrl.Size = new Size(340, h);
                pnl.Controls.Add(ctrl);
                y += h + 12;
            }

            txtHoTen = Input(340); AddRow("Họ và Tên *", txtHoTen);
            txtCCCD = Input(340); AddRow("CCCD / CMND (12 số) *", txtCCCD);
            txtDienThoai = Input(340); AddRow("Điện Thoại", txtDienThoai);
            txtPhuong = Input(340); AddRow("Phường / Xã", txtPhuong);

            chkTamTru = new CheckBox
            {
                Text = "  Đã đăng ký tạm trú",
                AutoSize = true,
                ForeColor = TEXT,
                Font = new Font("Segoe UI", 9.5f),
                Checked = false
            };
            chkTamTru.CheckedChanged += (s, e) =>
            {
                lblNgayTamTru.Visible = chkTamTru.Checked;
                dtpNgayTamTru.Visible = chkTamTru.Checked;
            };
            pnl.Controls.Add(Lbl("Đăng ký tạm trú", new Point(18, y)));
            y += 20;
            chkTamTru.Location = new Point(18, y);
            pnl.Controls.Add(chkTamTru);
            y += 30;

            lblNgayTamTru = Lbl("Ngày đăng ký", new Point(18, y));
            pnl.Controls.Add(lblNgayTamTru);
            y += 20;
            dtpNgayTamTru = new DateTimePicker
            {
                Location = new Point(18, y),
                Size = new Size(200, 28),
                Format = DateTimePickerFormat.Short,
                Value = DateTime.Today,
                CalendarForeColor = TEXT,
                CalendarMonthBackground = CARD,
                Visible = false
            };
            pnl.Controls.Add(dtpNgayTamTru);
            lblNgayTamTru.Visible = false;
            y += 40;

            LblError = new Label
            {
                Text = "",
                ForeColor = RED,
                Location = new Point(18, y),
                Size = new Size(340, 36),
                Font = new Font("Segoe UI", 8.5f)
            };
            pnl.Controls.Add(LblError);
            y += 40;

            BtnSave = Btn("💾  Lưu", GREEN, new Point(18, y), 110);
            BtnCancel = Btn("✖  Hủy", Color.FromArgb(70, 70, 90), new Point(135, y), 90);
            BtnSave.Click += (s, e) => OnSave();
            BtnCancel.Click += (s, e) => OnCancel();
            pnl.Controls.AddRange(new Control[] { BtnSave, BtnCancel });

            return pnl;
        }

        protected override void AddFormButtons(Panel pnl) { } // đã thêm trong BuildFormPanel

        protected override void LoadData()
        {
            var dt = _dal.GetAll(SafeSearch());
            Grid.DataSource = dt;
        }

        protected override void FillForm(DataGridViewRow row)
        {
            SelectedId = Val<int>(row, "MaKhachHang");
            txtHoTen.Text = Val<string>(row, "HoTen");
            txtCCCD.Text = Val<string>(row, "CCCD");
            txtDienThoai.Text = Val<string>(row, "DienThoai");
            txtPhuong.Text = Val<string>(row, "Phuong");
            bool tt = Val<string>(row, "TamTru") == "Có";
            chkTamTru.Checked = tt;
            string ngayTT = Val<string>(row, "NgayTamTru") ?? "";
            if (tt && DateTime.TryParse(ngayTT, out DateTime d))
                dtpNgayTamTru.Value = d;
        }

        protected override void ClearForm()
        {
            base.ClearForm();
            txtHoTen.Text = txtCCCD.Text = txtDienThoai.Text = txtPhuong.Text = "";
            chkTamTru.Checked = false;
            dtpNgayTamTru.Value = DateTime.Today;
        }

        private KhachHang BuildKH()
        {
            if (string.IsNullOrWhiteSpace(txtHoTen.Text)) throw new Exception("Họ tên không được trống!");
            if (txtCCCD.Text.Trim().Length != 12) throw new Exception("CCCD phải đúng 12 số!");

            return new KhachHang
            {
                MaKhachHang = SelectedId,
                HoTen = txtHoTen.Text.Trim(),
                CCCD = txtCCCD.Text.Trim(),
                DienThoai = txtDienThoai.Text.Trim(),
                Phuong = txtPhuong.Text.Trim(),
                DangKyTamTru = chkTamTru.Checked,
                NgayDangKyTamTru = chkTamTru.Checked ? (DateTime?)dtpNgayTamTru.Value : null
            };
        }

        protected override void SaveNew() => _dal.Insert(BuildKH());
        protected override void SaveUpdate() => _dal.Update(BuildKH());
        protected override void DeleteRow() => _dal.Delete(SelectedId);

        protected override void SetFormEnabled(bool v)
        {
            txtHoTen.Enabled = txtCCCD.Enabled = txtDienThoai.Enabled = txtPhuong.Enabled = v;
            chkTamTru.Enabled = dtpNgayTamTru.Enabled = v;
            BtnSave.Enabled = BtnCancel.Enabled = v;
        }
    }
}