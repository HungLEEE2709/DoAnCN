const express = require("express");
const dotenv = require("dotenv");
dotenv.config(); // Load env vars immediately

const connectDB = require("./config/db");

connectDB();

const app = express();

// ---- BODY PARSERS MUST COME FIRST ----
app.use(express.json()); // <-- JSON body
app.use(express.urlencoded({ extended: true })); // <-- FORM body (x-www-form-urlencoded)

// ---- VIEW ENGINE SETUP (EJS) ----
app.set("view engine", "ejs");
app.use(express.static("public")); // Serve static files (css, images)

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

// ---- WEB ROUTES (Frontend) ----
app.use("/", require("./routes/web"));

// ---- START SERVER ----
const PORT = process.env.PORT || 5000;
app.listen(PORT, () => {
  console.log(`🚀 Server chạy tại http://localhost:${PORT}`);
  console.log("DEBUG: Web routes should be mounted at /");
});
