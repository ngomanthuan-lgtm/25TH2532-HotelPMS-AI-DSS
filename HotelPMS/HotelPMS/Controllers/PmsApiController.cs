using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HotelPMS.Models;
using System;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace HotelPMS.Controllers
{
    [ApiController]
    public class PmsApiController : ControllerBase
    {
        private readonly HotelDbContext _context;

        public PmsApiController(HotelDbContext context)
        {
            _context = context;
        }

        // d. REST API Trả về Dữ liệu Chuẩn JSON (Chuẩn hóa 100% C#)
        [HttpGet]
        [Route("api/pms/data")]
        [Route("Booking/GetFullData")]
        public async Task<IActionResult> GetFullData()
        {
            try
            {
                // Tự động nạp dữ liệu mẫu vào SQL Server nếu chưa có
                await SeedDataIfEmptyAsync();

                // Truy vấn danh sách entity từ cơ sở dữ liệu
                var phongsList = await _context.Phongs.AsNoTracking().ToListAsync();
                var loaiPhongsList = await _context.LoaiPhongs.AsNoTracking().ToListAsync();
                var khachHangsList = await _context.KhachHangs.AsNoTracking().ToListAsync();
                var datPhongsList = await _context.DatPhongs.AsNoTracking().ToListAsync();
                var dichVusList = await _context.DichVus.AsNoTracking().ToListAsync();
                var hoaDonsList = await _context.HoaDons.AsNoTracking().ToListAsync();

                // Định dạng chuẩn đối tượng JSON
                var phongs = phongsList.Select(p => new {
                    maPhong = p.MaPhong,
                    soPhong = p.SoPhong,
                    maLoaiPhong = p.MaLoaiPhong,
                    trangThai = p.TrangThai
                }).ToList();

                var loaiPhongs = loaiPhongsList.Select(l => new {
                    maLoaiPhong = l.MaLoaiPhong,
                    tenLoaiPhong = l.TenLoaiPhong,
                    donGiaTheoNgay = l.DonGiaTheoNgay,
                    soNguoiToiDa = l.SoNguoiToiDa
                }).ToList();

                var khachHangs = khachHangsList.Select(k => new {
                    maKhachHang = k.MaKhachHang,
                    hoTen = k.HoTen,
                    soCCCD = k.SoCCCD,
                    soDienThoai = k.SoDienThoai,
                    email = k.Email
                }).ToList();

                var datPhongs = datPhongsList.Select(b => new {
                    maDatPhong = b.MaDatPhong,
                    maKhachHang = b.MaKhachHang,
                    maPhong = b.MaPhong,
                    ngayDat = b.NgayDat.ToString("yyyy-MM-dd"),
                    ngayNhanDuKien = b.NgayNhanDuKien.ToString("yyyy-MM-dd"),
                    ngayTraDuKien = b.NgayTraDuKien.ToString("yyyy-MM-dd"),
                    trangThai = b.TrangThai
                }).ToList();

                var dichVus = dichVusList.Select(d => new {
                    maDichVu = d.MaDichVu,
                    tenDichVu = d.TenDichVu,
                    donGia = d.DonGia
                }).ToList();

                var hoaDons = hoaDonsList.Select(h => new {
                    maHoaDon = h.MaHoaDon,
                    maDatPhong = h.MaDatPhong,
                    ngayLap = h.NgayLap.ToString("yyyy-MM-dd"),
                    tongTienPhong = h.TongTienPhong,
                    tongTienDichVu = h.TongTienDichVu,
                    tongThanhToan = h.TongThanhToan
                }).ToList();

                // Tính toán thống kê & báo cáo
                int tongPhong = phongs.Count;
                int dangO = phongs.Count(p => p.trangThai == "Đang ở");
                double occupancy = tongPhong > 0 ? Math.Round((double)dangO / tongPhong * 100, 1) : 0;
                decimal tongDoanhThu = hoaDons.Sum(h => h.tongThanhToan);

                var baoCaoLoaiPhong = loaiPhongs.Select(lp => {
                    var phongsCuaLoai = phongs.Where(p => p.maLoaiPhong == lp.maLoaiPhong).ToList();
                    int tongP = phongsCuaLoai.Count;
                    int dangOP = phongsCuaLoai.Count(p => p.trangThai == "Đang ở");
                    double occP = tongP > 0 ? Math.Round((double)dangOP / tongP * 100, 1) : 0;

                    var bookingIds = datPhongs.Where(b => phongsCuaLoai.Any(p => p.maPhong == b.maPhong)).Select(b => b.maDatPhong).ToList();
                    decimal dtPhong = hoaDons.Where(h => bookingIds.Contains(h.maDatPhong)).Sum(h => h.tongTienPhong);
                    decimal dtDichVu = hoaDons.Where(h => bookingIds.Contains(h.maDatPhong)).Sum(h => h.tongTienDichVu);

                    return new
                    {
                        tenLoaiPhong = lp.tenLoaiPhong,
                        tongPhong = tongP,
                        dangO = dangOP,
                        occupancy = occP,
                        tongTienPhong = dtPhong,
                        tongTienDichVu = dtDichVu,
                        tongDoanhThu = dtPhong + dtDichVu
                    };
                }).ToList();

                var fullResult = new
                {
                    phongs,
                    loaiPhongs,
                    khachHangs,
                    datPhongs,
                    dichVus,
                    hoaDons,
                    baoCaoLoaiPhong,
                    stats = new { tongPhong, dangO, occupancy, tongDoanhThu }
                };

                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                };

                string jsonString = JsonSerializer.Serialize(fullResult, options);
                return Content(jsonString, "application/json; charset=utf-8", Encoding.UTF8);
            }
            catch (Exception ex)
            {
                var errObj = new { error = true, message = ex.Message };
                string errJson = JsonSerializer.Serialize(errObj);
                return Content(errJson, "application/json; charset=utf-8", Encoding.UTF8);
            }
        }

        private async Task SeedDataIfEmptyAsync()
        {
            try
            {
                if (!await _context.LoaiPhongs.AnyAsync())
                {
                    _context.LoaiPhongs.AddRange(
                        new LoaiPhong { TenLoaiPhong = "Phòng Đơn Standard", DonGiaTheoNgay = 500000, SoNguoiToiDa = 2 },
                        new LoaiPhong { TenLoaiPhong = "Phòng Đôi Deluxe", DonGiaTheoNgay = 800000, SoNguoiToiDa = 4 },
                        new LoaiPhong { TenLoaiPhong = "Phòng VIP Suite", DonGiaTheoNgay = 1500000, SoNguoiToiDa = 4 }
                    );
                    await _context.SaveChangesAsync();
                }

                if (!await _context.Phongs.AnyAsync())
                {
                    _context.Phongs.AddRange(
                        new Phong { SoPhong = "101", MaLoaiPhong = 1, TrangThai = "Trống" },
                        new Phong { SoPhong = "102", MaLoaiPhong = 2, TrangThai = "Đang ở" },
                        new Phong { SoPhong = "103", MaLoaiPhong = 1, TrangThai = "Trống" },
                        new Phong { SoPhong = "201", MaLoaiPhong = 3, TrangThai = "Trống" },
                        new Phong { SoPhong = "202", MaLoaiPhong = 2, TrangThai = "Bảo trì" }
                    );
                    await _context.SaveChangesAsync();
                }

                if (!await _context.KhachHangs.AnyAsync())
                {
                    _context.KhachHangs.AddRange(
                        new KhachHang { HoTen = "Ngô Mẫn Thuận", SoCCCD = "079201998812", SoDienThoai = "0909123456", Email = "thuan.ngo@email.com" },
                        new KhachHang { HoTen = "Trần Văn An", SoCCCD = "079201887766", SoDienThoai = "0912345678", Email = "an.tran@email.com" },
                        new KhachHang { HoTen = "Lê Thị Bích", SoCCCD = "079201776655", SoDienThoai = "0988776655", Email = "bich.le@email.com" }
                    );
                    await _context.SaveChangesAsync();
                }

                if (!await _context.DichVus.AnyAsync())
                {
                    _context.DichVus.AddRange(
                        new DichVu { TenDichVu = "Ăn Sáng Buffet", DonGia = 150000 },
                        new DichVu { TenDichVu = "Giặt ủi quần áo", DonGia = 50000 },
                        new DichVu { TenDichVu = "Xe Đưa Đón Sân Bay", DonGia = 300000 },
                        new DichVu { TenDichVu = "Dịch Vụ Spa / Massage", DonGia = 450000 }
                    );
                    await _context.SaveChangesAsync();
                }

                if (!await _context.DatPhongs.AnyAsync())
                {
                    _context.DatPhongs.AddRange(
                        new DatPhong { MaKhachHang = 1, MaPhong = 1, NgayDat = DateTime.Now.AddDays(-2), NgayNhanDuKien = DateTime.Now.AddDays(-1), NgayTraDuKien = DateTime.Now.AddDays(2), TrangThai = "CheckIn" },
                        new DatPhong { MaKhachHang = 2, MaPhong = 2, NgayDat = DateTime.Now.AddDays(-1), NgayNhanDuKien = DateTime.Now.AddDays(3), NgayTraDuKien = DateTime.Now.AddDays(5), TrangThai = "Đã Xác Nhận" }
                    );
                    await _context.SaveChangesAsync();
                }

                if (!await _context.HoaDons.AnyAsync())
                {
                    _context.HoaDons.Add(
                        new HoaDon { MaDatPhong = 1, NgayLap = DateTime.Now.AddDays(-1), TongTienPhong = 1500000, TongTienDichVu = 300000, TongThanhToan = 1800000 }
                    );
                    await _context.SaveChangesAsync();
                }
            }
            catch { }
        }
    }
}