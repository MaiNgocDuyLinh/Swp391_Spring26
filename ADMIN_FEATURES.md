# Tài liệu Tính năng Admin - Quản lý Tài khoản

## Tổng quan
Tài liệu này mô tả chi tiết các tính năng quản lý tài khoản đã được thêm vào hệ thống Pet Medical Clinic cho vai trò Admin.

---

## 📋 Danh sách Tính năng

### 1. **Role Guard / Authorization Attribute**
**File:** `Attributes/AuthorizeRoleAttribute.cs`

- Tạo custom attribute để kiểm tra quyền truy cập theo role
- Chỉ cho phép các role được chỉ định truy cập vào các action/controller
- Tự động redirect về trang Login nếu chưa đăng nhập
- Trả về ForbidResult nếu không có quyền truy cập

**Cách sử dụng:**
```csharp
[AuthorizeRole("Admin")]
public class AdminController : Controller
{
    // Tất cả actions trong controller này chỉ Admin mới truy cập được
}
```

---

### 2. **ViewModels cho Account Management**
**File:** `ViewModels/AccountViewModel.cs`

#### 2.1. AccountViewModel
- Hiển thị thông tin tài khoản: UserId, Email, FullName, Phone, RoleName, RoleId, Status, CreatedAt

#### 2.2. AccountListViewModel
- Quản lý danh sách tài khoản với pagination
- Hỗ trợ search, filter theo role và status
- Chứa thông tin phân trang: CurrentPage, TotalPages, TotalRecords, PageSize

#### 2.3. CreateAccountViewModel
- Form tạo tài khoản mới: Email, Password, FullName, Phone, RoleId
- Danh sách roles khả dụng (chỉ Staff và Doctor)

#### 2.4. EditAccountViewModel
- Form sửa tài khoản: UserId, Email (readonly), FullName, Phone, RoleId, Status
- Danh sách tất cả roles để chọn

#### 2.5. RoleOption & RoleFilterOption
- Helper classes cho dropdown và filter

---

### 3. **AdminController**
**File:** `Controllers/AdminController.cs`

Toàn bộ controller được bảo vệ bởi `[AuthorizeRole("Admin")]`

#### 3.1. Index (GET) - Danh sách Tài khoản
**Route:** `/Admin/Index`

**Tính năng:**
- Hiển thị danh sách tất cả tài khoản trong hệ thống
- **Search:** Tìm kiếm theo email, tên, số điện thoại
- **Filter:** 
  - Lọc theo vai trò (Role)
  - Lọc theo trạng thái (Active/Inactive)
- **Pagination:** Phân trang với page size mặc định 10
- Sắp xếp theo ngày tạo (mới nhất trước)
- Hiển thị thông tin: ID, Email, Họ tên, Số điện thoại, Vai trò, Trạng thái, Ngày tạo

**Parameters:**
- `searchTerm` (string, optional): Từ khóa tìm kiếm
- `roleFilter` (string, optional): ID của role để lọc
- `statusFilter` (string, optional): "Active" hoặc "Inactive"
- `page` (int, default: 1): Trang hiện tại
- `pageSize` (int, default: 10): Số lượng record mỗi trang

#### 3.2. Create (GET) - Form Tạo Tài khoản
**Route:** `/Admin/Create`

**Tính năng:**
- Hiển thị form tạo tài khoản mới
- Chỉ cho phép tạo tài khoản với role Staff hoặc Doctor
- Validation đầy đủ các trường

#### 3.3. Create (POST) - Xử lý Tạo Tài khoản
**Route:** `/Admin/Create` (POST)

**Tính năng:**
- Kiểm tra email đã tồn tại chưa
- Kiểm tra role hợp lệ (chỉ Staff hoặc Doctor)
- Tạo tài khoản mới với status "Active"
- Thông báo thành công/lỗi

**Validation:**
- Email: Required, unique
- Password: Required
- FullName: Required
- RoleId: Required, chỉ Staff hoặc Doctor

#### 3.4. Edit (GET) - Form Sửa Tài khoản
**Route:** `/Admin/Edit/{id}`

**Tính năng:**
- Hiển thị form sửa tài khoản
- Không cho phép sửa tài khoản Admin
- Email hiển thị readonly (không thể sửa)
- Cho phép sửa: Họ tên, Số điện thoại, Vai trò, Trạng thái

#### 3.5. Edit (POST) - Xử lý Sửa Tài khoản
**Route:** `/Admin/Edit` (POST)

**Tính năng:**
- Cập nhật vai trò của tài khoản
- Cập nhật thông tin: Họ tên, Số điện thoại, Trạng thái
- Không cho phép sửa tài khoản Admin
- Validation đầy đủ

#### 3.6. ToggleLock (POST) - Khóa/Mở khóa Tài khoản
**Route:** `/Admin/ToggleLock` (POST, AJAX)

