const { execSync } = require("child_process");
const fs = require("fs");
const path = require("path");
const https = require("https");

const VERSION = require("../package.json").version;
const REPO = "j-carv/devonic";

const PLATFORM_MAP = {
  win32: { rid: "win-x64", archive: "zip", binary: "Devonic.CLI.exe" },
  linux: { rid: "linux-x64", archive: "tar.gz", binary: "Devonic.CLI" },
  darwin: { rid: "osx-x64", archive: "tar.gz", binary: "Devonic.CLI" },
};

async function install() {
  const platform = PLATFORM_MAP[process.platform];
  if (!platform) {
    console.error(`Unsupported platform: ${process.platform}`);
    process.exit(1);
  }

  const binDir = path.join(__dirname, "..", "bin");
  const binaryPath = path.join(binDir, platform.binary);

  if (fs.existsSync(binaryPath)) {
    return;
  }

  const archiveName = `devonic-${platform.rid}.${platform.archive}`;
  const url = `https://github.com/${REPO}/releases/download/v${VERSION}/${archiveName}`;
  const archivePath = path.join(binDir, archiveName);

  console.log(`Downloading devonic v${VERSION} for ${process.platform}...`);

  await download(url, archivePath);

  console.log("Extracting...");

  if (platform.archive === "zip") {
    execSync(`powershell -Command "Expand-Archive -Path '${archivePath}' -DestinationPath '${binDir}' -Force"`, {
      stdio: "inherit",
    });
  } else {
    execSync(`tar -xzf "${archivePath}" -C "${binDir}"`, {
      stdio: "inherit",
    });
  }

  fs.unlinkSync(archivePath);

  if (process.platform !== "win32") {
    fs.chmodSync(binaryPath, 0o755);
  }

  console.log("devonic installed successfully.");
}

function download(url, dest) {
  return new Promise((resolve, reject) => {
    const follow = (url) => {
      https
        .get(url, { headers: { "User-Agent": "devonic-npm" } }, (res) => {
          if (res.statusCode >= 300 && res.statusCode < 400 && res.headers.location) {
            follow(res.headers.location);
            return;
          }

          if (res.statusCode !== 200) {
            reject(new Error(`Download failed: HTTP ${res.statusCode}\nURL: ${url}\n\nMake sure version v${VERSION} exists in GitHub Releases.`));
            return;
          }

          const file = fs.createWriteStream(dest);
          res.pipe(file);
          file.on("finish", () => file.close(resolve));
          file.on("error", reject);
        })
        .on("error", reject);
    };
    follow(url);
  });
}

install().catch((err) => {
  console.error("Failed to install devonic:", err.message);
  console.error("\nYou can install manually:");
  console.error("  1. Download from https://github.com/j-carv/devonic/releases");
  console.error("  2. Or install via .NET: dotnet tool install -g Devonic");
  process.exit(1);
});
