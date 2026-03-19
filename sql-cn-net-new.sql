CREATE DATABASE quanliphongtro;
GO

USE quanliphongtro;
GO

-- 1. Bảng VaiTro
CREATE TABLE VaiTro (
    MaVaiTro INT IDENTITY(1,1) PRIMARY KEY,
    TenVaiTro NVARCHAR(50) -- Quản lý khu, Chủ trọ, Khách thuê
);
GO

-- 2. Bảng TaiKhoan (Khớp với điều kiện tiền đề cần đăng nhập phân quyền)
CREATE TABLE TaiKhoan (
    MaTaiKhoan INT IDENTITY(1,1) PRIMARY KEY,
    TenDangNhap VARCHAR(50) UNIQUE NOT NULL,
    MatKhau VARCHAR(255) NOT NULL,
    MaVaiTro INT NOT NULL,
    TrangThai BIT DEFAULT 1,

    FOREIGN KEY (MaVaiTro) REFERENCES VaiTro(MaVaiTro)
);
GO

-- 3. Bảng KhachHang (Bổ sung NgayDangKyTamTru để khớp UC-TCT-01: Tạo hồ sơ tạm trú)
CREATE TABLE KhachHang (
    MaKhachHang INT IDENTITY(1,1) PRIMARY KEY,
    HoTen NVARCHAR(100) NOT NULL,
    CCCD VARCHAR(12) UNIQUE NOT NULL,
    DienThoai VARCHAR(10),
    Phuong NVARCHAR(50),
    DangKyTamTru BIT DEFAULT 0,
    NgayDangKyTamTru DATE, -- Bổ sung thêm
    MaTaiKhoan INT,

    FOREIGN KEY (MaTaiKhoan) REFERENCES TaiKhoan(MaTaiKhoan)
);
GO

-- 4. Bảng KhuTro (Khớp UC-TCT-06)
CREATE TABLE KhuTro (
    MaKhuTro INT IDENTITY(1,1) PRIMARY KEY,
    TenKhuTro NVARCHAR(100),
    DiaChi NVARCHAR(255),
    TrangThai BIT DEFAULT 1 -- Bật/Tắt trạng thái khu trọ
);
GO

-- 5. Bảng Phong (Khớp UC-TCT-02)
CREATE TABLE Phong (
    MaPhong INT IDENTITY(1,1) PRIMARY KEY,
    MaKhuTro INT NOT NULL,
    TenPhong NVARCHAR(50),
    GiaPhong DECIMAL(18,0),
    TrangThai NVARCHAR(20), -- Trống / Đang thuê / Bảo trì
    GhiChu NVARCHAR(255),

    FOREIGN KEY (MaKhuTro) REFERENCES KhuTro(MaKhuTro)
);
GO

-- 6. Bảng ThietBi (Khớp UC-TCT-07)
CREATE TABLE ThietBi (
    MaThietBi INT IDENTITY(1,1) PRIMARY KEY,
    TenThietBi NVARCHAR(100),
    TrangThai NVARCHAR(50) 
);
GO

-- 7. Bảng Phong_ThietBi
CREATE TABLE Phong_ThietBi (
    MaPhong INT,
    MaThietBi INT,
    SoLuong INT,

    PRIMARY KEY (MaPhong, MaThietBi),
    FOREIGN KEY (MaPhong) REFERENCES Phong(MaPhong),
    FOREIGN KEY (MaThietBi) REFERENCES ThietBi(MaThietBi)
);
GO

-- 8. Bảng HopDong (Khớp UC-TCT-08: Có gia hạn, hủy hợp đồng)
CREATE TABLE HopDong (
    MaHopDong INT IDENTITY(1,1) PRIMARY KEY,
    MaPhong INT NOT NULL,
    MaKhachHang INT NOT NULL,
    NgayBatDau DATE,
    NgayKetThuc DATE,
    TienCoc DECIMAL(18,0),
    TrangThai NVARCHAR(50), -- Còn hiệu lực / Hết hạn / Đã Hủy
    DieuKhoan NVARCHAR(MAX),

    FOREIGN KEY (MaPhong) REFERENCES Phong(MaPhong),
    FOREIGN KEY (MaKhachHang) REFERENCES KhachHang(MaKhachHang)
);
GO

-- 9. Bảng ChiSoDienNuoc (Khớp UC-TCT-03 và UC-TCT-11: Khách hàng nhập số điện nước)
CREATE TABLE ChiSoDienNuoc (
    MaChiSo INT IDENTITY(1,1) PRIMARY KEY,
    MaPhong INT NOT NULL,
    Thang INT,
    Nam INT,
    DienCu INT,
    DienMoi INT,
    NuocCu INT,
    NuocMoi INT,
    NgayNhap DATE DEFAULT GETDATE(), -- Tự động lấy ngày lúc khách/chủ trọ nhập

    FOREIGN KEY (MaPhong) REFERENCES Phong(MaPhong)
);
GO

