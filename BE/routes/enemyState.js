const express = require("express");
const router = express.Router();
const EnemyState = require("../models/EnemyState");

// Save Enemy States
router.post("/save", async (req, res) => {
    try {
        const { idUser, States } = req.body;

        if (!idUser) return res.status(400).json({ success: false, message: "Missing idUser" });

        let state = await EnemyState.findOne({ idUser });

        if (state) {
            state.States = States;
            await state.save();
        } else {
            state = await EnemyState.create({ idUser, States });
        }

        res.json({ success: true, state });
    } catch (err) {
        res.status(500).json({ success: false, error: err.message });
    }
});

// Load Enemy States
router.get("/load/:idUser", async (req, res) => {
    try {
        const state = await EnemyState.findOne({ idUser: req.params.idUser });
        res.json({ success: true, States: state ? state.States : [] });
    } catch (err) {
        res.status(500).json({ success: false, error: err.message });
    }
});

module.exports = router;
