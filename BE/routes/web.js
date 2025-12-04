const express = require("express");
const router = express.Router();
const User = require("../models/User"); // Adjust path if needed

// 1. Landing Page
router.get("/", (req, res) => {
    res.render("index", { title: "Azura Land - Trang Chủ" });
});

// 2. Download Page
router.get("/download", (req, res) => {
    res.render("download", { title: "Tải Game - Azura Land" });
});

// 3. Login Page (Web)
router.get("/login", (req, res) => {
    res.render("login", { title: "Đăng Nhập", error: null });
});

// 4. Handle Login Logic
router.post("/login", async (req, res) => {
    const { username, password } = req.body;
    try {
        const user = await User.findOne({ username });
        if (!user) {
            return res.render("login", { title: "Đăng Nhập", error: "Tài khoản không tồn tại!" });
        }

        const bcrypt = require("bcryptjs");
        const isMatch = await bcrypt.compare(password, user.password);

        if (!isMatch) {
            return res.render("login", { title: "Đăng Nhập", error: "Sai mật khẩu!" });
        }

        // Success -> Redirect to Profile with ID
        res.redirect(`/profile/${user._id}`);

    } catch (err) {
        console.error(err);
        res.render("login", { title: "Đăng Nhập", error: "Lỗi Server" });
    }
});

// 5. Profile Page
router.get("/profile/:id", async (req, res) => {
    try {
        const user = await User.findById(req.params.id);
        if (!user) return res.redirect("/login");

        // Fetch Player Info (Character Stats)
        const PlayerInfo = require("../models/PlayerInfo");
        const player = await PlayerInfo.findOne({ idUser: user._id });

        res.render("profile", { title: "Hồ Sơ - " + user.username, user, player });
    } catch (err) {
        res.redirect("/login");
    }
});

// 6. Change Password Logic
router.post("/change-password", async (req, res) => {
    const { userId, oldPassword, newPassword } = req.body;
    try {
        const user = await User.findById(userId);
        if (!user) return res.redirect("/login");

        const bcrypt = require("bcryptjs");
        const isMatch = await bcrypt.compare(oldPassword, user.password);
        if (!isMatch) {
            return res.send(`<script>alert('Mật khẩu cũ không đúng!'); window.location.href='/profile/${userId}';</script>`);
        }

        const salt = await bcrypt.genSalt(10);
        user.password = await bcrypt.hash(newPassword, salt);
        await user.save();

        res.send(`<script>alert('Đổi mật khẩu thành công!'); window.location.href='/profile/${userId}';</script>`);
    } catch (err) {
        console.error(err);
        res.redirect("/login");
    }
});

// 7. Forgot Password Page
router.get("/forgot-password", (req, res) => {
    res.render("forgot-password", { title: "Quên Mật Khẩu", message: null });
});

// 8. Handle Forgot Password (Send OTP)
router.post("/forgot-password", async (req, res) => {
    const { email } = req.body;
    try {
        const user = await User.findOne({ email });
        if (!user) {
            return res.render("forgot-password", { title: "Quên Mật Khẩu", message: "Email không tồn tại!" });
        }

        // Generate OTP (6 digits)
        const otp = Math.floor(100000 + Math.random() * 900000).toString();
        const expires = Date.now() + 10 * 60 * 1000; // 10 minutes

        user.resetPasswordOtp = otp;
        user.resetPasswordExpires = expires;
        await user.save();

        // Send Email
        const nodemailer = require("nodemailer");

        // Config Transporter (Use Environment Variables in Production)
        const transporter = nodemailer.createTransport({
            service: "gmail",
            auth: {
                user: process.env.EMAIL_USER || "your-email@gmail.com", // Config on Render
                pass: process.env.EMAIL_PASS || "your-app-password",    // Config on Render
            },
        });

        const mailOptions = {
            from: '"Azura Land Support" <no-reply@azuraland.com>',
            to: email,
            subject: "Mã xác thực OTP - Azura Land",
            text: `Mã OTP của bạn là: ${otp}. Mã này có hiệu lực trong 10 phút.`,
            html: `<h3>Mã OTP Đặt Lại Mật Khẩu</h3><p>Mã của bạn là: <b style="font-size: 24px; color: #00d2ff;">${otp}</b></p><p>Mã này có hiệu lực trong 10 phút.</p>`,
        };

        // Try to send email
        try {
            await transporter.sendMail(mailOptions);
            console.log(`OTP sent to ${email}: ${otp}`);
        } catch (emailErr) {
            console.error("Email send error:", emailErr);
            // Fallback for testing without real email config
            console.log(`[TEST MODE] OTP for ${email}: ${otp}`);
        }

        res.render("verify-otp", { title: "Xác Thực OTP", email, message: null });

    } catch (err) {
        console.error(err);
        res.render("forgot-password", { title: "Quên Mật Khẩu", message: "Lỗi Server" });
    }
});

// 9. Verify OTP
router.post("/verify-otp", async (req, res) => {
    const { email, otp } = req.body;
    try {
        const user = await User.findOne({
            email,
            resetPasswordOtp: otp,
            resetPasswordExpires: { $gt: Date.now() }
        });

        if (!user) {
            return res.render("verify-otp", { title: "Xác Thực OTP", email, message: "Mã OTP không đúng hoặc đã hết hạn!" });
        }

        // OTP Valid -> Show Reset Password Form
        res.render("reset-password", { title: "Đặt Mật Khẩu Mới", email, otp, message: null });

    } catch (err) {
        console.error(err);
        res.render("verify-otp", { title: "Xác Thực OTP", email, message: "Lỗi Server" });
    }
});

// 10. Reset Password
router.post("/reset-password", async (req, res) => {
    const { email, otp, newPassword, confirmPassword } = req.body;

    if (newPassword !== confirmPassword) {
        return res.render("reset-password", { title: "Đặt Mật Khẩu Mới", email, otp, message: "Mật khẩu xác nhận không khớp!" });
    }

    try {
        const user = await User.findOne({
            email,
            resetPasswordOtp: otp,
            resetPasswordExpires: { $gt: Date.now() }
        });

        if (!user) {
            return res.render("forgot-password", { title: "Quên Mật Khẩu", message: "Phiên giao dịch hết hạn. Vui lòng thử lại." });
        }

        // Hash new password
        const bcrypt = require("bcryptjs");
        const salt = await bcrypt.genSalt(10);
        user.password = await bcrypt.hash(newPassword, salt);

        // Clear OTP
        user.resetPasswordOtp = undefined;
        user.resetPasswordExpires = undefined;
        await user.save();

        res.render("login", { title: "Đăng Nhập", error: "Đổi mật khẩu thành công! Vui lòng đăng nhập." });

    } catch (err) {
        console.error(err);
        res.render("reset-password", { title: "Đặt Mật Khẩu Mới", email, otp, message: "Lỗi Server" });
    }
});

module.exports = router;
