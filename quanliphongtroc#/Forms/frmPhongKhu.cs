using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using quanliphongtro.DAL;

namespace quanliphongtro.Forms
{
    // ═══════════════════════════════════════════════════════════
    //  FORM KHU TRỌ
    // ═══════════════════════════════════════════════════════════
    public class frmKhuTro : BaseListForm
    {
        protected override string FormTitle => "🏘️  Quản Lý Khu Trọ";
        protected override int FormPanelWidth => 380;

        private TextBox txtTen, txtDiaChi;
        private CheckBox chkActive;
        private readonly KhuTroDAL _dal = new KhuTroDAL();

        public frmKhuTro() { InitBase(); LoadData(); }

        protected override DataGridView BuildGrid()
        {
            var g = MakeGrid();
            g.Columns.AddRange(
                Col("MaKhuTro", "Mã", 55),
                Col("TenKhuTro", "Tên Khu"),
                Col("DiaChi", "Địa Chỉ"),
                Col("TrangThai", "Trạng Thái", 90)
            );
            g.CellFormatting += (s, e) =>
            {
                if (e.ColumnIndex < 0 || g.Columns[e.ColumnIndex].Name != "TrangThai") return;
                e.CellStyle.ForeColor = (bool)g.Rows[e.RowIndex].Cells["TrangThai"].Value == true ||
                                         e.Value?.ToString() == "True"
                                         ? GREEN : RED;
            };
            return g;
        }

        protected override Panel BuildFormPanel()
        {
            var pnl = new Panel { Dock = DockStyle.Fill, BackColor = CARD };
            int y = 18;

            pnl.Controls.Add(new Label
            {
                Text = "THÔNG TIN KHU TRỌ",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = ACCENT,
                AutoSize = true,
                Location = new Point(18, y)
            });
            y += 34;

            void AddF(string lbl, Control ctrl)
            {
                pnl.Controls.Add(Lbl(lbl, new Point(18, y))); y += 20;
                ctrl.Location = new Point(18, y); ctrl.Size = new Size(320, 28);
                pnl.Controls.Add(ctrl); y += 42;
            }

            txtTen = Input(320); AddF("Tên khu trọ *", txtTen);
            txtDiaChi = Input(320); txtDiaChi.Multiline = true; txtDiaChi.Height = 60;
            pnl.Controls.Add(Lbl("Địa chỉ", new Point(18, y))); y += 20;
            txtDiaChi.Location = new Point(18, y); txtDiaChi.Size = new Size(320, 60);
            pnl.Controls.Add(txtDiaChi); y += 74;

            chkActive = new CheckBox
            {
                Text = "  Đang hoạt động",
                Checked = true,
                AutoSize = true,
                ForeColor = TEXT,
                Location = new Point(18, y),
                Font = new Font("Segoe UI", 9.5f)
            };
            pnl.Controls.Add(chkActive); y += 40;

            LblError = new Label { Text = "", ForeColor = RED, Location = new Point(18, y), Size = new Size(320, 36) };
            pnl.Controls.Add(LblError); y += 40;

            BtnSave = Btn("💾  Lưu", GREEN, new Point(18, y), 110);
            BtnCancel = Btn("✖  Hủy", Color.FromArgb(70, 70, 90), new Point(133, y), 90);
            BtnSave.Click += (s, e) => OnSave(); BtnCancel.Click += (s, e) => OnCancel();
            pnl.Controls.AddRange(new Control[] { BtnSave, BtnCancel });
            return pnl;
        }

