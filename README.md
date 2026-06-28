# 📝 TodoList App with Google Calendar Sync & Eisenhower Matrix

Ứng dụng quản lý công việc cá nhân (TodoList) được xây dựng trên nền tảng **WPF (Windows Presentation Foundation)** theo mô hình kiến trúc **MVVM** chuẩn, tích hợp hệ thống lưu trữ **SQL Server**, đồng bộ hai chiều thời gian thực với **Google Calendar** và hỗ trợ sắp xếp công việc thông minh theo **Ma trận quyết định Eisenhower**.

---

## 🚀 Tính Năng Nổi Bật

### 1. 🔐 Đăng Nhập & Bảo Mật Đa Người Dùng
- Cho phép nhiều người dùng đăng ký và đăng nhập tài khoản riêng biệt.
- Cơ chế mã hóa một chiều mật khẩu bằng thuật toán **PBKDF2** kết hợp Muối ngẫu nhiên (Salt) và 10,000 vòng lặp vô cùng bảo mật.
- Cô lập hoàn toàn dữ liệu công việc và phiên làm việc giữa các tài khoản khác nhau.

### 2. 🔄 Đồng Bộ Hóa 2 Chiều Google Calendar (Tối Ưu Hiệu Năng)
- Tự động đồng bộ các công việc giữa ứng dụng và Lịch cá nhân của Google sau khi đăng nhập thành công.
- Hỗ trợ thêm/sửa/xóa sự kiện thời gian thực (nền).
- **Thuật toán tối ưu hóa:** Sử dụng cơ chế nạp bộ nhớ đệm `Dictionary` trên RAM để triệt tiêu lỗi nghẽn truy vấn mạng $N+1$ (N+1 Query Issue), giúp tốc độ đồng bộ nhanh gấp nhiều lần.

### 3. 🧩 Ma Trận Quyết Định Eisenhower
- Phân loại trực quan công việc thành 4 ô kinh điển: **P1 (Do)**, **P2 (Schedule)**, **P3 (Delegate)**, **P4 (Eliminate)**.
- **Hộp thư đến (Brain Dump / Inbox):** Ghi nhanh công việc chưa phân loại vào hộp thư và ném nhanh vào các vùng tương ứng sau.
- **Giới hạn tải P1 (WIP Limit = 5):** Cảnh báo quá tải và gợi ý chuyển sang P2 khi ô khẩn cấp P1 vượt quá 5 việc nhằm hạn chế stress.
- **Gợi ý lịch trình P2 (Schedule Suggestions):** Quét phát hiện và tự động gợi ý đặt lịch ngày mai cho các task P2 quan trọng đang bị thiếu hạn chót.

### 4. 📊 Phân Tích Hiệu Suất (Analytics)
- Biểu đồ đo tỷ lệ phân bổ phần trăm công việc hoàn thành trong tuần theo từng vùng ma trận bằng WPF `ProgressBar`.
- Đưa ra lời khuyên cá nhân hóa từ hệ thống chuyên gia (Decision Tree) giúp bạn cân đối thời gian và công việc hiệu quả hơn.

### 5. 📅 Xem Lịch Trình (Agenda & Calendar)
- Giao diện xem lịch trình trực quan theo tuần và tháng giúp người dùng dễ dàng bao quát tiến độ.

---

## 🛠️ Công Nghệ Sử Dụng

- **Framework:** .NET 8.0-windows (WPF)
- **MVVM Helper:** CommunityToolkit.Mvvm (Source Generators cho Properties/Commands)
- **UI & Themes:** Material Design In XAML Toolkit v5.3.2
- **ORM / Database:** Entity Framework Core 8.0 + Microsoft SQL Server Express
- **API Integration:** Google.Apis.Calendar.v3 v1.75.0
- **Unit Testing:** xUnit + Moq + EF Core InMemory Database

---

## 💻 Hướng Dẫn Cài Đặt & Chạy Thử

