using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;

namespace quanliphongtro.DAL
{
    // ═══════════════════════════════════════════════════════════════
    //  DB HELPER
    // ═══════════════════════════════════════════════════════════════
    public static class DBHelper
    {
        public static string ConnStr =>
            ConfigurationManager.ConnectionStrings["quanliphongtro"]?.ConnectionString
            ?? @"Server=(local);Database=quanliphongtro;Integrated Security=True;TrustServerCertificate=True;";

        public static SqlConnection GetConn() => new SqlConnection(ConnStr);

        public static DataTable Query(string sql, params SqlParameter[] p)
        {
            using (var conn = GetConn())
            {
                conn.Open();
                using (var cmd = new SqlCommand(sql, conn))
                {
                    if (p != null) cmd.Parameters.AddRange(p);
                    var dt = new DataTable();
                    new SqlDataAdapter(cmd).Fill(dt);
                    return dt;
                }
            }
        }

        public static int Exec(string sql, params SqlParameter[] p)
        {
            using (var conn = GetConn())
            {
                conn.Open();
                using (var cmd = new SqlCommand(sql, conn))
                {
                    if (p != null) cmd.Parameters.AddRange(p);
                    return cmd.ExecuteNonQuery();
                }
            }
        }

        public static object Scalar(string sql, params SqlParameter[] p)
        {
            using (var conn = GetConn())
            {
                conn.Open();
                using (var cmd = new SqlCommand(sql, conn))
                {
                    if (p != null) cmd.Parameters.AddRange(p);
                    return cmd.ExecuteScalar();
                }
            }
        }

        public static DataTable StoredProc(string proc, params SqlParameter[] p)
        {
            using (var conn = GetConn())
            {
                conn.Open();
                using (var cmd = new SqlCommand(proc, conn) { CommandType = CommandType.StoredProcedure })
                {
                    if (p != null) cmd.Parameters.AddRange(p);
                    var dt = new DataTable();
                    new SqlDataAdapter(cmd).Fill(dt);
                    return dt;
                }
            }
        }

        public static SqlParameter P(string name, object val)
            => new SqlParameter(name, val ?? DBNull.Value);
    }

    // ═══════════════════════════════════════════════════════════════
    //  MODELS
    // ═══════════════════════════════════════════════════════════════
    public class TaiKhoan
    {
        public int MaTaiKhoan { get; set; }
        public string TenDangNhap { get; set; }
        public string MatKhau { get; set; }
        public int MaVaiTro { get; set; }
        public string TenVaiTro { get; set; }
        public bool TrangThai { get; set; }
    }

    public class KhachHang
    {
        public int MaKhachHang { get; set; }
        public string HoTen { get; set; }
        public string CCCD { get; set; }
        public string DienThoai { get; set; }
        public string Phuong { get; set; }
        public bool DangKyTamTru { get; set; }
        public DateTime? NgayDangKyTamTru { get; set; }
        public int? MaTaiKhoan { get; set; }
    }

    public class KhuTro
    {
        public int MaKhuTro { get; set; }
        public string TenKhuTro { get; set; }
        public string DiaChi { get; set; }
        public bool TrangThai { get; set; }
    }

    public class Phong
    {
        public int MaPhong { get; set; }
        public int MaKhuTro { get; set; }
        public string TenPhong { get; set; }
        public decimal GiaPhong { get; set; }
        public string TrangThai { get; set; }
        public string GhiChu { get; set; }
    }

    public class HopDong
    {
        public int MaHopDong { get; set; }
        public int MaPhong { get; set; }
        public int MaKhachHang { get; set; }
        public DateTime NgayBatDau { get; set; }
        public DateTime NgayKetThuc { get; set; }
        public decimal TienCoc { get; set; }
        public string TrangThai { get; set; }
        public string DieuKhoan { get; set; }
    }

    public class ChiSoDienNuoc
    {
        public int MaChiSo { get; set; }
        public int MaPhong { get; set; }
        public int Thang { get; set; }
        public int Nam { get; set; }
        public int DienCu { get; set; }
        public int DienMoi { get; set; }
        public int NuocCu { get; set; }
        public int NuocMoi { get; set; }
        public DateTime NgayNhap { get; set; }
    }

    public class HoaDon
    {
        public int MaHoaDon { get; set; }
        public int MaHopDong { get; set; }
        public int Thang { get; set; }
        public int Nam { get; set; }
        public decimal TongTien { get; set; }
        public string TrangThai { get; set; }
        public DateTime NgayTao { get; set; }
    }

