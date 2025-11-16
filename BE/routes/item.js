const express = require("express");
const router = express.Router();
const Item = require("../models/Item");

router.get("/:idUser", async (req, res) => {
  try {
    const items = await Item.find({ idUser: req.params.idUser });
    res.json(items);
  } catch (err) {
    res.status(500).json({ message: err.message });
  }
});

module.exports = router;
