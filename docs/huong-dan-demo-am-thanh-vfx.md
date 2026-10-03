# Demo âm thanh và hiệu ứng đặc biệt trong game 3D

## Mục tiêu

Project minh họa cách kết hợp âm thanh và hiệu ứng hình ảnh trong một phòng tập bắn 3D. Người chơi có thể di chuyển, ngắm bắn, phá hủy bia và quan sát sự thay đổi của âm thanh theo vị trí trong không gian.

## Cách chạy

1. Mở project bằng Unity `6000.0.58f2`.
2. Mở scene `Assets/Scenes/SoundVfxDemo.unity`.
3. Nhấn nút Play.
4. Dùng `WASD` để di chuyển, `Space` để nhảy, chuột để nhìn, chuột trái để bắn và `R` để nạp đạn. Phá hủy 5 bia để nạp đầy thanh năng lượng trên thân súng, sau đó giữ chuột phải để bắn laser. Nhấn `G` để cầm lựu đạn nổ hoặc `H` để cầm bom khói, sau đó click chuột trái để ném. Nhấn lại cùng phím để cất bom và quay về súng.

Nếu cần tạo lại scene, chọn menu `Tools > Mia Shooter > Build Sound & VFX Demo`. Có thể kiểm tra cấu trúc scene bằng menu `Tools > Mia Shooter > Validate Demo Scene`.

## Nội dung đáp ứng yêu cầu

### Hiệu ứng âm thanh

Bốn âm thanh gameplay cơ bản `gunshot`, `reload`, `footstep` và `land` cùng âm thanh khai hỏa chùm năng lượng `laser` được nạp trực tiếp từ `Assets/Resources/Audio` khi vào Play Mode. Vì vậy Unity luôn dùng bản MP3 hiện tại ngay cả khi Editor vừa khôi phục một scene backup cũ; tham chiếu trong scene chỉ đóng vai trò dự phòng.

- Tiếng súng được phát từ vị trí người chơi với `AudioSource` có `Spatial Blend`.
- Âm thanh bắn laser sử dụng file `Assets/Audio/laser.mp3` (`Assets/Resources/Audio/laser.mp3`): tích hợp qua `AudioSource` 2D chuyên dụng chạy lặp (`loop = true`) liên tục trong suốt thời gian giữ chuột phải, tự động biến điệu cao độ (dynamic pitch modulation) dao động nhẹ theo luồng plasma và dừng ngay lập tức khi nhả chuột hoặc cạn kiệt pin năng lượng.
- Tiếng va chạm và tiếng nổ được phát tại đúng vị trí bia trong không gian 3D.
- Tiếng bước chân thay đổi nhẹ cao độ để tránh cảm giác lặp máy móc.
- Tiếng nạp đạn kết hợp âm cơ khí và hai nhịp khóa hộp tiếp đạn.
- Tiếng súng, nạp đạn, bước chân và laser sử dụng asset MP3; hiệu ứng tiếp đất MP3 chỉ phát sau khi người chơi thực sự rơi đủ nhanh.
- Âm thanh môi trường chạy lặp để tạo không khí cho phòng tập.
- Khoảng cách nghe sử dụng `minDistance`, `maxDistance` và logarithmic rolloff.

### Hiệu ứng đặc biệt

- Muzzle flash gồm particle và ánh sáng điểm xuất hiện trong thời gian ngắn.
- Vệt đạn dùng `LineRenderer` và tự biến mất sau mỗi phát bắn.
- **Tiến trình năng lượng laser trực tiếp trên thân súng**:
  - Màn hình OLED chiến thuật gắn tại sườn trên bên trái súng (`Euler(14°, -24°, 0°)`), đặt đúng góc nhìn tự nhiên của người chơi.
  - Hiển thị chỉ số kỹ thuật số thời gian thực (`LASER 0%`, `LASER 40%`, `READY 100%`).
  - 5 khối pin năng lượng phân đoạn (segmented cells) tự động phát sáng dần theo từng nấc 20% khi phá hủy bia.
  - Rãnh trượt mức năng lượng liên tục (gauge trough & fill) tăng/giảm mượt mà theo năng lượng tích lũy hoặc tiêu hao.
  - Ống dẫn năng lượng dọc sống lưng súng (top energy rail conduit) và dải sạc phía sau (rear charge strip) tích điện đồng bộ và phát xung nhịp khi súng đạt 100%.
