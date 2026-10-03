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
- **Mô hình súng viễn tưởng hiện đại hóa & Hạ thấp vị trí (Futuristic Low-Ready Blaster)**:
  - *Vị trí ngắm hạ thấp & Giấu kín báng cầm*: Thân súng `Demo Blaster` được hạ xuống góc dưới bên phải (`localPosition = (0.24, -0.42, 0.92)`), báng cầm (`Grip Frame`, `Grip Backstrap`) và hộp tiếp đạn (`Magazine`) được thu gọn và giấu hoàn toàn bên dưới mép màn hình (viewport boundary), loại bỏ triệt để hiện tượng tay cầm lơ lửng, tạo tư thế low-ready tự nhiên, giải phóng hoàn toàn khu vực trung tâm và tâm ngắm crosshair.
  - *Khung thân modular góc cạnh*: Kết hợp lớp giáp ceramic satin chống nhiệt, khung receiver hợp kim titanium stealth siêu bền, cánh giáp hai bên vuốt khí động học kèm 6 khe tản nhiệt mang cá phát sáng nhiệt cam lửa (`#FF600A`).
  - *Kính ngắm phản xạ viễn tưởng (Reflex Holographic Sight)*: Khung che optic vát cạnh công nghệ cao trên thanh ray Picatinny, thấu kính phủ chống chói (optic glass window) và chấm ngắm holographic laser đỏ rực (`Front Holo Sight`).
  - *Nòng hãm nảy đa khoang (Angular Muzzle Compensator)*: Cụm họng súng 4 chấu góc cạnh (quad-prong compensator) với vòng hào quang nhiệt và lõi ion plasma bên trong buồng tản nhiệt.
  - *Dải sạc phía sau tinh tế (Rear Neon Indicator)*: Dải LED neon âm tường phía sau hiển thị trạng thái tích điện phản ứng theo mức năng lượng laser bằng sắc đỏ cam êm dịu, không bị chói lóa.
- **Tiến trình năng lượng laser góc bên trái thân súng (Tactical Left-Corner OLED Energy Monitor)**:
  - *Vị trí góc bên trái*: Module màn hình OLED chiến thuật được bố trí tại góc trên bên trái thân súng (`localPosition = (-0.165, 0.155, -0.04)`, `Euler(18°, -30°, 2°)`), nghiêng góc công thái học quay thẳng vào trục nhìn của người chơi.
  - *Bảng màu nóng rực lửa (Hot Red & Orange Scheme)*:
    - Màn hình OLED chữ số thời gian thực màu cam neon rực rỡ (`LASER 0%` -> `LASER 100%` / `READY 100%`).
    - 5 khối pin năng lượng phân đoạn (segmented cells) chuyển màu theo dải quang phổ nhiệt độ cao: Cell 1 (Đỏ thẫm `#FF1804`), Cell 2 (Đỏ cam Vermilion `#FF3004`), Cell 3 (Cam lửa `#FF4804`), Cell 4 (Cam hổ phách `#FF7006`), Cell 5 (Vàng kim Solar Gold `#FF9018`).
    - Rãnh trượt mức năng lượng liên tục (gauge trough & fill) tăng dần với dải gradient từ đỏ lửa sang cam rực sáng.
    - Ống dẫn năng lượng dọc sống lưng súng (top energy rail conduit) tích điện bằng luồng plasma đỏ-cam sống động.
- **Tâm ngắm thông minh viễn tưởng (Sci-Fi Reactive Crosshair)**:
  - Điểm tâm chính xác (precision center dot) kết hợp 4 thanh định hướng có khoảng hở tâm và bóng viền đen chống lóa trên mọi điều kiện ánh sáng.
  - Nhận diện mục tiêu thời gian thực: 4 góc bracket tự động thắt chặt và chuyển sang sắc đỏ cam rực lửa (`#FF5238`) khi lia vào bia địch.
  - Phản hồi trúng đạn (Reactive Hitmarker): dấu chéo chữ X chớp nháy trong 0.18 giây mỗi khi bắn trúng mục tiêu bằng đạn hoặc laser.
  - Vòng hào quang laser (Laser Aura Reticle): tự động hiển thị 4 điểm kim cương hào quang cùng chỉ báo "⚡ READY" màu cam lửa nóng bỏng khi nạp đầy, và mở rộng dao động theo luồng plasma khi khai hỏa chuột phải.
  - Thích ứng khi cầm lựu đạn: tâm ngắm tự động mở rộng khoảng hở tạo cảm giác ném quăng tự nhiên.