**Tính năng:**
- Khóa/Mở khóa tài khoản bằng AJAX
- Không cho phép khóa tài khoản Admin
- Trả về JSON response
- Tự động reload trang sau khi thành công

**Response:**
```json
{
    "success": true,
    "message": "Khóa tài khoản thành công.",
    "newStatus": "Inactive"
}
```

#### 3.7. Details (GET) - Chi tiết Tài khoản
**Route:** `/Admin/Details/{id}`

**Tính năng:**
- Hiển thị chi tiết thông tin tài khoản
- Hiển thị tất cả thông tin: ID, Email, Họ tên, Số điện thoại, Vai trò, Trạng thái, Ngày tạo
- Có nút "Sửa vai trò" và "Khóa/Mở khóa" (nếu không phải Admin)

---

### 4. **Views**

#### 4.1. Index.cshtml - Danh sách Tài khoản
**File:** `Views/Admin/Index.cshtml`

**Giao diện:**
- Hero section với tiêu đề "Quản lý Tài khoản"
- Nút "Tạo tài khoản mới" ở header
- Form search và filter:
  - Input tìm kiếm (email, tên, số điện thoại)
  - Dropdown lọc theo vai trò
  - Dropdown lọc theo trạng thái
- Bảng danh sách với các cột:
  - ID, Email, Họ tên, Số điện thoại, Vai trò (badge), Trạng thái (badge), Ngày tạo, Thao tác
- Các nút thao tác:
  - **Chi tiết** (màu xanh info)
  - **Sửa** (màu xanh primary) - chỉ hiển thị nếu không phải Admin
  - **Khóa/Mở khóa** (màu vàng/xanh) - chỉ hiển thị nếu không phải Admin
- Pagination với navigation
- Hiển thị thông tin phân trang: "Hiển thị X - Y trong tổng số Z tài khoản"
- AJAX script để xử lý khóa/mở khóa tài khoản

#### 4.2. Create.cshtml - Tạo Tài khoản
**File:** `Views/Admin/Create.cshtml`

**Giao diện:**
- Hero section với tiêu đề "Tạo Tài khoản Mới"
- Form với các trường:
  - Email (required)
  - Mật khẩu (required, type password)
  - Họ và tên (required)
  - Số điện thoại (optional)
  - Vai trò (required, dropdown chỉ Staff và Doctor)
- Validation messages
- Nút "Quay lại" và "Tạo tài khoản"
- Client-side validation với jQuery Validation

#### 4.3. Edit.cshtml - Sửa Tài khoản
**File:** `Views/Admin/Edit.cshtml`

**Giao diện:**
- Hero section với tiêu đề "Sửa Tài khoản"
- Form với các trường:
  - Email (readonly, disabled)
  - Họ và tên (required)
  - Số điện thoại (optional)
  - Vai trò (required, dropdown tất cả roles)
  - Trạng thái (required, dropdown Active/Inactive)
- Validation messages
- Nút "Quay lại" và "Lưu thay đổi"
- Client-side validation

#### 4.4. Details.cshtml - Chi tiết Tài khoản
**File:** `Views/Admin/Details.cshtml`

**Giao diện:**
- Hero section với tiêu đề "Chi tiết Tài khoản"
- Hiển thị thông tin dạng definition list:
  - ID, Email, Họ và tên, Số điện thoại, Vai trò (badge), Trạng thái (badge), Ngày tạo
- Các nút thao tác (nếu không phải Admin):
  - "Sửa vai trò" (màu xanh primary)
  - "Khóa/Mở khóa tài khoản" (màu vàng/xanh)
- AJAX script để xử lý khóa/mở khóa

---

### 5. **Menu Navigation**
**File:** `Views/Shared/_Layout.cshtml`

**Thay đổi:**
- Thêm menu "Admin" vào navigation bar (chỉ hiển thị cho Admin)
- Submenu:
  - "Quản lý Tài khoản" → `/Admin/Index`
  - "Tạo Tài khoản" → `/Admin/Create`
- Thêm menu vào dropdown của user (bên phải header):
  - "Quản lý Tài khoản" (icon cog)
  - "Tạo Tài khoản" (icon user-plus)
- Thêm `@RenderSection("Scripts", required: false)` vào cuối layout để hỗ trợ section Scripts từ các view

**Điều kiện hiển thị:**
```csharp
@if (User.Identity.IsAuthenticated && 
     (User.IsInRole("Admin") || 
      User.Claims.Any(c => c.Type == ClaimTypes.Role && c.Value == "Admin")))
```

---

## 🔒 Bảo mật

### 1. Role-based Authorization
- Toàn bộ AdminController được bảo vệ bởi `[AuthorizeRole("Admin")]`
- Chỉ user có role "Admin" mới truy cập được

