const mongoose = require("mongoose");

const characterSchema = new mongoose.Schema({
  CharacterName: { type: String, required: true },  
  Planet: { type: String, required: true },     
  PrefabKey: { type: String, required: true },   
  BasePower: { type: Number, required: true },
  BaseHp: { type: Number, required: true },
  BaseKi: { type: Number, required: true },
  BaseDamage: { type: Number, required: true }
});

module.exports = mongoose.model("Character", characterSchema, "Character");
