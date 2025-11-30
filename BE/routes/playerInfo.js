
const express = require("express");
const router = express.Router();
const PlayerInfo = require("../models/PlayerInfo");
const Character = require("../models/Character");
const Inventory = require("../models/Inventory");

// 1) Tạo nhân vật mới cho user
router.post("/create", async (req, res) => {
  try {
    const data = req.body; // { idUser, UserName, CharacterName }

    // check trùng
    const existed = await PlayerInfo.findOne({
      idUser: data.idUser,
      CharacterName: data.CharacterName
    });
    if (existed) {
      return res
        .status(400)
        .json({ success: false, message: "Nhân vật này đã tồn tại cho user" });
    }

    // lấy data mẫu từ Character DB
    const template = await Character.findOne({
      CharacterName: data.CharacterName
    });

    if (!template)
      return res
        .status(404)
        .json({ success: false, message: "Character template not found" });

    // bỏ chọn tất cả nhân vật cũ của user
    await PlayerInfo.updateMany(
      { idUser: data.idUser },
      { $set: { CharacterChosen: false } }
    );

    // tạo nhân vật mới, đặt luôn là CharacterChosen = true
    const player = await PlayerInfo.create({
      idUser: data.idUser,
      UserName: data.UserName,
      Planet: template.Planet,
      CharacterName: data.CharacterName,
      PrefabKey: template.PrefabKey,
      SucManh: template.BasePower,
      Hp: template.BaseHp,
      Ki: template.BaseKi,
      Dame: template.BaseDamage,
      CharacterChosen: true
    });

    res.json({ success: true, player });
  } catch (err) {
    res.status(500).json({ success: false, error: err.message });
  }
});

// 2) Lấy danh sách tất cả nhân vật của user
router.get("/list/:idUser", async (req, res) => {
  try {
    const players = await PlayerInfo.find({ idUser: req.params.idUser });
    res.json({ success: true, players });
  } catch (err) {
    res.status(500).json({ success: false, error: err.message });
  }
});

// 3) User chọn 1 nhân vật để chơi
router.post("/select", async (req, res) => {
  try {
    const { idUser, CharacterName } = req.body;

    // bỏ chọn các nhân vật khác
    await PlayerInfo.updateMany(
      { idUser },
      { $set: { CharacterChosen: false } }
    );

    // chọn nhân vật hiện tại
    const chosen = await PlayerInfo.findOneAndUpdate(
      { idUser, CharacterName },
      { $set: { CharacterChosen: true } },
      { new: true }
    );

    if (!chosen)
      return res
        .status(404)
        .json({ success: false, message: "Character does not exist!" });

    res.json({ success: true, chosen });
  } catch (err) {
    res.status(500).json({ success: false, error: err.message });
  }
});

// 4) Lấy nhân vật đang được chọn để vào game
router.get("/chosen/:idUser", async (req, res) => {
  try {
    const selected = await PlayerInfo.findOne({
      idUser: req.params.idUser,
      CharacterChosen: true
    });

    res.json({ success: true, player: selected });
  } catch (err) {
    res.status(500).json({ success: false, error: err.message });
  }
});

router.get("/check/:idUser", async (req, res) => {
  try {
    const player = await PlayerInfo.findOne({ idUser: req.params.idUser });

    const created = !!(
      player &&
      player.CharacterName &&
      player.PrefabKey &&
      player.Planet
    );

    res.json({
      success: true,
      created: created,   // giờ là boolean thật sự
      player: player || null
    });

  } catch (err) {
    res.status(500).json({ success: false, error: err.message });
  }
});

module.exports = router;