    public class ChiPhi
    {
        public int MaChiPhi { get; set; }
        public int? MaKhuTro { get; set; }
        public string TenChiPhi { get; set; }
        public decimal SoTien { get; set; }
        public DateTime NgayChi { get; set; }
        public string GhiChu { get; set; }
    }

    // ═══════════════════════════════════════════════════════════════
    //  DAL: TAIKHOAN
    // ═══════════════════════════════════════════════════════════════
    public class TaiKhoanDAL
    {
        public TaiKhoan DangNhap(string tenDN, string matKhau)
        {
            var dt = DBHelper.Query(
                @"SELECT tk.*, vt.TenVaiTro FROM TaiKhoan tk
                  INNER JOIN VaiTro vt ON tk.MaVaiTro = vt.MaVaiTro
                  WHERE tk.TenDangNhap=@TenDN AND tk.MatKhau=@MK AND tk.TrangThai=1",
                DBHelper.P("@TenDN", tenDN), DBHelper.P("@MK", matKhau));
            if (dt.Rows.Count == 0) return null;
            var r = dt.Rows[0];
            return new TaiKhoan
            {
                MaTaiKhoan = (int)r["MaTaiKhoan"],
                TenDangNhap = r["TenDangNhap"].ToString(),
                MaVaiTro = (int)r["MaVaiTro"],
                TenVaiTro = r["TenVaiTro"].ToString(),
                TrangThai = (bool)r["TrangThai"]
            };
        }

        public DataTable GetAll(string search = "")
        {
            return DBHelper.Query(
                @"SELECT tk.MaTaiKhoan, tk.TenDangNhap, vt.TenVaiTro,
                         CASE WHEN tk.TrangThai=1 THEN N'Hoạt động' ELSE N'Bị khóa' END AS TrangThai
                  FROM TaiKhoan tk INNER JOIN VaiTro vt ON tk.MaVaiTro=vt.MaVaiTro
                  WHERE @S='' OR tk.TenDangNhap LIKE '%'+@S+'%'
                  ORDER BY tk.MaTaiKhoan",
                DBHelper.P("@S", search));
        }

        public void Insert(string tenDN, string matKhau, int maVaiTro)
        {
            DBHelper.Exec(
                "INSERT INTO TaiKhoan(TenDangNhap,MatKhau,MaVaiTro,TrangThai) VALUES(@T,@M,@V,1)",
                DBHelper.P("@T", tenDN), DBHelper.P("@M", matKhau), DBHelper.P("@V", maVaiTro));
        }

        public void Update(int maTK, string tenDN, string matKhau, int maVaiTro, bool active)
        {
            bool hasMK = !string.IsNullOrWhiteSpace(matKhau);
            if (hasMK)
                DBHelper.Exec(
                    "UPDATE TaiKhoan SET TenDangNhap=@T,MatKhau=@M,MaVaiTro=@V,TrangThai=@A WHERE MaTaiKhoan=@ID",
                    DBHelper.P("@T", tenDN), DBHelper.P("@M", matKhau),
                    DBHelper.P("@V", maVaiTro), DBHelper.P("@A", active ? 1 : 0), DBHelper.P("@ID", maTK));
            else
                DBHelper.Exec(
                    "UPDATE TaiKhoan SET TenDangNhap=@T,MaVaiTro=@V,TrangThai=@A WHERE MaTaiKhoan=@ID",
                    DBHelper.P("@T", tenDN), DBHelper.P("@V", maVaiTro),
                    DBHelper.P("@A", active ? 1 : 0), DBHelper.P("@ID", maTK));
        }

        public void Delete(int maTK)
            => DBHelper.Exec("DELETE FROM TaiKhoan WHERE MaTaiKhoan=@ID", DBHelper.P("@ID", maTK));

        public DataTable GetVaiTro()
            => DBHelper.Query("SELECT MaVaiTro, TenVaiTro FROM VaiTro ORDER BY MaVaiTro");
    }

