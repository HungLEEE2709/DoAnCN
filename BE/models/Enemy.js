const mongoose = require("mongoose");

const enemySchema = new mongoose.Schema({
  idMoster: { type: Number, required: true, unique: true },
  MosterName: { type: String, required: true },
  Hp: { type: Number, default: 50 },
  Dame: { type: Number, default: 10 },
});

module.exports = mongoose.model("Enemy", enemySchema);
