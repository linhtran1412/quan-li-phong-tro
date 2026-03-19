using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using quanliphongtro.DAL;

namespace quanliphongtro.Forms
{
    public class frmHopDong : BaseListForm
    {
        protected override string FormTitle => "📋  Quản Lý Hợp Đồng";
        protected override int FormPanelWidth => 430;

        private ComboBox cboPhong, cboKhach;
        private DateTimePicker dtpBatDau, dtpKetThuc;
        private TextBox txtCoc, txtDieuKhoan;
        private ComboBox cboTrangThai;
        private Label lblNgayConLai;
        private Button btnThanhLy, btnGiaHan;

        private readonly HopDongDAL _dal = new HopDongDAL();
        private readonly PhongDAL _phongDal = new PhongDAL();
        private readonly KhachHangDAL _khachDal = new KhachHangDAL();

        public frmHopDong() { InitBase(); LoadData(); }

        protected override DataGridView BuildGrid()
        {
            var g = MakeGrid();
            g.Columns.AddRange(
                Col("MaHopDong", "Mã", 50),
                Col("TenPhong", "Phòng", 70),
                Col("TenKhuTro", "Khu Trọ", 100),
                Col("HoTen", "Khách Thuê"),
                Col("NgayBatDau", "Bắt Đầu", 90),
                Col("NgayKetThuc", "Kết Thúc", 90),
                Col("TienCoc", "Tiền Cọc", 100),
                Col("NgayConLai", "Còn (ngày)", 85),
                Col("TrangThai", "Trạng Thái", 110)
            );
            g.CellFormatting += (s, e) =>
            {
                if (e.ColumnIndex < 0) return;
                string col = g.Columns[e.ColumnIndex].Name;
                if (col == "TrangThai")
                    switch (e.Value?.ToString())
                    {
                        case "Còn hiệu lực": e.CellStyle.ForeColor = GREEN; break;
                        case "Đã Hủy": e.CellStyle.ForeColor = RED; break;
                        case "Hết hạn": e.CellStyle.ForeColor = YELLOW; break;
                    }
                if (col == "NgayConLai" && int.TryParse(e.Value?.ToString(), out int n))
                    e.CellStyle.ForeColor = n <= 30 ? RED : n <= 60 ? YELLOW : GREEN;
            };
            return g;
        }

        protected override void ExtraToolbarButtons(Panel pnl, ref int x)
        {
            btnThanhLy = Btn("🔴  Thanh Lý", RED, new Point(x, 11), 115); x += 120;
            btnGiaHan = Btn("🔄  Gia Hạn", Color.FromArgb(14, 122, 100), new Point(x, 11), 110); x += 115;
            btnThanhLy.Click += (s, e) => DoThanhLy();
            btnGiaHan.Click += (s, e) => DoGiaHan();
            pnl.Controls.AddRange(new Control[] { btnThanhLy, btnGiaHan });
        }

        protected override Panel BuildFormPanel()
        {
            var pnl = new Panel { Dock = DockStyle.Fill, BackColor = CARD };
            int y = 18;

            pnl.Controls.Add(new Label { Text = "THÔNG TIN HỢP ĐỒNG", Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = ACCENT, AutoSize = true, Location = new Point(18, y) });
            y += 34;

            void F(string lbl, Control ctrl, int h = 28)
            {
                pnl.Controls.Add(Lbl(lbl, new Point(18, y))); y += 20;
                ctrl.Location = new Point(18, y); ctrl.Size = new Size(380, h);
                pnl.Controls.Add(ctrl); y += h + 12;
            }

            cboPhong = MakeCbo(); F("Phòng (chỉ phòng trống) *", cboPhong);
            cboKhach = MakeCbo(); F("Khách Thuê *", cboKhach);

            dtpBatDau = MakeDtp(); F("Ngày Bắt Đầu", dtpBatDau);
            dtpKetThuc = MakeDtp(); dtpKetThuc.Value = DateTime.Today.AddYears(1); F("Ngày Kết Thúc", dtpKetThuc);
            dtpKetThuc.ValueChanged += (s, e) => UpdateNgayConLai();

            txtCoc = Input(380); F("Tiền Cọc (₫)", txtCoc);

            cboTrangThai = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, BackColor = PANEL, ForeColor = TEXT, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9.5f) };
            cboTrangThai.Items.AddRange(new object[] { "Còn hiệu lực", "Hết hạn", "Đã Hủy" });
            cboTrangThai.SelectedIndex = 0;
            F("Trạng Thái", cboTrangThai);

            txtDieuKhoan = Input(380); txtDieuKhoan.Multiline = true; txtDieuKhoan.Height = 55;
            F("Điều Khoản", txtDieuKhoan, 55);

            lblNgayConLai = new Label { Text = "", ForeColor = GREEN, AutoSize = true, Location = new Point(18, y), Font = new Font("Segoe UI", 9f, FontStyle.Bold) };
            pnl.Controls.Add(lblNgayConLai); y += 26;

            LblError = new Label { Text = "", ForeColor = RED, Location = new Point(18, y), Size = new Size(380, 36) };
            pnl.Controls.Add(LblError); y += 40;

            BtnSave = Btn("💾  Lưu", GREEN, new Point(18, y), 110);
            BtnCancel = Btn("✖  Hủy", Color.FromArgb(70, 70, 90), new Point(133, y), 90);
            BtnSave.Click += (s, e) => OnSave(); BtnCancel.Click += (s, e) => OnCancel();
            pnl.Controls.AddRange(new Control[] { BtnSave, BtnCancel });