    // ═══════════════════════════════════════════════════════════════
    //  DAL: KHACHHANG
    // ═══════════════════════════════════════════════════════════════
    public class KhachHangDAL
    {
        public DataTable GetAll(string search = "")
        {
            return DBHelper.Query(
                @"SELECT MaKhachHang, HoTen, CCCD, DienThoai, Phuong,
                         CASE WHEN DangKyTamTru=1 THEN N'Có' ELSE N'Không' END AS TamTru,
                         CONVERT(nvarchar,NgayDangKyTamTru,103) AS NgayTamTru
                  FROM KhachHang
                  WHERE @S='' OR HoTen LIKE '%'+@S+'%' OR CCCD LIKE '%'+@S+'%' OR DienThoai LIKE '%'+@S+'%'
                  ORDER BY MaKhachHang",
                DBHelper.P("@S", search));
        }

        public void Insert(KhachHang kh)
        {
            DBHelper.Exec(
                @"INSERT INTO KhachHang(HoTen,CCCD,DienThoai,Phuong,DangKyTamTru,NgayDangKyTamTru,MaTaiKhoan)
                  VALUES(@HT,@CC,@DT,@PH,@TT,@NT,@MK)",
                DBHelper.P("@HT", kh.HoTen), DBHelper.P("@CC", kh.CCCD),
                DBHelper.P("@DT", kh.DienThoai), DBHelper.P("@PH", kh.Phuong),
                DBHelper.P("@TT", kh.DangKyTamTru ? 1 : 0),
                DBHelper.P("@NT", (object)kh.NgayDangKyTamTru),
                DBHelper.P("@MK", (object)kh.MaTaiKhoan));
        }

        public void Update(KhachHang kh)
        {
            DBHelper.Exec(
                @"UPDATE KhachHang SET HoTen=@HT,CCCD=@CC,DienThoai=@DT,Phuong=@PH,
                  DangKyTamTru=@TT,NgayDangKyTamTru=@NT,MaTaiKhoan=@MK
                  WHERE MaKhachHang=@ID",
                DBHelper.P("@HT", kh.HoTen), DBHelper.P("@CC", kh.CCCD),
                DBHelper.P("@DT", kh.DienThoai), DBHelper.P("@PH", kh.Phuong),
                DBHelper.P("@TT", kh.DangKyTamTru ? 1 : 0),
                DBHelper.P("@NT", (object)kh.NgayDangKyTamTru),
                DBHelper.P("@MK", (object)kh.MaTaiKhoan),
                DBHelper.P("@ID", kh.MaKhachHang));
        }

        public void Delete(int ma)
            => DBHelper.Exec("DELETE FROM KhachHang WHERE MaKhachHang=@ID", DBHelper.P("@ID", ma));
    }

    // ═══════════════════════════════════════════════════════════════
    //  DAL: KHUTRO
    // ═══════════════════════════════════════════════════════════════
    public class KhuTroDAL
    {
        public DataTable GetAll(bool? active = null)
        {
            string where = active.HasValue ? " WHERE TrangThai=" + (active.Value ? "1" : "0") : "";
            return DBHelper.Query("SELECT * FROM KhuTro" + where + " ORDER BY MaKhuTro");
        }

        public void Insert(KhuTro kt)
            => DBHelper.Exec(
                "INSERT INTO KhuTro(TenKhuTro,DiaChi,TrangThai) VALUES(@T,@D,@S)",
                DBHelper.P("@T", kt.TenKhuTro), DBHelper.P("@D", kt.DiaChi), DBHelper.P("@S", kt.TrangThai ? 1 : 0));

        public void Update(KhuTro kt)
            => DBHelper.Exec(
                "UPDATE KhuTro SET TenKhuTro=@T,DiaChi=@D,TrangThai=@S WHERE MaKhuTro=@ID",
                DBHelper.P("@T", kt.TenKhuTro), DBHelper.P("@D", kt.DiaChi),
                DBHelper.P("@S", kt.TrangThai ? 1 : 0), DBHelper.P("@ID", kt.MaKhuTro));

        public void Delete(int ma)
            => DBHelper.Exec("DELETE FROM KhuTro WHERE MaKhuTro=@ID", DBHelper.P("@ID", ma));
    }