### 2. Business Rules
- **Không cho phép khóa tài khoản Admin:**
  - Kiểm tra trong ToggleLock action
  - Kiểm tra trong Edit action
- **Không cho phép sửa tài khoản Admin:**
  - Kiểm tra trong Edit action
  - Ẩn nút sửa trong UI nếu là Admin
- **Chỉ tạo tài khoản Staff/Doctor:**
  - Validation trong Create action
  - Dropdown chỉ hiển thị Staff và Doctor

### 3. Validation
- Server-side validation đầy đủ
- Client-side validation với jQuery Validation
- Anti-forgery token cho các POST requests

---

## 📁 Cấu trúc Files đã tạo

```
Group3_SWP391_PetMedical/
├── Attributes/
│   └── AuthorizeRoleAttribute.cs          [MỚI]
├── ViewModels/
│   └── AccountViewModel.cs                [MỚI]
├── Controllers/
│   └── AdminController.cs                 [MỚI]
├── Views/
│   ├── Admin/
│   │   ├── Index.cshtml                    [MỚI]
│   │   ├── Create.cshtml                  [MỚI]
│   │   ├── Edit.cshtml                    [MỚI]
│   │   └── Details.cshtml                 [MỚI]
│   └── Shared/
│       └── _Layout.cshtml                 [SỬA - thêm menu Admin]
└── Views/
    └── _ViewImports.cshtml                [KHÔNG SỬA - chỉ thêm @using trong views]
```

---

## 🎯 Các Route đã thêm

| Route | Method | Mô tả |
|-------|--------|-------|
| `/Admin/Index` | GET | Danh sách tài khoản (có search, filter, pagination) |
| `/Admin/Create` | GET | Form tạo tài khoản mới |
| `/Admin/Create` | POST | Xử lý tạo tài khoản |
| `/Admin/Edit/{id}` | GET | Form sửa tài khoản |
| `/Admin/Edit` | POST | Xử lý cập nhật tài khoản |
| `/Admin/ToggleLock` | POST | Khóa/Mở khóa tài khoản (AJAX) |
| `/Admin/Details/{id}` | GET | Chi tiết tài khoản |

---

## 💡 Cách sử dụng

### 1. Truy cập Admin Panel
- Đăng nhập với tài khoản có role "Admin"
- Menu "Admin" sẽ xuất hiện trong navigation bar
- Hoặc truy cập trực tiếp: `/Admin/Index`

### 2. Xem danh sách tài khoản
- Vào `/Admin/Index`
- Sử dụng search box để tìm kiếm
- Sử dụng filter để lọc theo role hoặc status
- Sử dụng pagination để xem các trang khác

### 3. Tạo tài khoản mới
- Click "Tạo tài khoản mới" hoặc vào `/Admin/Create`
- Điền đầy đủ thông tin
- Chọn role Staff hoặc Doctor
- Click "Tạo tài khoản"

### 4. Sửa tài khoản
- Trong danh sách, click nút "Sửa" (icon edit)
- Hoặc vào trang chi tiết rồi click "Sửa vai trò"
- Thay đổi thông tin cần thiết
- Click "Lưu thay đổi"

### 5. Khóa/Mở khóa tài khoản
- Trong danh sách, click nút khóa/mở khóa (icon lock/unlock)
- Hoặc trong trang chi tiết, click nút tương ứng
- Xác nhận trong popup
- Trang sẽ tự động reload sau khi thành công

### 6. Xem chi tiết
- Click nút "Chi tiết" (icon eye) trong danh sách
- Xem đầy đủ thông tin tài khoản

---

## ⚠️ Lưu ý

1. **Đăng xuất và đăng nhập lại:** Sau khi tạo tài khoản Admin mới hoặc thay đổi role, cần đăng xuất và đăng nhập lại để claims được cập nhật.

2. **Bảo vệ tài khoản Admin:** 
   - Không thể khóa tài khoản Admin
   - Không thể sửa vai trò của tài khoản Admin
   - Các nút thao tác sẽ tự động ẩn đối với tài khoản Admin

3. **Password:** Hiện tại password được lưu plain text. Trong production nên hash password bằng BCrypt hoặc các phương pháp bảo mật khác.

4. **Email unique:** Email phải là duy nhất trong hệ thống.

---

## 📝 Ghi chú kỹ thuật

- **Framework:** ASP.NET Core MVC (.NET 8.0)
- **Database:** SQL Server với Entity Framework Core
- **Authentication:** Cookie-based authentication
- **Authorization:** Custom Role-based với AuthorizeRoleAttribute
- **Frontend:** Bootstrap, jQuery, Font Awesome
- **Validation:** Data Annotations + jQuery Validation

---

**Ngày tạo:** 2025-01-27  
**Người phát triển:** Hưng  
**Phiên bản:** 1.0
