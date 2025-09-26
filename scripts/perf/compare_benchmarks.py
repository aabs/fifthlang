#!/usr/bin/env python3
import json
import sys
import glob
import os
from pathlib import Path
import platform
import subprocess

# Usage: compare_benchmarks.py --baseline baseline.json --threshold 5

import argparse
parser = argparse.ArgumentParser()
parser.add_argument('--baseline', required=True)
parser.add_argument('--threshold', type=float, default=5.0)
parser.add_argument('--update-baseline', action='store_true')
parser.add_argument('--allow-env-mismatch', action='store_true', help='Allow comparing baselines produced on different OS/CPU without failing due to env mismatch')
parser.add_argument('--baseline-family', default=None, help='Optional family name (runner/CPU) used to name per-runner baselines')
args = parser.parse_args()

results_dir = Path('BenchmarkDotNet.Artifacts') / 'results'
if not results_dir.exists():
    print('No BenchmarkDotNet results directory found; nothing to compare.')
    sys.exit(0)

# small helper to capture environment metadata
def capture_env_meta():
    meta = {}
    meta['os'] = platform.system() + ' ' + platform.release()
    # Try common ways to obtain a human-friendly CPU id
    cpu = platform.processor() or ''
    if not cpu:
        # Linux: try /proc/cpuinfo
        try:
            if platform.system().lower() == 'linux':
                with open('/proc/cpuinfo', 'r') as f:
                    for line in f:
                        if line.strip().startswith('model name'):
                            cpu = line.split(':',1)[1].strip()
                            break
        except Exception:
            cpu = ''
    # fallback to lscpu
    if not cpu:
        try:
            out = subprocess.check_output(['lscpu'], stderr=subprocess.DEVNULL, text=True)
            for line in out.splitlines():
                if line.startswith('Model name:') or line.startswith('Model name'): 
                    cpu = line.split(':',1)[1].strip()
                    break
        except Exception:
            pass
    meta['cpu'] = cpu or 'unknown'
    # dotnet version (best-effort)
    try:
        dotnet_ver = subprocess.check_output(['dotnet','--version'], stderr=subprocess.DEVNULL, text=True).strip()
    except Exception:
        dotnet_ver = ''
    meta['dotnet_version'] = dotnet_ver
    # Git commit if available from environment
    meta['commit'] = os.environ.get('GITHUB_SHA') or os.environ.get('COMMIT') or ''
    return meta

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
baseline_meta = None
if baseline_path.exists():
    try:
        baseline_raw = json.loads(baseline_path.read_text())
        # backward compatibility: baseline may be a simple mapping, or a wrapper with 'benchmarks' + 'meta'
        if isinstance(baseline_raw, dict) and 'benchmarks' in baseline_raw and isinstance(baseline_raw['benchmarks'], dict):
            baseline = baseline_raw['benchmarks']
            baseline_meta = baseline_raw.get('meta')
        elif isinstance(baseline_raw, dict):
            baseline = baseline_raw
        else:
            baseline = {}
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

# include environment metadata in the printed summary
current_meta = capture_env_meta()
# attach provided family to meta if given
if args.baseline_family:
    current_meta['family'] = args.baseline_family
print('\nEnvironment:')
print(f"  OS: {current_meta['os']}")
print(f"  CPU: {current_meta['cpu']}")
print(f"  dotnet: {current_meta['dotnet_version']}")

if baseline_meta:
    print('\nBaseline metadata:')
    print(f"  OS: {baseline_meta.get('os','(unknown)')}")
    print(f"  CPU: {baseline_meta.get('cpu','(unknown)')}")
    print(f"  dotnet: {baseline_meta.get('dotnet_version','(unknown)')}")
    if baseline_meta.get('commit'):
        print(f"  commit: {baseline_meta.get('commit')}")
    if baseline_meta.get('family'):
        print(f"  family: {baseline_meta.get('family')}")

# Write a concise summary to step summary file if running in GH actions
github_step_summary = os.environ.get('GITHUB_STEP_SUMMARY')
if github_step_summary:
    with open(github_step_summary, 'a') as s:
        s.write('\n### Guard validation benchmark summary\n')
        for l in summary_lines:
            s.write('- ' + l + '\n')
        s.write('\n#### Environment\n')
        s.write(f"- OS: {current_meta['os']}\n")
        s.write(f"- CPU: {current_meta['cpu']}\n")
        s.write(f"- dotnet: {current_meta['dotnet_version']}\n")
        if baseline_meta:
            s.write('\n#### Baseline metadata\n')
            s.write(f"- OS: {baseline_meta.get('os','(unknown)')}\n")
            s.write(f"- CPU: {baseline_meta.get('cpu','(unknown)')}\n")

# Optionally update baseline artifact
if args.update_baseline:
    out_benchmarks = { name: median for name, median in benchmarks.items() }
    out = { 'benchmarks': out_benchmarks, 'meta': current_meta }
    # determine output filename
    if args.baseline_family:
        out_filename = f'guard_validation_current_baseline.{args.baseline_family}.json'
    else:
        out_filename = 'guard_validation_current_baseline.json'
    with open(out_filename, 'w') as ob:
        json.dump(out, ob, indent=2)
    print(f"\nWrote current baseline to {out_filename} (upload as artifact to capture).")

# If baseline exists, detect environment mismatch
if baseline_meta and not args.allow_env_mismatch:
    mismatch_reasons = []
    if baseline_meta.get('os') and baseline_meta.get('os') != current_meta.get('os'):
        mismatch_reasons.append(f"OS mismatch (baseline='{baseline_meta.get('os')}' vs current='{current_meta.get('os')}')")
    if baseline_meta.get('cpu') and baseline_meta.get('cpu') and baseline_meta.get('cpu') not in current_meta.get('cpu', ''):
        # CPU strings can vary; check for substring match first
        mismatch_reasons.append(f"CPU mismatch (baseline contains '{baseline_meta.get('cpu')}' vs current contains '{current_meta.get('cpu')}')")
    if mismatch_reasons:
        print('\nBaseline environment mismatch detected:')
        for r in mismatch_reasons:
            print(' - ' + r)
        print('\nTo override this behaviour and allow comparison across environments, run with --allow-env-mismatch.\nTo update the baseline for this environment, run with --update-baseline and then promote the new baseline file.')
        # exit with a distinct exit code to indicate env mismatch
        sys.exit(3)

if regressions:
    print('\nPerformance regressions detected:')
    for r in regressions:
        print(f' - {r[0]} increased by {r[3]:.2f}% (from {r[2]:.3f} to {r[1]:.3f} ns)')
    print('\nFailing due to regressions exceeding threshold of {}%'.format(args.threshold))
    sys.exit(2)

print('\nNo regressions exceeding threshold. OK.')
sys.exit(0)