- **Tâm ngắm thông minh viễn tưởng (Sci-Fi Reactive Crosshair)**:
  - Điểm tâm chính xác (precision center dot) kết hợp 4 thanh định hướng có khoảng hở tâm và bóng viền đen chống lóa trên mọi điều kiện ánh sáng.
  - Nhận diện mục tiêu thời gian thực: 4 góc bracket tự động thắt chặt và chuyển sang sắc đỏ cam rực lửa (`#FF5238`) khi lia vào bia địch.
  - Phản hồi trúng đạn (Reactive Hitmarker): dấu chéo chữ X chớp nháy trong 0.18 giây mỗi khi bắn trúng mục tiêu bằng đạn hoặc laser.
  - Vòng hào quang laser (Laser Aura Reticle): tự động hiển thị 4 điểm kim cương hào quang cùng chỉ báo "⚡ READY" khi nạp đầy, và mở rộng dao động theo luồng plasma khi khai hỏa chuột phải.
  - Thích ứng khi cầm lựu đạn: tâm ngắm tự động mở rộng khoảng hở tạo cảm giác ném quăng tự nhiên.
- **Hệ thống ánh sáng hào quang laser (Volumetric Laser Aura Light)**:
  - Tia laser đa tầng: gồm chùm tia lõi trắng tinh khiết nhiệt độ cao (`laserBeam`) được bao bọc bởi chùm hào quang ngọc lam rộng gấp 2.8 lần (`laserAuraBeam`) dao động liên tục theo tần số plasma.
  - Ánh sáng hào quang họng súng (`muzzleAuraLight`): nguồn sáng điểm cường độ cao (4.8f, bán kính 15m) hắt ánh sáng xanh ngọc rực rỡ lên thân súng, các chấu muzzle và sàn arena; khi laser tích đủ 100% ở trạng thái nghỉ, đèn chuyển sang nhịp thở êm dịu báo hiệu sẵn sàng.
  - Ánh sáng hào quang điểm chạm (`impactAuraLight`): nguồn sáng điểm tại vị trí va chạm (cường độ 4.4f, bán kính 11m) chiếu sáng rực rỡ bề mặt bia/vật cản, kết hợp với các chùm tia lửa plasma nổ liên tục 20 lần/giây.
