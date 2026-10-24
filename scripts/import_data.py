import os
import re
import sqlite3
import sys

# Ensure UTF-8 output on Windows terminal
sys.stdout.reconfigure(encoding='utf-8')

def import_data(sql_path: str, db_path: str):
    if not os.path.exists(sql_path):
        print(f"Lỗi: Không tìm thấy file {sql_path}", file=sys.stderr)
        sys.exit(1)

    print(f"Bắt đầu đọc dữ liệu từ: {sql_path}")

    # 1. Khởi tạo danh sách chuẩn bị nạp
    # Users: (Id, FullName, Phone, Email, PlateNumber, Room, DateOfBirth, Gender, Status, CreatedAt, IsDeleted)
    users = [
        # Dữ liệu tài khoản quản trị mẫu
        ("a0000000-0000-0000-0000-000000000001", "Quản Trị Viên Hệ Thống", "0900000001", "admin@parking.vn", None, "BQL", None, "Other", "Active", "2025-11-09T00:00:00Z", 0),
        ("a0000000-0000-0000-0000-000000000002", "Bảo Vệ Ca Sáng - Nguyễn Văn An", "0900000002", "baove1@parking.vn", None, "Chốt 1", None, "Male", "Active", "2025-11-09T00:00:00Z", 0),
        ("a0000000-0000-0000-0000-000000000003", "Bảo Vệ Ca Tối - Trần Văn Bình", "0900000003", "baove2@parking.vn", None, "Chốt 2", None, "Male", "Active", "2025-11-09T00:00:00Z", 0),
    ]

    # Cards: (Id, CardNumber, UserId, Type, Status, SkipValidatePlate, CreatedAt, IsDeleted)
    cards = []

    # 2. Đọc file data.sql để lấy thông tin Card và Cư Dân
    with open(sql_path, "r", encoding="utf-8", errors="ignore") as f:
        lines = f.readlines()

    is_card_table = False
    linked_count = 0
    unassigned_count = 0
    skip_plate_count = 0

    for line in lines:
        line_s = line.strip()
        if 'COPY public."Card"' in line_s:
            is_card_table = True
            continue
        if is_card_table and line_s == r"\.":
            is_card_table = False
            continue
        if is_card_table and line_s:
            parts = line_s.split("\t")
            cid = int(parts[0])
            uuid = parts[1].strip()
            if not uuid:
                continue
            raw_type = parts[2].strip().lower() if len(parts) > 2 else "unknown"

            plate = parts[6].strip() if len(parts) > 6 and parts[6] != r"\N" and parts[6].strip() else None
            skip_plate_raw = parts[7].strip().lower() if len(parts) > 7 else "f"
            skip_validate_plate = 1 if skip_plate_raw == "t" else 0
            if skip_validate_plate == 1:
                skip_plate_count += 1

            info = parts[8].strip() if len(parts) > 8 and parts[8] != r"\N" and parts[8].strip() else None
            phone = parts[9].strip() if len(parts) > 9 and parts[9] != r"\N" and parts[9].strip() else None
            room = parts[10].strip() if len(parts) > 10 and parts[10] != r"\N" and parts[10].strip() else None

            is_lost = bool(info and ("MAT THE" in info.upper() or "MẤT THẺ" in info.upper()))
            card_status = "inactive" if is_lost else "active"
            card_guid = f"c0000000-0000-0000-0000-{cid:012d}"

            # Kiểm tra xem thẻ có gắn với người dùng / xe thực tế hay không
            has_resident_data = bool((info and not is_lost) or plate or phone or room)

            if has_resident_data:
                user_guid = f"b0000000-0000-0000-0000-{cid:012d}"
                # Xác định họ tên hiển thị
                full_name = info if (info and not is_lost) else (phone if phone else f"Cư dân phòng {room}" if room else "Chủ xe " + str(plate))

                # Thêm vào danh sách Users
                users.append((
                    user_guid,
                    full_name,
                    phone,
                    None,      # Email
                    plate,     # PlateNumber (dùng để validate khi quẹt thẻ)
                    room,      # Room
                    None,      # DateOfBirth
                    None,      # Gender
                    "Active",  # Status
                    "2025-11-09T00:00:00Z",
                    0
                ))

                # Thêm vào danh sách Cards (đã liên kết với User)
                cards.append((
                    card_guid,
                    uuid,
                    user_guid,
                    "Registered",
                    card_status,
                    skip_validate_plate,
                    "2025-11-09T00:00:00Z",
                    0
                ))
                linked_count += 1
            else:
                # Thẻ chưa gán người dùng (khách hoặc thẻ chưa đăng ký)
                cards.append((
                    card_guid,
                    uuid,
                    None,      # UserId = NULL
                    "Unknown",
                    card_status,
                    skip_validate_plate,
                    "2025-11-09T00:00:00Z",
                    0
                ))
                unassigned_count += 1

    print(f"Tổng số Users chuẩn bị nạp : {len(users)} (3 hệ thống + {linked_count} cư dân có số xe)")
    print(f"Tổng số Cards chuẩn bị nạp : {len(cards)} ({linked_count} thẻ đã liên kết 1-1 + {unassigned_count} thẻ chưa gán)")
    print(f"Trong đó có {skip_plate_count} thẻ SkipValidatePlate = 1 (true)")

    # 3. Nạp vào SQLite
    conn = sqlite3.connect(db_path)
    cursor = conn.cursor()

    try:
        cursor.execute("BEGIN TRANSACTION;")

        # Đảm bảo cột SkipValidatePlate tồn tại trong bảng Cards
        cursor.execute("PRAGMA table_info(Cards);")
        existing_columns = [col[1] for col in cursor.fetchall()]
        if "SkipValidatePlate" not in existing_columns:
            print("Thêm cột SkipValidatePlate vào bảng Cards trong SQLite...")
            cursor.execute("ALTER TABLE Cards ADD COLUMN SkipValidatePlate INTEGER NOT NULL DEFAULT 0;")

        cursor.executemany("""
            INSERT OR REPLACE INTO Users (Id, FullName, Phone, Email, PlateNumber, Room, DateOfBirth, Gender, Status, CreatedAt, IsDeleted)
            VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)
        """, users)

        cursor.executemany("""
            INSERT OR REPLACE INTO Cards (Id, CardNumber, UserId, Type, Status, SkipValidatePlate, CreatedAt, IsDeleted)
            VALUES (?, ?, ?, ?, ?, ?, ?, ?)
        """, cards)

        # Cập nhật migration history
        cursor.execute("""
            INSERT OR IGNORE INTO "__EFMigrationsHistory" (MigrationId, ProductVersion)
            VALUES ('20261024103000_AddSkipValidatePlateToCards', '8.0.8');
        """)

        conn.commit()
        print("\n=> Nạp và chuẩn hóa dữ liệu vào SQLite THÀNH CÔNG!")
    except Exception as e:
        conn.rollback()
        print(f"Lỗi transaction: {e}", file=sys.stderr)
        raise
    finally:
        conn.close()

if __name__ == "__main__":
    sql_file = r"C:\Users\PHU\Downloads\data.sql"
    db_file = os.path.join(os.path.dirname(__file__), "..", "src", "ParkingService.Api", "parking_service.db")
    import_data(sql_file, db_file)
