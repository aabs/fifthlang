#!/usr/bin/env python3
import argparse
import json
import subprocess
import sys
import time

parser = argparse.ArgumentParser()
parser.add_argument('--workflow', required=True)
parser.add_argument('--repo', required=True)
parser.add_argument('--ref', default='master')
parser.add_argument('--interval', type=int, default=15)
parser.add_argument('--timeout', type=int, default=3600)
args = parser.parse_args()

start = time.time()
print(f"Watching workflow '{args.workflow}' in repo {args.repo} on ref {args.ref}")
run_info = None
while True:
    try:
        p = subprocess.run([
            'gh','run','list',
            '--repo', args.repo,
            '--workflow', args.workflow,
            '--limit', '1',
            '--json', 'databaseId,status,conclusion,headBranch,headSha'
        ], capture_output=True, text=True, check=True)
        data = json.loads(p.stdout)
        if not data:
            print('No workflow runs found yet.')
            run_info = None
        else:
            run = data[0]
            run_info = run
            rid = run.get('databaseId')
            status = run.get('status')
            concl = run.get('conclusion')
            print(f"Run {rid}: status={status} conclusion={concl} branch={run.get('headBranch')} sha={run.get('headSha')}")
            if status == 'completed':
                print('Workflow run completed; fetching logs...')
                # fetch logs
                try:
                    view = subprocess.run(['gh','run','view', str(rid), '--repo', args.repo, '--log'], capture_output=True, text=True, check=True)
                    out = view.stdout
                    logfile = f'BenchmarkRun-{rid}.log'
                    with open(logfile, 'w') as f:
                        f.write(out)
                    print(f'Logs written to {logfile}')
                except subprocess.CalledProcessError as e:
                    print('Failed to fetch logs:', e, file=sys.stderr)
                sys.exit(0 if (concl == 'success' or concl is None) else 2)
    except subprocess.CalledProcessError as e:
        print('gh run list failed:', e, file=sys.stderr)

    if time.time() - start > args.timeout:
        print('Timeout waiting for workflow run to complete', file=sys.stderr)
        sys.exit(3)

    time.sleep(args.interval)