    // ═══════════════════════════════════════════════════════════════
    //  DAL: PHONG
    // ═══════════════════════════════════════════════════════════════
    public class PhongDAL
    {
        public DataTable GetAll(string search = "", int maKhuTro = 0, string trangThai = "")
        {
            return DBHelper.Query(
                @"SELECT p.MaPhong, p.TenPhong, kt.TenKhuTro, p.GiaPhong, p.TrangThai, p.GhiChu, p.MaKhuTro
                  FROM Phong p INNER JOIN KhuTro kt ON p.MaKhuTro=kt.MaKhuTro
                  WHERE (@S='' OR p.TenPhong LIKE '%'+@S+'%')
                    AND (@KT=0 OR p.MaKhuTro=@KT)
                    AND (@TT='' OR p.TrangThai=@TT)
                  ORDER BY kt.TenKhuTro, p.TenPhong",
                DBHelper.P("@S", search), DBHelper.P("@KT", maKhuTro), DBHelper.P("@TT", trangThai));
        }

        public DataTable GetPhongTrong()
            => DBHelper.Query("SELECT MaPhong, TenPhong FROM Phong WHERE TrangThai=N'Trống' ORDER BY TenPhong");

        public void Insert(Phong p)
            => DBHelper.Exec(
                "INSERT INTO Phong(MaKhuTro,TenPhong,GiaPhong,TrangThai,GhiChu) VALUES(@K,@T,@G,@S,@N)",
                DBHelper.P("@K", p.MaKhuTro), DBHelper.P("@T", p.TenPhong),
                DBHelper.P("@G", p.GiaPhong), DBHelper.P("@S", p.TrangThai), DBHelper.P("@N", p.GhiChu));

        public void Update(Phong p)
            => DBHelper.Exec(
                "UPDATE Phong SET MaKhuTro=@K,TenPhong=@T,GiaPhong=@G,TrangThai=@S,GhiChu=@N WHERE MaPhong=@ID",
                DBHelper.P("@K", p.MaKhuTro), DBHelper.P("@T", p.TenPhong),
                DBHelper.P("@G", p.GiaPhong), DBHelper.P("@S", p.TrangThai),
                DBHelper.P("@N", p.GhiChu), DBHelper.P("@ID", p.MaPhong));

        public void Delete(int ma)
            => DBHelper.Exec("DELETE FROM Phong WHERE MaPhong=@ID", DBHelper.P("@ID", ma));

        public void CapNhatTrangThai(int maPhong, string trangThai)
            => DBHelper.Exec("UPDATE Phong SET TrangThai=@S WHERE MaPhong=@ID",
                DBHelper.P("@S", trangThai), DBHelper.P("@ID", maPhong));

        // Thiết bị của phòng
        public DataTable GetThietBi(int maPhong)
            => DBHelper.Query(
                @"SELECT tb.MaThietBi, tb.TenThietBi, tb.TrangThai, pt.SoLuong
                  FROM Phong_ThietBi pt INNER JOIN ThietBi tb ON pt.MaThietBi=tb.MaThietBi
                  WHERE pt.MaPhong=@ID",
                DBHelper.P("@ID", maPhong));
    }

    // ═══════════════════════════════════════════════════════════════
    //  DAL: HOPDONG
    // ═══════════════════════════════════════════════════════════════
    public class HopDongDAL
    {
        public DataTable GetAll(string search = "", string trangThai = "")
        {
            return DBHelper.Query(
                @"SELECT hd.MaHopDong, p.TenPhong, kt.TenKhuTro,
                         kh.HoTen, kh.DienThoai,
                         CONVERT(nvarchar,hd.NgayBatDau,103) AS NgayBatDau,
                         CONVERT(nvarchar,hd.NgayKetThuc,103) AS NgayKetThuc,
                         hd.TienCoc, hd.TrangThai,
                         DATEDIFF(DAY, GETDATE(), hd.NgayKetThuc) AS NgayConLai,
                         hd.MaPhong, hd.MaKhachHang, hd.DieuKhoan
                  FROM HopDong hd
                  INNER JOIN Phong p ON hd.MaPhong=p.MaPhong
                  INNER JOIN KhuTro kt ON p.MaKhuTro=kt.MaKhuTro
                  INNER JOIN KhachHang kh ON hd.MaKhachHang=kh.MaKhachHang
                  WHERE (@S='' OR kh.HoTen LIKE '%'+@S+'%' OR p.TenPhong LIKE '%'+@S+'%')
                    AND (@TT='' OR hd.TrangThai=@TT)
                  ORDER BY hd.MaHopDong DESC",
                DBHelper.P("@S", search), DBHelper.P("@TT", trangThai));
        }

