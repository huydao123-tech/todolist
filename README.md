# ADHD-Friendly Todo List App 🚀

Một ứng dụng quản lý công việc và thời gian được thiết kế đặc biệt dựa trên tâm lý học hành vi, dành riêng cho những bộ não tư duy phân nhánh (Ne) như INTP hoặc những người có hội chứng ADHD nhẹ. 

Thay vì ép buộc người dùng vào các khuôn khổ cứng nhắc truyền thống, ứng dụng này mang đến sự linh hoạt tối đa, tập trung vào việc giảm tải nhận thức (cognitive load) và cung cấp phần thưởng tức thời (dopamine hits).

## 🌟 Các Tính Năng Nổi Bật

### 1. Ma Trận Eisenhower (Có tích hợp AI & Tự Động Hóa)
- Phân loại công việc theo 4 mức độ: Quan trọng & Khẩn cấp (P1), Quan trọng nhưng Không Khẩn cấp (P2), Khẩn cấp nhưng Không Quan trọng (P3), Không Quan trọng & Không Khẩn cấp (P4).
- **Khuyên bảo từ AI**: Tích hợp các lời khuyên dựa trên dữ liệu thật của bạn (ví dụ: Cảnh báo nếu P3 quá nhiều, hoặc biểu dương nếu bạn đang làm tốt ở P2).
- **Lặp lại 12 tuần (Auto-Schedule)**: Gạt đi nỗi lo phải tạo lại công việc lặp đi lặp lại. Tính năng tự động xếp lịch sẽ sinh ra chuỗi task cho 12 tuần tới.

### 2. Hố Đen Ý Tưởng (Mind Sandbox) 🛸
- Một không gian hoàn toàn "Distraction-free". Không yêu cầu ngày tháng, không cần ưu tiên, không quy tắc.
- Bạn có thể xả ngay bất kỳ ý tưởng điên rồ nào lóe lên trong đầu.
- Giao diện dạng bảng dán giấy nhớ (Post-it notes) xếp lộn xộn một cách có tổ chức. Từ đây, bạn có thể biến chúng thành công việc nghiêm túc, hoặc ném vào sọt rác.

### 3. Nút Xúc Xắc (Surprise Me / Randomizer) 🎲
- **Vũ khí chống Tê liệt quyết định (ADHD Paralysis)**.
- Khi bạn nhìn vào danh sách việc quá dài và không biết bắt đầu từ đâu, hãy nhấn nút Xúc Xắc.
- Ứng dụng sẽ đưa bạn vào **Chế độ Tập Trung (Focus Mode)**: Che khuất toàn bộ mọi thứ xung quanh và chỉ chỉ định đúng 1 việc ưu tiên ngẫu nhiên cho bạn làm. "Hoặc làm việc này, hoặc bốc việc khác".

### 4. Bảng Thành Tựu (Ta-Da List) 🏆
- Đánh bại chứng "mù thời gian" và suy nghĩ tiêu cực "hôm nay chưa làm được gì".
- Chỉ hiển thị những việc **ĐÃ LÀM XONG** trong ngày hôm nay.
- Biến các công việc thành huân chương lấp lánh kèm theo các lời động viên được tự động sinh ra tùy theo mức độ năng suất của bạn.

### 5. Lịch & Đồng bộ hóa Google Calendar 📅
- Hiển thị trực quan dưới dạng lịch tháng.
- Nút đồng bộ hóa (Sync) cho phép kéo các sự kiện từ Google Calendar về ứng dụng mà không cần phải mở web. Dữ liệu chỉ đồng bộ 1 lần hoặc khi bạn chủ động ấn nút, không gây gián đoạn hay quá tải trải nghiệm.

### 6. Quản Lý Tài Khoản 🔒
- Hệ thống Đăng nhập (Login) và Đăng ký (Register) an toàn bằng mật khẩu đã được mã hóa (Bcrypt).
- Dữ liệu của ai người đó dùng, bảo mật và riêng tư.

## 🛠️ Công Nghệ Sử Dụng
- **Ngôn ngữ**: C# / .NET 8.0
- **Giao diện**: WPF (Windows Presentation Foundation) kết hợp Material Design In XAML Toolkit.
- **Kiến trúc**: MVVM (Model-View-ViewModel) với CommunityToolkit.Mvvm.
- **Cơ sở dữ liệu**: SQLite (qua Entity Framework Core).
- **Tích hợp**: Google Calendar API.

## 🚀 Cách Khởi Chạy
1. Đảm bảo máy tính đã cài đặt .NET 8 SDK.
2. Mở Terminal/Command Prompt tại thư mục chứa file `.csproj` (thư mục `TodoList`).
3. Chạy lệnh:
   ```bash
   dotnet build
   dotnet run
   ```
4. Ứng dụng sẽ tự động khởi tạo Database SQLite (`todo.db`) vào lần đầu tiên chạy.

## 🧠 Triết Lý Thiết Kế
*“Không có người lười biếng, chỉ có hệ thống chưa đủ kích thích để não bộ bắt tay vào việc.”*
Ứng dụng này không cố gắng biến bạn thành một cái máy vô tri, nó cố gắng trở thành một trợ lý hiểu rõ nhịp độ sinh học và những lúc "tụt mood" của bạn để đẩy bạn lên.
