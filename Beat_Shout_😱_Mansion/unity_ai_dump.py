import os

# ===== 設定 =====

TARGET_DIR = "./"              # Unityプロジェクトルート
OUTPUT_FILE = "unity_project_dump.md"

MAX_FILE_SIZE = 100_000       # 100KB以上はスキップ

ALLOWED_EXT = {
    ".cs", ".shader", ".compute",
    ".cginc", ".hlsl",
    ".py", ".json", ".txt",
    ".md", ".xml", ".yaml",
    ".yml", ".js"
}

IGNORE_DIRS = {
    ".git",
    "Library",
    "Logs",
    "Temp",
    "Obj",
    "Build",
    "UserSettings",
    "Packages/PackageCache",
    "Assets/CRIMW",
    "Assets/Plugins/Demigiant/DOTween/Modules",
    "Assets/Rewired/",
    "Assets/Mains/Prefabs/Level/StaticObjects/DownloadAssets/Diabolical Games"
}

IGNORE_EXT = {
    ".meta",
    ".png", ".jpg", ".jpeg",
    ".tga", ".psd",
    ".fbx", ".wav",
    ".mp3", ".ogg"
}

def is_ignored_dir(root, path, d):
    full_path = os.path.join(path, d)
    rel_path = os.path.relpath(full_path, root).replace("\\", "/")
    return any(rel_path.startswith(ignore) for ignore in IGNORE_DIRS)

# ===== ファイル収集 =====

def collect_files(root):

    files = []

    for path, dirs, filenames in os.walk(root):

        dirs[:] = [d for d in dirs if not is_ignored_dir(root, path, d)]

        for name in filenames:

            ext = os.path.splitext(name)[1].lower()

            if ext in IGNORE_EXT:
                continue

            if ext not in ALLOWED_EXT:
                continue

            full = os.path.join(path, name)

            try:
                size = os.path.getsize(full)
            except:
                continue

            if size > MAX_FILE_SIZE:
                continue

            rel = os.path.relpath(full, root)

            files.append((rel, full))

    return sorted(files)

# ===== tree作成 =====

def build_tree(root):

    lines = []

    for path, dirs, files in os.walk(root):

        dirs[:] = [d for d in dirs if not is_ignored_dir(root, path, d)]

        level = path.replace(root, "").count(os.sep)

        indent = "  " * level

        folder = os.path.basename(path)

        if folder:
            lines.append(f"{indent}{folder}/")

        subindent = "  " * (level + 1)

        for f in files:

            ext = os.path.splitext(f)[1].lower()

            if ext in IGNORE_EXT:
                continue

            if ext not in ALLOWED_EXT:
                continue

            lines.append(f"{subindent}{f}")

    return "\n".join(lines)

# ===== markdown出力 =====

def dump_markdown(files, tree):

    with open(OUTPUT_FILE, "w", encoding="utf8") as out:

        out.write("# Unity Project Dump for AI\n\n")

        out.write("## Project Tree\n\n")
        out.write("```\n")
        out.write(tree)
        out.write("\n```\n\n")

        out.write("## File List\n\n")

        for rel, _ in files:
            out.write(f"- {rel}\n")

        out.write("\n---\n\n")

        for rel, full in files:

            out.write(f"## {rel}\n\n")

            ext = os.path.splitext(rel)[1][1:]

            out.write(f"```{ext}\n")

            try:
                with open(full, "r", encoding="utf8") as f:
                    out.write(f.read())
            except:
                out.write("[Unreadable File]")

            out.write("\n```\n\n---\n\n")

# ===== main =====

def main():

    print("Scanning project...")

    files = collect_files(TARGET_DIR)

    tree = build_tree(TARGET_DIR)

    dump_markdown(files, tree)

    print(f"Done. {len(files)} files exported.")
    print(f"Output: {OUTPUT_FILE}")

if __name__ == "__main__":
    main()