- Va chạm tạo chùm tia lửa particle tại bề mặt trúng đạn.
- Bia nổ bằng hai lớp particle, ánh sáng và các mảnh vỡ có Rigidbody.
- Nhấn `G` hoặc `H` sẽ cất súng và đưa loại bom tương ứng vào tay. Click chuột trái mới chạy animation ném; projectile được thả đúng giữa chuyển động rồi tay thu khỏi khung hình và súng xuất hiện lại.
- **Mô hình cánh tay góc nhìn thứ nhất & Găng tay chiến thuật thủ tục (Procedural Tactical Arm & Combat Glove Viewmodel)**:
  - **Cánh tay liên tục tự nhiên (2-Bone Analytical IK)**: Cánh tay xuất phát từ góc dưới bên trái ngoài màn hình (`ShoulderLocalPos = (-0.32, -0.38, 0.10)`), loại bỏ hoàn toàn hiện tượng tay lơ lửng đứt đoạn; cánh tay trên (bắp tay) bọc ống tay áo tác chiến với đai bắp tay kim loại, khớp cùi chỏ có giáp bảo vệ góc cạnh.
  - **Giáp cẳng tay công nghệ cao (Forearm Exo-Gauntlet)**: Ống cẳng tay trang bị giáp bảo vệ vát cạnh carbon, tích hợp thanh dẫn năng lượng sinh trắc học phát sáng cyan (`Biometric Power Rail`), đèn báo trạng thái viễn trắc màu hổ phách (`Status Node`) và đinh ốc kim loại.
  - **Găng tay chiến thuật đa lớp (Combat Glove)**: Cổ tay có khóa kẹp cơ khí; lòng bàn tay công thái học có đệm cao su vân nhám tăng độ bám; mu bàn tay bọc giáp carbon với đường viền mạch năng lượng cyan và 4 chấu giáp bảo vệ khớp ngón tay (knuckle studs).
  - **Hệ thống 5 ngón tay chuyển động độc lập (5 Articulated Digits)**: Ngón cái và 4 ngón tay (trỏ, giữa, áp út, út) đều có 2 khớp xoay (`rootPivot` và `distalPivot`), đệm bọc khớp carbon và miếng đệm ma sát đầu ngón tay. Các góc xoay được hiệu chỉnh chính xác để ôm chặt lựu đạn khi cầm và bung xòe tự nhiên khi ném.
  - **Kích thước lựu đạn chuẩn cầm tay (Tactical Grenade Model)**: Thu nhỏ kích thước quả bom về chuẩn thực tế ~10cm x 12.5cm nằm gọn trong lòng bàn tay, bổ sung chi tiết đai gân xích đạo, cổ ngòi nổ kim loại, nắp kíp nổ, đòn bẩy an toàn (thìa giữ - safety spoon) ôm sát thân dưới các ngón tay, chốt vòng giật bằng đồng thau và đèn LED chỉ báo tác chiến (đỏ cam cho bom nổ, xanh ngọc cho bom khói).
- **Hoạt ảnh ném bom cơ học linh hoạt (Dynamic Grenade Throw Animation)**:
  - *Nhịp thở tự nhiên khi ngắm (Organic Breathing Sway)*: Cánh tay dao động vi mô hình sin nhịp nhàng khi ở trạng thái sẵn sàng ném (`HoldPose`), tạo cảm giác nhân vật còn sống động và có trọng lượng.
  - *Đưa bom vào tay (Equip)*: Cánh tay vung từ dưới lên theo đường cong đàn hồi (`EaseOutBack`), các ngón tay siết chặt dần quanh quả lựu đạn.
  - *Lấy đà (Windup)*: Kéo tay về sau, cổ tay ngửa lên và gập chặt các ngón tay chuẩn bị phát lực.
  - *Vung tay & Bung ngón (Heave & Dynamic Finger Release)*: Cánh tay vung tới tột đỉnh với gia tốc mạnh mẽ; ngay tại điểm nhả lựu đạn (`releasePoint = 0.54`), các ngón tay lập tức bung xòe mạnh mẽ giải phóng quả bom xoay về phía trước, kèm chấn động rung camera nhẹ (`CameraShake`).
  - *Hãm đà quán tính & Thu tay (Follow-Through & Recovery)*: Cánh tay tiếp tục vung chúc xuống triệt tiêu quán tính, các ngón tay thả lỏng dần về độ cong thư giãn tự nhiên rồi hạ cánh tay mượt mà khỏi màn hình và hoàn trả vũ khí.
