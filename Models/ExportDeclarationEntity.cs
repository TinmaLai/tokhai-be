using System.ComponentModel.DataAnnotations;

namespace z76_backend.Models
{
    [TableName("export_declaration")]
    public class ExportDeclarationEntity
    {
        [Key]
        public Guid id { get; set; } // Khóa chính

        /// <summary>
        /// STT
        /// </summary>
        public int stt { get; set; }

        /// <summary>
        /// Số tờ khai
        /// </summary>
        public string so_to_khai { get; set; }

        /// <summary>
        /// Số tờ khai đầu tiên
        /// </summary>
        public string so_to_khai_dau_tien { get; set; }

        /// <summary>
        /// Số nhánh
        /// </summary>
        public string so_nhanh { get; set; }

        /// <summary>
        /// Số tờ khai tạm nhập tái xuất tương ứng
        /// </summary>
        public int tong_so_to_khai_tam_nhap_tai_xuat_tuong_ung { get; set; }

        /// <summary>
        /// (Không có mô tả)
        /// </summary>
        public string so_to_khai_tam_nhap_tai_xuat_tuong_ung { get; set; }

        /// <summary>
        /// Mã loại hình
        /// </summary>
        public string ma_loai_hinh { get; set; }

        /// <summary>
        /// Mã phân loại hàng hóa
        /// </summary>
        public string ma_phan_loai_hang_hoa { get; set; }

        /// <summary>
        /// Cơ quan Hải quan
        /// </summary>
        public string co_quan_hai_quan { get; set; }

        /// <summary>
        /// Mã bộ phận xử lý tờ khai
        /// </summary>
        public string ma_bo_phan_xu_ly_to_khai { get; set; }

        /// <summary>
        /// Thời hạn tái nhập khẩu
        /// </summary>
        public DateTime thoi_han_tai_nhap_khau { get; set; }

        /// <summary>
        /// Mã hiệu phương thức vận chuyển
        /// </summary>
        public string ma_hieu_phuong_thuc_van_chuyen { get; set; }

        /// <summary>
        /// Ngày khai báo (dự kiến)
        /// </summary>
        public DateTime ngay_khai_bao_du_kien { get; set; }

        /// <summary>
        /// Mã số thuế người xuất khẩu
        /// </summary>
        public string ma_so_thue_nguoi_xuat_khau { get; set; }

        /// <summary>
        /// Tên người xuất khẩu
        /// </summary>
        public string ten_nguoi_xuat_khau { get; set; }

        /// <summary>
        /// Mã bưu chính người xuất khẩu
        /// </summary>
        public string ma_buu_chinh_nguoi_xuat_khau { get; set; }

        /// <summary>
        /// Địa chỉ người xuất khẩu
        /// </summary>
        public string dia_chi_nguoi_xuat_khau { get; set; }

        /// <summary>
        /// Điện thoại người xuất khẩu
        /// </summary>
        public string dien_thoai_nguoi_xuat_khau { get; set; }

        /// <summary>
        /// Mã người ủy thác xuất khẩu
        /// </summary>
        public string ma_nguoi_uy_thac_xuat_khau { get; set; }

        /// <summary>
        /// Tên người ủy thác xuất khẩu
        /// </summary>
        public string ten_nguoi_uy_thac_xuat_khau { get; set; }

        /// <summary>
        /// Mã người nhập khẩu
        /// </summary>
        public string ma_nguoi_nhap_khau { get; set; }

        /// <summary>
        /// Tên người nhập khẩu
        /// </summary>
        public string ten_nguoi_nhap_khau { get; set; }

        /// <summary>
        /// Mã bưu chính người nhập khẩu
        /// </summary>
        public string ma_buu_chinh_nguoi_nhap_khau { get; set; }

        /// <summary>
        /// Địa chỉ người nhập khẩu
        /// </summary>
        public string dia_chi_nguoi_nhap_khau1 { get; set; }

        /// <summary>
        /// (Không có mô tả)
        /// </summary>
        public string dia_chi_nguoi_nhap_khau2 { get; set; }

        /// <summary>
        /// (Không có mô tả)
        /// </summary>
        public string dia_chi_nguoi_nhap_khau3 { get; set; }

