const mongoose = require("mongoose");

const itemSchema = new mongoose.Schema({
  idItem: { type: Number, required: true, unique: true },
  ItemName: { type: String, required: true },
  MoTa: { type: String }, 
  HanSuDung: { type: Date }, 
  idUser: { type: Number, ref: "User" },
});

module.exports = mongoose.model("Item", itemSchema);
