#!/usr/bin/env node

const { execFileSync } = require("child_process");
const path = require("path");
const fs = require("fs");

const binDir = __dirname;
const binaryName = process.platform === "win32" ? "Devonic.CLI.exe" : "Devonic.CLI";
const binaryPath = path.join(binDir, binaryName);

if (!fs.existsSync(binaryPath)) {
  console.error("devonic binary not found. Try reinstalling: npm install -g devonic");
  process.exit(1);
}

try {
  execFileSync(binaryPath, process.argv.slice(2), { stdio: "inherit" });
} catch (err) {
  process.exit(err.status ?? 1);
}
