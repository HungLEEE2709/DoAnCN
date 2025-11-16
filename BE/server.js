const express = require("express");
const dotenv = require("dotenv");
const connectDB = require("./config/db");
const characterSelectRoutes = require("./routes/characterSelect");
const playerInfoRoutes = require("./routes/playerInfo");
const itemRoutes = require("./routes/item");


dotenv.config();
connectDB();

const app = express();
app.use(express.json());

// Routes
app.use("/api/users", require("./routes/userRoutes"));
app.use("/api/character", characterSelectRoutes);
app.use("/api/player-info", playerInfoRoutes);
app.use("/api/items", itemRoutes);

const PORT = process.env.PORT || 5000;

app.listen(PORT, () => {
  console.log(`🚀 Server chạy tại http://localhost:${PORT}`);
});
