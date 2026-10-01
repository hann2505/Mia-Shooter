# Demo âm thanh và hiệu ứng đặc biệt trong game 3D

## Mục tiêu

Project minh họa cách kết hợp âm thanh và hiệu ứng hình ảnh trong một phòng tập bắn 3D. Người chơi có thể di chuyển, ngắm bắn, phá hủy bia và quan sát sự thay đổi của âm thanh theo vị trí trong không gian.

## Cách chạy

1. Mở project bằng Unity `6000.0.58f2`.
2. Mở scene `Assets/Scenes/SoundVfxDemo.unity`.
3. Nhấn nút Play.
4. Dùng `WASD` để di chuyển, `Space` để nhảy, chuột để nhìn, chuột trái để bắn, `R` để nạp đạn và `ESC` để mở hoặc khóa con trỏ.

Nếu cần tạo lại scene, chọn menu `Tools > Mia Shooter > Build Sound & VFX Demo`. Có thể kiểm tra cấu trúc scene bằng menu `Tools > Mia Shooter > Validate Demo Scene`.

## Nội dung đáp ứng yêu cầu

### Hiệu ứng âm thanh

Bốn âm thanh gameplay `gunshot`, `reload`, `footstep` và `land` được nạp trực tiếp từ `Assets/Resources/Audio` khi vào Play Mode. Vì vậy Unity luôn dùng bản MP3 hiện tại ngay cả khi Editor vừa khôi phục một scene backup cũ; tham chiếu trong scene chỉ đóng vai trò dự phòng.

- Tiếng súng được phát từ vị trí người chơi với `AudioSource` có `Spatial Blend`.
- Tiếng va chạm và tiếng nổ được phát tại đúng vị trí bia trong không gian 3D.
- Tiếng bước chân thay đổi nhẹ cao độ để tránh cảm giác lặp máy móc.
- Tiếng nạp đạn kết hợp âm cơ khí và hai nhịp khóa hộp tiếp đạn.
- Tiếng súng, nạp đạn và bước chân sử dụng asset MP3; hiệu ứng tiếp đất MP3 chỉ phát sau khi người chơi thực sự rơi đủ nhanh.
- Âm thanh môi trường chạy lặp để tạo không khí cho phòng tập.
- Khoảng cách nghe sử dụng `minDistance`, `maxDistance` và logarithmic rolloff.

### Hiệu ứng đặc biệt

- Muzzle flash gồm particle và ánh sáng điểm xuất hiện trong thời gian ngắn.
- Vệt đạn dùng `LineRenderer` và tự biến mất sau mỗi phát bắn.
- Va chạm tạo chùm tia lửa particle tại bề mặt trúng đạn.
- Bia nổ bằng hai lớp particle, ánh sáng và các mảnh vỡ có Rigidbody.
- Camera rung nhẹ khi bắn.
- Súng giật theo mỗi phát bắn; nhấn `R` luôn chạy nạp chiến thuật, kể cả khi băng còn đầy. Súng hạ xuống, hộp tiếp đạn tháo–lắp và phát ra một pulse năng lượng.
- Mẫu ion blaster gồm vỏ ceramic, receiver kim loại, armor vát, rail, holo sight, grip, trigger, lõi ion hình cầu, ba energy coil, vent hai bên, muzzle bốn chấu và magazine phát sáng.
- Vật liệu phát sáng, đèn màu và sương mù tạo không khí cho scene 3D.
- Bia bị phá hủy sẽ hồi sinh sau 2,5 giây để tiếp tục demo.

## Các script chính

- `FirstPersonController.cs`: di chuyển, nhìn và phát tiếng bước chân.
- `WeaponController.cs`: raycast bắn súng, băng đạn 12 viên, nạp đạn, recoil, âm thanh, rung camera, muzzle flash và tracer.
- `ShootableTarget.cs`: nhận sát thương, hiệu ứng trúng đạn, nổ và hồi sinh.
- `VfxUtility.cs`: tạo particle, ánh sáng, tracer và mảnh vỡ tại runtime.
- `SoundVfxDemoBuilder.cs`: dựng lại toàn bộ scene và kiểm tra asset.

## Gợi ý thuyết trình

1. Đứng xa một bia rồi tiến lại gần để minh họa attenuation của âm thanh 3D.
2. Bắn vào tường để trình bày tia lửa va chạm và âm thanh hit.
3. Bắn hết 12 viên hoặc nhấn `R` để trình bày animation, particle và âm thanh nạp đạn.
4. Bắn hai lần vào bia để trình bày chuỗi muzzle flash, tracer, nổ, mảnh vỡ và âm thanh tại vị trí bia.
5. Chỉ vào hai bia di động để giải thích rằng hiệu ứng và âm thanh luôn bám theo vị trí va chạm thực tế.