- **Hệ thống ánh sáng hào quang laser (Volumetric Laser Aura Light)**:
  - *Tia laser thẳng tắp theo hướng nòng súng (Coaxial Muzzle-Forward Beam)*: Tia laser khai hỏa dọc thẳng tắp theo đúng trục nòng súng (`Ray(muzzle.position, muzzle.forward)`), loại bỏ hoàn toàn hiện tượng lệch góc hay bắn chéo màn hình. Chùm tia lõi trắng tinh khiết nhiệt độ cao (`laserBeam`) được bao bọc bởi chùm hào quang cam lửa rực cháy rộng gấp 2.8 lần (`laserAuraBeam`) dao động liên tục theo tần số plasma.
  - Ánh sáng hào quang họng súng (`muzzleAuraLight`): nguồn sáng điểm cường độ cao hắt ánh sáng cam đỏ rực rỡ lên thân súng, các chấu muzzle và sàn arena; khi laser tích đủ 100% ở trạng thái nghỉ, đèn chuyển sang nhịp thở êm dịu báo hiệu sẵn sàng.
  - Ánh sáng hào quang điểm chạm (`impactAuraLight`): nguồn sáng điểm tại vị trí va chạm chiếu sáng rực rỡ bề mặt bia/vật cản, kết hợp với các chùm tia lửa plasma nổ liên tục 20 lần/giây.
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
- **Hiệu ứng kết liễu mục tiêu điện ảnh đa tầng (AAA Target Elimination VFX & Kill Confirmation)**:
  - *Ánh sáng chớp nổ bùng cháy (Dynamic Blast Flash Light)*: Đèn điểm cường độ cao (`intensity = 32f`, `range = 18m`) chớp sáng trắng-neon tức thì trong 0.42 giây tại tâm bia, đổi dần sang sắc neon chủ đạo của bia (`TargetCyan` hoặc `TargetOrange`), hắt sáng lên toàn bộ môi trường xung quanh.
  - *Lõi chớp sáng hủy diệt (White-Hot Annihilation Flash Core)*: Quả cầu chớp sáng cực đại đường kính 4.8m tại tâm chấn trong 0.11s tạo cảm giác bộc phát năng lượng mãnh liệt.
  - *Sóng xung kích plasma phẳng (Horizontal Plasma Shockwave Ring)*: Vòng sóng plasma phát quang LineRenderer nở rộng tức thì từ tâm ra bán kính 5.5m với độ dày 0.38m thu hẹp dần theo đường cong ease-out trong 0.45s.
  - *Cột tháp ion phóng thiên (Ascending Ion Spire / Kill Beacon)*: Chùm tia ion thẳng đứng phóng vút lên trời cao 8–10 mét với tốc độ 20–32m/s, tạo cột mốc tiêu diệt nổi bật (kill beacon) có thể quan sát thấy từ khắp mọi góc trong phòng tập.
  - *Mưa tia lửa plasma phân tán cao tốc (High-Speed Plasma Sparks)*: 85 tia lửa plasma kéo dãn (`stretch lengthScale = 2.8`) bung tỏa theo trọng lực và hướng bắn với vận tốc 16–30m/s.
  - *Khói phân rã thể tích (Volumetric Dissolution Smoke)*: 28 cụm khói hữu cơ nhuốm ánh sáng neon của bia trôi bồng bềnh lên trên và cuộn xoay nhẹ nhàng trong 2.5 giây.
  - *Mảnh vỡ giáp phát quang vật lý (Emissive Armor Shards & Metallic Splinters)*: 18 mảnh vỡ vật lý gồm các phiến giáp ngoài phát sáng viền neon rực rỡ (`_EMISSION`) và các thanh kim loại ruột bia văng tung toé theo lực nổ `580f` và xung lực hướng đạn bắn, xoay 3D hỗn loạn, tự động mờ dần độ sáng và thu nhỏ êm ái trước khi biến mất (`TargetShardFader`).
  - *Âm thanh kết liễu tinh thể thủ tục (Procedural Crystal Kill Chime Audio)*: Tự động tổng hợp xung âm thanh chuông tinh thể 2 hòa âm tần số cao (C6 1046.5Hz + G6 1568Hz) kết hợp sub-kick 85Hz đanh gọn với tốc độ tấn công 3ms, mang lại phản hồi thính giác "Kill Confirmed" cực kỳ đã tai và thỏa mãn.
  - *Huy hiệu tiêu diệt trên HUD (HUD Kill Confirmation Banner)*: Xuất hiện huy hiệu viễn tưởng công nghệ cao viền cyan và góc tab phong cách sci-fi ngay dưới tâm ngắm hiển thị `✦ TARGET ELIMINATED ✦` kèm điểm số `+100 PTS` và bộ đếm chuỗi hạ gục `COMBO x2!` khi tiêu diệt liên tiếp.
  - *Tâm ngắm kết liễu 8 cánh (8-Point Expanding Kill Hitmarker)*: Tâm ngắm bùng nổ 8 cánh hình sao đỏ-vàng kim (4 cánh chéo X đỏ rực mở rộng từ 11px ra 22px cùng 4 hạt kim cương vàng kim ở các trục chính) tạo phản hồi trực quan sắc nét khi hạ gục mục tiêu.
  - *Rung chấn camera vật lý*: Camera rung chấn đanh gọn (`0.14s`, `strength = 0.045f`) khi hạ gục bia tạo cảm giác tác động cơ học chân thực.