        public DataTable GetSapHetHan(int ngay = 30)
            => DBHelper.Query(
                @"SELECT hd.MaHopDong, p.TenPhong, kh.HoTen, kh.DienThoai,
                         CONVERT(nvarchar,hd.NgayKetThuc,103) AS NgayKetThuc,
                         DATEDIFF(DAY,GETDATE(),hd.NgayKetThuc) AS NgayConLai
                  FROM HopDong hd
                  INNER JOIN Phong p ON hd.MaPhong=p.MaPhong
                  INNER JOIN KhachHang kh ON hd.MaKhachHang=kh.MaKhachHang
                  WHERE hd.TrangThai=N'Còn hiệu lực'
                    AND DATEDIFF(DAY,GETDATE(),hd.NgayKetThuc) BETWEEN 0 AND @N
                  ORDER BY NgayConLai",
                DBHelper.P("@N", ngay));

        public void Insert(HopDong hd)
        {
            DBHelper.Exec(
                @"INSERT INTO HopDong(MaPhong,MaKhachHang,NgayBatDau,NgayKetThuc,TienCoc,TrangThai,DieuKhoan)
                  VALUES(@P,@KH,@BD,@KT,@TC,@TT,@DK)",
                DBHelper.P("@P", hd.MaPhong), DBHelper.P("@KH", hd.MaKhachHang),
                DBHelper.P("@BD", hd.NgayBatDau), DBHelper.P("@KT", hd.NgayKetThuc),
                DBHelper.P("@TC", hd.TienCoc), DBHelper.P("@TT", hd.TrangThai),
                DBHelper.P("@DK", hd.DieuKhoan));
            // Cập nhật trạng thái phòng → Đang thuê
            DBHelper.Exec("UPDATE Phong SET TrangThai=N'Đang thuê' WHERE MaPhong=@P",
                DBHelper.P("@P", hd.MaPhong));
        }

        public void Update(HopDong hd)
            => DBHelper.Exec(
                @"UPDATE HopDong SET NgayBatDau=@BD,NgayKetThuc=@KT,TienCoc=@TC,TrangThai=@TT,DieuKhoan=@DK
                  WHERE MaHopDong=@ID",
                DBHelper.P("@BD", hd.NgayBatDau), DBHelper.P("@KT", hd.NgayKetThuc),
                DBHelper.P("@TC", hd.TienCoc), DBHelper.P("@TT", hd.TrangThai),
                DBHelper.P("@DK", hd.DieuKhoan), DBHelper.P("@ID", hd.MaHopDong));

        public void ThanhLy(int maHopDong, int maPhong)
        {
            DBHelper.Exec("UPDATE HopDong SET TrangThai=N'Đã Hủy' WHERE MaHopDong=@ID",
                DBHelper.P("@ID", maHopDong));
            DBHelper.Exec("UPDATE Phong SET TrangThai=N'Trống' WHERE MaPhong=@P",
                DBHelper.P("@P", maPhong));
        }

        public void GiaHan(int maHopDong, DateTime ngayKetThucMoi)
            => DBHelper.Exec(
                "UPDATE HopDong SET NgayKetThuc=@NK, TrangThai=N'Còn hiệu lực' WHERE MaHopDong=@ID",
                DBHelper.P("@NK", ngayKetThucMoi), DBHelper.P("@ID", maHopDong));
    }

    // ═══════════════════════════════════════════════════════════════
    //  DAL: CHISO DIEN NUOC
    // ═══════════════════════════════════════════════════════════════
    public class ChiSoDienNuocDAL
    {
        public DataTable GetByPhong(int maPhong)
            => DBHelper.Query(
                @"SELECT MaChiSo, MaPhong, Thang, Nam, DienCu, DienMoi, NuocCu, NuocMoi,
                         (DienMoi-DienCu) AS TieuThuDien, (NuocMoi-NuocCu) AS TieuThuNuoc,
                         CONVERT(nvarchar,NgayNhap,103) AS NgayNhap
                  FROM ChiSoDienNuoc WHERE MaPhong=@P ORDER BY Nam DESC, Thang DESC",
                DBHelper.P("@P", maPhong));

        public DataTable GetThangTruoc(int maPhong, int thang, int nam)
        {
            int thangTruoc = thang == 1 ? 12 : thang - 1;
            int namTruoc = thang == 1 ? nam - 1 : nam;
            return DBHelper.Query(
                "SELECT * FROM ChiSoDienNuoc WHERE MaPhong=@P AND Thang=@T AND Nam=@N",
                DBHelper.P("@P", maPhong), DBHelper.P("@T", thangTruoc), DBHelper.P("@N", namTruoc));
        }

