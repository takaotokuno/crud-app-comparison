import { execFileSync } from "node:child_process";
import { readFileSync } from "node:fs";
import { extname } from "node:path";

const textExtensions = new Set([
  ".css",
  ".js",
  ".jsx",
  ".json",
  ".md",
  ".mjs",
  ".ts",
  ".tsx",
  ".yml",
  ".yaml",
]);

const files = execFileSync("git", ["ls-files"], { encoding: "utf8" })
  .trim()
  .split("\n")
  .filter(Boolean)
  .filter((file) => textExtensions.has(extname(file)));

const errors = [];

for (const file of files) {
  const content = readFileSync(file, "utf8");

  if (content.includes("\r")) {
    errors.push(`${file}: uses CRLF line endings; expected LF`);
  }

  if (content.length > 0 && !content.endsWith("\n")) {
    errors.push(`${file}: missing final newline`);
  }

  const lines = content.split("\n");
  lines.forEach((line, index) => {
    if (/[ \t]$/.test(line)) {
      errors.push(`${file}:${index + 1}: trailing whitespace`);
    }

    if (line.startsWith("\t")) {
      errors.push(`${file}:${index + 1}: tab indentation; expected spaces`);
    }
  });
}

if (errors.length > 0) {
  console.error("Format check failed:");
  for (const error of errors) {
    console.error(`- ${error}`);
  }
  process.exit(1);
}

console.log(`Format check passed for ${files.length} files.`);
