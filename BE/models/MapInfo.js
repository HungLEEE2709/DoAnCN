const mongoose = require("mongoose");

const mapInfoSchema = new mongoose.Schema({
  idMap: { type: Number, required: true, unique: true },
  MapName: { type: String, required: true },
  idUser: { type: Number, ref: "User" },
  idMoster: { type: Number, ref: "Enemy" },
});

module.exports = mongoose.model("MapInfo", mapInfoSchema);