### Yêu Cầu Hệ Thống:
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) hoặc mới hơn.
- SQL Server Express cục bộ đang hoạt động.
- Một tài khoản Google dùng để đồng bộ.

### Các Bước Thực Hiện:

1. **Tải mã nguồn:**
   ```bash
   git clone <url-cua-repository>
   cd TodoList
   ```

2. **Cấu hình Kết nối Cơ sở dữ liệu (Connection String):**
   Mở file [appsettings.json](file:///c:/Users/huy/source/repos/TodoList/TodoList/appsettings.json) trong thư mục dự án và điều chỉnh chuỗi kết nối SQL Server của bạn:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Data Source=TEN_MAY_TINH\\SQLEXPRESS;Initial Catalog=PersonalTodoList;Trusted_Connection=SSPI;Encrypt=false;TrustServerCertificate=true"
     }
   }
   ```

3. **Cập nhật Cơ sở dữ liệu (Database Migration):**
   Chạy lệnh sau tại thư mục chứa file Solution (`.sln`) để tự động tạo cấu trúc bảng dữ liệu trên SQL Server:
   ```bash
   dotnet ef database update --project TodoList
   ```

4. **Thiết lập Google Calendar API Credentials:**
   - Truy cập Google Cloud Console, tạo một dự án OAuth 2.0 và cấp quyền truy cập dịch vụ Calendar API.
   - Tải file cấu hình xác thực Client ID và Client Secret dưới dạng file JSON và đổi tên thành `credentials.json`.
   - Sao chép file `credentials.json` và lưu vào thư mục: `TodoList/google calendar/credentials.json`.

5. **Build và Chạy ứng dụng:**
   Mở Solution bằng Visual Studio hoặc chạy lệnh:
   ```bash
   dotnet run --project TodoList
   ```

*Ghi chú: Lần đầu tiên bạn nhấn nút **Sync Google Calendar** (biểu tượng Google ở góc phải bên trên), trình duyệt web sẽ tự động bật lên yêu cầu bạn đăng nhập tài khoản Google để cấp quyền truy cập. Sau khi đồng ý, hệ thống sẽ tự động liên kết trơn tru dưới nền.*

---

## 🧪 Hướng Dẫn Chạy Kiểm Thử (Unit Tests)

Dự án có sẵn một bộ Unit Test tự động bao phủ đầy đủ các luồng logic quan trọng (luồng chính, luồng phụ và luồng ngoại lệ) của xác thực băm mật khẩu, CRUD nghiệp vụ Database và logic của ma trận Eisenhower.

Để thực hiện chạy kiểm thử, mở cửa sổ dòng lệnh tại thư mục chứa file Solution và chạy lệnh:
```bash
dotnet test
```

---

## 📂 Cấu Trúc Thư Mục Dự Án

```text
TodoList/
│
├── TodoList/                      # Dự án mã nguồn chính (WPF Application)
│   ├── Converters/                # Bộ chuyển đổi dữ liệu XAML (Value Converters)
│   ├── Helpers/                   # Các thư viện phụ trợ (Mã hóa mật khẩu, v.v.)
│   ├── Models/                    # Các thực thể dữ liệu (User, TaskItem, v.v.)
│   ├── Providers/                 # DbContext kết nối SQL Server (EF Core)
│   ├── Services/                  # Lớp xử lý nghiệp vụ & Google API Sync
│   ├── ViewModels/                # Lớp logic giao diện (Dashboard, Eisenhower, v.v.)
│   ├── Views/                     # Các file giao diện XAML (MainWindow, Dashboard, v.v.)
│   └── google calendar/           # Thư mục lưu credentials.json kết nối lịch Google
│
├── TodoList.Tests/                # Dự án Unit Tests (xUnit)
│   └── TodoListTests.cs           # Chứa các kịch bản kiểm thử tự động
│
└── TodoList.sln                   # File Solution quản lý dự án
```
