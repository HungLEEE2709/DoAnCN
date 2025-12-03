const express = require("express");
const router = express.Router();
const MapState = require("../models/MapState");

// Save Map State
router.post("/save", async (req, res) => {
    try {
        const { idUser, PlayerPosition, Enemies, Vang, TiemNang, SucManh } = req.body;

        if (!idUser) return res.status(400).json({ success: false, message: "Missing idUser" });

        // Update MapState
        let state = await MapState.findOne({ idUser });

        if (state) {
            state.PlayerPosition = PlayerPosition;
            state.Enemies = Enemies;
            await state.save();
        } else {
            state = await MapState.create({ idUser, PlayerPosition, Enemies });
        }

        // Update PlayerInfo (Stats)
        if (Vang !== undefined && TiemNang !== undefined && SucManh !== undefined) {
            const PlayerInfo = require("../models/PlayerInfo");
            await PlayerInfo.findOneAndUpdate(
                { idUser },
                { Vang, TiemNang, SucManh },
                { new: true }
            );
            console.log(`Updated Stats for ${idUser}: Vang=${Vang}, TiemNang=${TiemNang}, SucManh=${SucManh}`);
        }

        res.json({ success: true, state });
    } catch (err) {
        res.status(500).json({ success: false, error: err.message });
    }
});

// Load Map State
router.get("/load/:idUser", async (req, res) => {
    try {
        const state = await MapState.findOne({ idUser: req.params.idUser });
        if (state) {
            res.json({ success: true, PlayerPosition: state.PlayerPosition, Enemies: state.Enemies });
        } else {
            res.json({ success: false, message: "No state found" });
        }
    } catch (err) {
        res.status(500).json({ success: false, error: err.message });
    }
});

module.exports = router;