-- 10. Bảng DichVu (Khớp UC-TCT-04)
CREATE TABLE DichVu (
    MaDichVu INT IDENTITY(1,1) PRIMARY KEY,
    TenDichVu NVARCHAR(100),
    TrangThai BIT DEFAULT 1
);
GO

-- 11. Bảng GiaDichVu
CREATE TABLE GiaDichVu (
    MaGia INT IDENTITY(1,1) PRIMARY KEY,
    MaDichVu INT NOT NULL,
    DonGia DECIMAL(18,0),
    NgayApDung DATE,

    FOREIGN KEY (MaDichVu) REFERENCES DichVu(MaDichVu)
);
GO

-- 12. Bảng HoaDon (Khớp UC-TCT-09)
CREATE TABLE HoaDon (
    MaHoaDon INT IDENTITY(1,1) PRIMARY KEY,
    MaHopDong INT NOT NULL,
    Thang INT,
    Nam INT,
    TongTien DECIMAL(18,0),
    TrangThai NVARCHAR(50), -- Chưa thanh toán / Đã thanh toán
    NgayTao DATE DEFAULT GETDATE(),

    FOREIGN KEY (MaHopDong) REFERENCES HopDong(MaHopDong)
);
GO

-- 13. Bảng HoaDon_DichVu
CREATE TABLE HoaDon_DichVu (
    MaHoaDon INT,
    MaDichVu INT,
    SoLuong INT,
    DonGia DECIMAL(18,0),

    PRIMARY KEY (MaHoaDon, MaDichVu),
    FOREIGN KEY (MaHoaDon) REFERENCES HoaDon(MaHoaDon),
    FOREIGN KEY (MaDichVu) REFERENCES DichVu(MaDichVu)
);
GO

-- 14. Bảng ThanhToan (Khớp UC-TCT-11: Thanh toán tiền mặt / Online bằng QR)
CREATE TABLE ThanhToan (
    MaThanhToan INT IDENTITY(1,1) PRIMARY KEY,
    MaHoaDon INT NOT NULL,
    NgayThanhToan DATE DEFAULT GETDATE(),
    SoTien DECIMAL(18,0),
    HinhThuc NVARCHAR(50), -- Tiền mặt / Online
    CongThanhToan NVARCHAR(50), -- VD: Momo, VNPay, Chuyển khoản ngân hàng
    TrangThai NVARCHAR(50), -- Chờ duyệt / Thành công / Thất bại

    FOREIGN KEY (MaHoaDon) REFERENCES HoaDon(MaHoaDon)
);
GO

-- 15. BỔ SUNG BẢNG MỚI: Bảng ChiPhi (ĐỂ KHỚP UC-TCT-10: Thống kê chi phí & Báo cáo lợi nhuận)
CREATE TABLE ChiPhi (
    MaChiPhi INT IDENTITY(1,1) PRIMARY KEY,
    MaKhuTro INT, -- Tùy chọn: Khoản chi này thuộc khu trọ nào
    TenChiPhi NVARCHAR(100) NOT NULL, -- VD: Sửa ống nước, thay bóng đèn, tiền rác...
    SoTien DECIMAL(18,0) NOT NULL,
    NgayChi DATE DEFAULT GETDATE(),
    GhiChu NVARCHAR(MAX),
    
    FOREIGN KEY (MaKhuTro) REFERENCES KhuTro(MaKhuTro)
);
GO

INSERT INTO VaiTro(TenVaiTro) VALUES (N'Quản lý'), (N'Khách thuê');
GO
 
INSERT INTO TaiKhoan(TenDangNhap, MatKhau, MaVaiTro, TrangThai)
VALUES ('admin', 'admin123', 1, 1),
       ('khach01', '123456', 2, 1);
GO
 
INSERT INTO KhuTro(TenKhuTro, DiaChi, TrangThai)
VALUES (N'Khu Trọ Bình An', N'123 Nguyễn Trãi, P.2, Q.5, TP.HCM', 1),
       (N'Khu Trọ Hòa Bình', N'45 Lê Văn Sỹ, P.12, Q.3, TP.HCM', 1);
GO
 
INSERT INTO Phong(MaKhuTro, TenPhong, GiaPhong, TrangThai, GhiChu)
VALUES (1, N'P101', 2500000, N'Đang thuê', N'Phòng đơn, có điều hòa'),
       (1, N'P102', 2500000, N'Trống', N'Phòng đôi'),
       (1, N'P103', 3000000, N'Bảo trì', N'Đang sơn lại'),
       (2, N'P201', 3500000, N'Đang thuê', N'Phòng đôi, ban công'),
       (2, N'P202', 3000000, N'Trống', NULL);
GO
 
INSERT INTO KhachHang(HoTen, CCCD, DienThoai, Phuong, DangKyTamTru, NgayDangKyTamTru, MaTaiKhoan)
VALUES (N'Nguyễn Văn An', '001234567890', '0901234567', N'P.2, Q.5', 1, '2024-01-15', 2),
       (N'Trần Thị Bình', '002345678901', '0912345678', N'P.12, Q.3', 0, NULL, NULL);
