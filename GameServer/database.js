// database.js
const sqlite3 = require('sqlite3').verbose();
const path = require('path');

// game.db 파일에 SQLite 데이터베이스 생성
const dbPath = path.resolve(__dirname, 'game.db');
const db = new sqlite3.Database(dbPath, (err) => {
    if (err) {
        console.error('[DB] 데이터베이스 연결 실패:', err.message);
    } else {
        console.log('[DB] SQLite 데이터베이스 연결 완료 (game.db)');
    }
});

// 테이블 초기화 (Users & PlayerSaves)
db.serialize(() => {
    // 1. 유저 계정 테이블
    db.run(`
        CREATE TABLE IF NOT EXISTS Users (
            user_id INTEGER PRIMARY KEY AUTOINCREMENT,
            account_id TEXT UNIQUE NOT NULL,
            password_hash TEXT NOT NULL,
            created_at DATETIME DEFAULT CURRENT_TIMESTAMP
        )
    `);

    // 2. 플레이어 세이브 데이터 테이블 (인스턴스 Model 직렬화 JSON 저장)
    db.run(`
        CREATE TABLE IF NOT EXISTS PlayerSaves (
            save_id INTEGER PRIMARY KEY AUTOINCREMENT,
            user_id INTEGER UNIQUE NOT NULL,
            save_json TEXT NOT NULL,
            last_save_ticks INTEGER NOT NULL,
            updated_at DATETIME DEFAULT CURRENT_TIMESTAMP,
            FOREIGN KEY (user_id) REFERENCES Users(user_id)
        )
    `);
});

module.exports = db;