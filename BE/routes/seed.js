const express = require("express");
const router = express.Router();
const Character = require("../models/Character");

// Seed 6 nhân vật
router.get("/characters", async (req, res) => {
  try {
    await Character.deleteMany({});

    const characters = [
      {
        CharacterName: "RYU DAIKI",
        Planet: "Warrior",
        PrefabKey: "ryu",
        BasePower: 1000,
        BaseHp: 120,
        BaseKi: 60,
        BaseDamage: 15
      },
      {
        CharacterName: "LUNA BLADE",
        Planet: "Warrior",
        PrefabKey: "luna",
        BasePower: 950,
        BaseHp: 110,
        BaseKi: 70,
        BaseDamage: 14
      },
      {
        CharacterName: "GRIMJAW",
        Planet: "Beast",
        PrefabKey: "grim",
        BasePower: 900,
        BaseHp: 150,
        BaseKi: 40,
        BaseDamage: 20
      },
      {
        CharacterName: "ZIKK FANG",
        Planet: "Beast",
        PrefabKey: "zikk",
        BasePower: 880,
        BaseHp: 140,
        BaseKi: 45,
        BaseDamage: 18
      },
      {
        CharacterName: "ELDRIA",
        Planet: "Mage",
        PrefabKey: "eldria",
        BasePower: 1100,
        BaseHp: 80,
        BaseKi: 120,
        BaseDamage: 25
      },
      {
        CharacterName: "MOROK",
        Planet: "Mage",
        PrefabKey: "morok",
        BasePower: 1050,
        BaseHp: 85,
        BaseKi: 115,
        BaseDamage: 22
      }
    ];

    await Character.insertMany(characters);

    res.json({
      success: true,
      message: "Seed thành công! Đã thêm 6 nhân vật vào MongoDB.",
      count: characters.length
    });

  } catch (err) {
    res.status(500).json({ success: false, error: err.message });
  }
});

module.exports = router;