        /// <summary>
        /// (Không có mô tả)
        /// </summary>
        public string dia_chi_nguoi_nhap_khau4 { get; set; }

        /// <summary>
        /// Mã nước người nhập khẩu
        /// </summary>
        public string ma_nuoc_nguoi_nhap_khau { get; set; }

        /// <summary>
        /// Mã người khai Hải quan
        /// </summary>
        public string ma_nguoi_khai_hai_quan { get; set; }

        /// <summary>
        /// Số vận đơn
        /// </summary>
        public string so_van_don { get; set; }

        /// <summary>
        /// Số lượng kiện
        /// </summary>
        public int so_luong_kien { get; set; }

        /// <summary>
        /// Loại kiện hàng
        /// </summary>
        public string loai_kien_hang { get; set; }

        /// <summary>
        /// Tổng trọng lượng hàng (Gross)
        /// </summary>
        public decimal tong_trong_luong_hang_gross { get; set; }

        /// <summary>
        /// Đơn vị trọng lượng
        /// </summary>
        public string don_vi_trong_luong { get; set; }

        /// <summary>
        /// Mã địa điểm lưu kho hàng chờ thông quan dự kiến
        /// </summary>
        public string ma_dia_diem_luu_kho_hang_cho_thong_quan_du_kien { get; set; }

        /// <summary>
        /// Địa điểm nhận hàng cuối cùng
        /// </summary>
        public string dia_diem_nhan_hang_cuoi_cung { get; set; }

        /// <summary>
        /// Địa điểm xếp hàng
        /// </summary>
        public string dia_diem_xep_hang { get; set; }

        /// <summary>
        /// Mã Phương tiện vận chuyển
        /// </summary>
        public string ma_phuong_tien_van_chuyen { get; set; }

        /// <summary>
        /// Ngày hàng đi dự kiến
        /// </summary>
        public DateTime ngay_hang_di_du_kien { get; set; }

        /// <summary>
        /// Ký hiệu và số hiệu
        /// </summary>
        public string ky_hieu_va_so_hieu { get; set; }

        /// <summary>
        /// Số hợp đồng
        /// </summary>
        public string so_hop_dong { get; set; }

        /// <summary>
        /// Ngày hợp đồng
        /// </summary>
        public DateTime ngay_hop_dong { get; set; }

        /// <summary>
        /// Ngày hết hạn
        /// </summary>
        public DateTime ngay_het_han { get; set; }

        /// <summary>
        /// Giấy phép xuất khẩu:
        /// </summary>
        public string giay_phep_xuat_khau1 { get; set; }

        /// <summary>
        /// (Không có mô tả)
        /// </summary>
        public string giay_phep_xuat_khau2 { get; set; }

        /// <summary>
        /// (Không có mô tả)
        /// </summary>
        public string giay_phep_xuat_khau3 { get; set; }

        /// <summary>
        /// (Không có mô tả)
        /// </summary>
        public string giay_phep_xuat_khau4 { get; set; }

        /// <summary>
        /// (Không có mô tả)
        /// </summary>
        public string giay_phep_xuat_khau5 { get; set; }

        /// <summary>
        /// Phân loại hình thức hóa đơn
        /// </summary>
        public string phan_loai_hinh_thuc_hoa_don { get; set; }

        /// <summary>
        /// Số tiếp nhận hóa đơn điện tử
        /// </summary>
        public string so_tiep_nhan_hoa_don_dien_tu { get; set; }

        /// <summary>
        /// Số hóa đơn
        /// </summary>
        public string so_hoa_don { get; set; }

        /// <summary>
        /// Ngày phát hành
        /// </summary>
        public DateTime ngay_phat_hanh { get; set; }

        /// <summary>
        /// Phương thức thanh toán
        /// </summary>
        public string phuong_thuc_thanh_toan { get; set; }

        /// <summary>
        /// Mã phân loại giá hóa đơn
        /// </summary>
        public string ma_phan_loai_gia_hoa_don { get; set; }

        /// <summary>
        /// Điều kiện giá hóa đơn
        /// </summary>
        public string dieu_kien_gia_hoa_don { get; set; }

        /// <summary>
        /// Tổng trị giá hóa đơn
        /// </summary>
        public decimal tong_tri_gia_hoa_don { get; set; }

