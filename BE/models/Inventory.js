const mongoose = require("mongoose");

const SlotSchema = new mongoose.Schema({
  itemId: { type: String, default: null },
  quantity: { type: Number, default: 0 }
}, { _id: false });

const InventorySchema = new mongoose.Schema({
  userId: { type: String, required: true, unique: true },
  slots: { 
    type: [SlotSchema], 
    default: function () {
      return Array.from({ length: 12 }, () => ({
        itemId: null,
        quantity: 0
      }));
    }
  }
});

module.exports = mongoose.model("Inventory", InventorySchema);
