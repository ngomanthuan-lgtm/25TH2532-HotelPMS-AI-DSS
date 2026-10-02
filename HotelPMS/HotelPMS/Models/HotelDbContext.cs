using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace HotelPMS.Models
{
    public class HotelDbContext : DbContext
    {
        public HotelDbContext(DbContextOptions<HotelDbContext> options) : base(options) { }

        public DbSet<LoaiPhong> LoaiPhongs { get; set; }
        public DbSet<Phong> Phongs { get; set; }
        public DbSet<KhachHang> KhachHangs { get; set; }
        public DbSet<DatPhong> DatPhongs { get; set; }
        public DbSet<DichVu> DichVus { get; set; }
        public DbSet<HoaDon> HoaDons { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<LoaiPhong>().HasKey(e => e.MaLoaiPhong);
            modelBuilder.Entity<Phong>().HasKey(e => e.MaPhong);
            modelBuilder.Entity<KhachHang>().HasKey(e => e.MaKhachHang);
            modelBuilder.Entity<DatPhong>().HasKey(e => e.MaDatPhong);
            modelBuilder.Entity<DichVu>().HasKey(e => e.MaDichVu);
            modelBuilder.Entity<HoaDon>().HasKey(e => e.MaHoaDon);
        }
    }

    public class LoaiPhong
    {
        [Key]
        public int MaLoaiPhong { get; set; }
        public string TenLoaiPhong { get; set; } = string.Empty;
        public decimal DonGiaTheoNgay { get; set; }
        public int SoNguoiToiDa { get; set; }
    }

    public class Phong
    {
        [Key]
        public int MaPhong { get; set; }
        public string SoPhong { get; set; } = string.Empty;
        public int MaLoaiPhong { get; set; }
        public string TrangThai { get; set; } = "Trống"; // Trống, Đang ở, Bảo trì
    }

    public class KhachHang
    {
        [Key]
        public int MaKhachHang { get; set; }
        public string HoTen { get; set; } = string.Empty;
        public string SoCCCD { get; set; } = string.Empty;
        public string? SoDienThoai { get; set; }
        public string? Email { get; set; }
    }

    public class DatPhong
    {
        [Key]
        public int MaDatPhong { get; set; }
        public int MaKhachHang { get; set; }
        public int MaPhong { get; set; }
        public DateTime NgayDat { get; set; } = DateTime.Now;
        public DateTime NgayNhanDuKien { get; set; }
        public DateTime NgayTraDuKien { get; set; }
        public string TrangThai { get; set; } = "Đã Xác Nhận"; // Đã Xác Nhận, CheckIn, CheckOut, Đã Hủy
    }

    public class DichVu
    {
        [Key]
        public int MaDichVu { get; set; }
        public string TenDichVu { get; set; } = string.Empty;
        public decimal DonGia { get; set; }
    }

    public class HoaDon
    {
        [Key]
        public int MaHoaDon { get; set; }
        public int MaDatPhong { get; set; }
        public DateTime NgayLap { get; set; } = DateTime.Now;
        public decimal TongTienPhong { get; set; }
        public decimal TongTienDichVu { get; set; } = 0;
        public decimal TongThanhToan { get; set; }
    }
}