- **Vụ nổ lựu đạn siêu uy lực (Cinematic Heavy Bomb Explosion VFX)**:
  - **Chớp sáng chói lòa & Ánh sáng động**: Chớp sáng tâm trắng tức thời kết hợp nguồn sáng điểm động cường lực (cường độ 55, bán kính 36m) bừng sáng toàn bộ đấu trường trong tích tắc rồi chuyển thành ánh lửa ấm áp.
  - **Cầu lửa bùng phát dữ dội & Cột lửa hình nấm**: Cầu lửa trung tâm nở rộng 7–8.5m với dải màu gradient từ vàng kim rực lửa sang cam đỏ cuồn cuộn; kèm cột lửa bốc đứng hình phễu/nón phụt thẳng lên cao.
  - **Cột khói đen thể tích cuồn cuộn (Volumetric Smoke Plume)**: 180 hạt khói hữu cơ Perlin đa tầng nở rộng đến 10–12m với dải màu bồ hóng sẫm chuyển tro xám, bốc cao cuồn cuộn theo dòng đối lưu nhiệt cùng vành khói quét sát mặt sàn (80 hạt).
  - **Hệ thống siêu sóng xung kích đa tầng (Multi-Tier Shockwaves)**:
    - *Vòng plasma lửa sơ cấp*: Độ dày 1.1m, bung tỏa bùng nổ theo đường cong ease-out đạt bán kính 17.5m.
    - *Vòng nén khí siêu thanh (Supersonic Compression Wave)*: Sóng khí xanh lam nhạt sắc bén quét nhanh tới 23m.
    - *Vòm sóng xung kích 3D đứng*: Vòng sóng nghiêng 3D bán kính 15m tạo cảm giác khối cầu áp suất trong không gian.
    - *Sóng bụi quét sàn (Ground Dust Wave)*: 180 hạt bụi đất quét sát bề mặt sàn với tốc độ 20–28m/s mô phỏng luồng gió nén cực mạnh.
  - **Mưa tia lửa kim loại nóng chảy & Mảnh vỡ vật lý**: 180 tia lửa văng hình parabol theo trọng lực cùng 16 khối mảnh vỡ vật lý nảy tung toé trên sàn đấu trường.
  - **Vết cháy hố nổ mặt sàn (Ground Scorch Crater)**: Để lại vết cháy xém đường kính 7m với viền nhiễu hữu cơ, duy trì và mờ dần trong 8 giây.
  - **Uy lực vật lý & Rung chấn màn hình**: Bán kính sát thương tăng lên 11.5m, lực hất văng tăng lên 1700f, sát thương 3; camera rung chấn dữ dội theo khoảng cách từ tâm nổ trong phạm vi 32m.
- **Hệ thống khói chiến thuật chân thực (Volumetric Tactical Smoke VFX)**:
  - **Texture khói hữu cơ vi mô (Procedural Organic fBm Perlin)**: Tạo vân khói 128x128 tính toán bằng 3 tầng Perlin fBm kết hợp hàm suy giảm cosin bán kính và khử cạnh quad hoàn toàn, tích hợp shading vi mô đa diện tạo các rãnh khối 3D cho từng cụm khói.
  - **Vật liệu Alpha-Blended chuẩn màu**: Cân bằng Tint Color triệt tiêu hiện tượng nhân đôi độ sáng của Unity Legacy Particle Shader, ngăn chặn cháy trắng và bảo toàn màu xám bạc chiến thuật.
  - **Màn khói 4 tầng chuyên sâu (4-Tier Tactical Smoke Screen)**:
    - *Chớp lửa ngòi nổ*: Ánh sáng điểm cam ấm chớp nhẹ trong 0.15s mô phỏng phản ứng pyrotechnic ban đầu.
    - *Khối khói cuộn thể tích trung tâm (Dense Core)*: 105 hạt khói phân tầng sáng tối tự nhiên (`MinMaxGradient`), bung nở từ 1.5m lên 4.5m tạo màn chắn tầm nhìn đục mờ, cuộn xoay và trôi nhẹ theo dòng đối lưu.
    - *Thảm sương khói bò sát mặt sàn (Ground Creeping Carpet)*: 55 hạt khói quét sát mặt sàn mô phỏng tính chất khí nặng đặc trưng của lựu đạn khói quân sự.
    - *Dải khói khuếch tán không khí (Atmospheric Wisps)*: 30 dải khói mỏng nhẹ trôi lơ lửng xung quanh, hòa quyện vào môi trường và tan biến nhẹ nhàng sau 7–9 giây.
