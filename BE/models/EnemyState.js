const mongoose = require("mongoose");

const enemyStateSchema = new mongoose.Schema({
    idUser: { type: String, required: true, ref: "User" },
    States: [
        {
            id: Number,
            x: Number,
            y: Number,
            hp: Number,
            isDead: Boolean
        }
    ]
});

module.exports = mongoose.model("EnemyState", enemyStateSchema, "EnemyState");
