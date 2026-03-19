using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using quanliphongtro.DAL;

namespace quanliphongtro.Forms
{
    // ═══════════════════════════════════════════════════════════
    //  FORM CHỈ SỐ ĐIỆN NƯỚC
    // ═══════════════════════════════════════════════════════════
    public class frmChiSoDienNuoc : Form
    {
        private readonly Color BG = Color.FromArgb(15, 15, 30);
        private readonly Color CARD = Color.FromArgb(24, 24, 46);
        private readonly Color ACC = Color.FromArgb(99, 102, 241);
        private readonly Color TEXT = Color.FromArgb(230, 230, 255);
        private readonly Color SEC = Color.FromArgb(160, 160, 200);
        private readonly Color GRN = Color.FromArgb(16, 185, 129);
        private readonly Color RED = Color.FromArgb(220, 38, 38);
        private readonly Color YEL = Color.FromArgb(234, 179, 8);

        private ComboBox cboPhong;
        private NumericUpDown numThang, numNam;
        private TextBox txtDienCu, txtDienMoi, txtNuocCu, txtNuocMoi;
        private Label lblDuTinh, lblNgayNhap, lblError;
        private DataGridView grid;
        private readonly ChiSoDienNuocDAL _dal = new ChiSoDienNuocDAL();
        private readonly PhongDAL _phongDal = new PhongDAL();
        private readonly TaiKhoan _user;
        private int _maChiSo = -1;

        public frmChiSoDienNuoc(TaiKhoan user = null)
        {
            _user = user;
            this.BackColor = BG;
            this.Font = new Font("Segoe UI", 9.5f);
            Build();
        }

        private void Build()
        {
            // Header
            var pnlH = new Panel { Dock = DockStyle.Top, Height = 52, BackColor = Color.FromArgb(20, 20, 42) };
            pnlH.Controls.Add(new Label { Text = "⚡  Chỉ Số Điện - Nước", Font = new Font("Segoe UI", 14, FontStyle.Bold), ForeColor = TEXT, AutoSize = true, Location = new Point(18, 12) });

            // Toolbar
            var pnlT = new Panel { Dock = DockStyle.Top, Height = 52, BackColor = CARD };
            var lblP = new Label { Text = "Phòng:", ForeColor = SEC, AutoSize = true, Location = new Point(14, 17) };
            cboPhong = new ComboBox { Location = new Point(60, 14), Size = new Size(180, 28), DropDownStyle = ComboBoxStyle.DropDownList, BackColor = Color.FromArgb(20, 20, 40), ForeColor = TEXT, FlatStyle = FlatStyle.Flat };
            cboPhong.SelectedIndexChanged += (s, e) => LoadGrid();

            var lblTh = new Label { Text = "Tháng:", ForeColor = SEC, AutoSize = true, Location = new Point(255, 17) };
            numThang = new NumericUpDown { Location = new Point(300, 14), Size = new Size(55, 28), Minimum = 1, Maximum = 12, Value = DateTime.Now.Month, BackColor = Color.FromArgb(20, 20, 40), ForeColor = TEXT };
            var lblNam = new Label { Text = "Năm:", ForeColor = SEC, AutoSize = true, Location = new Point(365, 17) };
            numNam = new NumericUpDown { Location = new Point(400, 14), Size = new Size(75, 28), Minimum = 2020, Maximum = 2099, Value = DateTime.Now.Year, BackColor = Color.FromArgb(20, 20, 40), ForeColor = TEXT };
            var btnLoad = MkBtn("🔍  Lọc", ACC, new Point(485, 13), 90);
            btnLoad.Click += (s, e) => LoadGrid();
            pnlT.Controls.AddRange(new Control[] { lblP, cboPhong, lblTh, numThang, lblNam, numNam, btnLoad });

            // Split
          
            var split = new SplitContainer { Size = new Size(1000, 600), Dock = DockStyle.Fill, SplitterDistance = 600, IsSplitterFixed = true, BackColor = BG, Panel1MinSize = 400, Panel2MinSize = 320 };

            // Grid
            grid = MkGrid();
            grid.Columns.AddRange(
                GCol("MaChiSo", "Mã", 55), GCol("Thang", "T", 40), GCol("Nam", "Năm", 65),
                GCol("DienCu", "Điện Cũ", 75), GCol("DienMoi", "Điện Mới", 80), GCol("TieuThuDien", "Tiêu Thụ kWh", 100),
                GCol("NuocCu", "Nước Cũ", 75), GCol("NuocMoi", "Nước Mới", 80), GCol("TieuThuNuoc", "Tiêu Thụ m³", 100),
                GCol("NgayNhap", "Ngày Nhập", 90)
            );
            grid.SelectionChanged += Grid_SelectionChanged;
            split.Panel1.Controls.Add(grid);

            // Form nhập
            split.Panel2.Controls.Add(BuildInputPanel());

            this.Controls.AddRange(new Control[] { split, pnlT, pnlH });
            LoadPhong();
        }

        private Panel BuildInputPanel()
        {
            var pnl = new Panel { Dock = DockStyle.Fill, BackColor = CARD };
            int y = 18;

            pnl.Controls.Add(new Label { Text = "NHẬP CHỈ SỐ", Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = ACC, AutoSize = true, Location = new Point(18, y) });
            y += 34;

            lblNgayNhap = new Label { Text = $"Ngày nhập: {DateTime.Now:dd/MM/yyyy}", ForeColor = SEC, AutoSize = true, Location = new Point(18, y) };
            pnl.Controls.Add(lblNgayNhap); y += 26;

            // Điện
            pnl.Controls.Add(HLine(new Point(18, y), 300, "⚡  Điện (kWh)", YEL)); y += 30;
            var r1 = Row(pnl, "Chỉ số cũ:", "Chỉ số mới:", y, out txtDienCu, out txtDienMoi); y += r1 + 10;

            // Nước
            pnl.Controls.Add(HLine(new Point(18, y), 300, "💧  Nước (m³)", Color.FromArgb(59, 130, 246))); y += 30;
            var r2 = Row(pnl, "Chỉ số cũ:", "Chỉ số mới:", y, out txtNuocCu, out txtNuocMoi); y += r2 + 14;

            lblDuTinh = new Label { Text = "", ForeColor = GRN, Location = new Point(18, y), Size = new Size(310, 50), Font = new Font("Segoe UI", 9.5f, FontStyle.Bold) };
            pnl.Controls.Add(lblDuTinh); y += 60;

            txtDienMoi.TextChanged += (s, e) => CalcPreview();
            txtNuocMoi.TextChanged += (s, e) => CalcPreview();

            lblError = new Label { Text = "", ForeColor = RED, Location = new Point(18, y), Size = new Size(310, 36) };
            pnl.Controls.Add(lblError); y += 40;

            var btnLuu = MkBtn("💾  Lưu", GRN, new Point(18, y), 110);
            var btnXoa = MkBtn("🗑️  Xóa dòng", RED, new Point(133, y), 115);
            var btnMoi = MkBtn("✨  Mới", ACC, new Point(253, y), 75);
            btnLuu.Click += (s, e) => DoSave();
            btnXoa.Click += (s, e) => DoDelete();
            btnMoi.Click += (s, e) => ClearForm();
            pnl.Controls.AddRange(new Control[] { btnLuu, btnXoa, btnMoi });

            return pnl;
        }

