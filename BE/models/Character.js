const mongoose = require("mongoose");

const characterSchema = new mongoose.Schema({
  CharacterName: { type: String, required: true },
  Planet: { type: String, required: true },
  BaseHp: Number,
  BasePower: Number,
  BaseKi: Number,
  BaseDamage: Number,
  PrefabKey: String   
});

module.exports = mongoose.model("Character", characterSchema);
