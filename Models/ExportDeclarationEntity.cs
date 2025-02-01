using System.ComponentModel.DataAnnotations;

namespace z76_backend.Models
{
    [TableName("export_declaration")]
    public class ExportDeclarationEntity
    {
        [Key]
        public Guid id { get; set; } // Khóa chính

        public int? stt { get; set; } // Số thứ tự
        public string? so_to_khai { get; set; } // Số tờ khai
        public DateTime? ngay_dang_ky { get; set; } // Ngày đăng ký
        public string? ma_hq { get; set; } // Mã HQ
        public string? ma_lh { get; set; } // Mã LH
        public string? ten_doi_tac { get; set; } // Tên đối tác
        public string? ma_dai_ly_hq { get; set; } // Mã đại lý HQ
        public string? phan_loai_ca_nhan_to_chuc { get; set; } // Phân loại cá nhân/tổ chức
        public string? ma_bo_phan_xu_ly_tk { get; set; } // Mã bộ phận xử lý tờ khai
        public string? ma_hieu_phuong_thuc_van_chuyen { get; set; } // Mã hiệu phương thức vận chuyển
        public string? van_don { get; set; } // Vận đơn
        public int? so_luong_kien { get; set; } // Số lượng kiện
        public decimal? tong_trong_luong_hang_gross { get; set; } // Tổng trọng lượng hàng (Gross)
        public string? ma_dia_diem_luu_kho { get; set; } // Mã địa điểm lưu kho
        public string? ten_dia_diem_luu_kho { get; set; } // Tên địa điểm lưu kho
        public string? ma_phuong_tien_van_chuyen { get; set; } // Mã phương tiện vận chuyển
        public string? ten_phuong_tien_van_chuyen { get; set; } // Tên phương tiện vận chuyển
        public DateTime? ngay_den { get; set; } // Ngày đến
        public string? ma_dia_diem_xep_hang { get; set; } // Mã địa điểm xếp hàng
        public string? ten_dia_diem_xep_hang { get; set; } // Tên địa điểm xếp hàng
        public string? ma_dia_diem_do_hang { get; set; } // Mã địa điểm dỡ hàng
        public string? ten_dia_diem_do_hang { get; set; } // Tên địa điểm dỡ hàng
        public int? so_luong_cont { get; set; } // Số lượng container
        public string? so_giay_phep { get; set; } // Số giấy phép
        public string? so_hd_tm { get; set; } // Số HĐTM
        public DateTime? ngay_hd_tm { get; set; } // Ngày HĐTM
        public decimal? tong_tri_gia_hd { get; set; } // Tổng trị giá hợp đồng
        public string? phuong_thuc_thanh_toan { get; set; } // Phương thức thanh toán
        public string? dieu_kien_gia_hoa_don { get; set; } // Điều kiện giá hóa đơn
        public string? ngoai_te_hoa_don { get; set; } // Ngoại tệ hóa đơn
        public decimal? ty_gia_vnd { get; set; } // Tỷ giá VNĐ
        public decimal? phi_bh { get; set; } // Phí bảo hiểm
        public decimal? phi_vc { get; set; } // Phí vận chuyển
        public string? nguoi_nop_thue { get; set; } // Người nộp thuế
        public decimal? tri_gia_kb { get; set; } // Trị giá không bao gồm thuế
        public decimal? tong_tri_gia_tt { get; set; } // Tổng trị giá tính thuế
        public decimal? tong_tien_thue { get; set; } // Tổng tiền thuế
        public string? ma_ly_do_de_nghi_bp { get; set; } // Mã lý do đề nghị bảo lãnh
        public string? ma_ngan_hang_tra_thue { get; set; } // Mã ngân hàng trả thuế
        public int? nam_phat_hanh_han_muc { get; set; } // Năm phát hành hạn mức
        public string? ky_hieu_ct_han_muc { get; set; } // Ký hiệu CT hạn mức
        public string? so_ct_han_muc { get; set; } // Số CT hạn mức
        public string? ma_xac_dinh_thoi_han_nop_thue { get; set; } // Mã xác định thời hạn nộp thuế
        public string? ma_ngan_hang_bao_lanh { get; set; } // Mã ngân hàng bảo lãnh
        public int? nam_phat_hanh_bao_lanh { get; set; } // Năm phát hành bảo lãnh
        public string? ky_hieu_ct_bao_lanh { get; set; } // Ký hiệu CT bảo lãnh
        public string? so_hieu_ct_bao_lanh { get; set; } // Số hiệu CT bảo lãnh
        public string? so_hd { get; set; } // Số hợp đồng
        public DateTime? ngay_hd { get; set; } // Ngày hợp đồng
        public DateTime? ngay_hh_hd { get; set; } // Ngày hết hạn hợp đồng
        public int? trang_thai { get; set; } // Trạng thái
        public string? phan_luong { get; set; } // Phân luồng
        public string? loai_to_khai_nhap_xuat { get; set; } // Loại tờ khai nhập xuất
        public string? ten_don_vi_xuat_nhap_khau { get; set; } // Tên đơn vị xuất/nhập khẩu
        public string? loai_hinh_to_khai { get; set; } // Loại hình tờ khai
        public string? dia_chi_dn_xuat_nhap_khau { get; set; } // Địa chỉ doanh nghiệp xuất/nhập khẩu
        public string? ma_nuoc_xuat_khau { get; set; } // Mã nước xuất khẩu
        public string? loai_kien { get; set; } // Loại kiện
        public string? don_vi_tinh_trong_luong { get; set; } // Đơn vị tính trọng lượng
        public string? hoa_don_thuong_mai { get; set; } // Hóa đơn thương mại
        public string? ma_phan_loai_hoa_don { get; set; } // Mã phân loại hóa đơn
        public string? ma_phan_loai_phi_van_chuyen { get; set; } // Mã phân loại phí vận chuyển
        public string? ma_phan_loai_phi_bao_hiem { get; set; } // Mã phân loại phí bảo hiểm
        public string? ma_tien_phi_van_chuyen { get; set; } // Mã tiền phí vận chuyển
        public string? ma_tien_phi_bao_hiem { get; set; } // Mã tiền phí bảo hiểm
        public string? dia_chi_nguoi_xuat_nhap_khau_1 { get; set; } // Địa chỉ người xuất/nhập khẩu 1
        public string? ghi_chu { get; set; } // Ghi chú
        public string? ma_phan_loai_hinh_thuc_hoa_don { get; set; } // Mã phân loại hình thức hóa đơn
        public string? dia_diem_dich_cho_van_chuyen_bao_thue { get; set; } // Địa điểm đích cho vận chuyển báo thuế
        public DateTime? ngay_du_kien_den_dia_diem_dich { get; set; } // Ngày dự kiến đến địa điểm đích
        public DateTime? ngay_khoi_hanh_van_chuyen { get; set; } // Ngày khởi hành vận chuyển
        public string? ma_phan_loai_to_khai_tri_gia { get; set; } // Mã phân loại tờ khai trị giá
        public string? dia_chi_nguoi_xuat_nhap_khau_2 { get; set; } // Địa chỉ người xuất/nhập khẩu 2
        public string? dia_chi_nguoi_xuat_nhap_khau_3 { get; set; } // Địa chỉ người xuất/nhập khẩu 3
        public string? dia_chi_nguoi_xuat_nhap_khau_4 { get; set; } // Địa chỉ người xuất/nhập khẩu 4
        public string? ma_dong_tien_tri_gia_tinh_thue { get; set; } // Mã đồng tiền trị giá tính thuế
    }
}
