const express = require("express");
const router = express.Router();
const Inventory = require("../models/Inventory");

console.log(">>> ROUTER INVENTORY LOADED <<<");


// ============================
//  1) GET INVENTORY (AUTO CREATE IF NOT EXISTS)
// ============================
router.get("/:userId", async (req, res) => {
  try {
    const userId = req.params.userId;

    if (!userId)
      return res.status(400).json({ success: false, message: "Missing userId" });

    let inv = await Inventory.findOne({ userId });

    // 🔥 Nếu chưa có hành trang → tự tạo 12 slot rỗng
    if (!inv) {
      console.log("Creating new inventory for user:", userId);

      const emptySlots = Array.from({ length: 12 }, () => ({
        itemId: null,
        quantity: 0
      }));

      inv = await Inventory.create({
        userId,
        slots: emptySlots
      });
    }

    return res.json({ success: true, inventory: inv });

  } catch (err) {
    console.error(err);
    res.status(500).json({ success: false, error: err.message });
  }
});


// ============================
//  2) SAVE INVENTORY
// ============================
router.post("/save", async (req, res) => {
  try {
    const { userId, slots } = req.body;

    if (!userId)
      return res.status(400).json({ success: false, message: "Missing userId" });

    if (!slots || !Array.isArray(slots) || slots.length !== 12)
      return res.status(400).json({ success: false, message: "Inventory must have 12 slots" });

    const inv = await Inventory.findOneAndUpdate(
      { userId },
      { $set: { slots } },
      { new: true }
    );

    if (!inv)
      return res.status(404).json({ success: false, message: "Inventory not found" });

    res.json({ success: true, inventory: inv });

  } catch (err) {
    console.error(err);
    res.status(500).json({ success: false, error: err.message });
  }
});


// ============================
//  3) ADD ITEM
// ============================
router.post("/add", async (req, res) => {
  try {
    const { userId, itemId, quantity } = req.body;

    if (!userId || !itemId)
      return res.status(400).json({ success: false, message: "Missing userId or itemId" });

    let inv = await Inventory.findOne({ userId });

    // 🔥 Auto create nếu chưa có inventory
    if (!inv) {
      const emptySlots = Array.from({ length: 12 }, () => ({
        itemId: null,
        quantity: 0
      }));

      inv = await Inventory.create({ userId, slots: emptySlots });
    }

    // Stack item
    let slot = inv.slots.find(s => s.itemId === itemId);

    if (slot) {
      slot.quantity += quantity;
    } else {
      let empty = inv.slots.find(s => s.itemId === null);
      if (!empty)
        return res.status(400).json({ success: false, message: "Inventory full" });

      empty.itemId = itemId;
      empty.quantity = quantity;
    }

    await inv.save();
    res.json({ success: true, inventory: inv });

  } catch (err) {
    console.error(err);
    res.status(500).json({ success: false, error: err.message });
  }
});



// ============================
//  EXPORT ROUTER
// ============================
module.exports = router;
