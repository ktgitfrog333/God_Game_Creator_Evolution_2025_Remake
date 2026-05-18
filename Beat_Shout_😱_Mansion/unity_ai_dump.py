import os

# ===== 設定 =====

# 複数の対象ディレクトリを指定可能（Unityプロジェクトルートからの相対パスまたは絶対パス）
TARGET_DIRS = [
    "./Assets/Mains/Scripts",
    "./Assets/Script_xylo",
    "./Assets/Mains/Scenes",
    # 必要に応じて追加:
    # "./Assets/Mains/Prefabs",
    # "./Assets/Shaders",
]

OUTPUT_FILE = "unity_project_dump.md"

MAX_FILE_SIZE = 100_000       # 100KB以上はスキップ

ALLOWED_EXT = {
    ".cs", ".shader", ".compute",
    ".cginc", ".hlsl",
    ".py", ".json", ".txt",
    ".md", ".xml", ".yaml",
    ".yml", ".js",
    ".unity", ".prefab", ".mat", ".asset",
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
    """指定されたディレクトリが無視対象かチェック"""
    full_path = os.path.join(path, d)
    rel_path = os.path.relpath(full_path, root).replace("\\", "/")
    return any(rel_path.startswith(ignore) for ignore in IGNORE_DIRS)

def is_ignored_path(file_path, root):
    """ファイルパス全体が無視対象かをチェック（特定パスの除外用）"""
    rel_path = os.path.relpath(file_path, root).replace("\\", "/")
    return any(rel_path.startswith(ignore) for ignore in IGNORE_DIRS)

# ===== ファイル収集（複数ディレクトリ対応） =====

def collect_files(target_dirs):
    """
    複数の対象ディレクトリからファイルを収集
    戻り値: (相対パス, 絶対パス, 所属ディレクトリ) のリスト
    """
    all_files = []
    
    for target_dir in target_dirs:
        # 絶対パスに変換（存在チェック用）
        abs_target = os.path.abspath(target_dir)
        
        if not os.path.exists(abs_target):
            print(f"警告: ディレクトリが存在しません - {target_dir}")
            continue
        
        if not os.path.isdir(abs_target):
            print(f"警告: ディレクトリではありません - {target_dir}")
            continue
        
        print(f"スキャン中: {target_dir}")
        
        for path, dirs, filenames in os.walk(abs_target):
            # 無視ディレクトリをスキップ
            dirs[:] = [d for d in dirs if not is_ignored_dir(abs_target, path, d)]
            
            for name in filenames:
                ext = os.path.splitext(name)[1].lower()
                
                if ext in IGNORE_EXT:
                    continue
                
                if ext not in ALLOWED_EXT:
                    continue
                
                full = os.path.join(path, name)
                
                # ファイルパス全体が無視対象かをチェック
                if is_ignored_path(full, abs_target):
                    continue
                
                try:
                    size = os.path.getsize(full)
                except:
                    continue
                
                if size > MAX_FILE_SIZE:
                    continue
                
                # 元の指定ディレクトリからの相対パスを取得
                rel = os.path.relpath(full, abs_target)
                # どのベースディレクトリからのものか記録
                base_name = os.path.basename(abs_target.rstrip('/\\'))
                
                all_files.append((rel, full, base_name, target_dir))
    
    # ベースディレクトリ名＋相対パスでソート
    return sorted(all_files, key=lambda x: (x[2], x[0]))

# ===== tree作成（複数ディレクトリ対応） =====

def build_tree(target_dirs):
    """
    複数ディレクトリのツリー構造を1つに合成
    """
    lines = ["Project Root (Specified Directories Only):", ""]
    
    for target_dir in target_dirs:
        abs_target = os.path.abspath(target_dir)
        
        if not os.path.exists(abs_target) or not os.path.isdir(abs_target):
            continue
        
        dir_name = os.path.basename(abs_target.rstrip('/\\'))
        lines.append(f"{dir_name}/")
        
        _build_tree_recursive(abs_target, lines, level=1)
        lines.append("")  # ディレクトリ間の区切り
    
    return "\n".join(lines)

def _build_tree_recursive(current_path, lines, level):
    """再帰的にツリーを構築"""
    indent = "  " * level
    
    try:
        entries = sorted(os.listdir(current_path))
    except:
        return
    
    dirs = []
    files = []
    
    for entry in entries:
        full_entry = os.path.join(current_path, entry)
        ext = os.path.splitext(entry)[1].lower()
        
        # 無視対象のチェック
        if is_ignored_path(full_entry, current_path):
            continue
        
        if os.path.isdir(full_entry):
            # 無視ディレクトリをスキップ
            if not is_ignored_dir(current_path, current_path, entry):
                dirs.append(entry)
        else:
            if ext not in IGNORE_EXT and ext in ALLOWED_EXT:
                files.append(entry)
    
    for d in dirs:
        lines.append(f"{indent}{d}/")
        _build_tree_recursive(os.path.join(current_path, d), lines, level + 1)
    
    for f in files:
        lines.append(f"{indent}{f}")

# ===== markdown出力 =====

def dump_markdown(files, tree, target_dirs):
    """
    収集したファイルをマークダウン出力
    """
    with open(OUTPUT_FILE, "w", encoding="utf8") as out:
        out.write("# Unity Project Dump for AI\n\n")
        
        # スキャンしたディレクトリ一覧を記載
        out.write("## Scanned Directories\n\n")
        for d in target_dirs:
            out.write(f"- {d}\n")
        out.write("\n")
        
        out.write("## Project Tree (Partial)\n\n")
        out.write("```\n")
        out.write(tree)
        out.write("\n```\n\n")
        
        out.write("## File List\n\n")
        
        # ディレクトリごとにファイルリストをグループ化
        current_base = None
        for rel, _, base_name, src_dir in files:
            if current_base != base_name:
                current_base = base_name
                out.write(f"\n### {base_name} (from: {src_dir})\n\n")
            out.write(f"- {rel}\n")
        
        out.write("\n---\n\n")
        
        # ファイル内容（ディレクトリごとにグループ化）
        current_base = None
        for rel, full, base_name, src_dir in files:
            if current_base != base_name:
                current_base = base_name
                out.write(f"\n## 📁 {base_name} (Source: {src_dir})\n\n")
            
            out.write(f"### {rel}\n\n")
            
            # 拡張子を言語指定として使用
            ext = os.path.splitext(rel)[1][1:] if os.path.splitext(rel)[1] else ""
            # 特殊な拡張子のエイリアス
            lang_map = {
                "cs": "csharp",
                "shader": "hlsl",
                "compute": "hlsl",
                "cginc": "hlsl",
                "hlsl": "hlsl",
                "py": "python",
                "js": "javascript",
                "yml": "yaml",
                "unity": "xml",
                "prefab": "xml",
                "mat": "yaml",
            }
            lang = lang_map.get(ext, ext or "text")
            
            out.write(f"```{lang}\n")
            
            try:
                with open(full, "r", encoding="utf8") as f:
                    out.write(f.read())
            except UnicodeDecodeError:
                try:
                    with open(full, "r", encoding="shift-jis") as f:
                        out.write(f.read())
                except:
                    out.write("[Unreadable File: Encoding Error]")
            except Exception as e:
                out.write(f"[Unreadable File: {str(e)}]")
            
            out.write("\n```\n\n---\n\n")

# ===== main =====

def main():
    print("Scanning project...")
    print(f"Target directories: {TARGET_DIRS}")
    
    # 存在するディレクトリのみフィルタリング
    valid_dirs = [d for d in TARGET_DIRS if os.path.exists(os.path.abspath(d))]
    
    if not valid_dirs:
        print("エラー: 有効なディレクトリが指定されていません。")
        return
    
    files = collect_files(valid_dirs)
    tree = build_tree(valid_dirs)
    dump_markdown(files, tree, valid_dirs)
    
    print(f"Done. {len(files)} files exported from {len(valid_dirs)} directories.")
    print(f"Output: {OUTPUT_FILE}")

if __name__ == "__main__":
    main()