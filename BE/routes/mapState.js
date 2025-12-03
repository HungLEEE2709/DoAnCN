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
        // Update PlayerInfo (Stats)
        const PlayerInfo = require("../models/PlayerInfo");
        const updateFields = {};
        if (Vang !== undefined) updateFields.Vang = Vang;
        if (TiemNang !== undefined) updateFields.TiemNang = TiemNang;
        if (SucManh !== undefined) updateFields.SucManh = SucManh;

        // Add new stats
        const { MaxHp, MaxKi, Dame } = req.body;
        if (MaxHp !== undefined) {
            updateFields.MaxHp = MaxHp;
            // updateFields.Hp = MaxHp; // REMOVED: Don't reset current HP
        }
        if (MaxKi !== undefined) {
            updateFields.MaxKi = MaxKi;
            // updateFields.Ki = MaxKi; // REMOVED: Don't reset current Ki
        }
        if (Dame !== undefined) updateFields.Dame = Dame;

        if (Object.keys(updateFields).length > 0) {
            await PlayerInfo.findOneAndUpdate(
                { idUser, CharacterChosen: true }, // Only update chosen character
                { $set: updateFields },
                { new: true }
            );
            console.log(`Updated Stats for ${idUser}:`, updateFields);
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
