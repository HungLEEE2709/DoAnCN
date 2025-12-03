const mongoose = require("mongoose");

const mapStateSchema = new mongoose.Schema({
    idUser: { type: String, required: true, ref: "User" },
    PlayerPosition: {
        x: { type: Number, default: 0 },
        y: { type: Number, default: 0 }
    },
    Enemies: [
        {
            id: Number,
            x: Number,
            y: Number,
            hp: Number,
            isDead: Boolean
        }
    ]
});

module.exports = mongoose.model("MapState", mapStateSchema, "MapState");
