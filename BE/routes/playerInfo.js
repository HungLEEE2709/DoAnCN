
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
      MaxHp: template.BaseHp,
      Ki: template.BaseKi,
      MaxKi: template.BaseKi,
      Dame: template.BaseDamage,
      CharacterChosen: true
    });

    res.json({ success: true, player });
  } catch (err) {
    res.status(500).json({ success: false, error: err.message });
  }
});
// 5) Cập nhật HP / KI của nhân vật đang chọn
router.post("/updatestats", async (req, res) => {
  try {
    const { idUser, Hp, Ki } = req.body;

    if (!idUser)
      return res.status(400).json({ success: false, message: "Missing idUser" });

    const player = await PlayerInfo.findOneAndUpdate(
      { idUser, CharacterChosen: true },
      { $set: { Hp, Ki } },
      { new: true }
    );

    if (!player)
      return res
        .status(404)
        .json({ success: false, message: "Player not found" });

    res.json({ success: true, player });
  } catch (err) {
    res.status(500).json({ success: false, error: err.message });
  }
});
router.post("/update-stats", async (req, res) => {
  try {
    const { idUser, Hp, Ki, SucManh, TiemNang } = req.body;

    if (!idUser)
      return res.status(400).json({ success: false, message: "Missing idUser" });

    const updateData = { Hp, Ki };
    if (SucManh !== undefined) updateData.SucManh = SucManh;
    if (TiemNang !== undefined) updateData.TiemNang = TiemNang;

    const player = await PlayerInfo.findOneAndUpdate(
      { idUser, CharacterChosen: true },
      { $set: updateData },
      { new: true }
    );

    if (!player)
      return res.status(404).json({ success: false, message: "Player not found" });

    res.json({ success: true, player });
  } catch (err) {
    res.status(500).json({ success: false, error: err.message });
  }
});

// 6) Cộng tiềm năng (TiemNang -> Stat)
router.post("/add-potential", async (req, res) => {
  try {
    const { idUser, statType } = req.body; // statType: "hp", "ki", "sd"

    if (!idUser)
      return res.status(400).json({ success: false, message: "Missing idUser" });

    const player = await PlayerInfo.findOne({ idUser, CharacterChosen: true });
    if (!player)
      return res.status(404).json({ success: false, message: "Player not found" });

    let cost = 0;
    let gain = 0;

    // Logic tính cost và gain
    switch (statType) {
      case "hp":
        // Cost = MaxHp * 102%
        cost = Math.floor(player.MaxHp * 1.02);
        gain = 20;
        break;

      case "ki":
        // Cost = MaxKi * 102%
        cost = Math.floor(player.MaxKi * 1.02);
        gain = 20;
        break;

      case "sd":
        // Cost = Dame * 200%
        cost = Math.floor(player.Dame * 2.0);
        gain = 5;
        break;

      default:
        return res.status(400).json({ success: false, message: "Invalid statType" });
    }

    // Kiểm tra đủ điểm không
    if (player.TiemNang < cost) {
      return res.status(400).json({
        success: false,
        message: `Không đủ điểm tiềm năng! Cần ${cost} điểm.`
      });
    }

    // Trừ điểm và cộng chỉ số
    player.TiemNang -= cost;

    if (statType === "hp") {
      player.MaxHp += gain;
      // player.Hp += gain; // Giữ nguyên máu hiện tại
    } else if (statType === "ki") {
      player.MaxKi += gain;
      // player.Ki += gain; // Giữ nguyên ki hiện tại
    } else if (statType === "sd") {
      player.Dame += gain;
      player.SucManh += gain; // Tăng sức mạnh tổng (nếu cần)
    }

    await player.save();

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

    // 1. Get Base Stats from Character collection
    const template = await Character.findOne({ CharacterName });
    if (!template) {
      return res.status(404).json({ success: false, message: "Character template not found!" });
    }

    // 2. Deselect others
    await PlayerInfo.updateMany(
      { idUser },
      { $set: { CharacterChosen: false } }
    );
    return res
      .status(404)
      .json({ success: false, message: "Character does not exist for this user!" });

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
    // Tìm nhân vật đã tạo (có tên nhân vật)
    const player = await PlayerInfo.findOne({
      idUser: req.params.idUser,
      CharacterName: { $ne: null }
    });

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
