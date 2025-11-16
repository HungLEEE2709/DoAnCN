const express = require("express");
const router = express.Router();
const User = require("../models/User");
const bcrypt = require("bcryptjs"); 
const jwt = require("jsonwebtoken"); 
const PlayerInfo = require("../models/PlayerInfo");
const PORT = process.env.PORT || 5000;

const SECRET_KEY = "azura_secret_key"; 

router.post("/register", async (req, res) => {
  try {
    const { username, email, password } = req.body;

    if (!username || !email || !password)
      return res.status(400).json({ message: "Thiếu dữ liệu!" });

    const existUser = await User.findOne({ username });
    if (existUser)
      return res.status(400).json({ message: "Username đã tồn tại" });

    const existEmail = await User.findOne({ email });
    if (existEmail)
      return res.status(400).json({ message: "Email đã tồn tại" });

    const hashed = await bcrypt.hash(password, 10);

    // 🟦 1. Tạo USER
    const newUser = await User.create({
      username,
      email,
      password: hashed
    });

    // 🟩 2. Tạo PlayerInfo tương ứng
    await PlayerInfo.create({
      idUser: newUser._id.toString(),
      UserName: newUser.username,
      Hp: 0,
      Ki: 0,
      Dame: 0,
      //TiemNang: 0,
      SucManh: 0,
      Planet: null,
      CharacterName: null,
      PrefabKey: null,
      CharacterChosen: false
    });

    res.json({
      message: "Đăng ký thành công!",
      user: newUser
    });

  } catch (err) {
    console.log(err);
    res.status(500).json({ message: err.message });
  }
});


router.post("/login", async (req, res) => {
  try {
    const { username, password } = req.body;

    const user = await User.findOne({ username });
    if (!user) return res.status(400).json({ message: "Tài khoản không tồn tại" });

    const isMatch = await bcrypt.compare(password, user.password);
    if (!isMatch) return res.status(400).json({ message: "Sai mật khẩu" });

    const token = jwt.sign({ id: user._id, username: user.username }, SECRET_KEY, {
      expiresIn: "7d",
    });

    res.json({ message: "Đăng nhập thành công", token, user });
  } catch (err) {
    res.status(500).json({ message: err.message });
  }
});

module.exports = router;
