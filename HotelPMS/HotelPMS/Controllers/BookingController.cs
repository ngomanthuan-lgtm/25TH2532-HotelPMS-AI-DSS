using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HotelPMS.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace HotelPMS.Controllers
{
    public class BookingController : Controller
    {
        private readonly HotelDbContext _context;

        public BookingController(HotelDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            await SeedDefaultDataAsync();
            return View("~/Views/Home/Index.cshtml");
        }

        private async Task SeedDefaultDataAsync()
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
            catch (Exception ex)
            {
                Console.WriteLine("SeedData Error: " + ex.Message);
            }
        }

        private async Task<bool> KiemTraPhongTrong(int maPhong, DateTime ngayNhan, DateTime ngayTra, int? currentBookingId = null)
        {
            var biTrung = await _context.DatPhongs
                .Where(b => b.MaPhong == maPhong && b.TrangThai != "Đã Hủy")
                .Where(b => currentBookingId == null || b.MaDatPhong != currentBookingId)
                .AnyAsync(b => !(ngayTra <= b.NgayNhanDuKien || ngayNhan >= b.NgayTraDuKien));

            return !biTrung;
        }

        [HttpPost]
        public async Task<IActionResult> DatPhongMoi([FromBody] DatPhong datPhong)
        {
            bool conTrong = await KiemTraPhongTrong(datPhong.MaPhong, datPhong.NgayNhanDuKien, datPhong.NgayTraDuKien);
            if (!conTrong)
            {
                return BadRequest(new { message = "Xung đột lịch! Phòng này đã có đơn đặt trong khoảng thời gian trên." });
            }

            datPhong.TrangThai = "Đã Xác Nhận";
            datPhong.NgayDat = DateTime.Now;
            _context.DatPhongs.Add(datPhong);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Đặt phòng mới thành công!" });
        }

        [HttpPost]
        public async Task<IActionResult> CheckIn(int id)
        {
            var booking = await _context.DatPhongs.FindAsync(id);
            if (booking == null) return NotFound();

            booking.TrangThai = "CheckIn";
            var phong = await _context.Phongs.FindAsync(booking.MaPhong);
            if (phong != null) phong.TrangThai = "Đang ở";

            await _context.SaveChangesAsync();
            return Ok(new { message = "Check-in thành công!" });
        }

        [HttpPost]
        public async Task<IActionResult> CheckOut(int id)
        {
            var booking = await _context.DatPhongs.FindAsync(id);
            if (booking == null) return NotFound();

            booking.TrangThai = "CheckOut";
            var phong = await _context.Phongs.FindAsync(booking.MaPhong);
            if (phong != null) phong.TrangThai = "Trống";

            var loaiPhong = await _context.LoaiPhongs.FindAsync(phong?.MaLoaiPhong);
            var khachHang = await _context.KhachHangs.FindAsync(booking.MaKhachHang);

            int soNgay = (int)Math.Max(1, (booking.NgayTraDuKien - booking.NgayNhanDuKien).TotalDays);
            decimal donGia = loaiPhong?.DonGiaTheoNgay ?? 500000;
            decimal tienPhong = soNgay * donGia;
            decimal tienDichVu = 300000;
            decimal tongThanhToan = tienPhong + tienDichVu;

            var hoaDon = new HoaDon
            {
                MaDatPhong = booking.MaDatPhong,
                NgayLap = DateTime.Now,
                TongTienPhong = tienPhong,
                TongTienDichVu = tienDichVu,
                TongThanhToan = tongThanhToan
            };

            _context.HoaDons.Add(hoaDon);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Check-out & Xuất hóa đơn thành công!",
                hoaDon = new
                {
                    maHoaDon = hoaDon.MaHoaDon,
                    tenKhachHang = khachHang?.HoTen ?? "Khách Hàng",
                    soCCCD = khachHang?.SoCCCD ?? "079201998812",
                    soPhong = phong?.SoPhong ?? "101",
                    tenLoaiPhong = loaiPhong?.TenLoaiPhong ?? "Standard",
                    ngayNhan = booking.NgayNhanDuKien.ToString("dd/MM/yyyy"),
                    ngayTra = booking.NgayTraDuKien.ToString("dd/MM/yyyy"),
                    soNgay = soNgay,
                    donGia = donGia,
                    tongTienPhong = tienPhong,
                    tongTienDichVu = tienDichVu,
                    tongThanhToan = tongThanhToan,
                    ngayLap = hoaDon.NgayLap.ToString("dd/MM/yyyy HH:mm")
                }
            });
        }

        [HttpPost]
        public async Task<IActionResult> HuyDatPhong(int id)
        {
            var booking = await _context.DatPhongs.FindAsync(id);
            if (booking == null) return NotFound();

            booking.TrangThai = "Đã Hủy";
            var phong = await _context.Phongs.FindAsync(booking.MaPhong);
            if (phong != null && phong.TrangThai == "Đang ở")
            {
                phong.TrangThai = "Trống";
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Hủy đơn đặt phòng thành công!" });
        }

        [HttpPost]
        public async Task<IActionResult> ThemKhachHang([FromBody] KhachHang khachHang)
        {
            _context.KhachHangs.Add(khachHang);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Thêm khách hàng thành công!" });
        }

        [HttpPost]
        public async Task<IActionResult> CapNhatKhachHang([FromBody] KhachHang khachHang)
        {
            _context.KhachHangs.Update(khachHang);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Cập nhật khách hàng thành công!" });
        }

        [HttpPost]
        public async Task<IActionResult> XoaKhachHang(int id)
        {
            var kh = await _context.KhachHangs.FindAsync(id);
            if (kh != null)
            {
                _context.KhachHangs.Remove(kh);
                await _context.SaveChangesAsync();
            }
            return Ok(new { message = "Xóa khách hàng thành công!" });
        }

        [HttpPost]
        public async Task<IActionResult> ThemPhong([FromBody] Phong phong)
        {
            _context.Phongs.Add(phong);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Thêm phòng mới thành công!" });
        }

        [HttpPost]
        public async Task<IActionResult> CapNhatPhong([FromBody] Phong phong)
        {
            _context.Phongs.Update(phong);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Cập nhật phòng thành công!" });
        }

        [HttpPost]
        public async Task<IActionResult> XoaPhong(int id)
        {
            var p = await _context.Phongs.FindAsync(id);
            if (p != null)
            {
                _context.Phongs.Remove(p);
                await _context.SaveChangesAsync();
            }
            return Ok(new { message = "Xóa phòng thành công!" });
        }

        [HttpPost]
        public async Task<IActionResult> ThemDichVu([FromBody] DichVu dichVu)
        {
            _context.DichVus.Add(dichVu);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Thêm dịch vụ thành công!" });
        }

        [HttpPost]
        public async Task<IActionResult> CapNhatDichVu([FromBody] DichVu dichVu)
        {
            _context.DichVus.Update(dichVu);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Cập nhật dịch vụ thành công!" });
        }

        [HttpPost]
        public async Task<IActionResult> XoaDichVu(int id)
        {
            var dv = await _context.DichVus.FindAsync(id);
            if (dv != null)
            {
                _context.DichVus.Remove(dv);
                await _context.SaveChangesAsync();
            }
            return Ok(new { message = "Xóa dịch vụ thành công!" });
        }
    }
}