        public bool DaTonTai(int maPhong, int thang, int nam)
        {
            var r = DBHelper.Scalar(
                "SELECT COUNT(*) FROM ChiSoDienNuoc WHERE MaPhong=@P AND Thang=@T AND Nam=@N",
                DBHelper.P("@P", maPhong), DBHelper.P("@T", thang), DBHelper.P("@N", nam));
            return Convert.ToInt32(r) > 0;
        }

        public void Insert(ChiSoDienNuoc cs)
            => DBHelper.Exec(
                @"INSERT INTO ChiSoDienNuoc(MaPhong,Thang,Nam,DienCu,DienMoi,NuocCu,NuocMoi,NgayNhap)
                  VALUES(@P,@TH,@N,@DC,@DM,@NC,@NM,GETDATE())",
                DBHelper.P("@P", cs.MaPhong), DBHelper.P("@TH", cs.Thang), DBHelper.P("@N", cs.Nam),
                DBHelper.P("@DC", cs.DienCu), DBHelper.P("@DM", cs.DienMoi),
                DBHelper.P("@NC", cs.NuocCu), DBHelper.P("@NM", cs.NuocMoi));

        public void Update(ChiSoDienNuoc cs)
            => DBHelper.Exec(
                @"UPDATE ChiSoDienNuoc SET DienCu=@DC,DienMoi=@DM,NuocCu=@NC,NuocMoi=@NM
                  WHERE MaChiSo=@ID",
                DBHelper.P("@DC", cs.DienCu), DBHelper.P("@DM", cs.DienMoi),
                DBHelper.P("@NC", cs.NuocCu), DBHelper.P("@NM", cs.NuocMoi), DBHelper.P("@ID", cs.MaChiSo));
    }

    // ═══════════════════════════════════════════════════════════════
    //  DAL: DICHVU
    // ═══════════════════════════════════════════════════════════════
    public class DichVuDAL
    {
        public DataTable GetAll()
            => DBHelper.Query(
                @"SELECT dv.MaDichVu, dv.TenDichVu,
                         CASE WHEN dv.TrangThai=1 THEN N'Đang dùng' ELSE N'Ngưng' END AS TrangThai,
                         gd.DonGia, CONVERT(nvarchar,gd.NgayApDung,103) AS NgayApDung
                  FROM DichVu dv
                  LEFT JOIN GiaDichVu gd ON dv.MaDichVu=gd.MaDichVu
                    AND gd.NgayApDung=(SELECT MAX(NgayApDung) FROM GiaDichVu WHERE MaDichVu=dv.MaDichVu)
                  ORDER BY dv.MaDichVu");

        public void InsertDichVu(string tenDV, bool active)
            => DBHelper.Exec("INSERT INTO DichVu(TenDichVu,TrangThai) VALUES(@T,@S)",
                DBHelper.P("@T", tenDV), DBHelper.P("@S", active ? 1 : 0));

        public void UpdateDichVu(int ma, string tenDV, bool active)
            => DBHelper.Exec("UPDATE DichVu SET TenDichVu=@T,TrangThai=@S WHERE MaDichVu=@ID",
                DBHelper.P("@T", tenDV), DBHelper.P("@S", active ? 1 : 0), DBHelper.P("@ID", ma));

        public void ThemGia(int maDichVu, decimal donGia, DateTime ngayApDung)
            => DBHelper.Exec("INSERT INTO GiaDichVu(MaDichVu,DonGia,NgayApDung) VALUES(@DV,@G,@N)",
                DBHelper.P("@DV", maDichVu), DBHelper.P("@G", donGia), DBHelper.P("@N", ngayApDung));
    }

    // ═══════════════════════════════════════════════════════════════
    //  DAL: HOADON
    // ═══════════════════════════════════════════════════════════════
    public class HoaDonDAL
    {
        public DataTable GetAll(string search = "", string trangThai = "")
            => DBHelper.Query(
                @"SELECT hd.MaHoaDon, p.TenPhong, kt.TenKhuTro, kh.HoTen,
                         hd.Thang, hd.Nam, hd.TongTien, hd.TrangThai,
                         CONVERT(nvarchar,hd.NgayTao,103) AS NgayTao, hd.MaHopDong
                  FROM HoaDon hd
                  INNER JOIN HopDong hop ON hd.MaHopDong=hop.MaHopDong
                  INNER JOIN Phong p ON hop.MaPhong=p.MaPhong
                  INNER JOIN KhuTro kt ON p.MaKhuTro=kt.MaKhuTro
                  INNER JOIN KhachHang kh ON hop.MaKhachHang=kh.MaKhachHang
                  WHERE (@S='' OR kh.HoTen LIKE '%'+@S+'%' OR p.TenPhong LIKE '%'+@S+'%')
                    AND (@TT='' OR hd.TrangThai=@TT)
                  ORDER BY hd.Nam DESC, hd.Thang DESC, hd.MaHoaDon DESC",
                DBHelper.P("@S", search), DBHelper.P("@TT", trangThai));