        private Label HLine(Point loc, int w, string title, Color c)
        {
            var lbl = new Label { Text = title, Location = loc, Size = new Size(w, 22), ForeColor = c, Font = new Font("Segoe UI", 9f, FontStyle.Bold) };
            return lbl;
        }

        private int Row(Panel pnl, string l1, string l2, int y, out TextBox t1, out TextBox t2)
        {
            pnl.Controls.Add(new Label { Text = l1, ForeColor = SEC, AutoSize = true, Location = new Point(18, y) });
            t1 = MkInput(new Point(18, y + 18));
            pnl.Controls.Add(t1);
            pnl.Controls.Add(new Label { Text = l2, ForeColor = SEC, AutoSize = true, Location = new Point(165, y) });
            t2 = MkInput(new Point(165, y + 18));
            pnl.Controls.Add(t2);
            return 50;
        }

        private TextBox MkInput(Point loc) => new TextBox { Location = loc, Size = new Size(130, 28), BackColor = Color.FromArgb(20, 20, 40), ForeColor = TEXT, BorderStyle = BorderStyle.FixedSingle, Font = new Font("Segoe UI", 9.5f) };
        private Button MkBtn(string t, Color c, Point loc, int w) { var b = new Button { Text = t, Location = loc, Size = new Size(w, 30), BackColor = c, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 8.5f, FontStyle.Bold), Cursor = Cursors.Hand }; b.FlatAppearance.BorderSize = 0; return b; }
        private DataGridView MkGrid() { var g = new DataGridView { Dock = DockStyle.Fill, BackgroundColor = BG, BorderStyle = BorderStyle.None, RowHeadersVisible = false, AllowUserToAddRows = false, ReadOnly = true, SelectionMode = DataGridViewSelectionMode.FullRowSelect, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, EnableHeadersVisualStyles = false, ColumnHeadersHeight = 34, RowTemplate = { Height = 30 }, Font = new Font("Segoe UI", 9f) }; g.DefaultCellStyle.BackColor = BG; g.DefaultCellStyle.ForeColor = TEXT; g.DefaultCellStyle.SelectionBackColor = ACC; g.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(30, 30, 55); g.ColumnHeadersDefaultCellStyle.ForeColor = SEC; g.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(20, 20, 40); return g; }
        private DataGridViewTextBoxColumn GCol(string n, string h, int w) { var c = new DataGridViewTextBoxColumn { Name = n, HeaderText = h, DataPropertyName = n }; c.AutoSizeMode = DataGridViewAutoSizeColumnMode.None; c.Width = w; return c; }

        private void LoadPhong()
        {
            cboPhong.Items.Clear();
            // Nếu khách thuê, chỉ load phòng của họ
            var dt = new PhongDAL().GetAll();
            foreach (DataRow r in dt.Rows)
                cboPhong.Items.Add(new ComboItem { Value = (int)r["MaPhong"], Text = $"{r["TenPhong"]} - {r["TenKhuTro"]}" });
            if (cboPhong.Items.Count > 0) cboPhong.SelectedIndex = 0;
        }

        private void LoadGrid()
        {
            var sel = (ComboItem)cboPhong.SelectedItem;
            if (sel == null) return;
            grid.DataSource = _dal.GetByPhong(sel.Value);
        }

        private void Grid_SelectionChanged(object sender, EventArgs e)
        {
            if (grid.SelectedRows.Count == 0) return;
            var row = grid.SelectedRows[0];
            _maChiSo = Convert.ToInt32(row.Cells["MaChiSo"].Value);
            txtDienCu.Text = row.Cells["DienCu"].Value?.ToString();
            txtDienMoi.Text = row.Cells["DienMoi"].Value?.ToString();
            txtNuocCu.Text = row.Cells["NuocCu"].Value?.ToString();
            txtNuocMoi.Text = row.Cells["NuocMoi"].Value?.ToString();
        }

        private void CalcPreview()
        {
            if (int.TryParse(txtDienCu.Text, out int dc) && int.TryParse(txtDienMoi.Text, out int dm) &&
                int.TryParse(txtNuocCu.Text, out int nc) && int.TryParse(txtNuocMoi.Text, out int nm))
            {
                decimal tDien = (dm - dc) * 3500m;
                decimal tNuoc = (nm - nc) * 15000m;
                lblDuTinh.Text = $"Dự tính: Điện {tDien:N0}₫ | Nước {tNuoc:N0}₫\nTổng ≈ {(tDien + tNuoc):N0} ₫";
            }
        }

