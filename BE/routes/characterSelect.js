const express = require("express");
const router = express.Router();
const PlayerInfo = require("../models/PlayerInfo");
const Character = require("../models/Character");

router.post("/select", async (req, res) => {
  try {
    const { idUser, Planet, CharacterName } = req.body;

    if (!idUser || !Planet || !CharacterName)
      return res.status(400).json({ message: "Thiếu dữ liệu gửi lên!" });

    // 1. Lấy PlayerInfo
    const player = await PlayerInfo.findOne({ idUser });
    if (!player)
      return res.status(404).json({ message: "Không tìm thấy người chơi!" });

    if (player.CharacterChosen)
      return res.status(400).json({ message: "Nhân vật đã được chọn rồi!" });

    const characterData = await Character.findOne({ CharacterName });
    if (!characterData)
      return res.status(404).json({ message: "Không tìm thấy dữ liệu nhân vật!" });

    player.Planet = Planet;                              
    player.CharacterName = CharacterName;
    player.CharacterChosen = true;
    player.PrefabKey = characterData.PrefabKey;

    player.Hp = characterData.BaseHp;
    player.Ki = characterData.BaseKi;
    player.SucManh = characterData.BasePower;
    player.Dame = characterData.BaseDamage;

    await player.save();

    res.json({
      message: "Chọn nhân vật thành công!",
      data: player,
    });

  } catch (err) {
    console.log(err);
    res.status(500).json({ message: err.message });
  }
});

module.exports = router;