        public DataTable TaoHoaDon(int maHopDong, int thang, int nam)
            => DBHelper.StoredProc("sp_TaoHoaDonThang",
                DBHelper.P("@MaHopDong", maHopDong),
                DBHelper.P("@Thang", thang),
                DBHelper.P("@Nam", nam));

        public DataTable GetChiTiet(int maHoaDon)
            => DBHelper.Query(
                @"SELECT dv.TenDichVu, hd_dv.SoLuong, hd_dv.DonGia,
                         (hd_dv.SoLuong * hd_dv.DonGia) AS ThanhTien
                  FROM HoaDon_DichVu hd_dv
                  INNER JOIN DichVu dv ON hd_dv.MaDichVu=dv.MaDichVu
                  WHERE hd_dv.MaHoaDon=@ID",
                DBHelper.P("@ID", maHoaDon));

        public DataTable GetThanhToan(int maHoaDon)
            => DBHelper.Query(
                @"SELECT MaThanhToan, SoTien, HinhThuc, CongThanhToan, TrangThai,
                         CONVERT(nvarchar,NgayThanhToan,103) AS NgayThanhToan
                  FROM ThanhToan WHERE MaHoaDon=@ID ORDER BY MaThanhToan",
                DBHelper.P("@ID", maHoaDon));

        public void GhiThanhToan(int maHoaDon, decimal soTien, string hinhThuc, string congTT)
        {
            DBHelper.Exec(
                @"INSERT INTO ThanhToan(MaHoaDon,SoTien,HinhThuc,CongThanhToan,TrangThai)
                  VALUES(@HD,@ST,@HT,@CT,N'Thành công')",
                DBHelper.P("@HD", maHoaDon), DBHelper.P("@ST", soTien),
                DBHelper.P("@HT", hinhThuc), DBHelper.P("@CT", congTT));
            // Cập nhật trạng thái hóa đơn
            DBHelper.Exec(
                @"UPDATE HoaDon SET TrangThai=
                    CASE WHEN (SELECT SUM(SoTien) FROM ThanhToan WHERE MaHoaDon=@HD AND TrangThai=N'Thành công') >= TongTien
                         THEN N'Đã thanh toán' ELSE N'Thanh toán một phần' END
                  WHERE MaHoaDon=@HD",
                DBHelper.P("@HD", maHoaDon));
        }
    }

    // ═══════════════════════════════════════════════════════════════
    //  DAL: CHI PHI
    // ═══════════════════════════════════════════════════════════════
    public class ChiPhiDAL
    {
        public DataTable GetAll(int maKhuTro = 0, int nam = 0)
            => DBHelper.Query(
                @"SELECT cp.MaChiPhi, ISNULL(kt.TenKhuTro, N'Chung') AS KhuTro,
                         cp.TenChiPhi, cp.SoTien,
                         CONVERT(nvarchar,cp.NgayChi,103) AS NgayChi, cp.GhiChu, cp.MaKhuTro
                  FROM ChiPhi cp LEFT JOIN KhuTro kt ON cp.MaKhuTro=kt.MaKhuTro
                  WHERE (@KT=0 OR cp.MaKhuTro=@KT)
                    AND (@NAM=0 OR YEAR(cp.NgayChi)=@NAM)
                  ORDER BY cp.NgayChi DESC",
                DBHelper.P("@KT", maKhuTro), DBHelper.P("@NAM", nam));

        public void Insert(ChiPhi cp)
            => DBHelper.Exec(
                "INSERT INTO ChiPhi(MaKhuTro,TenChiPhi,SoTien,NgayChi,GhiChu) VALUES(@KT,@TC,@ST,@NC,@GC)",
                DBHelper.P("@KT", (object)cp.MaKhuTro), DBHelper.P("@TC", cp.TenChiPhi),
                DBHelper.P("@ST", cp.SoTien), DBHelper.P("@NC", cp.NgayChi), DBHelper.P("@GC", cp.GhiChu));

