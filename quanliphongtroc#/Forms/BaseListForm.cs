using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace quanliphongtro.Forms
{
    /// <summary>
    /// Lớp cha dùng chung: SplitContainer (Grid trái | Form phải), Toolbar tìm kiếm + CRUD.
    /// Subclass override: BuildFormPanel(), LoadData(), SaveNew(), SaveUpdate(), DeleteRow().
    /// </summary>
    public abstract class BaseListForm : Form
    {
        // ─── Palette ──────────────────────────────────────────────────────
        protected readonly Color BG = Color.FromArgb(15, 15, 30);
        protected readonly Color PANEL = Color.FromArgb(20, 20, 40);
        protected readonly Color CARD = Color.FromArgb(24, 24, 46);
        protected readonly Color ACCENT = Color.FromArgb(99, 102, 241);
        protected readonly Color ADIM = Color.FromArgb(67, 70, 180);
        protected readonly Color GREEN = Color.FromArgb(16, 185, 129);
        protected readonly Color RED = Color.FromArgb(220, 38, 38);
        protected readonly Color YELLOW = Color.FromArgb(234, 179, 8);
        protected readonly Color TEXT = Color.FromArgb(230, 230, 255);
        protected readonly Color SEC = Color.FromArgb(160, 160, 200);
        protected readonly Color BORDER = Color.FromArgb(50, 50, 80);

        // ─── Controls ─────────────────────────────────────────────────────
        protected DataGridView Grid;
        protected Panel PnlForm;
        protected TextBox TxtSearch;
        protected Label LblError;
        protected Button BtnAdd, BtnEdit, BtnDelete, BtnSave, BtnCancel;

        protected int SelectedId = -1;
        protected bool IsEditing;

        protected abstract string FormTitle { get; }
        protected abstract int FormPanelWidth { get; }
        protected virtual bool HasSearch => true;

        // ─── Init ─────────────────────────────────────────────────────────
        protected void InitBase()
        {
            this.BackColor = BG;
            this.Font = new Font("Segoe UI", 9.5f);

            // Header
            var pnlHeader = new Panel { Dock = DockStyle.Top, Height = 52, BackColor = PANEL };
            var lbl = new Label
            {
                Text = FormTitle,
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = TEXT,
                AutoSize = true,
                Location = new Point(18, 12)
            };
            pnlHeader.Controls.Add(lbl);

            // Toolbar
            var pnlTool = new Panel { Dock = DockStyle.Top, Height = 52, BackColor = CARD };
            int tx = 14;

            if (HasSearch)
            {
                TxtSearch = new TextBox
                {
                    Location = new Point(tx, 13),
                    Size = new Size(260, 28),
                    BackColor = PANEL,
                    ForeColor = SEC,
                    BorderStyle = BorderStyle.FixedSingle,
                    Font = new Font("Segoe UI", 9.5f),
                    Text = "🔍  Tìm kiếm..."
                };
                TxtSearch.GotFocus += (s, e) => { if (TxtSearch.Text.StartsWith("🔍")) { TxtSearch.Text = ""; TxtSearch.ForeColor = TEXT; } };
                TxtSearch.LostFocus += (s, e) => { if (TxtSearch.Text == "") { TxtSearch.Text = "🔍  Tìm kiếm..."; TxtSearch.ForeColor = SEC; } };
                TxtSearch.TextChanged += (s, e) => { if (!TxtSearch.Text.StartsWith("🔍")) LoadData(); };
                pnlTool.Controls.Add(TxtSearch);
                tx += 275;
            }

            BtnAdd = Btn("➕  Thêm", ACCENT, new Point(tx, 11)); tx += 108;
            BtnEdit = Btn("✏️  Sửa", ADIM, new Point(tx, 11)); tx += 108;
            BtnDelete = Btn("🗑️  Xóa", RED, new Point(tx, 11)); tx += 108;

            BtnAdd.Click += (s, e) => OnAdd();
            BtnEdit.Click += (s, e) => OnEdit();
            BtnDelete.Click += (s, e) => OnDelete();
            pnlTool.Controls.AddRange(new Control[] { BtnAdd, BtnEdit, BtnDelete });
            ExtraToolbarButtons(pnlTool, ref tx);

            // Split
            var split = new SplitContainer
            {
                Size = new Size(1000, 600),
                Dock = DockStyle.Fill,
                Orientation = Orientation.Vertical,
                IsSplitterFixed = true,
                BackColor = BG,
                Panel1MinSize = 300,
                Panel2MinSize = 250
            };
            split.Resize += (s, e) =>
            {
                if (split.Width <= 0) return;
                int dist = split.Width - FormPanelWidth - 4;
                int min = split.Panel1MinSize + 1;
                int max = split.Width - split.Panel2MinSize - 1;
                if (dist < min) dist = min;
                if (dist > max) dist = max;
                try { split.SplitterDistance = dist; } catch { }
            };

            Grid = BuildGrid();
            Grid.SelectionChanged += Grid_SelectionChanged;
            split.Panel1.Controls.Add(Grid);
            split.Panel1.BackColor = BG;

            PnlForm = BuildFormPanel();
            split.Panel2.Controls.Add(PnlForm);
            split.Panel2.BackColor = BG;

            // Bottom buttons inside form panel (override to add elsewhere)
            LblError = new Label
            {
                Text = "",
                ForeColor = RED,
                Size = new Size(FormPanelWidth - 40, 40),
                Font = new Font("Segoe UI", 8.5f)
            };
            BtnSave = Btn("💾  Lưu", GREEN, Point.Empty, 115);
            BtnCancel = Btn("✖  Hủy", Color.FromArgb(70, 70, 90), Point.Empty, 90);
            BtnSave.Click += (s, e) => OnSave();
            BtnCancel.Click += (s, e) => OnCancel();
            AddFormButtons(PnlForm);
            SetFormEnabled(false);

            this.Controls.AddRange(new Control[] { split, pnlTool, pnlHeader });
        }

        // ─── Abstract / virtual hooks ──────────────────────────────────────
        protected abstract Panel BuildFormPanel();
        protected abstract DataGridView BuildGrid();
        protected abstract void LoadData();
        protected abstract void FillForm(DataGridViewRow row);
        protected abstract void SaveNew();
        protected abstract void SaveUpdate();
        protected abstract void DeleteRow();
        protected virtual void ClearForm() { LblError.Text = ""; SelectedId = -1; }
        protected virtual void ExtraToolbarButtons(Panel pnl, ref int x) { }
        protected virtual void AddFormButtons(Panel pnl) { }

        // ─── CRUD handlers ────────────────────────────────────────────────
        protected virtual void OnAdd()
        {
            SelectedId = -1; ClearForm();
            SetFormEnabled(true); IsEditing = true;
        }

        protected virtual void OnEdit()
        {
            if (SelectedId < 0) { Msg("Chọn dòng cần sửa!"); return; }
            SetFormEnabled(true); IsEditing = true;
        }

        protected virtual void OnDelete()
        {
            if (SelectedId < 0) { Msg("Chọn dòng cần xóa!"); return; }
            if (MessageBox.Show("Xóa dòng đã chọn?", "Xác nhận",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                try { DeleteRow(); LoadData(); ClearForm(); }
                catch (Exception ex) { Msg("Lỗi: " + ex.Message, true); }
            }
        }

        protected virtual void OnSave()
        {
            LblError.Text = "";
            try
            {
                if (SelectedId < 0) SaveNew(); else SaveUpdate();
                LoadData(); ClearForm(); SetFormEnabled(false); IsEditing = false;
                Msg("Đã lưu thành công!", false, true);
            }
            catch (Exception ex) { LblError.Text = "⚠  " + ex.Message; }
        }

        protected virtual void OnCancel()
        {
            ClearForm(); SetFormEnabled(false); IsEditing = false;
        }

        private void Grid_SelectionChanged(object sender, EventArgs e)
        {
            if (Grid.SelectedRows.Count == 0 || IsEditing) return;
            FillForm(Grid.SelectedRows[0]);
        }

        // ─── Helpers ──────────────────────────────────────────────────────
        protected virtual void SetFormEnabled(bool v)
        {
            BtnSave.Enabled = v; BtnCancel.Enabled = v;
        }

        protected Button Btn(string text, Color bg, Point loc, int w = 100)
        {
            var b = new Button
            {
                Text = text,
                Location = loc,
                Size = new Size(w, 30),
                BackColor = bg,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            b.FlatAppearance.BorderSize = 0;
            return b;
        }

        protected TextBox Input(int w = 380)
            => new TextBox
            {
                Size = new Size(w, 28),
                BackColor = PANEL,
                ForeColor = TEXT,
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 9.5f)
            };

        protected Label Lbl(string text, Point loc, bool bold = false)
            => new Label
            {
                Text = text,
                Location = loc,
                AutoSize = true,
                ForeColor = SEC,
                Font = new Font("Segoe UI", 8.5f, bold ? FontStyle.Bold : FontStyle.Regular)
            };

        protected DataGridView MakeGrid()
        {
            var g = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = BG,
                BorderStyle = BorderStyle.None,
                GridColor = BORDER,
                RowHeadersVisible = false,
                AllowUserToAddRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                EnableHeadersVisualStyles = false,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing,
                ColumnHeadersHeight = 36,
                RowTemplate = { Height = 32 },
                Font = new Font("Segoe UI", 9.5f)
            };
            g.DefaultCellStyle.BackColor = BG;
            g.DefaultCellStyle.ForeColor = TEXT;
            g.DefaultCellStyle.SelectionBackColor = ACCENT;
            g.DefaultCellStyle.SelectionForeColor = Color.White;
            g.DefaultCellStyle.Padding = new Padding(5, 0, 0, 0);
            g.AlternatingRowsDefaultCellStyle.BackColor = PANEL;
            g.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(35, 35, 62);
            g.ColumnHeadersDefaultCellStyle.ForeColor = SEC;
            g.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            return g;
        }

        protected DataGridViewTextBoxColumn Col(string name, string header, int w = -1)
        {
            var c = new DataGridViewTextBoxColumn
            { Name = name, HeaderText = header, DataPropertyName = name };
            if (w > 0) { c.AutoSizeMode = DataGridViewAutoSizeColumnMode.None; c.Width = w; }
            return c;
        }

        protected void Msg(string text, bool isError = false, bool isSuccess = false)
        {
            Color c = isError ? RED : (isSuccess ? GREEN : YELLOW);
            MessageBox.Show(text, "Thông báo", MessageBoxButtons.OK,
                isError ? MessageBoxIcon.Error : MessageBoxIcon.Information);
        }

        protected string SafeSearch()
        {
            if (TxtSearch == null) return "";
            string t = TxtSearch.Text;
            return t.StartsWith("🔍") ? "" : t.Trim();
        }

        protected T Val<T>(DataGridViewRow row, string col)
        {
            var v = row.Cells[col].Value;
            if (v == null || v == DBNull.Value) return default;
            return (T)Convert.ChangeType(v, typeof(T));
        }
    }
}