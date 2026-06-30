# Kịch bản kiểm thử Manual (Test Cases)

Tài liệu này bao gồm các trường hợp kiểm thử (Test Cases) để xác minh tất cả các tính năng hiện có trong ứng dụng Todo List.

---

## MÔ-ĐUN 1: Xác Thực Người Dùng (Authentication)

### TC_AUTH_01: Đăng ký tài khoản mới thành công
- **Bước thực hiện**: 
  1. Khởi chạy ứng dụng, chọn "Chưa có tài khoản? Đăng ký ngay".
  2. Nhập Email hợp lệ, Password, và xác nhận Password.
  3. Bấm "Đăng Ký".
- **Kết quả mong đợi**: 
  - Hệ thống thông báo đăng ký thành công.
  - Tự động chuyển về màn hình Đăng Nhập.

### TC_AUTH_02: Đăng nhập thành công
- **Bước thực hiện**: 
  1. Nhập Email và Password vừa đăng ký.
  2. Bấm "Đăng Nhập".
- **Kết quả mong đợi**: 
  - Đăng nhập thành công, chuyển hướng vào màn hình chính (Dashboard hoặc Eisenhower).

---

## MÔ-ĐUN 2: Quản lý Công Việc Cơ Bản & Ma Trận Eisenhower

### TC_EISEN_01: Thêm công việc trực tiếp vào các ô (Quadrants)
- **Bước thực hiện**: 
  1. Vào tab **Eisenhower**.
  2. Ở ô "P1 - Do First", gõ "Làm bài tập" vào ô TextBox bên dưới.
  3. Bấm nút (+) để thêm.
- **Kết quả mong đợi**: 
  - Công việc "Làm bài tập" xuất hiện ngay trong danh sách P1.

### TC_EISEN_02: Chức năng Auto-Schedule (Tự động lên lịch lặp lại 12 tuần)
- **Bước thực hiện**: 
  1. Bật công tắc (Toggle) hình Đồng hồ cát bên cạnh ô P2.
  2. Thêm công việc "Tập Gym" vào ô P2.
  3. Cuộn xuống cuối trang, bấm nút to "Tự động xếp các công việc mới vào lịch".
- **Kết quả mong đợi**: 
  - Công việc "Tập Gym" được clone thành 12 task (cho 12 tuần tới), cách nhau 7 ngày.
  - Hệ thống hiển thị thông báo đã lên lịch thành công cho 12 tuần.

### TC_EISEN_03: Đánh dấu Hoàn thành (Done) & Xóa
- **Bước thực hiện**: 
  1. Check vào ô vuông (Checkbox) cạnh công việc "Làm bài tập".
  2. Bấm biểu tượng Thùng rác (Xóa) ở một công việc khác.
- **Kết quả mong đợi**: 
  - Khi check Done, tên công việc bị gạch ngang và chữ chuyển sang màu xám.
  - Khi ấn Xóa, công việc biến mất khỏi danh sách.

---

## MÔ-ĐUN 3: Lịch & Đồng Bộ Google Calendar (Calendar Sync)

### TC_CAL_01: Đồng bộ 1 lần từ Google Calendar
- **Bước thực hiện**: 
  1. Chuyển sang Tab **Lịch (Calendar)**.
  2. Bấm nút "Sync Calendar".
  3. Đăng nhập vào Google qua trình duyệt (nếu có yêu cầu).
- **Kết quả mong đợi**: 
  - Ứng dụng tải các sự kiện từ Google Calendar về Database.
  - Các sự kiện này xuất hiện trên màn hình lịch dưới dạng các khung màu xanh.
  - Thoát app và vào lại, dữ liệu vẫn còn mà KHÔNG tự động nhảy ra trình duyệt bắt Sync lại (Trừ khi chủ động bấm Sync lại).

---

## MÔ-ĐUN 4: Hố Đen Ý Tưởng (Mind Sandbox)

### TC_SANDBOX_01: Thêm ý tưởng siêu tốc
- **Bước thực hiện**: 
  1. Vào Menu **Mind Sandbox**.
  2. Ở ô nhập liệu khổng lồ, gõ "Nghiên cứu hố đen vũ trụ" và bấm `Enter` (hoặc nút Gửi).
- **Kết quả mong đợi**: 
  - Ý tưởng biến thành một tấm Post-it note màu vàng/hồng ngẫu nhiên dán ở bên dưới.
  - Ô nhập liệu được làm trống ngay lập tức để gõ tiếp.

### TC_SANDBOX_02: Chuyển ý tưởng thành Công Việc (Promote to Task)
- **Bước thực hiện**: 
  1. Trên tấm Post-it note "Nghiên cứu hố đen", bấm icon Cặp xách màu xanh (Briefcase).
- **Kết quả mong đợi**: 
  - Hộp thoại chi tiết công việc hiện ra.
  - Người dùng có thể chọn Phân loại (P1, P2...) và Đặt ngày giờ, sau đó Lưu lại. Ý tưởng chính thức trở thành Task trong Eisenhower.

---

## MÔ-ĐUN 5: Nút Xúc Xắc (Surprise Me / Randomizer)

### TC_DICE_01: Bốc ngẫu nhiên khi có việc ưu tiên
- **Bước thực hiện**: 
  1. Chắc chắn rằng ô P1 hoặc P2 đang có ít nhất 2-3 việc CHƯA hoàn thành.
  2. Bấm nút **Cục Xúc Xắc 🎲** màu cam ở góc phải dưới của Eisenhower.
- **Kết quả mong đợi**: 
  - Màn hình xám mờ lại, một Popup khổng lồ hiện ra chọn ĐÚNG 1 việc trong số P1/P2.
  - Bấm "Bốc lại (Cái khác đi...)", popup thay đổi sang 1 việc ngẫu nhiên khác.
  - Bấm "✅ Đã làm xong", popup đóng lại, công việc đó ở ma trận bị gạch ngang (Done).

### TC_DICE_02: Bốc ngẫu nhiên khi ĐÃ HẾT việc
- **Bước thực hiện**: 
  1. Tick Done hoặc Xóa toàn bộ việc ở P1 và P2.
  2. Bấm nút **Cục Xúc Xắc 🎲**.
- **Kết quả mong đợi**: 
  - Không hiện Popup bốc việc, mà hiện thông báo: "Bạn đã hoàn thành hết các việc ưu tiên rồi! Quá tuyệt vời!".

---

## MÔ-ĐUN 6: Bảng Thành Tựu (Ta-Da List)

### TC_TADA_01: Hiển thị và Động viên
- **Bước thực hiện**: 
  1. Sáng sớm, vào Tab **Ta-Da List** (khi chưa làm xong việc nào).
  2. Quay lại Eisenhower, hoàn thành 5 công việc.
  3. Vào lại Tab **Ta-Da List**.
- **Kết quả mong đợi**: 
  - Bước 1: Hiện số "0" và thông báo "Nghỉ ngơi cũng là một loại công việc!".
  - Bước 3: Hiện số "5" khổng lồ, thông báo chuyển thành "Thật điên rồ! Bạn đang có một ngày siêu năng suất!".
  - 5 công việc vừa hoàn thành được hiển thị dưới dạng các tấm huân chương viền vàng lấp lánh (Glow effect).
