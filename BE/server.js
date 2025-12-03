const express = require("express");
const dotenv = require("dotenv");

dotenv.config(); // Load env vars before requiring other files

const connectDB = require("./config/db");

connectDB();

const app = express();

// ---- BODY PARSERS MUST COME FIRST ----
app.use(express.json()); // <-- JSON body
app.use(express.urlencoded({ extended: true })); // <-- FORM body (x-www-form-urlencoded)

// -----------------------------------------
console.log("Loading all routes...");

// ---- ROUTES ----
app.use("/api/users", require("./routes/userRoutes"));
app.use("/api/character", require("./routes/characterSelect"));
app.use("/api/playerInfo", require("./routes/playerInfo"));
app.use("/api/items", require("./routes/item"));
app.use("/api/seed", require("./routes/seed"));
app.use("/api/enemystate", require("./routes/enemyState"));
app.use("/api/mapstate", require("./routes/mapState"));

console.log("🔗 Mounting /api/inventory ...");
app.use("/api/inventory", require("./routes/inventory"));
console.log("✔ /api/inventory OK");

// ---- START SERVER ----
const PORT = process.env.PORT || 5000;
app.listen(PORT, () => {
  console.log(`🚀 Server chạy tại http://localhost:${PORT}`);
});