        private void DoSave()
        {
            lblError.Text = "";
            try
            {
                var sel = (ComboItem)cboPhong.SelectedItem;
                if (sel == null) throw new Exception("Chọn phòng!");
                if (!int.TryParse(txtDienCu.Text, out int dc)) throw new Exception("Chỉ số điện cũ không hợp lệ!");
                if (!int.TryParse(txtDienMoi.Text, out int dm)) throw new Exception("Chỉ số điện mới không hợp lệ!");
                if (!int.TryParse(txtNuocCu.Text, out int nc)) throw new Exception("Chỉ số nước cũ không hợp lệ!");
                if (!int.TryParse(txtNuocMoi.Text, out int nm)) throw new Exception("Chỉ số nước mới không hợp lệ!");
                if (dm < dc) throw new Exception("Chỉ số điện mới < điện cũ!");
                if (nm < nc) throw new Exception("Chỉ số nước mới < nước cũ!");

                var cs = new ChiSoDienNuoc { MaChiSo = _maChiSo, MaPhong = sel.Value, Thang = (int)numThang.Value, Nam = (int)numNam.Value, DienCu = dc, DienMoi = dm, NuocCu = nc, NuocMoi = nm };

                if (_maChiSo < 0)
                {
                    if (_dal.DaTonTai(sel.Value, cs.Thang, cs.Nam)) throw new Exception("Tháng này đã có chỉ số!");
                    _dal.Insert(cs);
                }
                else _dal.Update(cs);

                LoadGrid(); ClearForm();
                MessageBox.Show("Đã lưu chỉ số!", "OK", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex) { lblError.Text = "⚠  " + ex.Message; }
        }

        private void DoDelete()
        {
            if (_maChiSo < 0) return;
            if (MessageBox.Show("Xóa chỉ số này?", "Xác nhận", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                new System.Data.SqlClient.SqlCommand(); // placeholder
                // _dal.Delete(_maChiSo); // thêm method Delete vào DAL nếu cần
                LoadGrid(); ClearForm();
            }
        }

        private void ClearForm()
        {
            _maChiSo = -1;
            txtDienCu.Text = txtDienMoi.Text = txtNuocCu.Text = txtNuocMoi.Text = "";
            lblDuTinh.Text = lblError.Text = "";
        }
    }

    // ═══════════════════════════════════════════════════════════
    //  FORM HÓA ĐƠN
    // ═══════════════════════════════════════════════════════════
    public class frmHoaDon : Form
    {
        private readonly Color BG = Color.FromArgb(15, 15, 30);
        private readonly Color CARD = Color.FromArgb(24, 24, 46);
        private readonly Color ACC = Color.FromArgb(99, 102, 241);
        private readonly Color TEXT = Color.FromArgb(230, 230, 255);
        private readonly Color SEC = Color.FromArgb(160, 160, 200);
        private readonly Color GRN = Color.FromArgb(16, 185, 129);
        private readonly Color RED = Color.FromArgb(220, 38, 38);
        private readonly Color YEL = Color.FromArgb(234, 179, 8);

        private DataGridView grid;
        private TextBox txtSearch;
        private ComboBox cboFilter, cboHinhThuc, cboCong;
        private NumericUpDown numSoTien;
        private Label lblTongTien, lblDaTT, lblConLai, lblError;
        private readonly HoaDonDAL _dal = new HoaDonDAL();
        private int _selectedHD = -1;

        public frmHoaDon()
        {
            this.BackColor = BG;
            this.Font = new Font("Segoe UI", 9.5f);
            Build();
        }

        private void Build()
        {
            var pnlH = new Panel { Dock = DockStyle.Top, Height = 52, BackColor = Color.FromArgb(20, 20, 42) };
            pnlH.Controls.Add(new Label { Text = "🧾  Quản Lý Hóa Đơn", Font = new Font("Segoe UI", 14, FontStyle.Bold), ForeColor = TEXT, AutoSize = true, Location = new Point(18, 12) });

            var pnlT = new Panel { Dock = DockStyle.Top, Height = 52, BackColor = CARD };
            txtSearch = new TextBox { Location = new Point(14, 14), Size = new Size(220, 28), BackColor = Color.FromArgb(20, 20, 40), ForeColor = SEC, BorderStyle = BorderStyle.FixedSingle, Font = new Font("Segoe UI", 9.5f), Text = "🔍  Tìm kiếm..." };
            txtSearch.GotFocus += (s, e) => { if (txtSearch.Text.StartsWith("🔍")) { txtSearch.Text = ""; txtSearch.ForeColor = TEXT; } };
            txtSearch.LostFocus += (s, e) => { if (txtSearch.Text == "") { txtSearch.Text = "🔍  Tìm kiếm..."; txtSearch.ForeColor = SEC; } };
            txtSearch.TextChanged += (s, e) => LoadData();
            cboFilter = new ComboBox { Location = new Point(244, 14), Size = new Size(160, 28), DropDownStyle = ComboBoxStyle.DropDownList, BackColor = Color.FromArgb(20, 20, 40), ForeColor = TEXT, FlatStyle = FlatStyle.Flat };
            cboFilter.Items.AddRange(new object[] { "Tất cả", "Chưa thanh toán", "Thanh toán một phần", "Đã thanh toán" });
            cboFilter.SelectedIndex = 0; cboFilter.SelectedIndexChanged += (s, e) => LoadData();

            var btnTao = MkBtn("⚡  Tạo HĐ Tháng", ACC, new Point(415, 12), 140);
            btnTao.Click += (s, e) => TaoHoaDon();
            pnlT.Controls.AddRange(new Control[] { txtSearch, cboFilter, btnTao });

            var split = new SplitContainer { Dock = DockStyle.Fill, SplitterDistance = 700, IsSplitterFixed = true, BackColor = BG };

            grid = MkGrid();
            grid.Columns.AddRange(
                GCol("MaHoaDon", "Mã", 55), GCol("TenPhong", "Phòng", 70), GCol("TenKhuTro", "Khu", 100),
                GCol("HoTen", "Khách Thuê"), GCol("Thang", "T", 40), GCol("Nam", "Năm", 65),
                GCol("TongTien", "Tổng Tiền", 110), GCol("TrangThai", "Trạng Thái", 120), GCol("NgayTao", "Ngày Tạo", 90)
            );
            grid.CellFormatting += (s, e) =>
            {
                if (e.ColumnIndex < 0 || grid.Columns[e.ColumnIndex].Name != "TrangThai") return;
                switch (e.Value?.ToString())
                {
                    case "Đã thanh toán": e.CellStyle.ForeColor = GRN; break;
                    case "Chưa thanh toán": e.CellStyle.ForeColor = RED; break;
                    case "Thanh toán một phần": e.CellStyle.ForeColor = YEL; break;
                }
            };
            grid.SelectionChanged += (s, e) => OnSelect();
            split.Panel1.Controls.Add(grid);
            split.Panel2.Controls.Add(BuildPayPanel());

            this.Controls.AddRange(new Control[] { split, pnlT, pnlH });
            LoadData();
        }

        private Panel BuildPayPanel()
        {
            var pnl = new Panel { Dock = DockStyle.Fill, BackColor = CARD };
            int y = 18;
            pnl.Controls.Add(new Label { Text = "THANH TOÁN HÓA ĐƠN", Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = ACC, AutoSize = true, Location = new Point(18, y) }); y += 34;

            lblTongTien = MkLbl("Tổng tiền: ---", new Point(18, y), TEXT, bold: true); pnl.Controls.Add(lblTongTien); y += 24;
            lblDaTT = MkLbl("Đã thanh toán: ---", new Point(18, y), GRN, bold: true); pnl.Controls.Add(lblDaTT); y += 24;
            lblConLai = MkLbl("Còn lại: ---", new Point(18, y), YEL, bold: true); pnl.Controls.Add(lblConLai); y += 34;

            pnl.Controls.Add(new Label { Text = "Số tiền thanh toán:", ForeColor = SEC, AutoSize = true, Location = new Point(18, y) }); y += 20;
            numSoTien = new NumericUpDown { Location = new Point(18, y), Size = new Size(250, 28), Minimum = 0, Maximum = 999999999, Increment = 100000, BackColor = Color.FromArgb(20, 20, 40), ForeColor = TEXT, Font = new Font("Segoe UI", 9.5f) }; pnl.Controls.Add(numSoTien); y += 40;

            pnl.Controls.Add(new Label { Text = "Hình thức:", ForeColor = SEC, AutoSize = true, Location = new Point(18, y) }); y += 20;
            cboHinhThuc = new ComboBox { Location = new Point(18, y), Size = new Size(250, 28), DropDownStyle = ComboBoxStyle.DropDownList, BackColor = Color.FromArgb(20, 20, 40), ForeColor = TEXT, FlatStyle = FlatStyle.Flat };
            cboHinhThuc.Items.AddRange(new object[] { "Tiền mặt", "Online" });
            cboHinhThuc.SelectedIndex = 0;
            cboHinhThuc.SelectedIndexChanged += (s, e) => cboCong.Enabled = cboHinhThuc.Text == "Online";
            pnl.Controls.Add(cboHinhThuc); y += 40;

            pnl.Controls.Add(new Label { Text = "Cổng thanh toán:", ForeColor = SEC, AutoSize = true, Location = new Point(18, y) }); y += 20;
            cboCong = new ComboBox { Location = new Point(18, y), Size = new Size(250, 28), DropDownStyle = ComboBoxStyle.DropDownList, BackColor = Color.FromArgb(20, 20, 40), ForeColor = TEXT, FlatStyle = FlatStyle.Flat, Enabled = false };
            cboCong.Items.AddRange(new object[] { "Momo", "VNPay", "ZaloPay", "Chuyển khoản ngân hàng" });
            cboCong.SelectedIndex = 0;
            pnl.Controls.Add(cboCong); y += 42;

            lblError = new Label { Text = "", ForeColor = RED, Location = new Point(18, y), Size = new Size(290, 36) }; pnl.Controls.Add(lblError); y += 40;

            var btnTT = MkBtn("💳  Ghi Thanh Toán", GRN, new Point(18, y), 150);
            btnTT.Click += (s, e) => DoThanhToan();
            pnl.Controls.Add(btnTT);
            return pnl;
        }

        private void OnSelect()
        {
            if (grid.SelectedRows.Count == 0) return;
            var row = grid.SelectedRows[0];
            _selectedHD = Convert.ToInt32(row.Cells["MaHoaDon"].Value);
            decimal tong = Convert.ToDecimal(row.Cells["TongTien"].Value);
            var dtTT = _dal.GetThanhToan(_selectedHD);
            decimal daTT = 0;
            foreach (DataRow r in dtTT.Rows)
                if (r["TrangThai"].ToString() == "Thành công") daTT += Convert.ToDecimal(r["SoTien"]);
            decimal conLai = tong - daTT;
            lblTongTien.Text = $"Tổng tiền: {tong:N0} ₫";
            lblDaTT.Text = $"Đã thanh toán: {daTT:N0} ₫";
            lblConLai.Text = $"Còn lại: {conLai:N0} ₫";
            numSoTien.Value = Math.Max(0, Math.Min((decimal)numSoTien.Maximum, conLai));
        }

        private void TaoHoaDon()
        {
            if (grid.SelectedRows.Count == 0) { MessageBox.Show("Chọn hóa đơn hoặc hợp đồng!"); return; }
            int maHD = Convert.ToInt32(grid.SelectedRows[0].Cells["MaHopDong"].Value);
            int th = DateTime.Now.Month, nam = DateTime.Now.Year;
            try { _dal.TaoHoaDon(maHD, th, nam); LoadData(); MessageBox.Show("Đã tạo hóa đơn!", "OK"); }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Lỗi"); }
        }

        private void DoThanhToan()
        {
            lblError.Text = "";
            if (_selectedHD < 0) { lblError.Text = "⚠  Chọn hóa đơn!"; return; }
            if (numSoTien.Value <= 0) { lblError.Text = "⚠  Nhập số tiền!"; return; }
            try
            {
                string cong = cboHinhThuc.Text == "Online" ? cboCong.Text : "";
                _dal.GhiThanhToan(_selectedHD, (decimal)numSoTien.Value, cboHinhThuc.Text, cong);
                LoadData(); OnSelect();
                MessageBox.Show("Đã ghi thanh toán!", "OK", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex) { lblError.Text = "⚠  " + ex.Message; }
        }

        private void LoadData()
        {
            string s = txtSearch?.Text.StartsWith("🔍") == true ? "" : txtSearch?.Text.Trim() ?? "";
            string tt = cboFilter?.SelectedIndex == 0 ? "" : cboFilter.Text;
            grid.DataSource = _dal.GetAll(s, tt);
        }

        private Button MkBtn(string t, Color c, Point loc, int w) { var b = new Button { Text = t, Location = loc, Size = new Size(w, 30), BackColor = c, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 8.5f, FontStyle.Bold), Cursor = Cursors.Hand }; b.FlatAppearance.BorderSize = 0; return b; }
        private Label MkLbl(string t, Point loc, Color c, bool bold = false) => new Label { Text = t, Location = loc, AutoSize = true, ForeColor = c, Font = new Font("Segoe UI", 9.5f, bold ? FontStyle.Bold : FontStyle.Regular) };
        private DataGridView MkGrid() { var g = new DataGridView { Dock = DockStyle.Fill, BackgroundColor = BG, BorderStyle = BorderStyle.None, RowHeadersVisible = false, AllowUserToAddRows = false, ReadOnly = true, SelectionMode = DataGridViewSelectionMode.FullRowSelect, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, EnableHeadersVisualStyles = false, ColumnHeadersHeight = 34, RowTemplate = { Height = 30 }, Font = new Font("Segoe UI", 9f) }; g.DefaultCellStyle.BackColor = BG; g.DefaultCellStyle.ForeColor = TEXT; g.DefaultCellStyle.SelectionBackColor = ACC; g.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(30, 30, 55); g.ColumnHeadersDefaultCellStyle.ForeColor = SEC; g.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(20, 20, 40); return g; }
        private DataGridViewTextBoxColumn GCol(string n, string h, int w = -1) { var c = new DataGridViewTextBoxColumn { Name = n, HeaderText = h, DataPropertyName = n }; if (w > 0) { c.AutoSizeMode = DataGridViewAutoSizeColumnMode.None; c.Width = w; } return c; }
    }

    // ═══════════════════════════════════════════════════════════
    //  FORM DỊCH VỤ
    // ═══════════════════════════════════════════════════════════
    public class frmDichVu : BaseListForm
    {
        protected override string FormTitle => "🛎️  Quản Lý Dịch Vụ & Bảng Giá";
        protected override int FormPanelWidth => 380;
        private TextBox txtTen; private CheckBox chkActive;
        private NumericUpDown numGia; private DateTimePicker dtpNgay;
        private readonly DichVuDAL _dal = new DichVuDAL();
        public frmDichVu() { InitBase(); LoadData(); }
        protected override DataGridView BuildGrid() { var g = MakeGrid(); g.Columns.AddRange(Col("MaDichVu", "Mã", 55), Col("TenDichVu", "Tên Dịch Vụ"), Col("TrangThai", "Trạng Thái", 100), Col("DonGia", "Đơn Giá", 100), Col("NgayApDung", "Ngày Áp Dụng", 100)); return g; }
        protected override Panel BuildFormPanel()
        {
            var pnl = new Panel { Dock = DockStyle.Fill, BackColor = CARD };
            int y = 18;
            pnl.Controls.Add(new Label { Text = "THÔNG TIN DỊCH VỤ", Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = ACCENT, AutoSize = true, Location = new Point(18, y) }); y += 34;
            void F(string l, Control c, int h = 28) { pnl.Controls.Add(Lbl(l, new Point(18, y))); y += 20; c.Location = new Point(18, y); c.Size = new Size(320, h); pnl.Controls.Add(c); y += h + 12; }
            txtTen = Input(320); F("Tên dịch vụ *", txtTen);
            chkActive = new CheckBox { Text = "  Đang sử dụng", Checked = true, AutoSize = true, ForeColor = TEXT, Location = new Point(18, y) }; pnl.Controls.Add(chkActive); y += 36;
            pnl.Controls.Add(new Label { Text = "── Cập nhật đơn giá ──", ForeColor = ACCENT, AutoSize = true, Location = new Point(18, y), Font = new Font("Segoe UI", 9f, FontStyle.Bold) }); y += 26;
            numGia = new NumericUpDown { Minimum = 0, Maximum = 9999999, Increment = 1000, BackColor = Color.FromArgb(20, 20, 40), ForeColor = TEXT }; F("Đơn giá mới (₫)", numGia);
            dtpNgay = new DateTimePicker { Format = DateTimePickerFormat.Short, Value = DateTime.Today }; F("Ngày áp dụng", dtpNgay);
            LblError = new Label { Text = "", ForeColor = RED, Location = new Point(18, y), Size = new Size(320, 36) }; pnl.Controls.Add(LblError); y += 40;
            BtnSave = Btn("💾  Lưu", GREEN, new Point(18, y), 110); BtnCancel = Btn("✖  Hủy", Color.FromArgb(70, 70, 90), new Point(133, y), 90);
            BtnSave.Click += (s, e) => OnSave(); BtnCancel.Click += (s, e) => OnCancel();
            pnl.Controls.AddRange(new Control[] { BtnSave, BtnCancel });
            return pnl;
        }
        protected override void AddFormButtons(Panel pnl) { }
        protected override void LoadData() => Grid.DataSource = _dal.GetAll();
        protected override void FillForm(DataGridViewRow row) { SelectedId = Val<int>(row, "MaDichVu"); txtTen.Text = Val<string>(row, "TenDichVu"); chkActive.Checked = Val<string>(row, "TrangThai") == "Đang dùng"; }
        protected override void ClearForm() { base.ClearForm(); txtTen.Text = ""; chkActive.Checked = true; numGia.Value = 0; dtpNgay.Value = DateTime.Today; }
        protected override void SaveNew() { if (string.IsNullOrWhiteSpace(txtTen.Text)) throw new Exception("Tên dịch vụ trống!"); _dal.InsertDichVu(txtTen.Text.Trim(), chkActive.Checked); if (numGia.Value > 0) { var dt = _dal.GetAll(); int id = Convert.ToInt32(dt.Rows[dt.Rows.Count - 1]["MaDichVu"]); _dal.ThemGia(id, (decimal)numGia.Value, dtpNgay.Value); } }
        protected override void SaveUpdate() { _dal.UpdateDichVu(SelectedId, txtTen.Text.Trim(), chkActive.Checked); if (numGia.Value > 0) _dal.ThemGia(SelectedId, (decimal)numGia.Value, dtpNgay.Value); }
        protected override void DeleteRow() => throw new Exception("Tắt dịch vụ thay vì xóa!");
        protected override void SetFormEnabled(bool v) { txtTen.Enabled = chkActive.Enabled = numGia.Enabled = dtpNgay.Enabled = v; BtnSave.Enabled = BtnCancel.Enabled = v; }
    }

    // ═══════════════════════════════════════════════════════════
    //  FORM CHI PHÍ
    // ═══════════════════════════════════════════════════════════
    public class frmChiPhi : BaseListForm
    {
        protected override string FormTitle => "💸  Quản Lý Chi Phí";
        protected override int FormPanelWidth => 390;
        private ComboBox cboKhu; private TextBox txtTen, txtGhiChu;
        private NumericUpDown numSoTien; private DateTimePicker dtpNgay;
        private readonly ChiPhiDAL _dal = new ChiPhiDAL();
        private readonly KhuTroDAL _khuDal = new KhuTroDAL();
        public frmChiPhi() { InitBase(); LoadData(); }
        protected override DataGridView BuildGrid()
        {
            var g = MakeGrid();
            g.Columns.AddRange(Col("MaChiPhi", "Mã", 55), Col("KhuTro", "Khu Trọ", 120), Col("TenChiPhi", "Nội Dung"), Col("SoTien", "Số Tiền", 110), Col("NgayChi", "Ngày Chi", 90), Col("GhiChu", "Ghi Chú"));
            return g;
        }
        protected override Panel BuildFormPanel()
        {
            var pnl = new Panel { Dock = DockStyle.Fill, BackColor = CARD };
            int y = 18;
            pnl.Controls.Add(new Label { Text = "THÔNG TIN CHI PHÍ", Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = ACCENT, AutoSize = true, Location = new Point(18, y) }); y += 34;
            void F(string l, Control c, int h = 28) { pnl.Controls.Add(Lbl(l, new Point(18, y))); y += 20; c.Location = new Point(18, y); c.Size = new Size(340, h); pnl.Controls.Add(c); y += h + 12; }
            cboKhu = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, BackColor = Color.FromArgb(20, 20, 40), ForeColor = TEXT, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9.5f) };
            cboKhu.Items.Add(new ComboItem { Value = 0, Text = "(Chung - không thuộc khu nào)" });
            foreach (DataRow r in _khuDal.GetAll().Rows) cboKhu.Items.Add(new ComboItem { Value = (int)r["MaKhuTro"], Text = r["TenKhuTro"].ToString() });
            cboKhu.SelectedIndex = 0; F("Khu trọ", cboKhu);
            txtTen = Input(340); F("Tên chi phí *", txtTen);
            numSoTien = new NumericUpDown { Minimum = 0, Maximum = 999999999, Increment = 10000, BackColor = Color.FromArgb(20, 20, 40), ForeColor = TEXT }; F("Số tiền (₫) *", numSoTien);
            dtpNgay = new DateTimePicker { Format = DateTimePickerFormat.Short, Value = DateTime.Today }; F("Ngày chi", dtpNgay);
            txtGhiChu = Input(340); txtGhiChu.Multiline = true; txtGhiChu.Height = 52; F("Ghi chú", txtGhiChu, 52);
            LblError = new Label { Text = "", ForeColor = RED, Location = new Point(18, y), Size = new Size(340, 36) }; pnl.Controls.Add(LblError); y += 40;
            BtnSave = Btn("💾  Lưu", GREEN, new Point(18, y), 110); BtnCancel = Btn("✖  Hủy", Color.FromArgb(70, 70, 90), new Point(133, y), 90);
            BtnSave.Click += (s, e) => OnSave(); BtnCancel.Click += (s, e) => OnCancel();
            pnl.Controls.AddRange(new Control[] { BtnSave, BtnCancel });
            return pnl;
        }
        protected override void AddFormButtons(Panel pnl) { }
        protected override void LoadData() => Grid.DataSource = _dal.GetAll();
        protected override void FillForm(DataGridViewRow row) { SelectedId = Val<int>(row, "MaChiPhi"); txtTen.Text = Val<string>(row, "TenChiPhi"); numSoTien.Value = Math.Min((decimal)numSoTien.Maximum, Val<decimal>(row, "SoTien")); txtGhiChu.Text = Val<string>(row, "GhiChu"); if (DateTime.TryParse(row.Cells["NgayChi"].Value?.ToString(), out DateTime d)) dtpNgay.Value = d; }
        protected override void ClearForm() { base.ClearForm(); txtTen.Text = txtGhiChu.Text = ""; numSoTien.Value = 0; dtpNgay.Value = DateTime.Today; cboKhu.SelectedIndex = 0; }
        private ChiPhi Build() { if (string.IsNullOrWhiteSpace(txtTen.Text)) throw new Exception("Tên chi phí trống!"); if (numSoTien.Value <= 0) throw new Exception("Số tiền phải > 0!"); var khu = (ComboItem)cboKhu.SelectedItem; return new ChiPhi { MaChiPhi = SelectedId, MaKhuTro = khu?.Value == 0 ? (int?)null : khu?.Value, TenChiPhi = txtTen.Text.Trim(), SoTien = (decimal)numSoTien.Value, NgayChi = dtpNgay.Value, GhiChu = txtGhiChu.Text }; }
        protected override void SaveNew() => _dal.Insert(Build());
        protected override void SaveUpdate() => _dal.Update(Build());
        protected override void DeleteRow() => _dal.Delete(SelectedId);
        protected override void SetFormEnabled(bool v) { txtTen.Enabled = txtGhiChu.Enabled = numSoTien.Enabled = dtpNgay.Enabled = cboKhu.Enabled = v; BtnSave.Enabled = BtnCancel.Enabled = v; }
    }

    // ═══════════════════════════════════════════════════════════
    //  FORM BÁO CÁO
    // ═══════════════════════════════════════════════════════════
    public class frmBaoCao : Form
    {
        private readonly Color BG = Color.FromArgb(15, 15, 30);
        private readonly Color CARD = Color.FromArgb(24, 24, 46);
        private readonly Color ACC = Color.FromArgb(99, 102, 241);
        private readonly Color TEXT = Color.FromArgb(230, 230, 255);
        private readonly Color SEC = Color.FromArgb(160, 160, 200);
        private readonly Color GRN = Color.FromArgb(16, 185, 129);
        private readonly Color RED = Color.FromArgb(220, 38, 38);
        private readonly Color YEL = Color.FromArgb(234, 179, 8);
        private readonly BaoCaoDAL _dal = new BaoCaoDAL();

        public frmBaoCao() { this.BackColor = BG; this.Font = new Font("Segoe UI", 9.5f); Build(); }

        private void Build()
        {
            var pnlH = new Panel { Dock = DockStyle.Top, Height = 52, BackColor = Color.FromArgb(20, 20, 42) };
            pnlH.Controls.Add(new Label { Text = "📈  Báo Cáo & Thống Kê", Font = new Font("Segoe UI", 14, FontStyle.Bold), ForeColor = TEXT, AutoSize = true, Location = new Point(18, 12) });

            var tab = new TabControl { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 10f) };
            tab.DrawMode = TabDrawMode.OwnerDrawFixed;
            tab.DrawItem += (s, e) =>
            {
                e.Graphics.FillRectangle(new SolidBrush(tab.SelectedIndex == e.Index ? ACC : CARD), e.Bounds);
                e.Graphics.DrawString(tab.TabPages[e.Index].Text, new Font("Segoe UI", 9.5f), Brushes.White, e.Bounds.X + 6, e.Bounds.Y + 6);
            };

            tab.TabPages.Add(TabDoanhThu());
            tab.TabPages.Add(TabPhong());
            tab.TabPages.Add(TabLoiNhuan());

            this.Controls.AddRange(new Control[] { tab, pnlH });
        }

        private TabPage TabDoanhThu()
        {
            var pg = new TabPage("  📊 Doanh Thu Theo Tháng  ") { BackColor = BG };
            var pnlT = new Panel { Dock = DockStyle.Top, Height = 52, BackColor = CARD };
            var num = new NumericUpDown { Location = new Point(20, 14), Size = new Size(80, 28), Minimum = 2020, Maximum = 2099, Value = DateTime.Now.Year, BackColor = Color.FromArgb(20, 20, 40), ForeColor = TEXT };
            var btn = MkBtn("🔍  Xem", ACC, new Point(110, 14), 90);
            var grid = MkGrid(pg);
            grid.Columns.AddRange(GCol("Thang", "Tháng", 80), GCol("DoanhThu", "Doanh Thu (₫)"));
            btn.Click += (s, e) => { grid.DataSource = _dal.DoanhThuTheoThang((int)num.Value); };
            pnlT.Controls.AddRange(new Control[] { new Label { Text = "Năm:", ForeColor = SEC, AutoSize = true, Location = new Point(14, 18) }, num, btn });
            pg.Controls.AddRange(new Control[] { grid, pnlT });
            btn.PerformClick();
            return pg;
        }

        private TabPage TabPhong()
        {
            var pg = new TabPage("  🚪 Tình Trạng Phòng  ") { BackColor = BG };
            var grid = MkGrid(pg);
            grid.Columns.AddRange(GCol("TrangThai", "Trạng Thái"), GCol("SoPhong", "Số Phòng", 100));
            grid.CellFormatting += (s, e) =>
            {
                if (e.ColumnIndex < 0 || grid.Columns[e.ColumnIndex].Name != "TrangThai") return;
                switch (e.Value?.ToString()) { case "Trống": e.CellStyle.ForeColor = GRN; break; case "Đang thuê": e.CellStyle.ForeColor = YEL; break; case "Bảo trì": e.CellStyle.ForeColor = RED; break; }
            };
            try { grid.DataSource = _dal.TinhTrangPhong(); } catch { }
            return pg;
        }

        private TabPage TabLoiNhuan()
        {
            var pg = new TabPage("  💰 Lợi Nhuận Tháng  ") { BackColor = BG };
            var pnlT = new Panel { Dock = DockStyle.Top, Height = 52, BackColor = CARD };
            var numTh = new NumericUpDown { Location = new Point(65, 14), Size = new Size(55, 28), Minimum = 1, Maximum = 12, Value = DateTime.Now.Month, BackColor = Color.FromArgb(20, 20, 40), ForeColor = TEXT };
            var numNam = new NumericUpDown { Location = new Point(185, 14), Size = new Size(80, 28), Minimum = 2020, Maximum = 2099, Value = DateTime.Now.Year, BackColor = Color.FromArgb(20, 20, 40), ForeColor = TEXT };
            var btnX = MkBtn("🔍  Xem", ACC, new Point(275, 14), 90);
            var lblResult = new Label { Text = "", ForeColor = GRN, Font = new Font("Segoe UI", 16, FontStyle.Bold), Location = new Point(20, 80), Size = new Size(900, 100), AutoSize = false };
            btnX.Click += (s, e) =>
            {
                try
                {
                    var dt = _dal.LoiNhuan((int)numTh.Value, (int)numNam.Value);
                    if (dt.Rows.Count > 0)
                    {
                        decimal dt2 = Convert.ToDecimal(dt.Rows[0]["DoanhThu"]);
                        decimal cp = Convert.ToDecimal(dt.Rows[0]["ChiPhi"]);
                        decimal ln = dt2 - cp;
                        lblResult.Text = $"Tháng {numTh.Value}/{numNam.Value}\n\n" +
                                         $"💰 Doanh thu:  {dt2:N0} ₫\n" +
                                         $"💸 Chi phí:    {cp:N0} ₫\n" +
                                         $"📈 Lợi nhuận:  {ln:N0} ₫";
                        lblResult.ForeColor = ln >= 0 ? GRN : RED;
                    }
                }
                catch (Exception ex) { lblResult.Text = "Lỗi: " + ex.Message; }
            };
            pnlT.Controls.AddRange(new Control[] { new Label { Text = "Tháng:", ForeColor = SEC, AutoSize = true, Location = new Point(14, 18) }, numTh, new Label { Text = "Năm:", ForeColor = SEC, AutoSize = true, Location = new Point(130, 18) }, numNam, btnX });
            pg.Controls.AddRange(new Control[] { lblResult, pnlT });
            btnX.PerformClick();
            return pg;
        }

        private DataGridView MkGrid(Control parent) { var g = new DataGridView { Dock = DockStyle.Fill, BackgroundColor = BG, BorderStyle = BorderStyle.None, RowHeadersVisible = false, AllowUserToAddRows = false, ReadOnly = true, SelectionMode = DataGridViewSelectionMode.FullRowSelect, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, EnableHeadersVisualStyles = false, ColumnHeadersHeight = 34, RowTemplate = { Height = 34 }, Font = new Font("Segoe UI", 10f) }; g.DefaultCellStyle.BackColor = BG; g.DefaultCellStyle.ForeColor = TEXT; g.DefaultCellStyle.SelectionBackColor = ACC; g.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(30, 30, 55); g.ColumnHeadersDefaultCellStyle.ForeColor = SEC; parent.Controls.Add(g); return g; }
        private DataGridViewTextBoxColumn GCol(string n, string h, int w = -1) { var c = new DataGridViewTextBoxColumn { Name = n, HeaderText = h, DataPropertyName = n }; if (w > 0) { c.AutoSizeMode = DataGridViewAutoSizeColumnMode.None; c.Width = w; } return c; }
        private Button MkBtn(string t, Color c, Point loc, int w) { var b = new Button { Text = t, Location = loc, Size = new Size(w, 28), BackColor = c, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9f, FontStyle.Bold), Cursor = Cursors.Hand }; b.FlatAppearance.BorderSize = 0; return b; }
    }

    // ═══════════════════════════════════════════════════════════
    //  FORM TÀI KHOẢN
    // ═══════════════════════════════════════════════════════════
    public class frmTaiKhoan : BaseListForm
    {
        protected override string FormTitle => "🔑  Quản Lý Tài Khoản";
        protected override int FormPanelWidth => 380;
        private TextBox txtUser, txtPass, txtPassConfirm;
        private ComboBox cboVaiTro; private CheckBox chkActive;
        private readonly TaiKhoanDAL _dal = new TaiKhoanDAL();
        private readonly TaiKhoan _currentUser;
        public frmTaiKhoan(TaiKhoan user) { _currentUser = user; InitBase(); LoadData(); }
        protected override DataGridView BuildGrid()
        {
            var g = MakeGrid();
            g.Columns.AddRange(Col("MaTaiKhoan", "Mã", 60), Col("TenDangNhap", "Tên Đăng Nhập"), Col("TenVaiTro", "Vai Trò", 110), Col("TrangThai", "Trạng Thái", 100));
            g.CellFormatting += (s, e) => { if (e.ColumnIndex >= 0 && g.Columns[e.ColumnIndex].Name == "TrangThai") e.CellStyle.ForeColor = e.Value?.ToString() == "Hoạt động" ? GREEN : RED; };
            return g;
        }
        protected override Panel BuildFormPanel()
        {
            var pnl = new Panel { Dock = DockStyle.Fill, BackColor = CARD };
            int y = 18;
            pnl.Controls.Add(new Label { Text = "THÔNG TIN TÀI KHOẢN", Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = ACCENT, AutoSize = true, Location = new Point(18, y) }); y += 34;
            void F(string l, Control c, int h = 28) { pnl.Controls.Add(Lbl(l, new Point(18, y))); y += 20; c.Location = new Point(18, y); c.Size = new Size(320, h); pnl.Controls.Add(c); y += h + 12; }
            txtUser = Input(320); F("Tên đăng nhập *", txtUser);
            txtPass = Input(320); txtPass.PasswordChar = '●'; F("Mật khẩu (bỏ trống = không đổi)", txtPass);
            txtPassConfirm = Input(320); txtPassConfirm.PasswordChar = '●'; F("Xác nhận mật khẩu", txtPassConfirm);
            cboVaiTro = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, BackColor = Color.FromArgb(20, 20, 40), ForeColor = TEXT, FlatStyle = FlatStyle.Flat };
            foreach (DataRow r in _dal.GetVaiTro().Rows) cboVaiTro.Items.Add(new ComboItem { Value = (int)r["MaVaiTro"], Text = r["TenVaiTro"].ToString() });
            cboVaiTro.SelectedIndex = 0; F("Vai trò *", cboVaiTro);
            chkActive = new CheckBox { Text = "  Tài khoản đang hoạt động", Checked = true, AutoSize = true, ForeColor = TEXT, Location = new Point(18, y) }; pnl.Controls.Add(chkActive); y += 36;
            LblError = new Label { Text = "", ForeColor = RED, Location = new Point(18, y), Size = new Size(320, 36) }; pnl.Controls.Add(LblError); y += 40;
            BtnSave = Btn("💾  Lưu", GREEN, new Point(18, y), 110); BtnCancel = Btn("✖  Hủy", Color.FromArgb(70, 70, 90), new Point(133, y), 90);
            BtnSave.Click += (s, e) => OnSave(); BtnCancel.Click += (s, e) => OnCancel();
            pnl.Controls.AddRange(new Control[] { BtnSave, BtnCancel });
            return pnl;
        }
        protected override void AddFormButtons(Panel pnl) { }
        protected override void LoadData() => Grid.DataSource = _dal.GetAll(SafeSearch());
        protected override void FillForm(DataGridViewRow row)
        {
            SelectedId = Val<int>(row, "MaTaiKhoan");
            txtUser.Text = Val<string>(row, "TenDangNhap");
            txtPass.Text = txtPassConfirm.Text = "";
            chkActive.Checked = Val<string>(row, "TrangThai") == "Hoạt động";
            foreach (ComboItem item in cboVaiTro.Items) if (item.Text == Val<string>(row, "TenVaiTro")) { cboVaiTro.SelectedItem = item; break; }
        }
        protected override void ClearForm() { base.ClearForm(); txtUser.Text = txtPass.Text = txtPassConfirm.Text = ""; chkActive.Checked = true; if (cboVaiTro.Items.Count > 0) cboVaiTro.SelectedIndex = 0; }
        protected override void SaveNew()
        {
            if (string.IsNullOrWhiteSpace(txtUser.Text)) throw new Exception("Tên đăng nhập trống!");
            if (string.IsNullOrWhiteSpace(txtPass.Text)) throw new Exception("Phải nhập mật khẩu!");
            if (txtPass.Text.Length < 6) throw new Exception("Mật khẩu tối thiểu 6 ký tự!");
            if (txtPass.Text != txtPassConfirm.Text) throw new Exception("Mật khẩu xác nhận không khớp!");
            _dal.Insert(txtUser.Text.Trim(), txtPass.Text, ((ComboItem)cboVaiTro.SelectedItem).Value);
        }
        protected override void SaveUpdate()
        {
            if (txtUser.Text == _currentUser?.TenDangNhap && !chkActive.Checked) throw new Exception("Không thể khóa tài khoản đang đăng nhập!");
            if (!string.IsNullOrWhiteSpace(txtPass.Text) && txtPass.Text != txtPassConfirm.Text) throw new Exception("Mật khẩu xác nhận không khớp!");
            _dal.Update(SelectedId, txtUser.Text.Trim(), txtPass.Text, ((ComboItem)cboVaiTro.SelectedItem).Value, chkActive.Checked);
        }
        protected override void DeleteRow() { if (Val<string>(Grid.SelectedRows[0], "TenDangNhap") == _currentUser?.TenDangNhap) throw new Exception("Không thể xóa tài khoản đang đăng nhập!"); _dal.Delete(SelectedId); }
        protected override void SetFormEnabled(bool v) { txtUser.Enabled = txtPass.Enabled = txtPassConfirm.Enabled = cboVaiTro.Enabled = chkActive.Enabled = v; BtnSave.Enabled = BtnCancel.Enabled = v; }
    }
}