        /// <summary>
        /// Mã đồng tiền của hóa đơn
        /// </summary>
        public string ma_dong_tien_cua_hoa_don { get; set; }

        /// <summary>
        /// Trị giá tính thuế:
        /// </summary>
        public decimal tri_gia_tinh_thue { get; set; }

        /// <summary>
        /// Mã đồng tiền trị giá tính thuế:
        /// </summary>
        public string ma_dong_tien_tri_gia_tinh_thue { get; set; }

        /// <summary>
        /// Phân loại không cần quy đổi VNĐ
        /// </summary>
        public string phan_loai_khong_can_quy_doi_vnd { get; set; }

        /// <summary>
        /// Tổng hệ số phân bổ trị giá tính thuế
        /// </summary>
        public decimal tong_he_so_phan_bo_tri_gia_tinh_thue { get; set; }

        /// <summary>
        /// Người nộp thuế
        /// </summary>
        public string nguoi_nop_thue { get; set; }

        /// <summary>
        /// Mã ngân hàng trả thuế thay
        /// </summary>
        public string ma_ngan_hang_tra_thue_thay { get; set; }

        /// <summary>
        /// Năm phát hành hạn mức
        /// </summary>
        public int nam_phat_hanh_han_muc { get; set; }

        /// <summary>
        /// Ký hiệu chứng từ hạn mức
        /// </summary>
        public string ky_hieu_chung_tu_han_muc { get; set; }

        /// <summary>
        /// Số chứng từ hạn mức
        /// </summary>
        public string so_chung_tu_han_muc { get; set; }

        /// <summary>
        /// Mã xác định thời hạn nộp thuế
        /// </summary>
        public string ma_xac_dinh_thoi_han_nop_thue { get; set; }

        /// <summary>
        /// Mã ngân hàng bảo lãnh
        /// </summary>
        public string ma_ngan_hang_bao_lanh { get; set; }

        /// <summary>
        /// Năm phát hành bảo lãnh
        /// </summary>
        public int nam_phat_hanh_bao_lanh { get; set; }

        /// <summary>
        /// Ký hiệu chứng từ bảo lãnh
        /// </summary>
        public string ky_hieu_chung_tu_bao_lanh { get; set; }

        /// <summary>
        /// Số chứng từ bảo lãnh
        /// </summary>
        public string so_chung_tu_bao_lanh { get; set; }

        /// <summary>
        /// Phân loại đính kèm
        /// </summary>
        public string phan_loai_dinh_kem1 { get; set; }

        /// <summary>
        /// (Không có mô tả)
        /// </summary>
        public string phan_loai_dinh_kem2 { get; set; }

        /// <summary>
        /// (Không có mô tả)
        /// </summary>
        public string phan_loai_dinh_kem3 { get; set; }

        /// <summary>
        /// Số đính kèm
        /// </summary>
        public int so_dinh_kem1 { get; set; }

        /// <summary>
        /// (Không có mô tả)
        /// </summary>
        public int so_dinh_kem2 { get; set; }

        /// <summary>
        /// (Không có mô tả)
        /// </summary>
        public int so_dinh_kem3 { get; set; }

        /// <summary>
        /// Ngày khởi hành vận chuyển
        /// </summary>
        public DateTime ngay_khoi_hanh_van_chuyen { get; set; }

        /// <summary>
        /// Mã địa điểm (Thông tin trung chuyển) 1
        /// </summary>
        public string ma_dia_diem_thong_tin_trung_chuyen_1 { get; set; }

        /// <summary>
        /// Ngày đến (Thông tin trung chuyển) 1
        /// </summary>
        public DateTime ngay_den_thong_tin_trung_chuyen_1 { get; set; }

        /// <summary>
        /// Ngày khởi hành (Thông tin trung chuyển) 1
        /// </summary>
        public DateTime ngay_khoi_hanh_thong_tin_trung_chuyen_1 { get; set; }

        /// <summary>
        /// Mã địa điểm (Thông tin trung chuyển) 2
        /// </summary>
        public string ma_dia_diem_thong_tin_trung_chuyen_2 { get; set; }

