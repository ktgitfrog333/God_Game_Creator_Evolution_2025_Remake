import os
import shutil
import chardet

# 絶対パスでコピー元とコピー先を指定
FROM_DIR = r".\Script_xylo"
TO_DIR = r".\Mains\Scripts\Script_xylo"

# コピー先ディレクトリを作成（存在しない場合）
os.makedirs(TO_DIR, exist_ok=True)

# .cs ファイルのコピー（更新されたファイルのみ）
for filename in os.listdir(FROM_DIR):
    if filename.endswith(".cs"):
        src_path = os.path.join(FROM_DIR, filename)
        dst_path = os.path.join(TO_DIR, filename)

        # 更新されたファイルのみコピー
        if not os.path.exists(dst_path) or os.path.getmtime(src_path) > os.path.getmtime(dst_path):
            shutil.copy2(src_path, dst_path)

# エンコーディングを検出するが、変換は行わない
def detect_encoding(file_path):
    with open(file_path, "rb") as f:
        raw_data = f.read()
    result = chardet.detect(raw_data)
    encoding = result["encoding"]
    
    # 判定が None または誤検出された場合の補正
    if encoding is None:
        return "utf-8"  # デフォルト
    elif encoding.lower() in ["ascii", "windows-1252", "iso-8859-1", "cp1254"]:
        return "shift_jis"  # 日本語の可能性を優先
    return encoding

# .cs ファイルの編集処理（元のエンコードを維持）
for filename in os.listdir(TO_DIR):
    if filename.endswith(".cs"):
        file_path = os.path.join(TO_DIR, filename)

        file_encoding = detect_encoding(file_path)

        # 読み込み時のエンコーディングエラーを防ぐ
        try:
            with open(file_path, "r", encoding=file_encoding) as f:
                lines = f.readlines()
        except UnicodeDecodeError:
            # Shift-JIS で試す（日本語ファイルの可能性がある）
            file_encoding = "shift_jis"
            with open(file_path, "r", encoding=file_encoding) as f:
                lines = f.readlines()

        new_lines = []
        inserted_namespace = False
        inside_using_block = True

        for line in lines:
            # `using` の後で `namespace` を挿入
            if inside_using_block and not line.strip().startswith("using"):
                new_lines.append("\nnamespace Mains.Script_xylo\n{\n")
                inserted_namespace = True
                inside_using_block = False

            new_lines.append(line)

        # `namespace` の閉じカッコを追加
        if inserted_namespace:
            new_lines.append("\n}\n")

        # 元のエンコーディングで書き込む
        with open(file_path, "w", encoding=file_encoding) as f:
            f.writelines(new_lines)

print("コピーとnamespaceの追加が完了しました。")
