namespace StudentManagement.Modules.Reports.Dtos;

// DTO tạm cho module Reports. Tham khảo mẫu Modules/Academic/Dtos/KhoaDtos.cs.

// TODO RPT-01: dòng thống kê số lượng theo nhóm (khoa/ngành/trạng thái).
public record ThongKeItemDto(string Nhom, int SoLuong);