- Bia bị phá hủy sẽ hồi sinh sau 2,5 giây để tiếp tục demo.
- **Giao diện HUD viễn tưởng công nghệ cao (Next-Gen Sci-Fi Tactical HUD)**:
  - *Huy hiệu Hologram phân khu tác chiến (Góc trên bên trái)*: Bảng thông tin holographic viền cyan phát quang hiển thị mã hiệu căn cứ tác chiến `✦ MIA // LABS TACTICAL SUITE - FIRING RANGE SIMULATION`.
  - *Chỉ số điểm số & Nút Cài đặt (Góc trên bên phải)*:
    - Bảng đếm điểm kỹ thuật số vàng kim rực rỡ (`SCORE: 0100`).
    - Bộ đếm bia mục tiêu thời gian thực (`TARGETS: 08`).
    - Nút bấm tương tác chiến thuật `[ ⚙ CÀI ĐẶT / ESC ]` cho phép mở nhanh menu cài đặt bằng chuột.
  - *Module sinh trắc & Trạng thái cơ động (Góc dưới bên trái)*:
    - Định danh đặc vụ `OPERATOR // MIA-01`.
    - Thanh Khiên năng lượng (Shield) 5 phân đoạn xanh cyan phát quang.
    - Thanh Giáp thân (Armor) 5 phân đoạn xanh ngọc lục bảo.
    - Dải phím tắt cơ động nhanh: `[W/A/S/D] MOVE   [SPACE] JUMP   [SHIFT] SPRINT`.
  - *Module vũ khí & Tiến trình laser góc cạnh (Góc dưới bên phải)*:
    - Nhãn định danh vũ khí chuẩn quân sự: `ION BLASTER // MK-IV` kèm chế độ bắn `FIREMODE: PLASMA / LASER`.
    - Bộ đếm đạn số lớn `12 / 12` màu trắng-cyan sắc nét.
    - 12 đèn LED pips viên đạn chiến thuật (Tactical Cartridge Pips): phát sáng cyan rực rỡ khi đạn còn trong băng và tự động mờ tối khi khai hỏa từng viên.
    - Thanh năng lượng laser (Laser Energy Bar): dải màu nóng từ đỏ lửa sang cam rực sáng hiển thị tỷ lệ sạc phần trăm thời gian thực (`0%` -> `100%`).
    - Chip thông báo trạng thái khí tài ném: `[Q] HE FRAG: READY` và `[E] SMOKE: READY`.
