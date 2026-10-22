import argparse
import os
import sqlite3
import sys

DEFAULT_SQL_PATH = r"C:\Users\PHU\Downloads\data.sql"
DEFAULT_DB_PATH = os.path.join(
    os.path.dirname(os.path.dirname(os.path.abspath(__file__))),
    "src", "ParkingService.Api", "parking_service.db"
)

def import_data(sql_path: str, db_path: str):
    print("=" * 60)
    print("Core Parking Service - Data Import Tool")
    print(f"Source SQL : {sql_path}")
    print(f"Target DB  : {db_path}")
    print("=" * 60)

    if not os.path.exists(sql_path):
        print(f"Error: SQL file not found at '{sql_path}'.", file=sys.stderr)
        sys.exit(1)

    if not os.path.exists(db_path):
        print(f"Error: SQLite database not found at '{db_path}'. Please start the API once to generate the database schema first.", file=sys.stderr)
        sys.exit(1)

    with open(sql_path, "r", encoding="utf-8") as f:
        lines = f.readlines()

    # 1. Parse UserGroup
    user_groups = {}
    is_ug = False
    for line in lines:
        line_s = line.strip()
        if line_s.startswith('COPY public."UserGroup" '):
            is_ug = True
            continue
        if is_ug and line_s == r"\.":
            is_ug = False
            continue
        if is_ug and line_s:
            parts = line_s.split("\t")
            if len(parts) >= 3:
                gid = int(parts[1])
                code = parts[2]
                user_groups[gid] = code

    # 2. Parse User
    users = []
    is_user = False
    for line in lines:
        line_s = line.strip()
        if line_s.startswith('COPY public."User" '):
            is_user = True
            continue
        if is_user and line_s == r"\.":
            is_user = False
            continue
        if is_user and line_s:
            parts = line_s.split("\t")
            uid = int(parts[0])
            role_id = int(parts[1])
            name = parts[2]
            username = parts[3]
            role = user_groups.get(role_id, "user")
            guid = f"a0000000-0000-0000-0000-{uid:012d}"
            users.append((guid, name, username, role_id, role, "Active", "2025-11-09T00:00:00Z", 0))

    # 3. Parse Card
    cards = []
    is_card = False
    for line in lines:
        line_s = line.strip()
        if line_s.startswith('COPY public."Card" '):
            is_card = True
            continue
        if is_card and line_s == r"\.":
            is_card = False
            continue
        if is_card and line_s:
            parts = line_s.split("\t")
            cid = int(parts[0])
            uuid = parts[1].strip()
            raw_type = parts[2].strip().lower() if len(parts) > 2 else "unknown"
            if raw_type == "registered":
                card_type = "Registered"
            elif raw_type == "temporary":
                card_type = "Temporary"
            else:
                card_type = "Unknown"

            plate = parts[6].strip() if len(parts) > 6 and parts[6] != r"\N" and parts[6].strip() else None
            skip = 1 if len(parts) > 7 and parts[7].strip() == "t" else 0
            info = parts[8].strip() if len(parts) > 8 and parts[8] != r"\N" and parts[8].strip() else None
            phone = parts[9].strip() if len(parts) > 9 and parts[9] != r"\N" and parts[9].strip() else None
            room = parts[10].strip() if len(parts) > 10 and parts[10] != r"\N" and parts[10].strip() else None

            status = "Lost" if info and ("MAT THE" in info.upper() or "MẤT THẺ" in info.upper()) else "Active"
            guid = f"c0000000-0000-0000-0000-{cid:012d}"

            cards.append((
                guid,
                uuid,
                card_type,
                plate,
                skip,
                info,
                phone,
                room,
                None,
                status,
                "2025-11-09T00:00:00Z",
                0
            ))

    # 4. Insert into SQLite
    conn = sqlite3.connect(db_path)
    cursor = conn.cursor()

    try:
        cursor.execute("BEGIN TRANSACTION;")

        cursor.executemany("""
            INSERT OR REPLACE INTO Users (Id, FullName, Username, RoleId, Role, Status, CreatedAt, IsDeleted)
            VALUES (?, ?, ?, ?, ?, ?, ?, ?)
        """, users)

        cursor.executemany("""
            INSERT OR REPLACE INTO Cards (Id, CardNumber, Type, VehiclePlate, SkipValidateVehiclePlate, Info, Phone, Room, UserId, Status, CreatedAt, IsDeleted)
            VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)
        """, cards)

        conn.commit()
        print(f"Successfully imported {len(users)} users and {len(cards)} cards into SQLite database!")
    except Exception as e:
        conn.rollback()
        print(f"Transaction error: {e}", file=sys.stderr)
        raise
    finally:
        conn.close()

if __name__ == "__main__":
    parser = argparse.ArgumentParser(description="Import Users and Cards from PostgreSQL dump into SQLite.")
    parser.add_argument("--sql", default=DEFAULT_SQL_PATH, help="Path to data.sql")
    parser.add_argument("--db", default=DEFAULT_DB_PATH, help="Path to parking_service.db")
    args = parser.parse_args()

    import_data(args.sql, args.db)