GO
 
INSERT INTO ThietBi(TenThietBi, TrangThai)
VALUES (N'Điều hòa', N'Tốt'),
       (N'Máy nước nóng', N'Tốt'),
       (N'Tủ lạnh', N'Cũ'),
       (N'Giường', N'Tốt');
GO
 
INSERT INTO DichVu(TenDichVu, TrangThai) VALUES
(N'Điện', 1), (N'Nước', 1), (N'Internet', 1),
(N'Gửi xe', 1), (N'Vệ sinh', 1);
GO
 
INSERT INTO GiaDichVu(MaDichVu, DonGia, NgayApDung) VALUES
(1, 3500,    '2024-01-01'),
(2, 15000,   '2024-01-01'),
(3, 100000,  '2024-01-01'),
(4, 100000,  '2024-01-01'),
(5, 50000,   '2024-01-01');
GO
 
INSERT INTO HopDong(MaPhong, MaKhachHang, NgayBatDau, NgayKetThuc, TienCoc, TrangThai, DieuKhoan)
VALUES (1, 1, '2024-01-15', '2025-01-15', 5000000, N'Còn hiệu lực', N'Báo trước 30 ngày khi chấm dứt HĐ.'),
       (4, 2, '2024-03-01', '2025-03-01', 7000000, N'Còn hiệu lực', N'Không nuôi thú cưng.');
GO
 
INSERT INTO ChiSoDienNuoc(MaPhong, Thang, Nam, DienCu, DienMoi, NuocCu, NuocMoi)
VALUES (1, 2, 2025, 1200, 1350, 80, 92),
       (4, 2, 2025, 2100, 2280, 140, 158);
GO
 
INSERT INTO ChiPhi(MaKhuTro, TenChiPhi, SoTien, NgayChi, GhiChu)
VALUES (1, N'Sửa ống nước tầng 1', 500000, '2025-02-10', N'Thợ Minh Tuấn'),
       (1, N'Thay bóng đèn hành lang', 150000, '2025-02-15', NULL),
       (2, N'Sơn lại cầu thang',      800000, '2025-02-20', N'Thợ Hùng');
GO
 
-- Stored Procedure tạo hóa đơn tháng
CREATE PROCEDURE sp_TaoHoaDonThang
    @MaHopDong INT,
    @Thang INT,
    @Nam INT,
    @DonGiaDien DECIMAL(18,0) = 3500,
    @DonGiaNuoc DECIMAL(18,0) = 15000
AS
BEGIN
    SET NOCOUNT ON;
    -- Kiểm tra đã có HĐ tháng này chưa
    IF EXISTS (SELECT 1 FROM HoaDon WHERE MaHopDong=@MaHopDong AND Thang=@Thang AND Nam=@Nam)
    BEGIN
        RAISERROR(N'Hóa đơn tháng này đã tồn tại!', 16, 1); RETURN;
    END
 
    DECLARE @MaPhong INT, @GiaPhong DECIMAL(18,0);
    SELECT @MaPhong = MaPhong FROM HopDong WHERE MaHopDong = @MaHopDong;
    SELECT @GiaPhong = GiaPhong FROM Phong WHERE MaPhong = @MaPhong;
 
    DECLARE @DienCu INT, @DienMoi INT, @NuocCu INT, @NuocMoi INT;
    SELECT @DienCu=DienCu, @DienMoi=DienMoi, @NuocCu=NuocCu, @NuocMoi=NuocMoi
    FROM ChiSoDienNuoc
    WHERE MaPhong=@MaPhong AND Thang=@Thang AND Nam=@Nam;
 
    DECLARE @TienDien DECIMAL(18,0) = ISNULL((@DienMoi-@DienCu)*@DonGiaDien, 0);
    DECLARE @TienNuoc DECIMAL(18,0) = ISNULL((@NuocMoi-@NuocCu)*@DonGiaNuoc, 0);
 
    -- Dịch vụ khác (Internet, gửi xe…)
    DECLARE @TienDichVu DECIMAL(18,0) = 0;
    SELECT @TienDichVu = ISNULL(SUM(gd.DonGia),0)
    FROM DichVu dv
    INNER JOIN GiaDichVu gd ON dv.MaDichVu = gd.MaDichVu
    WHERE dv.TrangThai = 1
      AND dv.MaDichVu NOT IN (1,2)   -- bỏ điện, nước (đã tính riêng)
      AND gd.NgayApDung = (SELECT MAX(NgayApDung) FROM GiaDichVu WHERE MaDichVu = dv.MaDichVu);
 
    DECLARE @TongTien DECIMAL(18,0) = @GiaPhong + @TienDien + @TienNuoc + @TienDichVu;
 
    INSERT INTO HoaDon(MaHopDong, Thang, Nam, TongTien, TrangThai)
    VALUES (@MaHopDong, @Thang, @Nam, @TongTien, N'Chưa thanh toán');
 
    SELECT SCOPE_IDENTITY() AS MaHoaDonMoi, @TongTien AS TongTien;
END
GO