        public void Update(ChiPhi cp)
            => DBHelper.Exec(
                "UPDATE ChiPhi SET MaKhuTro=@KT,TenChiPhi=@TC,SoTien=@ST,NgayChi=@NC,GhiChu=@GC WHERE MaChiPhi=@ID",
                DBHelper.P("@KT", (object)cp.MaKhuTro), DBHelper.P("@TC", cp.TenChiPhi),
                DBHelper.P("@ST", cp.SoTien), DBHelper.P("@NC", cp.NgayChi),
                DBHelper.P("@GC", cp.GhiChu), DBHelper.P("@ID", cp.MaChiPhi));

        public void Delete(int ma)
            => DBHelper.Exec("DELETE FROM ChiPhi WHERE MaChiPhi=@ID", DBHelper.P("@ID", ma));
    }

    // ═══════════════════════════════════════════════════════════════
    //  DAL: THIETBI
    // ═══════════════════════════════════════════════════════════════
    public class ThietBiDAL
    {
        public DataTable GetAll()
            => DBHelper.Query("SELECT * FROM ThietBi ORDER BY MaThietBi");

        public void Insert(string ten, string trangThai)
            => DBHelper.Exec("INSERT INTO ThietBi(TenThietBi,TrangThai) VALUES(@T,@S)",
                DBHelper.P("@T", ten), DBHelper.P("@S", trangThai));

        public void Update(int ma, string ten, string trangThai)
            => DBHelper.Exec("UPDATE ThietBi SET TenThietBi=@T,TrangThai=@S WHERE MaThietBi=@ID",
                DBHelper.P("@T", ten), DBHelper.P("@S", trangThai), DBHelper.P("@ID", ma));

        public void GanVaoPhong(int maPhong, int maThietBi, int soLuong)
            => DBHelper.Exec(
                @"IF EXISTS (SELECT 1 FROM Phong_ThietBi WHERE MaPhong=@P AND MaThietBi=@TB)
                      UPDATE Phong_ThietBi SET SoLuong=@SL WHERE MaPhong=@P AND MaThietBi=@TB
                  ELSE
                      INSERT INTO Phong_ThietBi VALUES(@P,@TB,@SL)",
                DBHelper.P("@P", maPhong), DBHelper.P("@TB", maThietBi), DBHelper.P("@SL", soLuong));
    }

    // ═══════════════════════════════════════════════════════════════
    //  DAL: BAOCAO
    // ═══════════════════════════════════════════════════════════════
    public class BaoCaoDAL
    {
        /// <summary>Doanh thu theo tháng trong năm</summary>
        public DataTable DoanhThuTheoThang(int nam)
            => DBHelper.Query(
                @"SELECT Thang, SUM(SoTien) AS DoanhThu
                  FROM ThanhToan tt
                  INNER JOIN HoaDon hd ON tt.MaHoaDon=hd.MaHoaDon
                  WHERE YEAR(tt.NgayThanhToan)=@NAM AND tt.TrangThai=N'Thành công'
                  GROUP BY Thang ORDER BY Thang",
                DBHelper.P("@NAM", nam));

        /// <summary>Thống kê phòng theo trạng thái</summary>
        public DataTable TinhTrangPhong()
            => DBHelper.Query(
                @"SELECT TrangThai, COUNT(*) AS SoPhong FROM Phong GROUP BY TrangThai");

        /// <summary>Lợi nhuận = Doanh thu - Chi phí theo tháng</summary>
        public DataTable LoiNhuan(int thang, int nam)
            => DBHelper.Query(
                @"SELECT
                    ISNULL((SELECT SUM(SoTien) FROM ThanhToan tt
                            INNER JOIN HoaDon hd ON tt.MaHoaDon=hd.MaHoaDon
                            WHERE hd.Thang=@TH AND hd.Nam=@NAM AND tt.TrangThai=N'Thành công'), 0) AS DoanhThu,
                    ISNULL((SELECT SUM(SoTien) FROM ChiPhi
                            WHERE MONTH(NgayChi)=@TH AND YEAR(NgayChi)=@NAM), 0) AS ChiPhi",
                DBHelper.P("@TH", thang), DBHelper.P("@NAM", nam));
    }
}