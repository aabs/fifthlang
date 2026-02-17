#!/usr/bin/env python3
import json
import sys
from pathlib import Path

if len(sys.argv) < 4:
    print('Usage: generate_pr_body.py <family> <baseline-json> <out-md>')
    sys.exit(2)

family = sys.argv[1]
basefile = Path(sys.argv[2])
outfile = Path(sys.argv[3])

try:
    data = json.loads(basefile.read_text())
except Exception as e:
    print(f'Could not read {basefile}: {e}')
    data = {}

if isinstance(data, dict) and 'benchmarks' in data:
    bench = data.get('benchmarks', {})
else:
    bench = data if isinstance(data, dict) else {}

meta = data.get('meta', {}) if isinstance(data, dict) else {}

with open(outfile, 'w') as f:
    f.write(f"# Proposed baseline update for family `{family}`\n\n")
    f.write('This PR proposes updating the guard validation baseline for this runner family.\n\n')
    if meta:
        f.write('## Environment metadata\n')
        for k in ('os','cpu','dotnet_version','commit','family'):
            if meta.get(k):
                f.write(f'- **{k}**: {meta.get(k)}\n')
        f.write('\n')
    if bench:
        f.write('## Benchmarks (sample)\n')
        items = list(bench.items())[:12]
        f.write('| Benchmark | Median (ns) |\n')
        f.write('|---|---:|\n')
        for name,val in items:
            try:
                valf = float(val)
                f.write(f'| {name} | {valf:.3f} |\n')
            except Exception:
                f.write(f'| {name} | {val} |\n')
    else:
        f.write('No benchmark data present in baseline file.\n')

print(f'Wrote PR body to {outfile}')