        protected override void AddFormButtons(Panel pnl) { }
        protected override void LoadData() => Grid.DataSource = _dal.GetAll();
        protected override void FillForm(DataGridViewRow row)
        {
            SelectedId = Val<int>(row, "MaKhuTro");
            txtTen.Text = Val<string>(row, "TenKhuTro");
            txtDiaChi.Text = Val<string>(row, "DiaChi");
            // TrangThai stored as BIT → boolean
            var v = row.Cells["TrangThai"].Value;
            chkActive.Checked = v != null && (v.ToString() == "True" || v.ToString() == "1");
        }
        protected override void ClearForm()
        {
            base.ClearForm(); txtTen.Text = txtDiaChi.Text = ""; chkActive.Checked = true;
        }
        private KhuTro Build() =>
            string.IsNullOrWhiteSpace(txtTen.Text) ? throw new Exception("Tên khu trọ không được trống!") :
            new KhuTro { MaKhuTro = SelectedId, TenKhuTro = txtTen.Text.Trim(), DiaChi = txtDiaChi.Text.Trim(), TrangThai = chkActive.Checked };

        protected override void SaveNew() => _dal.Insert(Build());
        protected override void SaveUpdate() => _dal.Update(Build());
        protected override void DeleteRow() => _dal.Delete(SelectedId);
        protected override void SetFormEnabled(bool v)
        {
            txtTen.Enabled = txtDiaChi.Enabled = chkActive.Enabled = v;
            BtnSave.Enabled = BtnCancel.Enabled = v;
        }
    }

    // ═══════════════════════════════════════════════════════════
    //  FORM PHÒNG TRỌ
    // ═══════════════════════════════════════════════════════════
    public class frmPhongTro : BaseListForm
    {
        protected override string FormTitle => "🚪  Quản Lý Phòng Trọ";
        protected override int FormPanelWidth => 420;

        private TextBox txtTen, txtGia, txtGhiChu;
        private ComboBox cboKhu, cboTrangThai;
        private readonly PhongDAL _dal = new PhongDAL();
        private readonly KhuTroDAL _khuDal = new KhuTroDAL();
        private Button btnFilter;

        public frmPhongTro() { InitBase(); LoadData(); }

        protected override DataGridView BuildGrid()
        {
            var g = MakeGrid();
            g.Columns.AddRange(
                Col("MaPhong", "Mã", 50),
                Col("TenPhong", "Phòng", 70),
                Col("TenKhuTro", "Khu Trọ"),
                Col("GiaPhong", "Giá Phòng", 110),
                Col("TrangThai", "Trạng Thái", 100),
                Col("GhiChu", "Ghi Chú")
            );
            g.CellFormatting += (s, e) =>
            {
                if (e.ColumnIndex < 0 || g.Columns[e.ColumnIndex].Name != "TrangThai") return;
                switch (e.Value?.ToString())
                {
                    case "Trống": e.CellStyle.ForeColor = GREEN; break;
                    case "Đang thuê": e.CellStyle.ForeColor = YELLOW; break;
                    case "Bảo trì": e.CellStyle.ForeColor = RED; break;
                }
            };
            return g;
        }

        protected override void ExtraToolbarButtons(Panel pnl, ref int x)
        {
            var cboFilter = new ComboBox
            {
                Location = new Point(x, 12),
                Size = new Size(120, 28),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = PANEL,
                ForeColor = TEXT,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9f)
            };
            cboFilter.Items.AddRange(new object[] { "Tất cả", "Trống", "Đang thuê", "Bảo trì" });
            cboFilter.SelectedIndex = 0;
            cboFilter.SelectedIndexChanged += (s, e) => LoadData(cboFilter.Text == "Tất cả" ? "" : cboFilter.Text);
            pnl.Controls.Add(cboFilter);
            x += 130;
        }