- Lựu đạn nổ sử dụng `bomb-explosion.mp3`; bom khói sử dụng `smoke.mp3`. Cả hai âm thanh được phát 3D tại đúng vị trí va chạm.
- Camera rung nhẹ khi bắn.
- Súng giật theo mỗi phát bắn; nhấn `R` luôn chạy nạp chiến thuật, kể cả khi băng còn đầy. Súng hạ xuống, hộp tiếp đạn tháo–lắp và phát ra một pulse năng lượng.
- Mẫu ion blaster gồm vỏ ceramic, receiver kim loại, armor vát, rail, holo sight, grip, trigger, lõi ion hình cầu, ba energy coil, vent hai bên, muzzle bốn chấu, magazine phát sáng và màn hình hiển thị laser OLED.
- Vật liệu phát sáng, đèn màu và sương mù tạo không khí cho scene 3D.
- Bia bị phá hủy sẽ hồi sinh sau 2,5 giây để tiếp tục demo.

## Các script chính

- `FirstPersonController.cs`: di chuyển, nhìn và phát tiếng bước chân.
- `WeaponController.cs`: raycast bắn súng, băng đạn 12 viên, nạp đạn, recoil, âm thanh, rung camera, muzzle flash, tracer, cơ chế tích sạc laser, điều khiển màn hình năng lượng thân súng, chùm laser aura đa tầng cùng hệ thống đèn chiếu hào quang họng súng và điểm chạm.
- `DemoHud.cs`: HUD giao diện, thông tin đạn/điểm số, cùng hệ thống tâm ngắm thông minh đa chế độ (precision crosshair, target lock-on, reactive hitmarker, laser aura reticle).
- `GrenadeThrower.cs`, `GrenadeThrowAnimation.cs` và `GrenadeProjectile.cs`: animation tay ném, tạo projectile theo hướng nhìn, xử lý va chạm, sát thương và vụ nổ.
- `ShootableTarget.cs`: nhận sát thương, hiệu ứng trúng đạn, nổ và hồi sinh.
- `VfxUtility.cs`: tạo particle, ánh sáng, tracer và mảnh vỡ tại runtime.
- `SoundVfxDemoBuilder.cs`: dựng lại toàn bộ scene, kiểm tra asset và tự động tạo mô hình súng kèm màn hình hiển thị năng lượng.

## Gợi ý thuyết trình

1. Đứng xa một bia rồi tiến lại gần để minh họa attenuation của âm thanh 3D.
2. Trình bày tâm ngắm chính xác với bóng viền tương phản cao, lia tâm qua bia mục tiêu để minh họa tính năng khóa mục tiêu chuyển sang màu đỏ cam.
3. Bắn vào bia hoặc tường để trình bày tia lửa va chạm, âm thanh hit và hiệu ứng hitmarker chữ X chớp nháy tức thì.
4. Bắn hết 12 viên hoặc nhấn `R` để trình bày animation, particle và âm thanh nạp đạn.
5. Bắn hạ 5 bia mục tiêu, hướng sự chú ý vào màn hình OLED chiến thuật trên thân súng: quan sát từng vạch năng lượng sáng lên, rãnh trượt đầy dần và dải năng lượng chuyển sang trạng thái "READY 100%".
6. Quan sát hiệu ứng ánh sáng hào quang thở nhẹ nhàng ở đầu nòng súng khi laser đã sẵn sàng.
7. Giữ chuột phải để khai hỏa tia laser: trình bày tia laser kép với chùm hào quang ngọc lam cuồn cuộn, ánh sáng hào quang họng súng chiếu sáng rực rỡ và ánh sáng điểm chạm tại mục tiêu, trong khi màn hình năng lượng trên thân súng giảm dần chân thực.
8. Nhấn `G`, quan sát nhân vật cầm lựu đạn, click chuột trái để ném và theo dõi sóng xung kích tại điểm nổ.
9. Nhấn `H`, click chuột trái để ném bom khói và quan sát đám khói lan rộng, tồn tại trong nhiều giây.
