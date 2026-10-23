import argparse
import os
import sqlite3
import sys

if hasattr(sys.stdout, 'reconfigure'):
    sys.stdout.reconfigure(encoding='utf-8')
if hasattr(sys.stderr, 'reconfigure'):
    sys.stderr.reconfigure(encoding='utf-8')

DEFAULT_SQL_PATH = r"C:\Users\PHU\Downloads\data.sql"
DEFAULT_DB_PATH = os.path.join(
    os.path.dirname(os.path.dirname(os.path.abspath(__file__))),
    "src", "ParkingService.Api", "parking_service.db"
)

def import_data(sql_path: str, db_path: str):
    print("=" * 70)
    print("Core Parking Service - Data Import & Normalization Tool")
    print(f"Source SQL : {sql_path}")
    print(f"Target DB  : {db_path}")
    print("=" * 70)

    if not os.path.exists(sql_path):
        print(f"Error: SQL file not found at '{sql_path}'.", file=sys.stderr)
        sys.exit(1)

    if not os.path.exists(db_path):
        print(f"Error: SQLite database not found at '{db_path}'.", file=sys.stderr)
        sys.exit(1)

    with open(sql_path, "r", encoding="utf-8") as f:
        lines = f.readlines()

    users = []
    cards = []

    # 1. Thêm các tài khoản quản trị/nhân viên từ COPY public."User"
    is_user_table = False
    for line in lines:
        line_s = line.strip()
        if line_s.startswith('COPY public."User" '):
            is_user_table = True
            continue
        if is_user_table and line_s == r"\.":
            is_user_table = False
            continue
        if is_user_table and line_s:
            parts = line_s.split("\t")
            uid = int(parts[0])
            name = parts[2].strip()
            # Id, FullName, Phone, Email, PlateNumber, Room, DateOfBirth, Gender, Status, CreatedAt, IsDeleted
            user_guid = f"a0000000-0000-0000-0000-{uid:012d}"
            users.append((user_guid, name, None, None, None, None, None, None, "Active", "2025-11-09T00:00:00Z", 0))

    # 2. Xử lý 530 Cards từ COPY public."Card":
    # - Nếu card có thông tin người dùng (info / vehiclePlate / room / phone):
    #   -> Tách thông tin cá nhân và số xe sang bảng Users
    #   -> Liên kết 1 User - 1 Thẻ (Card.UserId = User.Id)
    # - Nếu card chưa gán người dùng: Card.UserId = NULL
    is_card_table = False
    linked_count = 0
    unassigned_count = 0

    for line in lines:
        line_s = line.strip()
        if line_s.startswith('COPY public."Card" '):
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
            info = parts[8].strip() if len(parts) > 8 and parts[8] != r"\N" and parts[8].strip() else None
            phone = parts[9].strip() if len(parts) > 9 and parts[9] != r"\N" and parts[9].strip() else None
            room = parts[10].strip() if len(parts) > 10 and parts[10] != r"\N" and parts[10].strip() else None

            is_lost = bool(info and ("MAT THE" in info.upper() or "MẤT THẺ" in info.upper()))
            card_status = "Lost" if is_lost else "Active"
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
                    "2025-11-09T00:00:00Z",
                    0
                ))
                unassigned_count += 1

    print(f"Tổng số Users chuẩn bị nạp : {len(users)} (3 hệ thống + {linked_count} cư dân có số xe)")
    print(f"Tổng số Cards chuẩn bị nạp : {len(cards)} ({linked_count} thẻ đã liên kết 1-1 + {unassigned_count} thẻ chưa gán)")

    # 3. Nạp vào SQLite
    conn = sqlite3.connect(db_path)
    cursor = conn.cursor()

    try:
        cursor.execute("BEGIN TRANSACTION;")

        cursor.executemany("""
            INSERT OR REPLACE INTO Users (Id, FullName, Phone, Email, PlateNumber, Room, DateOfBirth, Gender, Status, CreatedAt, IsDeleted)
            VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)
        """, users)

        cursor.executemany("""
            INSERT OR REPLACE INTO Cards (Id, CardNumber, UserId, Type, Status, CreatedAt, IsDeleted)
            VALUES (?, ?, ?, ?, ?, ?, ?)
        """, cards)

        conn.commit()
        print("\n=> Nạp và chuẩn hóa dữ liệu vào SQLite THÀNH CÔNG!")
    except Exception as e:
        conn.rollback()
        print(f"Lỗi transaction: {e}", file=sys.stderr)
        raise
    finally:
        conn.close()

if __name__ == "__main__":
    parser = argparse.ArgumentParser(description="Import & Normalize Users and Cards from data.sql into SQLite.")
    parser.add_argument("--sql", default=DEFAULT_SQL_PATH, help="Path to data.sql")
    parser.add_argument("--db", default=DEFAULT_DB_PATH, help="Path to parking_service.db")
    args = parser.parse_args()

    import_data(args.sql, args.db)