        /// <summary>
        /// Ngày đến (Thông tin trung chuyển) 2
        /// </summary>
        public DateTime ngay_den_thong_tin_trung_chuyen_2 { get; set; }

        /// <summary>
        /// Ngày khởi hành (Thông tin trung chuyển) 2
        /// </summary>
        public DateTime ngay_khoi_hanh_thong_tin_trung_chuyen_2 { get; set; }

        /// <summary>
        /// Mã địa điểm (Thông tin trung chuyển) 3
        /// </summary>
        public string ma_dia_diem_thong_tin_trung_chuyen_3 { get; set; }

        /// <summary>
        /// Ngày đến (Thông tin trung chuyển) 3
        /// </summary>
        public DateTime ngay_den_thong_tin_trung_chuyen_3 { get; set; }

        /// <summary>
        /// Ngày khởi hành (Thông tin trung chuyển) 3
        /// </summary>
        public DateTime ngay_khoi_hanh_thong_tin_trung_chuyen_3 { get; set; }

        /// <summary>
        /// Mã địa điểm (Địa điểm đích cho thuê vận chuyển bảo thuế)
        /// </summary>
        public string ma_dia_diem_dia_diem_dich_cho_thue_van_chuyen_bao_thue { get; set; }

        /// <summary>
        /// Ngày đến (Địa điểm đích cho thuê vận chuyển bảo thuế)
        /// </summary>
        public DateTime ngay_den_dia_diem_dich_cho_thue_van_chuyen_bao_thue { get; set; }

        /// <summary>
        /// Phần ghi chú
        /// </summary>
        public string phan_ghi_chu { get; set; }

        /// <summary>
        /// Số quản lý của nội bộ doanh nghiệp
        /// </summary>
        public string so_quan_ly_cua_noi_bo_doanh_nghiep { get; set; }

        /// <summary>
        /// Mã Địa điểm xếp hàng lên xe chở hàng 1
        /// </summary>
        public string ma_dia_diem_xep_hang_len_xe_cho_hang_1 { get; set; }

        /// <summary>
        /// Mã Địa điểm xếp hàng lên xe chở hàng 2
        /// </summary>
        public string ma_dia_diem_xep_hang_len_xe_cho_hang_2 { get; set; }

        /// <summary>
        /// Mã Địa điểm xếp hàng lên xe chở hàng 3
        /// </summary>
        public string ma_dia_diem_xep_hang_len_xe_cho_hang_3 { get; set; }

        /// <summary>
        /// Mã Địa điểm xếp hàng lên xe chở hàng 4
        /// </summary>
        public string ma_dia_diem_xep_hang_len_xe_cho_hang_4 { get; set; }

        /// <summary>
        /// Mã Địa điểm xếp hàng lên xe chở hàng 5
        /// </summary>
        public string ma_dia_diem_xep_hang_len_xe_cho_hang_5 { get; set; }

        /// <summary>
        /// Tên
        /// </summary>
        public string ten_dia_diem_xep_hang_len_xe { get; set; }

        /// <summary>
        /// Địa chỉ
        /// </summary>
        public string dia_chi_dia_diem_xep_hang_len_xe { get; set; }

        /// <summary>
        /// Số Container 1
        /// </summary>
        public string so_container_1 { get; set; }

        /// <summary>
        /// Số Container 2
        /// </summary>
        public string so_container_2 { get; set; }

        /// <summary>
        /// Số Container 3
        /// </summary>
        public string so_container_3 { get; set; }

        /// <summary>
        /// Số Container 4
        /// </summary>
        public string so_container_4 { get; set; }

        /// <summary>
        /// Số Container 5
        /// </summary>
        public string so_container_5 { get; set; }

        /// <summary>
        /// Số Container 6
        /// </summary>
        public string so_container_6 { get; set; }

        /// <summary>
        /// Số Container 7
        /// </summary>
        public string so_container_7 { get; set; }

        /// <summary>
        /// Số Container 8
        /// </summary>
        public string so_container_8 { get; set; }

        /// <summary>
        /// Số Container 9
        /// </summary>
        public string so_container_9 { get; set; }

        /// <summary>
        /// Số Container 10
        /// </summary>
        public string so_container_10 { get; set; }
    }
}
