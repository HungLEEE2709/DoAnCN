const express = require("express");
const router = express.Router();
const PlayerInfo = require("../models/PlayerInfo");

router.get("/:idUser", async (req, res) => {
  try {
    const idUser = req.params.idUser;

    const player = await PlayerInfo.findOne({ idUser });

    if (!player)
      return res.status(404).json({ message: "Không tìm thấy PlayerInfo!" });

    return res.json(player);
  } catch (err) {
    res.status(500).json({ message: err.message });
  }
});

module.exports = router;
