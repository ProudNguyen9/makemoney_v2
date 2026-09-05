#!/bin/bash
# Crawl toan bo link noi bo cua site, ghi lai HTTP status tung URL
BASE="http://localhost:5053"
OUT_DIR="/tmp/crawl"
mkdir -p "$OUT_DIR"
: > "$OUT_DIR/results.txt"
: > "$OUT_DIR/queue.txt"
echo "/" >> "$OUT_DIR/queue.txt"

declare -A seen
COUNT=0
MAX=2000

skip_asset() {
  case "$1" in
    *.css|*.js|*.png|*.jpg|*.jpeg|*.webp|*.gif|*.svg|*.ico|*.woff|*.woff2|*.ttf|*.eot|*.map|*.webmanifest|*.txt|*.xml) return 0 ;;
    *) return 1 ;;
  esac
}

while IFS= read -r raw; do
  [ ${COUNT} -lt "$MAX" ] || break
  # bo trong, bo anchor, bo query rong
  path="${raw%%#*}"
  path="${path%%\?*}${raw#*${path%%\?*}}" # keep as-is; normalize below
  path="$raw"
  path="${path%%#*}"
  [ -z "$path" ] && path="/"
  [ -n "${seen[$path]}" ] && continue
  seen[$path]=1
  COUNT=$((COUNT+1))

  code=$(curl -s -o "$OUT_DIR/page.html" -w "%{http_code}" --max-time 20 "$BASE$path")
  echo "$code $path" >> "$OUT_DIR/results.txt"

  if [ "$code" != "200" ]; then
    continue # khong trich link tu trang loi
  fi

  # trich link noi bo
  grep -oE 'href="[^"#]+"' "$OUT_DIR/page.html" \
    | sed 's/^href="//; s/"$//' \
    | while IFS= read -r l; do
        case "$l" in
          /*) echo "$l" ;;
          http://localhost:5053/*) echo "${l#http://localhost:5053}" ;;
        esac
      done > "$OUT_DIR/links.txt"

  while IFS= read -r l; do
    skip_asset "$l" && continue
    [ -n "${seen[$l]}" ] && continue
    echo "$l" >> "$OUT_DIR/queue.txt"
  done < "$OUT_DIR/links.txt"

done < "$OUT_DIR/queue.txt"

echo "TOTAL=$COUNT"