            LoadCombos();
            return pnl;
        }

        private ComboBox MakeCbo() => new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, BackColor = PANEL, ForeColor = TEXT, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9.5f) };
        private DateTimePicker MakeDtp() => new DateTimePicker { Format = DateTimePickerFormat.Short, Value = DateTime.Today };

        private void LoadCombos()
        {
            cboPhong.Items.Clear();
            foreach (DataRow r in _phongDal.GetPhongTrong().Rows)
                cboPhong.Items.Add(new ComboItem { Value = (int)r["MaPhong"], Text = r["TenPhong"].ToString() });
            if (cboPhong.Items.Count > 0) cboPhong.SelectedIndex = 0;

            cboKhach.Items.Clear();
            foreach (DataRow r in _khachDal.GetAll().Rows)
                cboKhach.Items.Add(new ComboItem { Value = (int)r["MaKhachHang"], Text = r["HoTen"].ToString() });
            if (cboKhach.Items.Count > 0) cboKhach.SelectedIndex = 0;
        }

        private void UpdateNgayConLai()
        {
            int n = (int)(dtpKetThuc.Value - DateTime.Today).TotalDays;
            lblNgayConLai.Text = n >= 0 ? $"📅  Còn {n} ngày" : "⚠  Đã hết hạn";
            lblNgayConLai.ForeColor = n <= 30 ? RED : n <= 60 ? YELLOW : GREEN;
        }

        private void DoThanhLy()
        {
            if (SelectedId < 0) { Msg("Chọn hợp đồng cần thanh lý!"); return; }
            if (MessageBox.Show("Thanh lý hợp đồng? Phòng sẽ chuyển về 'Trống'.", "Xác nhận",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                int maPhong = 0;
                if (Grid.SelectedRows.Count > 0) maPhong = Val<int>(Grid.SelectedRows[0], "MaPhong");
                _dal.ThanhLy(SelectedId, maPhong);
                LoadData(); ClearForm();
                Msg("Đã thanh lý hợp đồng!", false, true);
            }
        }

        private void DoGiaHan()
        {
            if (SelectedId < 0) { Msg("Chọn hợp đồng cần gia hạn!"); return; }
            using (var dtp = new DateTimePicker { Value = DateTime.Today.AddYears(1) })
            {
                var form = new Form { Size = new Size(350, 160), Text = "Gia hạn hợp đồng", BackColor = CARD, StartPosition = FormStartPosition.CenterParent };
                dtp.Location = new Point(20, 20); dtp.Size = new Size(290, 30); dtp.Format = DateTimePickerFormat.Short;
                var btn = new Button { Text = "Xác nhận", Location = new Point(20, 65), Size = new Size(120, 32), BackColor = GREEN, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
                btn.Click += (s, e) => { _dal.GiaHan(SelectedId, dtp.Value); LoadData(); form.Close(); Msg("Đã gia hạn!", false, true); };
                form.Controls.AddRange(new Control[] {
                    new Label { Text = "Ngày kết thúc mới:", Location = new Point(20, 0), AutoSize = true, ForeColor = TEXT },
                    dtp, btn
                });
                form.ShowDialog();
            }
        }

        protected override void AddFormButtons(Panel pnl) { }
        protected override void LoadData() => Grid.DataSource = _dal.GetAll(SafeSearch());
        protected override void FillForm(DataGridViewRow row)
        {
            SelectedId = Val<int>(row, "MaHopDong");
            cboTrangThai.Text = Val<string>(row, "TrangThai");
            if (DateTime.TryParse(row.Cells["NgayBatDau"].Value?.ToString(), out DateTime bd)) dtpBatDau.Value = bd;
            if (DateTime.TryParse(row.Cells["NgayKetThuc"].Value?.ToString(), out DateTime kt)) dtpKetThuc.Value = kt;
            txtCoc.Text = Val<decimal>(row, "TienCoc").ToString("N0");
            txtDieuKhoan.Text = Val<string>(row, "DieuKhoan");
            UpdateNgayConLai();
        }
        protected override void ClearForm()
        {
            base.ClearForm(); txtCoc.Text = txtDieuKhoan.Text = "";
            dtpBatDau.Value = DateTime.Today; dtpKetThuc.Value = DateTime.Today.AddYears(1);
            cboTrangThai.SelectedIndex = 0; lblNgayConLai.Text = "";
        }
        private HopDong Build()
        {
            if (!decimal.TryParse(txtCoc.Text.Replace(",", ""), out decimal coc)) coc = 0;
            var phong = (ComboItem)cboPhong.SelectedItem;
            var khach = (ComboItem)cboKhach.SelectedItem;
            if (phong == null || khach == null) throw new Exception("Chọn phòng và khách thuê!");
            return new HopDong
            {
                MaHopDong = SelectedId,
                MaPhong = phong.Value,
                MaKhachHang = khach.Value,
                NgayBatDau = dtpBatDau.Value,
                NgayKetThuc = dtpKetThuc.Value,
                TienCoc = coc,
                TrangThai = cboTrangThai.Text,
                DieuKhoan = txtDieuKhoan.Text
            };
        }
        protected override void SaveNew() => _dal.Insert(Build());
        protected override void SaveUpdate() => _dal.Update(Build());
        protected override void DeleteRow() => throw new Exception("Không xóa HĐ, hãy dùng Thanh Lý!");
        protected override void SetFormEnabled(bool v)
        {
            cboPhong.Enabled = cboKhach.Enabled = v;
            dtpBatDau.Enabled = dtpKetThuc.Enabled = v;
            txtCoc.Enabled = txtDieuKhoan.Enabled = cboTrangThai.Enabled = v;
            BtnSave.Enabled = BtnCancel.Enabled = v;
        }
    }
}