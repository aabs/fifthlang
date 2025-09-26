#!/usr/bin/env python3
import json
import sys
import glob
import os
from pathlib import Path

# Usage: compare_benchmarks.py --baseline baseline.json --threshold 5

import argparse
parser = argparse.ArgumentParser()
parser.add_argument('--baseline', required=True)
parser.add_argument('--threshold', type=float, default=5.0)
parser.add_argument('--update-baseline', action='store_true')
args = parser.parse_args()

results_dir = Path('BenchmarkDotNet.Artifacts') / 'results'
if not results_dir.exists():
    print('No BenchmarkDotNet results directory found; nothing to compare.')
    sys.exit(0)

# find report json files
report_files = list(results_dir.glob('**/*report.json')) + list(results_dir.glob('**/*.json'))
report_files = [p for p in report_files if 'report.json' in p.name or p.name.endswith('.json')]

report = None
data = None
if report_files:
    # prefer *report.json
    for p in report_files:
        if p.name.endswith('report.json'):
            report = p
            break
    if report is None:
        report = report_files[0]

    with open(report, 'r') as f:
        try:
            data = json.load(f)
        except Exception:
            data = None

# Attempt to extract benchmarks and median values
benchmarks = {}
# BenchmarkDotNet report format varies; search for "Benchmarks" array or top-level 'Benchmarks'
if data and isinstance(data, dict):
    if 'Benchmarks' in data and isinstance(data['Benchmarks'], list):
        for b in data['Benchmarks']:
            name = b.get('Method','Unknown')
            stats = b.get('Statistics') or b.get('Stats') or {}
            median = stats.get('Median')
            if median is None:
                # Try nested
                median = b.get('Summary',{}).get('Statistics',{}).get('Median') if isinstance(b.get('Summary'), dict) else None
            if median is not None:
                benchmarks[name] = float(median)
    elif 'Runs' in data and isinstance(data['Runs'], list):
        # Some formats
        for r in data['Runs']:
            name = r.get('Method','Unknown')
            stats = r.get('Statistics',{})
            median = stats.get('Median')
            if median is not None:
                benchmarks[name] = float(median)
    else:
        # fallback: inspect for top-level array
        for k,v in data.items():
            if isinstance(v, dict) and 'Statistics' in v:
                name = k
                stats = v['Statistics']
                if 'Median' in stats:
                    benchmarks[name] = float(stats['Median'])

# If no benchmarks were found via JSON, try CSV fallback
if not benchmarks:
    csv_files = list(results_dir.glob('**/*-report.csv')) + list(results_dir.glob('**/*report.csv'))
    if csv_files:
        import csv
        for p in csv_files:
            with open(p, newline='') as csvfile:
                reader = csv.DictReader(csvfile)
                for row in reader:
                    # Build a descriptive name from key parameters
                    method = row.get('Method') or row.get('Benchmark') or 'Unknown'
                    group = row.get('GroupCount') or row.get('Group') or ''
                    overloads = row.get('OverloadsPerGroup') or ''
                    pooling = row.get('UsePooling') or ''
                    name = f"{method} [GroupCount={group}, OverloadsPerGroup={overloads}, UsePooling={pooling}]"
                    mean_str = row.get('Mean') or ''
                    if mean_str:
                        # mean_str examples: '9.413 μs' or '183.958 us'
                        parts = mean_str.strip().split()
                        try:
                            val = float(parts[0].replace(',', ''))
                            unit = parts[1] if len(parts) > 1 else 'ns'
                        except Exception:
                            continue
                        # convert to nanoseconds for stable baseline storage
                        if unit.lower().startswith('ns'):
                            ns = val
                        elif unit.lower().startswith('μs') or unit.lower().startswith('us'):
                            ns = val * 1000.0
                        elif unit.lower().startswith('ms'):
                            ns = val * 1000.0 * 1000.0
                        else:
                            ns = val
                        benchmarks[name] = ns

if not benchmarks:
    print('No benchmarks with Median or Mean found in report. Searched JSON and CSV under BenchmarkDotNet.Artifacts/results')
    sys.exit(1)

# Load baseline
baseline_path = Path(args.baseline)
baseline = {}
if baseline_path.exists():
    try:
        baseline = json.loads(baseline_path.read_text())
    except Exception as e:
        print(f'Warning: could not read baseline: {e}')
        baseline = {}
else:
    print('Baseline file not present; skipping comparisons and reporting current medians as suggestion.')

# Compare
regressions = []
summary_lines = []
for name, median in sorted(benchmarks.items()):
    baseline_median = baseline.get(name)
    if baseline_median:
        delta = (median - baseline_median) / baseline_median * 100.0
        summary_lines.append(f'{name}: median={median:.3f}ns, baseline={baseline_median:.3f}ns, delta={delta:.2f}%')
        if delta > args.threshold:
            regressions.append((name, median, baseline_median, delta))
    else:
        summary_lines.append(f'{name}: median={median:.3f}ns, baseline=none')

# Emit summary
print('\nBenchmark summary:')
for l in summary_lines:
    print('  ' + l)

# Write a concise summary to step summary file if running in GH actions
github_step_summary = os.environ.get('GITHUB_STEP_SUMMARY')
if github_step_summary:
    with open(github_step_summary, 'a') as s:
        s.write('\n### Guard validation benchmark summary\n')
        for l in summary_lines:
            s.write('- ' + l + '\n')

# Optionally update baseline artifact
if args.update_baseline:
    out = { name: median for name, median in benchmarks.items() }
    with open('guard_validation_current_baseline.json', 'w') as ob:
        json.dump(out, ob, indent=2)
    print('\nWrote current baseline to guard_validation_current_baseline.json (upload as artifact to capture).')

if regressions:
    print('\nPerformance regressions detected:')
    for r in regressions:
        print(f' - {r[0]} increased by {r[3]:.2f}% (from {r[2]:.3f} to {r[1]:.3f} ns)')
    print('\nFailing due to regressions exceeding threshold of {}%'.format(args.threshold))
    sys.exit(2)

print('\nNo regressions exceeding threshold. OK.')
sys.exit(0)
