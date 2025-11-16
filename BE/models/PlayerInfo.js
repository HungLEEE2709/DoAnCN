const mongoose = require("mongoose");

const playerInfoSchema = new mongoose.Schema({
  idUser: { type: String, required: true, ref: "User" },
  UserName: { type: String, required: true },  
  SucManh: { type: Number, default: 1000 }, 
  //TiemNang: { type: Number, default: 0 }, 
  Hp: { type: Number, default: 100 },
  Ki: { type: Number, default: 50 },
  Dame: { type: Number, default: 5 },
  Planet: { type: String, default: null },
  CharacterName: { type: String, default: null },
  CharacterChosen: { type: Boolean, default: false },
  PrefabKey: { type: String, default: null },
});

module.exports = mongoose.model("PlayerInfo", playerInfoSchema, "PlayerInfo");