        protected override Panel BuildFormPanel()
        {
            var pnl = new Panel { Dock = DockStyle.Fill, BackColor = CARD };
            int y = 18;

            pnl.Controls.Add(new Label
            {
                Text = "THÔNG TIN PHÒNG",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = ACCENT,
                AutoSize = true,
                Location = new Point(18, y)
            });
            y += 34;

            void F(string lbl, Control ctrl, int h = 28)
            {
                pnl.Controls.Add(Lbl(lbl, new Point(18, y))); y += 20;
                ctrl.Location = new Point(18, y); ctrl.Size = new Size(360, h);
                pnl.Controls.Add(ctrl); y += h + 12;
            }

            cboKhu = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, BackColor = PANEL, ForeColor = TEXT, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9.5f) };
            F("Khu Trọ *", cboKhu);

            txtTen = Input(360); F("Tên Phòng *", txtTen);
            txtGia = Input(360); F("Giá Phòng (₫) *", txtGia);

            cboTrangThai = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, BackColor = PANEL, ForeColor = TEXT, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9.5f) };
            cboTrangThai.Items.AddRange(new object[] { "Trống", "Đang thuê", "Bảo trì" });
            cboTrangThai.SelectedIndex = 0;
            F("Trạng Thái", cboTrangThai);

            txtGhiChu = Input(360); txtGhiChu.Multiline = true; txtGhiChu.Height = 52;
            F("Ghi Chú", txtGhiChu, 52);

            LblError = new Label { Text = "", ForeColor = RED, Location = new Point(18, y), Size = new Size(360, 36) };
            pnl.Controls.Add(LblError); y += 40;

            BtnSave = Btn("💾  Lưu", GREEN, new Point(18, y), 110);
            BtnCancel = Btn("✖  Hủy", Color.FromArgb(70, 70, 90), new Point(133, y), 90);
            BtnSave.Click += (s, e) => OnSave(); BtnCancel.Click += (s, e) => OnCancel();
            pnl.Controls.AddRange(new Control[] { BtnSave, BtnCancel });

            // Load khu trọ
            LoadKhu();
            return pnl;
        }

        private void LoadKhu()
        {
            cboKhu.Items.Clear();
            foreach (DataRow r in _khuDal.GetAll(true).Rows)
                cboKhu.Items.Add(new ComboItem { Value = (int)r["MaKhuTro"], Text = r["TenKhuTro"].ToString() });
            if (cboKhu.Items.Count > 0) cboKhu.SelectedIndex = 0;
        }

        protected override void AddFormButtons(Panel pnl) { }
        protected override void LoadData() => LoadData("");
        private void LoadData(string tt) => Grid.DataSource = _dal.GetAll(SafeSearch(), 0, tt);
        protected override void FillForm(DataGridViewRow row)
        {
            SelectedId = Val<int>(row, "MaPhong");
            txtTen.Text = Val<string>(row, "TenPhong");
            txtGia.Text = Val<decimal>(row, "GiaPhong").ToString("N0");
            txtGhiChu.Text = Val<string>(row, "GhiChu");
            cboTrangThai.Text = Val<string>(row, "TrangThai");
            int khu = Val<int>(row, "MaKhuTro");
            foreach (ComboItem item in cboKhu.Items)
                if (item.Value == khu) { cboKhu.SelectedItem = item; break; }
        }
        protected override void ClearForm()
        {
            base.ClearForm();
            txtTen.Text = txtGia.Text = txtGhiChu.Text = "";
            cboTrangThai.SelectedIndex = 0;
            if (cboKhu.Items.Count > 0) cboKhu.SelectedIndex = 0;
        }
        private Phong Build()
        {
            if (string.IsNullOrWhiteSpace(txtTen.Text)) throw new Exception("Tên phòng không được trống!");
            if (!decimal.TryParse(txtGia.Text.Replace(",", "").Replace(".", ""), out decimal gia) || gia < 0)
                throw new Exception("Giá phòng không hợp lệ!");
            var khu = (ComboItem)cboKhu.SelectedItem;
            if (khu == null) throw new Exception("Chọn khu trọ!");
            return new Phong
            {
                MaPhong = SelectedId,
                MaKhuTro = khu.Value,
                TenPhong = txtTen.Text.Trim(),
                GiaPhong = gia,
                TrangThai = cboTrangThai.Text,
                GhiChu = txtGhiChu.Text.Trim()
            };
        }
        protected override void SaveNew() => _dal.Insert(Build());
        protected override void SaveUpdate() => _dal.Update(Build());
        protected override void DeleteRow() => _dal.Delete(SelectedId);
        protected override void SetFormEnabled(bool v)
        {
            txtTen.Enabled = txtGia.Enabled = txtGhiChu.Enabled = v;
            cboKhu.Enabled = cboTrangThai.Enabled = v;
            BtnSave.Enabled = BtnCancel.Enabled = v;
        }
    }

    // ═══════════════════════════════════════════════════════════
    //  FORM THIẾT BỊ
    // ═══════════════════════════════════════════════════════════
    public class frmThietBi : BaseListForm
    {
        protected override string FormTitle => "🔧  Quản Lý Thiết Bị";
        protected override int FormPanelWidth => 360;

        private TextBox txtTen, txtTrangThai;
        private readonly ThietBiDAL _dal = new ThietBiDAL();

        public frmThietBi() { InitBase(); LoadData(); }

        protected override DataGridView BuildGrid()
        {
            var g = MakeGrid();
            g.Columns.AddRange(
                Col("MaThietBi", "Mã", 60),
                Col("TenThietBi", "Tên Thiết Bị"),
                Col("TrangThai", "Tình Trạng", 110)
            );
            return g;
        }

        protected override Panel BuildFormPanel()
        {
            var pnl = new Panel { Dock = DockStyle.Fill, BackColor = CARD };
            int y = 18;
            pnl.Controls.Add(new Label { Text = "THÔNG TIN THIẾT BỊ", Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = ACCENT, AutoSize = true, Location = new Point(18, y) });
            y += 34;
            void F(string lbl, Control ctrl) { pnl.Controls.Add(Lbl(lbl, new Point(18, y))); y += 20; ctrl.Location = new Point(18, y); ctrl.Size = new Size(300, 28); pnl.Controls.Add(ctrl); y += 42; }
            txtTen = Input(300); F("Tên thiết bị *", txtTen);
            txtTrangThai = Input(300); txtTrangThai.Text = "Tốt"; F("Tình trạng", txtTrangThai);
            LblError = new Label { Text = "", ForeColor = RED, Location = new Point(18, y), Size = new Size(300, 36) }; pnl.Controls.Add(LblError); y += 40;
            BtnSave = Btn("💾  Lưu", GREEN, new Point(18, y), 110); BtnCancel = Btn("✖  Hủy", Color.FromArgb(70, 70, 90), new Point(133, y), 90);
            BtnSave.Click += (s, e) => OnSave(); BtnCancel.Click += (s, e) => OnCancel();
            pnl.Controls.AddRange(new Control[] { BtnSave, BtnCancel });
            return pnl;
        }

        protected override void AddFormButtons(Panel pnl) { }
        protected override void LoadData() => Grid.DataSource = _dal.GetAll();
        protected override void FillForm(DataGridViewRow row) { SelectedId = Val<int>(row, "MaThietBi"); txtTen.Text = Val<string>(row, "TenThietBi"); txtTrangThai.Text = Val<string>(row, "TrangThai"); }
        protected override void ClearForm() { base.ClearForm(); txtTen.Text = ""; txtTrangThai.Text = "Tốt"; }
        protected override void SaveNew() => _dal.Insert(txtTen.Text.Trim(), txtTrangThai.Text.Trim());
        protected override void SaveUpdate() => _dal.Update(SelectedId, txtTen.Text.Trim(), txtTrangThai.Text.Trim());
        protected override void DeleteRow() => throw new Exception("Không thể xóa thiết bị đã gán vào phòng!");
        protected override void SetFormEnabled(bool v) { txtTen.Enabled = txtTrangThai.Enabled = v; BtnSave.Enabled = BtnCancel.Enabled = v; }
    }

    // ─── Helper: ComboBox item ─────────────────────────────────────────────
    public class ComboItem
    {
        public int Value { get; set; }
        public string Text { get; set; }
        public override string ToString() => Text;
    }
}