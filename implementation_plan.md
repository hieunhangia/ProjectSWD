# Kế hoạch thực hiện: CRUD Nhân viên (Staff CRUD)

Yêu cầu là xây dựng chức năng CRUD cho Staff (Nhân viên) dựa trên cấu trúc dữ liệu hiện tại của project. Chức năng này sẽ được quản lý bởi tài khoản Admin.

## Xem xét từ User (User Review Required)

> [!IMPORTANT]
> - Hệ thống sử dụng ASP.NET Core Identity để quản lý tài khoản. Khi thêm/sửa/xóa một Staff, chúng ta cần đồng bộ giữa bảng `AspNetUsers` (thông qua `UserManager<IdentityUser>`) và bảng `Staffs` trong CSDL.
> - Khi xóa một Staff, các đơn hàng (`Orders`) đang được quản lý bởi Staff đó sẽ tự động chuyển `StaffId` thành `null` (đã được cấu hình `OnDelete(DeleteBehavior.SetNull)` trong `ApplicationDbContext.cs`). Do đó việc xóa Staff là an toàn và không gây lỗi dữ liệu.

## Các thay đổi đề xuất

---

### [Services]

Chúng ta sẽ tạo một dịch vụ mới để xử lý các nghiệp vụ liên quan đến Staff bao gồm: Lấy danh sách, tìm kiếm, lấy chi tiết, tạo mới, cập nhật và xóa Staff (bao gồm cả tài khoản Identity tương ứng).

#### [NEW] [StaffService.cs](file:///e:/SWD392/ProjectSWD/ProjectSWD/Services/Admin/StaffService.cs)
- Chứa các phương thức:
  - `GetAllAsync()`: Lấy danh sách tất cả các nhân viên (kèm theo thông tin tài khoản IdentityUser).
  - `GetByIdAsync(string id)`: Lấy thông tin chi tiết của một nhân viên bằng Id.
  - `CreateAsync(Staff staff, string password)`: Tạo mới tài khoản IdentityUser, gán vai trò `Staff`, và thêm bản ghi vào bảng `Staffs`.
  - `UpdateAsync(Staff staff, string? newPassword)`: Cập nhật thông tin FullName, Phone, Email (cập nhật cả IdentityUser) và đổi mật khẩu mới nếu có yêu cầu.
  - `DeleteAsync(string id)`: Xóa bản ghi trong bảng `Staffs` và tài khoản IdentityUser tương ứng.

---

### [Configuration / Registry]

#### [MODIFY] [Program.cs](file:///e:/SWD392/ProjectSWD/ProjectSWD/Program.cs)
- Đăng ký `StaffService` vào Dependency Injection container làm Scoped service:
  ```csharp
  builder.Services.AddScoped<ProjectSWD.Services.Admin.StaffService>();
  ```

---

### [Razor Pages (Admin/Staff)]

Tạo thư mục mới `Pages/Admin/Staff` để chứa giao diện CRUD.

#### [NEW] [Index.cshtml](file:///e:/SWD392/ProjectSWD/ProjectSWD/Pages/Admin/Staff/Index.cshtml)
- Trang hiển thị danh sách nhân viên.
- Chức năng tìm kiếm theo Tên, Email hoặc Số điện thoại.
- Nút Thêm mới dẫn đến trang `ConfigStaff`.
- Nút Sửa dẫn đến trang `ConfigStaff` kèm ID.
- Nút Xóa mở modal xác nhận để xóa nhân viên thông qua POST handler.

#### [NEW] [Index.cshtml.cs](file:///e:/SWD392/ProjectSWD/ProjectSWD/Pages/Admin/Staff/Index.cshtml.cs)
- PageModel kế thừa `PageModel` với bộ lọc `[Authorize(Roles = "Admin")]`.
- Xử lý tìm kiếm trong phương thức `OnGetAsync()`.
- Xử lý xóa nhân viên trong `OnPostDeleteAsync(string id)`.

#### [NEW] [ConfigStaff.cshtml](file:///e:/SWD392/ProjectSWD/ProjectSWD/Pages/Admin/Staff/ConfigStaff.cshtml)
- Giao diện form dùng chung cho cả tạo mới và chỉnh sửa nhân viên.
- Bao gồm các trường nhập: Họ tên, Email, Số điện thoại, và Mật khẩu.
- Đối với trường hợp Tạo mới: Mật khẩu là bắt buộc.
- Đối với trường hợp Chỉnh sửa: Mật khẩu là không bắt buộc (chỉ nhập khi muốn đổi mật khẩu mới).

#### [NEW] [ConfigStaff.cshtml.cs](file:///e:/SWD392/ProjectSWD/ProjectSWD/Pages/Admin/Staff/ConfigStaff.cshtml.cs)
- PageModel quản lý logic tạo mới và cập nhật.
- Ràng buộc dữ liệu (Validation) sử dụng DataAnnotations (Email hợp lệ, Số điện thoại hợp lệ, độ dài tối đa,...).
- Gọi `StaffService` để thực hiện lưu thay đổi và chuyển hướng về danh sách nhân viên kèm thông báo thành công.

---

### [Layout / Navigation]

#### [MODIFY] [_Layout.cshtml](file:///e:/SWD392/ProjectSWD/ProjectSWD/Pages/Shared/_Layout.cshtml)
- Thêm liên kết quản lý nhân viên vào menu dành cho Admin:
  ```html
  <li class="nav-item">
      <a class="nav-link text-dark" href="/Admin/Staff">Quản lý Nhân Viên</a>
  </li>
  ```

## Kế hoạch kiểm thử & Xác minh (Verification Plan)

### Kiểm thử thủ công (Manual Verification)
1. **Kiểm tra phân quyền**: Đăng nhập bằng tài khoản không phải Admin (Staff hoặc Customer) và truy cập `/Admin/Staff`, hệ thống phải từ chối quyền truy cập (trả về 403 hoặc chuyển hướng đăng nhập).
2. **Xem danh sách**: Đăng nhập bằng tài khoản Admin (`admin@bakinghouse.com` / `Admin@123`), truy cập trang Quản lý Nhân Viên để xem danh sách nhân viên đã được seed sẵn.
3. **Tìm kiếm**: Nhập từ khóa tìm kiếm để lọc nhân viên theo tên, email, hoặc số điện thoại.
4. **Tạo mới Staff**:
   - Thử tạo mới với các dữ liệu lỗi (để trống mật khẩu, sai định dạng email) -> hiển thị thông báo lỗi.
   - Thử tạo mới với email đã tồn tại -> hiển thị lỗi.
   - Tạo mới thành công -> kiểm tra xem tài khoản mới có thể đăng nhập được không và có đúng quyền Staff không.
5. **Cập nhật Staff**:
   - Sửa thông tin Họ tên, Số điện thoại của Staff và lưu -> kiểm tra thông tin được cập nhật.
   - Đổi mật khẩu mới cho Staff -> thử đăng nhập lại với mật khẩu mới.
6. **Xóa Staff**:
   - Xóa một Staff và xác nhận -> kiểm tra tài khoản biến mất khỏi danh sách và không thể đăng nhập được nữa.
