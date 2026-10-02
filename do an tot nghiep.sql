-- SCRIPT KHỞI TẠO CSDL HOTEL PMS (SQL SERVER 2022)
CREATE DATABASE HotelPMS_DB;
GO
USE HotelPMS_DB;
GO

-- Bảng Loại Phòng
CREATE TABLE LoaiPhong (
    MaLoaiPhong INT IDENTITY(1,1) PRIMARY KEY,
    TenLoaiPhong NVARCHAR(100) NOT NULL,
    DonGiaTheoNgay DECIMAL(18,2) NOT NULL,
    SoNguoiToiDa INT NOT NULL
);

-- Bảng Phòng
CREATE TABLE Phong (
    MaPhong INT IDENTITY(1,1) PRIMARY KEY,
    SoPhong NVARCHAR(20) NOT NULL UNIQUE,
    MaLoaiPhong INT FOREIGN KEY REFERENCES LoaiPhong(MaLoaiPhong),
    TrangThai NVARCHAR(50) DEFAULT N'Trống'
);

-- Bảng Khách Hàng (Dữ liệu ẩn danh theo tiêu chí kỹ thuật)
CREATE TABLE KhachHang (
    MaKhachHang INT IDENTITY(1,1) PRIMARY KEY,
    HoTen NVARCHAR(100) NOT NULL,
    SoCCCD NVARCHAR(20) UNIQUE NOT NULL, -- Đã ẩn danh (ví dụ: CUS_99812)
    SoDienThoai NVARCHAR(20),
    Email NVARCHAR(100)
);

-- Bảng Đặt Phòng
CREATE TABLE DatPhong (
    MaDatPhong INT IDENTITY(1,1) PRIMARY KEY,
    MaKhachHang INT FOREIGN KEY REFERENCES KhachHang(MaKhachHang),
    MaPhong INT FOREIGN KEY REFERENCES Phong(MaPhong),
    NgayDat DATETIME DEFAULT GETDATE(),
    NgayNhanDuKien DATETIME NOT NULL,
    NgayTraDuKien DATETIME NOT NULL,
    TrangThai NVARCHAR(50) DEFAULT N'Đã Xác Nhận' -- Đã Xác Nhận, CheckIn, CheckOut, Đã Hủy
);

-- Bảng Dịch Vụ & Hóa Đơn
CREATE TABLE DichVu (
    MaDichVu INT IDENTITY(1,1) PRIMARY KEY,
    TenDichVu NVARCHAR(100) NOT NULL,
    DonGia DECIMAL(18,2) NOT NULL
);

CREATE TABLE HoaDon (
    MaHoaDon INT IDENTITY(1,1) PRIMARY KEY,
    MaDatPhong INT FOREIGN KEY REFERENCES DatPhong(MaDatPhong),
    NgayLap DATETIME DEFAULT GETDATE(),
    TongTienPhong DECIMAL(18,2) NOT NULL,
    TongTienDichVu DECIMAL(18,2) DEFAULT 0,
    TongThanhToan DECIMAL(18,2) NOT NULL
);
GO