- **Hệ thống Menu Cài Đặt tương tác & Đổi phím (Interactive Settings & Keybind Rebinding Modal)**:
  - *Đóng/mở linh hoạt*: Bấm phím `ESC`, phím `P` hoặc click trực tiếp nút `[ ⚙ CÀI ĐẶT ]` trên góc phải HUD. Khi menu mở, trò chơi tự động khóa di chuyển, dừng xoay camera, nhả con trỏ chuột tự do để thao tác mượt mà.
  - *Tab Phím điều khiển (Keybinds)*:
    - Liệt kê toàn bộ các hành động: Di chuyển (Tiến, Lùi, Trái, Phải), Nhảy, Bắn chính, Bắn laser, Nạp đạn, Ném lựu đạn nổ, Ném bom khói.
    - Hỗ trợ đổi phím tương tác một chạm: bấm vào nút hành động, giao diện chuyển sang nhấp nháy `[ BẤM PHÍM BẤT KỲ... ]`, nhận ngay phím bàn phím hoặc nút chuột bất kỳ (Mouse 0, 1, 2) và tự động lưu bền vững vào `PlayerPrefs`.
    - Nút Đặt lại mặc định (`⟲ ĐẶT LẠI MẶC ĐỊNH`) khôi phục toàn bộ cấu hình ban đầu ngay tức thì.
  - *Tab Âm thanh (Audio)*:
    - 3 thanh trượt điều chỉnh âm lượng độc lập: Âm lượng tổng (Master), Hiệu ứng (SFX), Môi trường (Ambience) từ 0% đến 100%.
    - Nút kiểm tra âm thanh tức thì: `♫ PHÁT THỬ CHUÔNG TIÊU DIỆT` để nghe thử âm thanh chuông tinh thể kết liễu.
  - *Tab Gameplay*:
    - Thanh trượt độ nhạy chuột từ `0.5x` đến `5.0x`.
    - Hộp chọn bật/tắt rung chấn màn hình (`Screen Shake`).
    - Thanh trượt độ phóng đại tâm ngắm và tùy chọn ẩn/hiện phím gợi ý trên màn hình.

## Các script chính

- `FirstPersonController.cs`: di chuyển, nhìn, phát tiếng bước chân, kết nối với `GameSettings` và xử lý mở/đóng menu cài đặt.
- `WeaponController.cs`: raycast bắn súng, băng đạn 12 viên, nạp đạn, recoil, âm thanh, rung camera, muzzle flash, tracer, cơ chế tích sạc laser, điều khiển màn hình năng lượng thân súng, chùm laser aura đa tầng cùng hệ thống đèn chiếu hào quang họng súng và điểm chạm, tôn trọng cấu hình phím từ `GameSettings`.
- `GameSettings.cs`: quản lý tập trung toàn bộ cấu hình trò chơi, lưu trữ `PlayerPrefs`, xử lý đổi phím (key rebinding), âm lượng, độ nhạy chuột và trạng thái modal cài đặt.
- `DemoHud.cs`: HUD giao diện viễn tưởng thế hệ mới (module súng, đạn pips, thanh năng lượng, sinh trắc học, bảng điểm), hệ thống tâm ngắm thông minh đa chế độ và modal Menu Cài Đặt tương tác 3 tab.
- `GrenadeThrower.cs`, `GrenadeThrowAnimation.cs` và `GrenadeProjectile.cs`: animation tay ném, tạo projectile theo hướng nhìn, xử lý va chạm, sát thương và vụ nổ, điều khiển bằng phím gán trong `GameSettings`.
- `ShootableTarget.cs`: nhận sát thương, hiệu ứng trúng đạn, nổ, kích hoạt hiệu ứng kết liễu điện ảnh và hồi sinh.
- `VfxUtility.cs`: tạo particle, ánh sáng, tracer, vụ nổ, khói thể tích, hiệu ứng kết liễu mục tiêu đa tầng, âm thanh chuông kết liễu thủ tục và mảnh vỡ tại runtime.
- `SoundVfxDemoBuilder.cs`: dựng lại toàn bộ scene, kiểm tra asset và tự động tạo mô hình súng kèm màn hình hiển thị năng lượng.

## Gợi ý thuyết trình

