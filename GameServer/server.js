// server.js
const express = require('express');
const cors = require('cors');
const bcrypt = require('bcrypt');
const jwt = require('jsonwebtoken');
const db = require('./database');

const app = express();
const PORT = 3000;
const JWT_SECRET = "OzProjectDemonKingHeroSecretJWTTokenKey2026"; // 보안 키

app.use(cors());
app.use(express.json());

// ==========================================
// [인증 미들웨어] Bearer JWT 토큰 검증
// ==========================================
function authenticateToken(req, res, next) {
    const authHeader = req.headers['authorization'];
    const token = authHeader && authHeader.split(' ')[1]; // "Bearer <TOKEN>"

    if (!token) {
        return res.status(401).json({ success: false, message: "인증 토큰이 누락되었습니다." });
    }

    jwt.verify(token, JWT_SECRET, (err, user) => {
        if (err) {
            return res.status(403).json({ success: false, message: "토큰이 만료되었거나 유효하지 않습니다." });
        }
        req.user = user; // { userId, accountId }
        next();
    });
}

// ==========================================
// 1. 회원가입 API (POST /api/auth/register)
// ==========================================
app.post('/api/auth/register', async (req, res) => {
    const { accountId, password } = req.body;

    if (!accountId || !password) {
        return res.status(400).json({ success: false, message: "아이디와 비밀번호를 모두 입력하세요." });
    }

    try {
        // 비밀번호 Bcrypt 단방향 해싱 (보안)
        const saltRounds = 10;
        const hashedPassword = await bcrypt.hash(password, saltRounds);

        // 유저 생성
        const insertUserQuery = `INSERT INTO Users (account_id, password_hash) VALUES (?, ?)`;
        db.run(insertUserQuery, [accountId, hashedPassword], function (err) {
            if (err) {
                if (err.message.includes('UNIQUE')) {
                    return res.status(400).json({ success: false, message: "이미 존재하는 아이디입니다." });
                }
                return res.status(500).json({ success: false, message: "회원가입 실패: " + err.message });
            }

            const newUserId = this.lastID;

            // 신규 유저용 기본 세이브 데이터 초기화 (JSON)
            // server.js 의 회원가입(POST /api/auth/register) 부분 중
            const defaultSaveData = {
                Player: { Level: 1, CurrentExp: 0, Gold: 0, EnhanceCurrency: 0, RebirthPoints: 0, CurrentStage: 1, MaxStage: 1 },
                Equipments: [],
                Relics: [],
                LastSaveUnixMinutes: Math.floor(Date.now() / 60000), // Unix Time (분 단위)
                UserAccountId: accountId
            };

            const insertSaveQuery = `INSERT INTO PlayerSaves (user_id, save_json, last_save_ticks) VALUES (?, ?, ?)`;
            db.run(insertSaveQuery, [newUserId, JSON.stringify(defaultSaveData), defaultSaveData.LastSaveTimestamp], (saveErr) => {
                if (saveErr) {
                    console.error('[DB] 기본 세이브 생성 에러:', saveErr);
                }
                console.log(`[Auth] 신규 유저 생성 완료: ${accountId} (ID: ${newUserId})`);
                return res.json({ success: true, message: "회원가입이 완료되었습니다." });
            });
        });
    } catch (error) {
        return res.status(500).json({ success: false, message: "서버 내부 오류" });
    }
});

// ==========================================
// 2. 로그인 API (POST /api/auth/login)
// ==========================================
app.post('/api/auth/login', (req, res) => {
    const { accountId, password } = req.body;

    const findUserQuery = `SELECT * FROM Users WHERE account_id = ?`;
    db.get(findUserQuery, [accountId], async (err, user) => {
        if (err) return res.status(500).json({ success: false, message: "서버 오류" });
        if (!user) return res.status(400).json({ success: false, message: "존재하지 않는 아이디입니다." });

        // 비밀번호 검증
        const isMatch = await bcrypt.compare(password, user.password_hash);
        if (!isMatch) {
            return res.status(400).json({ success: false, message: "비밀번호가 일치하지 않습니다." });
        }

        // JWT 토큰 발급 (유효기간 7일)
        const token = jwt.sign({ userId: user.user_id, accountId: user.account_id }, JWT_SECRET, { expiresIn: '7d' });

        console.log(`[Auth] 유저 로그인 성공: ${accountId}`);
        return res.json({
            success: true,
            token: token,
            message: "로그인 성공"
        });
    });
});

// ==========================================
// 3. 세이브 로드 API (GET /api/save/load)
// ==========================================
app.get('/api/save/load', authenticateToken, (req, res) => {
    const userId = req.user.userId;

    const query = `SELECT save_json, last_save_ticks FROM PlayerSaves WHERE user_id = ?`;
    db.get(query, [userId], (err, row) => {
        if (err) return res.status(500).json({ success: false, message: "세이브 로드 오류" });
        if (!row) {
            return res.status(404).json({ success: false, message: "세이브 데이터가 존재하지 않습니다." });
        }

        return res.json({
            success: true,
            saveJson: row.save_json,
            lastSaveTicks: row.last_save_ticks
        });
    });
});

// ==========================================
// 4. 세이브 동기화 API (POST /api/save/sync)
// ==========================================
app.post('/api/save/sync', authenticateToken, (req, res) => {
    const userId = req.user.userId;
    const { saveJson, clientTicks } = req.body;

    if (!saveJson) {
        return res.status(400).json({ success: false, message: "저장할 데이터가 비어있습니다." });
    }

    const query = `
        UPDATE PlayerSaves 
        SET save_json = ?, last_save_ticks = ?, updated_at = CURRENT_TIMESTAMP 
        WHERE user_id = ?
    `;

    db.run(query, [saveJson, clientTicks || 0, userId], function (err) {
        if (err) return res.status(500).json({ success: false, message: "동기화 실패" });

        console.log(`[Save] 유저 (ID: ${userId}) 세이브 데이터 백업 완료`);
        return res.json({ success: true, message: "세이브 데이터가 정상적으로 저장되었습니다." });
    });
});

// 서버 구동
app.listen(PORT, () => {
    console.log(`🚀 게임 서버가 포트 ${PORT}에서 실행 중입니다. (http://localhost:${PORT})`);
});