1. Đứng xa một bia rồi tiến lại gần để minh họa attenuation của âm thanh 3D.
2. Trình bày **Giao diện HUD viễn tưởng thế hệ mới**:
   - Chỉ vào huy hiệu Hologram chiến thuật ở góc trên bên trái (`✦ MIA // LABS TACTICAL SUITE`).
   - Giới thiệu module sinh trắc học ở góc dưới bên trái với các thanh Khiên (Shield) và Giáp (Armor) phân đoạn sắc nét.
   - Giới thiệu module vũ khí ở góc dưới bên phải với bộ đếm đạn số lớn `12 / 12`, 12 đèn LED pips đạn chiến thuật tự động tắt khi bắn, và thanh năng lượng laser dải màu nóng đỏ-cam.
3. Bấm phím `ESC` hoặc click nút `[ ⚙ CÀI ĐẶT ]` ở góc trên bên phải HUD để mở **Menu Cài Đặt tương tác**:
   - Trình bày tab **Phím điều khiển (Keybinds)**: click vào một phím (ví dụ: Nạp đạn `R`), nhấn phím mới bất kỳ để minh họa tính năng rebind phím và click `ĐẶT LẠI MẶC ĐỊNH` để khôi phục cấu hình mặc định.
   - Chuyển sang tab **Âm thanh**: kéo thanh trượt âm lượng và bấm nút `♫ PHÁT THỬ CHUÔNG TIÊU DIỆT` để nghe thử âm thanh kết liễu.
   - Chuyển sang tab **Gameplay**: trình bày tùy chọn chỉnh độ nhạy chuột và bật/tắt rung chấn màn hình. Bấm `ĐÓNG MENU (ESC)` để trở lại trận đấu.
4. Trình bày tâm ngắm chính xác với bóng viền tương phản cao, lia tâm qua bia mục tiêu để minh họa tính năng khóa mục tiêu chuyển sang màu đỏ cam.
5. Bắn vào bia hoặc tường để trình bày tia lửa va chạm, âm thanh hit và hiệu ứng hitmarker chữ X chớp nháy tức thì.
6. Bắn hạ hoàn toàn bia mục tiêu để trình bày **Hiệu ứng kết liễu điện ảnh (Target Elimination)**:
   - Chiêm ngưỡng chùm sóng xung kích plasma phẳng bung tỏa cùng cột tháp ion phóng thiên (kill beacon) vút lên trời cao.
   - Các mảnh giáp phát quang neon rực rỡ và splinters kim loại văng tung toé theo lực nổ vật lý.
   - Thưởng thức âm thanh chuông tinh thể "Kill Chime" trong trẻo đanh gọn.
   - Quan sát huy hiệu tiêu diệt `✦ TARGET ELIMINATED ✦` và dấu tâm ngắm kết liễu 8 cánh đỏ-vàng bùng nở trên màn hình HUD.
7. Bắn hết 12 viên hoặc nhấn `R` để trình bày animation, particle và âm thanh nạp đạn, đồng thời quan sát 12 pips đạn trên HUD tự động nạp sáng lại.
8. Bắn hạ 5 bia mục tiêu, hướng sự chú ý vào màn hình OLED chiến thuật trên thân súng và thanh năng lượng trên HUD: quan sát từng vạch năng lượng sáng lên, rãnh trượt đầy dần và dải năng lượng chuyển sang trạng thái "READY 100%".
9. Quan sát hiệu ứng ánh sáng hào quang thở nhẹ nhàng ở đầu nòng súng khi laser đã sẵn sàng.
10. Giữ chuột phải để khai hỏa tia laser: trình bày tia laser kép với chùm hào quang cam lửa rực cháy dọc thẳng tắp trục nòng súng, ánh sáng hào quang họng súng chiếu sáng rực rỡ và ánh sáng điểm chạm tại mục tiêu, trong khi màn hình năng lượng trên thân súng và thanh HUD giảm dần chân thực.
11. Nhấn `G` (hoặc phím đã gán), quan sát nhân vật cầm lựu đạn, click chuột trái để ném và theo dõi vụ nổ siêu uy lực cùng cột khói nấm cuồn cuộn.
12. Nhấn `H` (hoặc phím đã gán), click chuột trái để ném bom khói và quan sát đám khói 4 tầng lan rộng, tồn tại trong nhiều giây.
